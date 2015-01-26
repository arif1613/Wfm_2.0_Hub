//using System;
//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts;
//using Security.Contracts.Messages.Events;

//namespace CommonReadModelLibrary.Tests.Views.UserWithHolderView
//{
//    public class when_a_user_is_created : RavenTest
//    {
//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.UserWithHolderView _view;
//        private static Guid _holderId;
//        private static Guid _userId;
//        private static string _username;

//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.UserWithHolderView(_session);
//                _holderId = Guid.NewGuid();
//                _userId = Guid.NewGuid();
//                _username = "test";
//            };

//        private Because of = () =>
//            {
//                _view.Handle(
//                    new UserCreated(Guid.NewGuid(), Guid.NewGuid(), _userId, _holderId,
//                                    new AuthenticationDetails(_username, "password"), "", "", ""), false).Await();
//            };

//        private It a_user_with_holder_document_is_created = () =>
//            {
//                var user =
//                    _documentStore.OpenSession()
//                                  .Load<UserWithHolder>(_userId.AsId(typeof (UserWithHolder)));
//                user.ShouldNotBeNull();
//            };

//        private It the_username_is_set = () =>
//        {
//            var user =
//                _documentStore.OpenSession()
//                              .Load<UserWithHolder>(_userId.AsId(typeof(UserWithHolder)));
//            user.Username.ShouldEqual(_username);
//        };

//        private It the_holder_id_is_set = () =>
//        {
//            var user =
//                _documentStore.OpenSession()
//                              .Load<UserWithHolder>(_userId.AsId(typeof(UserWithHolder)));
//            user.OwnerDocumentId.ShouldEqual(_holderId.AsId(typeof(HolderWithClients)));
//        };
//    }
//}