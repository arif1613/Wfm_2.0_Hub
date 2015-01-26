using System.Linq;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary;
using CommonReadModelLibrary.Security.Models;
using Nancy.Bootstrapper;

namespace CommonWebServiceLibrary.Security
{
    public static class MacAuthentication
    {
        public static void Enable(IPipelines pipelines, MacAuthenticationConfiguration configuration)
        {
            pipelines.BeforeRequest.AddItemToStartOfPipeline(async (c, t) =>
                {
                    AuthenticationMac authenticationMac;
                    if (AuthenticationMacParser.TryParse(c.Request, out authenticationMac))
                    {
                        var client = authenticationMac.Client.AsId(typeof (HolderClientCredentials));
                        var credentials = await configuration.CredentialsProvider.GetClientCredentials(client,
                                                                                                 authenticationMac.Username);
                        if (credentials.Count > 0 && authenticationMac.IsValid(configuration.CryptoProvider, credentials[0].AuthenticationKey))
                        {
                            var identity = new SecurityIdentity(credentials[0].UserId.AsGuid(), authenticationMac.Username,
                                                                  credentials[0].OwnerId.AsGuid(), authenticationMac.Client,
                                                                  credentials.Select(cc => cc.HolderId.AsGuid()), credentials[0].Roles.ToArray());
                            c.CurrentUser = identity;
                        }
                    }

                    return null;
                });
        }
    }
}