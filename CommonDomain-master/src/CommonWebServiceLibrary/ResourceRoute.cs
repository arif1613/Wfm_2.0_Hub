using System.Collections.Generic;

namespace CommonWebServiceLibrary
{
    public class ResourceRoute
    {
        public string Method { get; set; }
        public string Relation { get; set; }
        public string UriTemplate { get; set; }
        public bool IsCanonical { get; set; }
        public IEnumerable<string> Segments { get; set; }
    }
}