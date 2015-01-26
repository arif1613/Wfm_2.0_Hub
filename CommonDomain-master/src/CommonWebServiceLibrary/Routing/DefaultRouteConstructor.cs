using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nancy;

namespace CommonWebServiceLibrary.Routing
{
    public class DefaultRouteConstructor : IRouteConstructor
    {
        public NancyContext _context;

        private readonly Regex ParameterExpression =
            new Regex(@"{(?<name>[A-Za-z0-9_]*)}|\(\?\<(?<name>[A-Za-z0-9_]*)\>.*\)", RegexOptions.Compiled);
        private readonly Regex SegmentExpression =
            new Regex(@"(?<seg>{.*})|(?<seg>\(.*\))", RegexOptions.Compiled);

        public string ResolveResourceUrl(string uriTemplate, string[] parameterNames, IDictionary<string, string> tokens)
        {
            int i = 0;
            var scheme = _context.Request.Url.Scheme;
            var host = _context.Request.Url.HostName;

            if (_context.Request.Url.Port != 80)
            {
                host = string.Concat(host, ":", _context.Request.Url.Port);
            }

            return string.Format("{0}://{1}{2}",
                                 scheme,
                                 host,
                                 SegmentExpression.Replace(uriTemplate, match => tokens[parameterNames[i++]]));
        }

        public IEnumerable<string> ResolveParameterNames(string uriTemplate)
        {
            foreach (Match match in ParameterExpression.Matches(uriTemplate))
            {
                if (match.Success)
                {
                    yield return match.Groups["name"].Value;
                }
            }
        }
    }
}