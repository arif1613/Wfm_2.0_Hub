using System;

namespace CommonReadModelLibrary.Http
{
    public interface IRequestHelper
    {
        string GET(string baseUrl, string resource);
        string GET(string baseUrl, string resource, string username, Guid clientId, byte[] authenticationKey);
    }
}
