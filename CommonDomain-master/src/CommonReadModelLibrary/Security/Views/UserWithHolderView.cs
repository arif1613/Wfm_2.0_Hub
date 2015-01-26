//using System.Threading.Tasks;
//using CommonDomainLibrary.Common;
//using CommonReadModelLibrary.Security.Models;
//using Raven.Client;
//using Security.Contracts.Messages.Events;

//namespace CommonReadModelLibrary.Security.Views
//{
//    public class UserWithHolderView : IHandle<UserCreated>, IHandle<UserRemoved>, IHandle<UserRoleAdded>, IHandle<UserRoleRemoved>
//    {
//        private readonly IAsyncDocumentSession _session;

//        public UserWithHolderView(IAsyncDocumentSession session)
//        {
//            _session = session;
//        }

//        public async Task Handle(UserCreated e, bool lastTry)
//        {
//            await _session.ApplyOnce<UserWithHolder, UserCreated>(e.Id, e, (doc, m) =>
//            {
//                doc.Set(m, d => d.OwnerDocumentId, m.OwnerId.AsId(typeof(HolderWithClients)));
//                doc.Set(m, d => d.Username, m.AuthenticationDetails.Username);
//            });
//        }

//        public async Task Handle(UserRemoved e, bool lastTry)
//        {
//            await _session.ApplyOnce<UserWithHolder, UserRemoved>(e.Id, e, (doc, m) =>
//            {
//                doc.Set(m, d => d.Deleted, true);
//            });
//        }

//        public async Task Handle(UserRoleAdded e, bool lastTry)
//        {
//            await _session.ApplyOnce<UserWithHolder, UserRoleAdded>(e.Id, e, (doc, m) =>
//            {
//                doc.Add(m, d => d.RoleIds, e.RoleId.AsId(typeof(Role)));
//            });
//        }

//        public async Task Handle(UserRoleRemoved e, bool lastTry)
//        {
//            await _session.ApplyOnce<UserWithHolder, UserRoleRemoved>(e.Id, e, (doc, m) =>
//            {
//                doc.Remove(m, d => d.RoleIds, e.RoleId.AsId(typeof(Role)));
//            });
//        }
//    }
//}