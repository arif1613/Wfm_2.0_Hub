using System;
using System.Net;
using System.Text;
using CommonDomainLibrary.Security;
using NodaTime;

namespace CommonReadModelLibrary.Http
{
    public static class WebRequestExtensions
    {
        private const int NonceLength = 4;
        private const string NonceChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static void SetAuthenticationString(this WebRequest request, string resource, string username, Guid clientId, 
            byte[] authenticationKey, ICryptoProvider cryptoProvider)
        {
            var timestamp = Instant.FromDateTimeUtc(DateTime.UtcNow).Ticks;
            var nonce = GenerateNonce(NonceLength);

            request.Headers[HttpRequestHeader.Authorization] = new AuthenticationMac(username, clientId, timestamp,
                nonce, request.Method, resource, cryptoProvider, authenticationKey).ToString();
        }

        private static string GenerateNonce(int length)
        {
            var random = new Random(DateTime.UtcNow.Millisecond);
            var nonce = string.Empty;
            for (var i = 0; i < length; i++)
                nonce += (NonceChars[random.Next(0, NonceChars.Length - 1)]);

            return nonce;
        }
    }
}
