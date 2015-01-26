//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;
//using System;

//namespace CommonReadModelLibrary.Tests.Views.RolesView
//{
//    public class when_a_role_is_removed : RavenTest
//    {
//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.RolesView(_session);
//                _roleId = Guid.NewGuid();
//                _holerId = Guid.NewGuid();
//            };

//        private Because of = () =>
//            {
//                _view.Handle(new RoleRemoved(Guid.NewGuid(), Guid.NewGuid(), _roleId, _holerId), false).Await();
//            };

//        private It should_have_set_the_role_to_deleted = () =>
//            _documentStore.OpenSession().Load<Role>(_roleId.AsId(typeof(Role))).Deleted.ShouldBeTrue();

//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.RolesView _view;
//        private static Guid _roleId;
//        private static Guid _holerId;
//    }
//}