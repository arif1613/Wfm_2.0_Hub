using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomainLibrary.Common;
using CommonReadModelLibrary.Security.Indices;
using CommonReadModelLibrary.Security.Models;
using Machine.Specifications;
using Raven.Client;
using Raven.Client.Indexes;

namespace CommonReadModelLibrary.Tests.Indices.FlattenedClientCredentialsIndex
{
    public class when_a_holder_a_client_and_a_user_is_created : RavenTest
    {
        private Establish context = () =>
            {
                _documentStore = CreateDocumentStore();
                _session = _documentStore.OpenSession();
                _holderKey = Guid.NewGuid().AsId(typeof(HolderWithClients));
                _clientKey = Guid.NewGuid().AsId(typeof(HolderClientCredentials));
                _userKey = Guid.NewGuid().AsId(typeof (UserWithHolder));
                _username = "test";
                _roles= new List<string> {UserRoles.SuperAdmin};
                _pwdDetails = new PasswordDetails();

                _session.Store(new HolderWithClients
                    {
                        Id = _holderKey,
                        Clients = {_clientKey}
                    });
                _session.Store(new HolderClientCredentials
                    {
                        Id = _clientKey,
                        AuthenticationKey = new byte[100]
                    });
                _session.Store(new UserWithHolder
                    {
                        Id = _userKey,
                        OwnerDocumentId = _holderKey,
                        Username = _username,
                        Password = _pwdDetails,
                        Roles = _roles
                    });
                _session.SaveChanges();

                IndexCreation.CreateIndexes(typeof(FlattenedClientCredentials).Assembly, _documentStore);
            };

        private Because of = () =>
            {
                _clientCredentials = _documentStore.OpenSession()
                                                   .Query<ClientCredentials, FlattenedClientCredentials>()
                                                   .Customize(c => c.WaitForNonStaleResults()).SingleOrDefault(c => c.ClientId == _clientKey && c.Username == _username);
            };

        private It should_have_holder_credentials = () => _clientCredentials.ShouldNotBeNull();

        private It should_have_a_holder_id = () => _clientCredentials.HolderId.ShouldEqual(_holderKey);

        private It should_have_a_client_id = () => _clientCredentials.ClientId.ShouldEqual(_clientKey);

        private It should_have_a_client_authentication_key = () => _clientCredentials.AuthenticationKey.ShouldBeLike(new byte[100]);

        private It should_have_a_username = () => _clientCredentials.Username.ShouldEqual(_username);

        private It should_have_password_details = () => _clientCredentials.Password.ShouldBeLike(_pwdDetails);

        private It should_have_user_roles = () => _clientCredentials.Roles.ShouldBeLike(_roles);

        private static IDocumentStore _documentStore;
        private static IDocumentSession _session;
        private static string _holderKey;
        private static string _clientKey;
        private static string _userKey;
        private static string _username;
        private static IList<string> _roles;
        private static PasswordDetails _pwdDetails;
        private static ClientCredentials _clientCredentials;
    }
}