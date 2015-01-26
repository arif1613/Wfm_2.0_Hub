using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using CommonDomainLibrary.Common;
using CommonReadModelLibrary;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.Security.Models;
using CommonWebServiceLibrary.Security;
using Microsoft.WindowsAzure.MediaServices.Client;
using Nancy;
using Nancy.Security;
using Raven.Client;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace CommonWebServiceLibrary.Extensions
{
    public static class SecurityExtensions
    {
        public static ISecurityIdentity Identity(this INancyModule module)
        {
            return module.Context.CurrentUser as ISecurityIdentity;
        }

        public static HttpStatusCode RequiresAuthorization<T>(this INancyModule module, IAsyncDocumentSession session, Guid holderId,
            Guid resourceId = new Guid(), string[] roles = null, bool requireAllRoles = true) where T : IViewDocument
        {
            module.RequiresAuthentication();

            if (roles == null || roles.Length > 0)
            {
                var checkRole = module.CheckRoles(session, roles, requireAllRoles).Result;
                if (!checkRole)
                    return HttpStatusCode.Unauthorized;
            }


            var identity = module.Identity();

            if (resourceId != Guid.Empty)
            {
                try
                {
                    var document = session.LoadAsync<T>(resourceId.AsId(typeof(T))).Result;
                    if (document == null) return HttpStatusCode.NotFound;
                    if (identity != null && identity.Holders.All(h => h != document.HolderId)) return HttpStatusCode.Unauthorized;

                }
                catch (Exception)
                {
                    return HttpStatusCode.Unauthorized;
                }                        
            }
            else
            {
                try
                {
                    if (identity != null && !identity.Holders.Contains(holderId))
                    {
                        var holder = session.LoadAsync<HolderWithClients>(holderId.AsId(typeof(HolderWithClients))).Result;
                        if (holder == null || holder.Deleted) return HttpStatusCode.NotFound;
                        return HttpStatusCode.Unauthorized;
                    }
                }
                catch (Exception)
                {
                    return HttpStatusCode.Unauthorized;
                }                        
            }

            Thread.CurrentPrincipal = new GenericPrincipal(identity, new string[] { });
            return HttpStatusCode.OK;
        }

        public static async Task<UserWithHolder> GetUser(this INancyModule module, IAsyncDocumentSession session)
        {
            if (module.Identity() == null)
                return null;

            var id = module.Identity().Id.AsId(typeof(UserWithHolder));

            try
            {
                return (await session.Query<UserWithHolder>().Where(u => u.Id.Equals(id) && !u.Deleted).ToListAsync()).SingleOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<bool> CheckRoles(this INancyModule module, IAsyncDocumentSession session, string[] roles = null, 
            bool requireAllRoles = true)
        {
            roles = roles ?? new[] {UserRoles.SuperAdmin};

            var user = await GetUser(module, session);

            if (user == null)
                return false;

            if (user.Roles.Contains(UserRoles.SuperAdmin))
                return true;

            return requireAllRoles ? roles.Aggregate(true, (current, role1) => current && user.Roles.Contains(role1)) :
                                     roles.Aggregate(false, (current, role1) => current || user.Roles.Contains(role1));
        }
    }
}