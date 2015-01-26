using System.Collections.Generic;

namespace CommonWebServiceLibrary.Routing
{
    public interface IRouteConstructor
    {
        string ResolveResourceUrl(string uriTemplate, string[] parameterNames, IDictionary<string, string> tokens);
        IEnumerable<string> ResolveParameterNames(string uriTemplate);
    }
}