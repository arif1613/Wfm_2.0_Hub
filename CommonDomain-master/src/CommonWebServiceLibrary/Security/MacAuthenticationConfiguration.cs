using CommonDomainLibrary.Security;
using Raven.Client;

namespace CommonWebServiceLibrary.Security
{
    public class MacAuthenticationConfiguration
    {
        public IAsyncDocumentSession DocumentSession { get; set; }
        public ICryptoProvider CryptoProvider { get; set; }
        public ICredentialsProvider CredentialsProvider { get; set; }
    }
}