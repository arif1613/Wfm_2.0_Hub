//using CommonDomainLibrary.Common;
//using CommonReadModelLibrary.Security.Models;
//using Raven.Client;
//using Security.Contracts.Messages.Events;
//using System.Threading.Tasks;

//namespace CommonReadModelLibrary.Security.Views
//{
//    public class RolesView : IHandle<RoleCreated>, IHandle<RoleRemoved>, IHandle<RoleClaimAdded>, IHandle<RoleClaimRemoved>
//    {
//        private readonly IAsyncDocumentSession _session;

//        public RolesView(IAsyncDocumentSession session)
//        {
//            _session = session;
//        }

//        public async Task Handle(RoleCreated e, bool lastTry)
//        {
//            await _session.ApplyOnce<Role, RoleCreated>(e.Id, e, (d, m) =>
//                {
//                    d.Set(m, r => r.Name, m.Name);
//                });
//        }

//        public async Task Handle(RoleRemoved e, bool lastTry)
//        {
//            await _session.ApplyOnce<Role, RoleRemoved>(e.Id, e, (d, m) =>
//                {
//                    d.Set(m, r => r.Deleted, true);
//                });
//        }

//        public async Task Handle(RoleClaimAdded e, bool lastTry)
//        {
//            await _session.ApplyOnce<Role, RoleClaimAdded>(e.Id, e, (d, m) =>
//                {
//                    d.Add(m, r => r.Claims, m.Claim);
//                });
//        }

//        public async Task Handle(RoleClaimRemoved e, bool lastTry)
//        {
//            await _session.ApplyOnce<Role, RoleClaimRemoved>(e.Id, e, (d, m) =>
//                {
//                    d.Remove(m, r => r.Claims, m.Claim);
//                });
//        }
//    }
//}