using System;
using System.Net;
using System.Text;
using CommonDomainLibrary.Security;
using Newtonsoft.Json;
using NodaTime;

namespace CommonSystemTestLibrary
{
    public class RequestFactory
    {
        private readonly string _resourceUrl;
        private readonly Guid _clientId;
        private readonly byte[] _authenticationKey;

        public static void Initialize(string resourceUrl, Guid clientId, byte[] authenticationKey)
        {
            Instance = new RequestFactory(resourceUrl, clientId, authenticationKey);
        }

        private RequestFactory(string resourceUrl, Guid clientId, byte[] authenticationKey)
        {
            _resourceUrl = resourceUrl;
            _clientId = clientId;
            _authenticationKey = authenticationKey;
        }

        public static RequestFactory Instance { get; private set; }

        public WebRequest CreatePOSTRequest(string url, object data, string username)
        {
            return CreatePOSTRequest(url, data, new AuthenticationInfo {Username = username});
        }
        public WebRequest CreatePOSTRequest(string url, object data, AuthenticationInfo authentication)
        {
            var request = CreateRequest(url, "POST", authentication);
            SetData(request, data);
            return request;
        }
        public WebRequest CreateGETRequest(string url, AuthenticationInfo authentication)
        {
            return CreateRequest(url, "GET", authentication);
        }
        public WebRequest CreateDELETERequest(string url, string username)
        {
            return CreateDELETERequest(url, new AuthenticationInfo {Username = username});
        }
        public WebRequest CreateDELETERequest(string url, AuthenticationInfo authentication)
        {
            return CreateRequest(url, "DELETE", authentication);
        }
        public WebRequest CreatePUTRequest(string url, object data, string username)
        {
            return CreatePUTRequest(url, data, new AuthenticationInfo {Username = username});
        }
        public WebRequest CreatePUTRequest(string url, object data, AuthenticationInfo authentication)
        {
            var request = CreateRequest(url, "PUT", authentication);
            SetData(request, data);
            return request;
        }

        private WebRequest CreateRequest(string url, string method, AuthenticationInfo authentication)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null) return null;
            request.Method = method;

            if (!string.IsNullOrEmpty(authentication.Username))
                request.Headers[HttpRequestHeader.Authorization] = GetAuthenticationString(method, url, authentication);

            return request;
        }

        private string GetAuthenticationString(string method, string url, AuthenticationInfo authentication)
        {
            if (authentication.ClientId.Equals(default(Guid)))
                authentication.ClientId = _clientId;
            if (authentication.AuthenticationKey == null)
                authentication.AuthenticationKey = _authenticationKey;

            var timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow).Ticks;
            const string nonce = "d5c1";
            var resource = url.Replace(_resourceUrl, "");

            return
                new AuthenticationMac(authentication.Username, authentication.ClientId, timestamp, nonce, method, resource,
                    new CryptoProvider(), authentication.AuthenticationKey).ToString();
        }

        private static void SetData(WebRequest request, object data)
        {
            var json = JsonConvert.SerializeObject(data);
            var byteArray = Encoding.UTF8.GetBytes(json);

            request.ContentLength = byteArray.Length;
            request.ContentType = "application/json;charset=UTF-8";

            var dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
        }
    }
}
