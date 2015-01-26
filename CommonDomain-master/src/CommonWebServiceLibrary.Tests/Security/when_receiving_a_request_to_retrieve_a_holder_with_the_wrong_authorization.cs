using System;
using System.Collections.Generic;
using Autofac;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.Security.Models;
using CommonWebServiceLibrary.Extensions;
using Machine.Specifications;
using Nancy;
using Nancy.Testing;
using NodaTime;
using Raven.Client;

namespace CommonWebServiceLibrary.Tests.Security
{
    class when_receiving_a_request_to_retrieve_a_holder_with_the_wrong_authorization
    {
        private static Browser _browser;
        private static BrowserResponse _result;
        private static SecurityTestBootstrapper _bootstrapper;
        private static Guid _clientId;
        private static byte[] _authenticationKey;
        private static Guid _ownerId;
        private static Guid _userId;
        private static string _userName;
        private static long _timeStamp;
        private static string _nonce;
        private static Guid _subHolderId;

        private sealed class Holder : IViewDocument
        {
            public string Id { get; set; }
            public bool Deleted { get; set; }
            public Guid HolderId { get; set; }
            public IList<Guid> HandledMessages { get; set; }
            public IDictionary<string, Instant> FieldChanges { get; set; }
            public Instant LastChangeTime { get; set; }
        }

        private sealed class GetHolderModule : NancyModule
        {
            public GetHolderModule()
            {
                Post["/{holder_id}/holders/{id}", true] = async (parameters, token) =>
                {
                    using (var session = _bootstrapper.DocumentStore.OpenAsyncSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;

                        var result = this.RequiresAuthorization<Holder>(session, (Guid)parameters.id, default(Guid), new string[] { });

                        return result;
                    }
                };
            }
        }

        private Establish context = () =>
        {
            _bootstrapper = new SecurityTestBootstrapper();
            _bootstrapper.TestModules.Add(new GetHolderModule());
            _clientId = Guid.NewGuid();
            _authenticationKey = new byte[8];
            var random = new Random();
            random.NextBytes(_authenticationKey);
            _timeStamp = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmssffff"));
            _userName = "someUsername";
            _nonce = "someNonce";
            _subHolderId = Guid.NewGuid();
            _ownerId = Guid.NewGuid();
            _userId = Guid.NewGuid();

            SecurityTestBootstrapper.ClientCredentialsList = new List<ClientCredentials>
            {
                    new ClientCredentials
                    {
                            AuthenticationKey = _authenticationKey,
                            ClientId = _clientId.AsId(typeof(HolderClientCredentials)),
                            HolderId = _ownerId.AsId(typeof(HolderWithClients)),
                            UserId = _userId.AsId(typeof(UserWithHolder)),
                            Username = _userName,
                            OwnerId = _ownerId.AsId(typeof(HolderWithClients)),
                            Roles = new List<string>()
                    }
                };

            _browser = new Browser(_bootstrapper);

            using (var session = _bootstrapper.Container.Resolve<IAsyncDocumentSession>())
            {
                session.StoreAsync(new HolderWithClients
                {
                    Id = _subHolderId.AsId(typeof(HolderWithClients))
                }).Wait();
                session.SaveChangesAsync().Wait();
            }
        };

        private Because of = () =>
        {
            var url = "/" + _ownerId + "/holders/" + _subHolderId;

            _result = _browser.Post(url, ww =>
            {
                ww.HttpRequest();

                var authenticationMac = new AuthenticationMac(_userName, _clientId, _timeStamp, _nonce, "POST",
                    url, _bootstrapper.Container.Resolve<ICryptoProvider>(), _authenticationKey);

                ww.Header("Authorization", string.Concat("MAC ", authenticationMac.ToString()));
            });
        };

        private It the_result_should_be_Unauthorized = () => _result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }
}
