using CommonDomainLibrary.Security;
using Nancy;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommonWebServiceLibrary.Security
{
    class AuthenticationMacParser
    {
        private const string CredentialsPrefix = "MAC ";
        private static readonly Regex _qsRegex = new Regex("(?<key>[A-Za-z]+)=\"(?<value>.*)\"", RegexOptions.Compiled);

        public static AuthenticationMac Parse(Request request)
        {
            var authorization = request.Headers["Authorization"];
            if (authorization == null)
            {
                throw new FormatException("There is no authorization header.");
            }

            var header = authorization.FirstOrDefault();
            if (header == null || !header.StartsWith(CredentialsPrefix, StringComparison.Ordinal))
            {
                throw new FormatException("The header does not contain MAC credentials.");
            }

            var matches = header.Substring(CredentialsPrefix.Length)
                                .Split(',')
                                .Select(@group => _qsRegex.Match(@group.Trim()))
                                .Where(match => match.Success)
                                .ToDictionary(match => match.Groups["key"].Value, match => match.Groups["value"].Value);

            if (!matches.ContainsKey("username"))
            {
                throw new FormatException("The MAC credentials must contain a username.");
            }

            if (!matches.ContainsKey("client"))
            {
                throw new FormatException("The MAC credentials must contain a client.");
            }

            Guid client;
            if (!Guid.TryParse(matches["client"], out client))
            {
                throw new FormatException("The MAC credential's client must be a valid GUID.");
            }

            long ts;
            if (!matches.ContainsKey("ts"))
            {
                throw new FormatException("The MAC credentials must contain timestamp.");
            }
            if (!long.TryParse(matches["ts"], out ts))
            {
                throw new FormatException("The MAC credential's timestamp must be numeric.");
            }

            if (!matches.ContainsKey("nonce"))
            {
                throw new FormatException("The MAC credentials must contain a nonce.");
            }

            var method = request.Method;
            var resource = string.Concat(request.Url.Path, request.Url.Query);

            return new AuthenticationMac(matches["username"], client, ts, matches["nonce"], method, resource, matches["mac"]);
        }

        public static bool TryParse(Request request, out AuthenticationMac authenticationMac)
        {
            try
            {
                authenticationMac = Parse(request);
                return true;
            }
            catch (FormatException)
            {
                authenticationMac = null;
                return false;
            }
        }
    }
}
