using System;
using System.Collections.Generic;
using Autofac;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.Security.Models;
using CommonWebServiceLibrary.Extensions;
using CommonWebServiceLibrary.Security;
using Machine.Specifications;
using Nancy;
using Nancy.Testing;
using NodaTime;

namespace CommonWebServiceLibrary.Tests.Security
{
    public class when_receiving_a_create_request_with_the_wrong_authorization
    {
        private static Browser _browser;
        private static BrowserResponse _result;
        private static SecurityTestBootstrapper _bootstrapper;
        private static Guid _clientId;
        private static byte[] _authenticationKey;
        private static string _holderId;
        private static string _ownerId;
        private static string _userId;
        private static string _userName;
        private static long _timeStamp;
        private static string _nonce;

        private class TestResource : IViewDocument
        {
            public string Id { get; set; }
            public bool Deleted { get; set; }
            public Guid HolderId { get; set; }
            public IList<Guid> HandledMessages { get; set; }
            public IDictionary<string, Instant> FieldChanges { get; set; }
            public Instant LastChangeTime { get; set; }
        }

        private class TestModule : NancyModule
        {
            public TestModule()
            {
                Post["/{holder_id}/testResource", true] = async (parameters, token) =>
                    {
                        using (var session = _bootstrapper.DocumentStore.OpenAsyncSession())
                        {
                            session.Advanced.UseOptimisticConcurrency = true;

                            return this.RequiresAuthorization<TestResource>(session, (Guid)parameters.holder_id);
                        }
                    };
            }
        }

        private Establish context = () =>
        {
            _bootstrapper = new SecurityTestBootstrapper();
            _bootstrapper.TestModules.Add(new TestModule());
            _clientId = Guid.NewGuid();
            _authenticationKey = new byte[] { };
            var random = new Random();
            random.NextBytes(_authenticationKey);
            _timeStamp = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmssffff"));
            _userName = "someUsername";
            _nonce = "someNonce";

            SecurityTestBootstrapper.ClientCredentialsList = new List<ClientCredentials>()
                {
                    new ClientCredentials()
                        {
                            AuthenticationKey = _authenticationKey,
                            ClientId = _clientId.ToString(),
                            HolderId = _holderId,
                            OwnerId = _ownerId,
                            UserId = _userId,
                            Username = _userName
                        }
                };

            _browser = new Browser(_bootstrapper);
        };

        private Because of = () =>
        {
            _result = _browser.Post("/" + Guid.NewGuid() + "/testResource", ww =>
            {
                ww.HttpRequest();

                var authenticationMac = new AuthenticationMac(_userName, _clientId, _timeStamp, _nonce, "GET",
                                                              "/" + _holderId, _bootstrapper.Container.Resolve<ICryptoProvider>(), _authenticationKey);

                ww.Header("Authorization", string.Concat("MAC ", authenticationMac.ToString()));
            });
        };

        private It the_result_should_be_unauthorized = () => _result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }
}
