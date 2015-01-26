//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;
//using System;

//namespace CommonReadModelLibrary.Tests.Views.RolesView
//{
//    public class when_a_claim_is_added_to_a_role : RavenTest
//    {
//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.RolesView(_session);
//                _roleId = Guid.NewGuid();
//                _holerId = Guid.NewGuid();
//                _claim = "test";
//            };

//        private Because of = () =>
//            {
//                _view.Handle(new RoleClaimAdded(Guid.NewGuid(), Guid.NewGuid(), _roleId, _holerId, _claim), false).Await();
//            };

//        private It should_have_added_the_claim = () =>
//            _documentStore.OpenSession().Load<Role>(_roleId.AsId(typeof(Role))).Claims.ShouldContain(_claim);

//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.RolesView _view;
//        private static Guid _roleId;
//        private static Guid _holerId;
//        private static string _claim;
//    }
//}