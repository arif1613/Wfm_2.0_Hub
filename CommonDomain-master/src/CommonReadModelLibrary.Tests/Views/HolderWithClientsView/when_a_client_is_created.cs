//using System;
//using CommonReadModelLibrary.Security.Models;
//using Machine.Specifications;
//using Raven.Client;
//using Security.Contracts.Messages.Events;

//namespace CommonReadModelLibrary.Tests.Views.HolderWithClientsView
//{
//    public class when_a_client_is_created : RavenTest
//    {
//        private static IDocumentStore _documentStore;
//        private static IAsyncDocumentSession _session;
//        private static Security.Views.HolderWithClientsView _view;
//        private static Guid _holderId;
//        private static Guid _clientId;

//        private Establish context = () =>
//        {
//            _documentStore = CreateDocumentStore();
//            _session = _documentStore.OpenAsyncSession();
//            _view = new Security.Views.HolderWithClientsView(_session);
//            _holderId = Guid.NewGuid();
//            _clientId = Guid.NewGuid();

//            _session.StoreAsync(new HolderWithClients
//                {
//                    Id = _holderId.AsId(typeof(HolderWithClients))
//                }).Await();
//            _session.SaveChangesAsync().Await();
//        };

//        private Because of = () =>
//        {
//            _view.Handle(new ClientCreated(Guid.NewGuid(), Guid.NewGuid(), _clientId, _holderId, "test", new byte[100]), false).Await();
//        };

//        private It a_client_credentials_document_is_created = () =>
//        {
//            var clientCredentials = _documentStore.OpenSession().Load<HolderClientCredentials>(_clientId.AsId(typeof(HolderClientCredentials)));
//            clientCredentials.ShouldNotBeNull();
//        };

//        private It the_client_id_is_added_to_the_holder_clients_document = () =>
//        {
//            var holderClients = _documentStore.OpenSession().Load<HolderWithClients>(_holderId.AsId(typeof(HolderWithClients)));
//            holderClients.ShouldNotBeNull();
//            holderClients.Clients.ShouldContain(_clientId.AsId(typeof(HolderClientCredentials)));
//        };
//    }
//}