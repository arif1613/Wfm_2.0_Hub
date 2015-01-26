//using System;
//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;

//namespace CommonReadModelLibrary.Tests.Views.UserWithHolderView
//{
//    public class when_a_user_is_removed : RavenTest
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

//                _session.StoreAsync(new UserWithHolder
//                    {
//                        Id = _userId.AsId(typeof (UserWithHolder)),
//                        OwnerDocumentId = _holderId.AsId(typeof (HolderWithClients)),
//                        Username = _username
//                    }).Await();
//                _session.SaveChangesAsync().Await();
//            };

//        private Because of = () =>
//            {
//                _view.Handle(
//                    new UserRemoved(Guid.NewGuid(), Guid.NewGuid(), _userId, _holderId), false).Await();
//            };

//        private It the_user_with_holder_is_marked_as_deleted = () =>
//            {
//                var user =
//                    _documentStore.OpenSession()
//                                  .Load<UserWithHolder>(_userId.AsId(typeof (UserWithHolder)));
//                user.Deleted.ShouldBeTrue();
//            };
//    }
//}