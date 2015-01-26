//using System;
//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;

//namespace CommonReadModelLibrary.Tests.Views.HolderWithClientsView
//{
//    public class when_a_holder_is_created : RavenTest
//    {
//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.HolderWithClientsView _view;
//        private static Guid _holderId;
//        private static Guid _parentHolderId;
//        private static HolderWithClients _holder;
//        private static HolderWithClients _parent;

//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.HolderWithClientsView(_session);
//                _holderId = Guid.NewGuid();
//                _parentHolderId = Guid.NewGuid();
//            };

//        private Because of = () =>
//            {
//                _view.Handle(new HolderCreated(Guid.NewGuid(), Guid.NewGuid(), _holderId, _parentHolderId, "test", "test"), false).Await();
//                _parent = _documentStore.OpenSession().Load<HolderWithClients>(_parentHolderId.AsId(typeof(HolderWithClients)));
//                _holder =
//                    _documentStore.OpenSession().Load<HolderWithClients>(_holderId.AsId(typeof (HolderWithClients)));
//            };

//        private It a_holder_clients_document_is_created = () => _holder.ShouldNotBeNull();

//        private It the_new_holder_clients_document_should_have_the_parent_holder_id_set =
//            () => _holder.OwnerDocumentId.ShouldEqual(_parentHolderId.AsId(typeof(HolderWithClients)));

//        private It the_holder_id_is_added_to_the_parent_holder_clients_document = () =>
//            {
//                _parent.ShouldNotBeNull();
//                _parent.Children.ShouldContain(_holderId.AsId(typeof(HolderWithClients)));
//            };
//    }
//}