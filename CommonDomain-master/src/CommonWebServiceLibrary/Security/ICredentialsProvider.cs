using System.Collections.Generic;
using System.Threading.Tasks;
using CommonReadModelLibrary.Security.Models;

namespace CommonWebServiceLibrary.Security
{
    public interface ICredentialsProvider
    {
        Task<IList<ClientCredentials>> GetClientCredentials(string clientId, string username);
    }
}