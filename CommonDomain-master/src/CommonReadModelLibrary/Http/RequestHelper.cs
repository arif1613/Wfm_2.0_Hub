using System;
using System.Net;
using CommonDomainLibrary.Security;

namespace CommonReadModelLibrary.Http
{
    public class RequestHelper : IRequestHelper
    {
        private readonly ICryptoProvider _cryptoProvider;

        public RequestHelper(ICryptoProvider cryptoProvider)
        {
            _cryptoProvider = cryptoProvider;
        }

        public string GET(string baseUrl, string resource, string username, Guid clientId, byte[] authenticationKey)
        {
            var request = WebRequest.Create(baseUrl + resource) as HttpWebRequest;
            if (request == null) return null;
            request.Method = "GET";

            if (!string.IsNullOrEmpty(username) && !clientId.Equals(default(Guid)) && authenticationKey != null)
                request.SetAuthenticationString(resource, username, clientId, authenticationKey, _cryptoProvider);

            return request.GetResponse().AsString();
        }

        public string GET(string baseUrl, string resource)
        {
            return GET(baseUrl, resource, null, default(Guid), null);
        }
    }
}
