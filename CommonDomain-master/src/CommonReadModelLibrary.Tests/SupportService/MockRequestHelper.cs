using System;
using System.Collections.Generic;
using CommonReadModelLibrary.Http;

namespace CommonReadModelLibrary.Tests.SupportService
{
    public class MockRequestHelper : IRequestHelper
    {
        private readonly Dictionary<string, string> _responses;

        public Dictionary<string, object> MadeRequests { get; set; }

        public MockRequestHelper()
        {
            _responses = new Dictionary<string, string>();
            MadeRequests = new Dictionary<string, object>();
        }

        public void WhenGET(string url, string response)
        {
            _responses.Add(url, response);
        }

        public string GET(string baseUrl, string resource)
        {
            var url = baseUrl + resource;
            MadeRequests.Add(url, string.Empty);
            return _responses[url];
        }

        public string GET(string baseUrl, string resource, string username, Guid clientId, byte[] authenticationKey)
        {
            return GET(baseUrl, resource);
        }
    }
}
