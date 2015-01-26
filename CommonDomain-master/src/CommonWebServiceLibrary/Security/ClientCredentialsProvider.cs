using System.Collections.Generic;
using System.Threading.Tasks;
using CommonReadModelLibrary.Security.Indices;
using CommonReadModelLibrary.Security.Models;
using Raven.Client;
using Raven.Client.Linq;

namespace CommonWebServiceLibrary.Security
{
    public class ClientCredentialsProvider: ICredentialsProvider
    {
        private readonly IAsyncDocumentSession _session;

        public ClientCredentialsProvider(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<IList<ClientCredentials>> GetClientCredentials(string clientId, string username)
        {
            return await _session.Query<ClientCredentials, FlattenedClientCredentials>()
                                               .Where(cc => cc.ClientId == clientId && cc.Username == username)
                                               .ToListAsync();
        }
    }
}
