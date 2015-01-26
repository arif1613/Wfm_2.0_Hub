//using System;
//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;

//namespace CommonReadModelLibrary.Tests.Views.HolderWithClientsView
//{
//    public class when_a_holder_is_removed : RavenTest
//    {
//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.HolderWithClientsView _view;
//        private static Guid _holderId;
//        private static Guid _parentHolderId;

//        private Establish context = () =>
//            {
//                _documentStore = CreateDocumentStore();
//                _session = _documentStore.OpenAsyncSession();
//                _view = new Security.Views.HolderWithClientsView(_session);
//                _holderId = Guid.NewGuid();
//                _parentHolderId = Guid.NewGuid();

//                _session.StoreAsync(new HolderWithClients
//                    {
//                        Id = _holderId.AsId(typeof(HolderWithClients))
//                    }).Await();
//                _session.StoreAsync(new HolderWithClients
//                    {
//                        Id = _parentHolderId.AsId(typeof(HolderWithClients)),
//                        Children =
//                            {
//                                _holderId.AsId(typeof(HolderWithClients))
//                            }
//                    }).Await();
//                _session.SaveChangesAsync().Await();
//            };

//        private Because of = () =>
//            {
//                _view.Handle(new HolderRemoved(Guid.NewGuid(), Guid.NewGuid(), _holderId, _parentHolderId), false).Await();
//            };

//        private It the_holder_clients_document_is_marked_as_deleted = () =>
//            {
//                var holderClients = _documentStore.OpenSession().Load<HolderWithClients>(_holderId.AsId(typeof(HolderWithClients)));
//                holderClients.ShouldNotBeNull();
//                holderClients.Deleted.ShouldBeTrue();
//            };

//        private It the_holder_id_is_removed_from_the_parent_holder_clients_document = () =>
//            {
//                var parent = _documentStore.OpenSession().Load<HolderWithClients>(_parentHolderId.AsId(typeof(HolderWithClients)));
//                parent.ShouldNotBeNull();
//                parent.Children.ShouldNotContain(_holderId);
//            };
//    }
//}