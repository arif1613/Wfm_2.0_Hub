//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;
//using System;

//namespace CommonReadModelLibrary.Tests.Views.RolesView
//{
//    public class when_a_claim_is_removed_from_a_role : RavenTest
//    {
//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.RolesView(_session);
//                _roleId = Guid.NewGuid();
//                _holerId = Guid.NewGuid();
//                _claim = "test";

//                _session.StoreAsync(new Role
//                    {
//                        Id = _roleId.AsId(typeof (Role)),
//                        Claims = {_claim}
//                    }).Await();
//                _session.SaveChangesAsync().Await();
//            };

//        private Because of = () =>
//            {
//                _view.Handle(new RoleClaimRemoved(Guid.NewGuid(), Guid.NewGuid(), _roleId, _holerId, _claim), false).Await();
//            };

//        private It should_have_removed_the_claim = () =>
//            _documentStore.OpenSession().Load<Role>(_roleId.AsId(typeof(Role))).Claims.ShouldNotContain(_claim);

//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.RolesView _view;
//        private static Guid _roleId;
//        private static Guid _holerId;
//        private static string _claim;
//    }
//}