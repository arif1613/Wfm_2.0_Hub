//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;
//using System;

//namespace CommonReadModelLibrary.Tests.Views.RolesView
//{
//    public class when_a_role_is_created : RavenTest
//    {
//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.RolesView(_session);
//                _roleId = Guid.NewGuid();
//                _holerId = Guid.NewGuid();
//                _name = "test";
//            };

//        private Because of = () =>
//            {
//                _view.Handle(new RoleCreated(Guid.NewGuid(), Guid.NewGuid(), _roleId, _holerId, _name), false).Await();
//            };

//        private It should_have_created_a_role = () =>
//            _documentStore.OpenSession().Load<Role>(_roleId.AsId(typeof (Role))).ShouldNotBeNull();

//        private It should_have_set_the_role_name = () =>
//            _documentStore.OpenSession().Load<Role>(_roleId.AsId(typeof(Role))).Name.ShouldEqual(_name);

//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.RolesView _view;
//        private static Guid _roleId;
//        private static Guid _holerId;
//        private static string _name;
//    }
//}