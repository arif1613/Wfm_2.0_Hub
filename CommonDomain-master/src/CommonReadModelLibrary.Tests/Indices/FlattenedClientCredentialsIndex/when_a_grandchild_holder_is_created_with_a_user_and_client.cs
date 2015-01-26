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
    public class when_a_grandchild_holder_is_created_with_a_user_and_client : RavenTest
    {
        private static IDocumentStore _documentStore;
        private static IDocumentSession _session;
        private static string _holderKey;
        private static string _clientKey;
        private static string _userKey;
        private static string _username;
        private static IList<string> _roles;
        private static PasswordDetails _pwdDetails;
        private static IEnumerable<ClientCredentials> _clientCredentials;
        private static string _childId;
        private static string _childClientKey;
        private static string _childUserKey;
        private static string _childUsername;
        private static IList<string> _childRoles;
        private static PasswordDetails _childPwdDetails;
        private static string _grandChildId;
        private static string _grandChildClientKey;
        private static string _grandChildUserKey;
        private static string _grandChildUsername;
        private static IList<string> _grandChildRoles;
        private static PasswordDetails _grandChildPwdDetails;

        private Establish context = () =>
        {
            _documentStore = CreateDocumentStore();
            _session = _documentStore.OpenSession();
            _holderKey = Guid.NewGuid().AsId(typeof(HolderWithClients));
            _clientKey = Guid.NewGuid().AsId(typeof(HolderClientCredentials));
            _userKey = Guid.NewGuid().AsId(typeof(UserWithHolder));
            _username = "test";
            _roles = new List<string> { UserRoles.SuperAdmin };
            _pwdDetails = new PasswordDetails();

            _childId = Guid.NewGuid().AsId(typeof(HolderWithClients));
            _childClientKey = Guid.NewGuid().AsId(typeof(HolderClientCredentials));
            _childUserKey = Guid.NewGuid().AsId(typeof(UserWithHolder));
            _childUsername = "test2";
            _childRoles = new List<string> { "HolderAdministrator" };
            _childPwdDetails = new PasswordDetails();

            _grandChildId = Guid.NewGuid().AsId(typeof(HolderWithClients));
            _grandChildClientKey = Guid.NewGuid().AsId(typeof(HolderClientCredentials));
            _grandChildUserKey = Guid.NewGuid().AsId(typeof(UserWithHolder));
            _grandChildUsername = "test3";
            _grandChildRoles = new List<string> { "HolderAdministrator" };
            _grandChildPwdDetails = new PasswordDetails();

            //Master
            _session.Store(new HolderWithClients
            {
                Id = _holderKey,
                Clients = { _clientKey },
                Children = new List<string>() { _childId }
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

            //Child
            _session.Store(new HolderWithClients
            {
                Id = _childId,
                Clients = { _childClientKey },
                Children = new List<string>() { _grandChildId }
            });
            _session.Store(new HolderClientCredentials
            {
                Id = _childClientKey,
                AuthenticationKey = new byte[100]
            });
            _session.Store(new UserWithHolder
            {
                Id = _childUserKey,
                OwnerDocumentId = _childId,
                Username = _childUsername,
                Password = _childPwdDetails,
                Roles = _childRoles
            });

            //Grandchild
            _session.Store(new HolderWithClients
            {
                Id = _grandChildId,
                Clients = { _grandChildClientKey }                
            });
            _session.Store(new HolderClientCredentials
            {
                Id = _grandChildClientKey,
                AuthenticationKey = new byte[100]
            });
            _session.Store(new UserWithHolder
            {
                Id = _grandChildUserKey,
                OwnerDocumentId = _grandChildId,
                Username = _grandChildUsername,
                Password = _grandChildPwdDetails,
                Roles = _grandChildRoles
            });
            _session.SaveChanges();

            IndexCreation.CreateIndexes(typeof(FlattenedClientCredentials).Assembly, _documentStore);
        };

        private Because of = () =>
        {
            _clientCredentials = _documentStore.OpenSession()
                                               .Query<ClientCredentials, FlattenedClientCredentials>()
                                               .Customize(c => c.WaitForNonStaleResults())
                                               .Where(c => c.ClientId == _clientKey && c.Username == _username);
        };

        private It there_should_be_three_credentials_for_the_client_of_the_owner =
            () => _clientCredentials.Count().ShouldEqual(3);

        private It one_credential_for_its_parent = () =>
        {
            var credentials = _clientCredentials.SingleOrDefault(c => c.HolderId == _holderKey);
            credentials.ShouldNotBeNull();
        };

        private It one_credential_for_the_child_holder = () =>
        {
            var credentials = _clientCredentials.SingleOrDefault(c => c.HolderId == _childId);
            credentials.ShouldNotBeNull();
        };

        private It one_credential_for_the_grandchild_holder = () =>
        {
            var credentials = _clientCredentials.SingleOrDefault(c => c.HolderId == _grandChildId);
            credentials.ShouldNotBeNull();
        };

        private It the_child_client_should_have_a_credential_for_its_parent_and_one_for_its_child = () =>
        {
            var credentials = _documentStore.OpenSession()
                                           .Query<ClientCredentials, FlattenedClientCredentials>()
                                           .Customize(c => c.WaitForNonStaleResults())
                                           .Where(c => c.ClientId == _childClientKey && c.Username == _childUsername);
            credentials.Count().ShouldEqual(2);
            credentials.Count(c => c.HolderId == _childId).ShouldEqual(1);
            credentials.Count(c => c.HolderId == _grandChildId).ShouldEqual(1);
        };

        private It the_grandchild_client_should_have_a_credential_for_its_parent = () =>
        {
            var credential = _documentStore.OpenSession()
                                           .Query<ClientCredentials, FlattenedClientCredentials>()
                                           .Customize(c => c.WaitForNonStaleResults())
                                           .SingleOrDefault(c => c.ClientId == _grandChildClientKey && c.Username == _grandChildUsername);
            credential.ShouldNotBeNull();
            credential.HolderId.ShouldEqual(_grandChildId);
        };
    }
}
