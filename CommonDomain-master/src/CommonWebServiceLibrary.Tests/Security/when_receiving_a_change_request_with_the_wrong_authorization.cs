﻿using System;
using System.Collections.Generic;
using Autofac;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.Security.Models;
using CommonWebServiceLibrary.Extensions;
using CommonWebServiceLibrary.Security;
using Machine.Specifications;
using Nancy;
using Nancy.Testing;
using NodaTime;
using Raven.Client;

namespace CommonWebServiceLibrary.Tests.Security
{
    public class when_receiving_a_change_request_with_the_wrong_authorization
    {
        private static Browser _browser;
        private static BrowserResponse _result;
        private static SecurityTestBootstrapper _bootstrapper;
        private static Guid _clientId;
        private static byte[] _authenticationKey;
        private static Guid _holderId;
        private static Guid _userId;
        private static string _userName;
        private static long _timeStamp;
        private static string _nonce;
        private static Guid _resourceId;

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
                Post["/{holder_id}/testResource/{id}", true] = async (parameters, token) =>
                {
                    using (var session = _bootstrapper.DocumentStore.OpenAsyncSession())
                    {
                        session.Advanced.UseOptimisticConcurrency = true;

                        return this.RequiresAuthorization<TestResource>(session, (Guid)parameters.holder_id, (Guid)parameters.id);
                    }
                };
            }
        }

        private Establish context = () =>
        {
            _bootstrapper = new SecurityTestBootstrapper();
            _bootstrapper.TestModules.Add(new TestModule());
            _clientId = Guid.NewGuid();
            _authenticationKey = new byte[8];
            var random = new Random();
            random.NextBytes(_authenticationKey);
            _timeStamp = long.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmssffff"));
            _userName = "someUsername";
            _nonce = "someNonce";
            _resourceId = Guid.NewGuid();
            _holderId = Guid.NewGuid();
            _userId = Guid.NewGuid();

            SecurityTestBootstrapper.ClientCredentialsList = new List<ClientCredentials>()
                {
                    new ClientCredentials()
                        {
                            AuthenticationKey = _authenticationKey,
                            ClientId = _clientId.AsId(typeof(HolderClientCredentials)),
                            HolderId = Guid.NewGuid().AsId(typeof(HolderWithClients)),
                            UserId = _userId.AsId(typeof(UserWithHolder)),
                            Username = _userName,
                            OwnerId = _holderId.AsId(typeof(HolderWithClients)),
                            Roles = new List<string>(){}
                        }
                };

            _browser = new Browser(_bootstrapper);

            using (var session = _bootstrapper.Container.Resolve<IAsyncDocumentSession>())
            {
                session.StoreAsync(new TestResource
                {
                    Id = _resourceId.AsId(typeof(TestResource)),
                    HolderId = _holderId
                }).Wait();
                session.SaveChangesAsync().Wait();
            }
        };

        private Because of = () =>
        {
            _result = _browser.Post("/" + _holderId + "/testResource/" + _resourceId, ww =>
            {
                ww.HttpRequest();

                var authenticationMac = new AuthenticationMac(_userName, _clientId, _timeStamp, _nonce, "POST",
                                                              "/" + _holderId + "/testResource/" + _resourceId, _bootstrapper.Container.Resolve<ICryptoProvider>(), _authenticationKey);

                ww.Header("Authorization", string.Concat("MAC ", authenticationMac.ToString()));
            });
        };

        private It the_result_should_be_unauthorized = () => _result.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);
    }
}
