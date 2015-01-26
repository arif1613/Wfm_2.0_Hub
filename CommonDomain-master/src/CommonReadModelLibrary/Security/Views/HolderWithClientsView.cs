//using CommonDomainLibrary.Common;
//using CommonReadModelLibrary.Security.Models;
//using Raven.Client;
//using Security.Contracts.Messages.Events;
//using System.Threading.Tasks;

//namespace CommonReadModelLibrary.Security.Views
//{
//    public class HolderWithClientsView : IHandle<ClientCreated>, IHandle<ClientRemoved>, IHandle<HolderCreated>, IHandle<HolderRemoved>
//    {
//        private readonly IAsyncDocumentSession _session;

//        public HolderWithClientsView(IAsyncDocumentSession session)
//        {
//            _session = session;
//        }

//        public async Task Handle(ClientCreated e, bool lastTry)
//        {
//            await _session.ApplyBatch(async s =>
//                {
//                    await s.ApplyOnce<HolderClientCredentials, ClientCreated>(e.Id, e, (doc, m) =>
//                        {
//                            doc.Set(m, d => d.AuthenticationKey, m.AuthenticationKey);
//                            doc.Set(m, d => d.OwnerDocumentId, m.OwnerId.ToString());
//                        },
//                        save: false);

//                    await s.ApplyOnce<HolderWithClients, ClientCreated>(e.OwnerId, e, (doc, m) =>
//                        {
//                            doc.Add(m, d => d.Clients, m.Id.AsId(typeof(HolderClientCredentials)));
//                        }, 
//                        save: false);
//                });
//        }

//        public async Task Handle(ClientRemoved e, bool lastTry)
//        {
//            await _session.ApplyBatch(async s =>
//                {
//                    await s.ApplyOnce<HolderClientCredentials, ClientRemoved>(e.Id, e, (doc, m) =>
//                        {
//                            doc.Set(m, d => d.Deleted, true);
//                        },
//                        save: false);

//                    await s.ApplyOnce<HolderWithClients, ClientRemoved>(e.OwnerId, e, (doc, m) =>
//                        {
//                            doc.Remove(m, d => d.Clients, m.Id.AsId(typeof(HolderClientCredentials)));
//                        },
//                        save: false);
//                });
//        }

//        public async Task Handle(HolderCreated e, bool lastTry)
//        {
//            await _session.ApplyBatch(async s =>
//                {
//                    await s.ApplyOnce<HolderWithClients, HolderCreated>(e.Id, e, (doc, m) =>
//                    {
//                        doc.Set(m, d => d.OwnerDocumentId, m.OwnerId.AsId(typeof(HolderWithClients)));
//                    }, save: false);

//                    await s.ApplyOnce<HolderWithClients, HolderCreated>(e.OwnerId, e, (doc, m) =>
//                        {
//                            doc.Add(m, d => d.Children, m.Id.AsId(typeof(HolderWithClients)));                            
//                        },
//                        save: false);

//                    await s.ApplyOnce<HolderWithClients, HolderCreated>(e.Id, e, (doc, m) =>
//                        {
//                        },
//                        save: false);
//                });
//        }

//        public async Task Handle(HolderRemoved e, bool lastTry)
//        {
//            await _session.ApplyBatch(async s =>
//                {
//                    await s.ApplyOnce<HolderWithClients, HolderRemoved>(e.OwnerId, e, (doc, m) =>
//                        {
//                            doc.Remove(m, d => d.Children, m.Id.AsId(typeof(HolderWithClients)));
//                        },
//                        save: false);

//                    await s.ApplyOnce<HolderWithClients, HolderRemoved>(e.Id, e, (doc, m) =>
//                        {
//                            doc.Set(m, d => d.Deleted, true);
//                        },
//                        save: false);
//                });
//        }
//    }
//}