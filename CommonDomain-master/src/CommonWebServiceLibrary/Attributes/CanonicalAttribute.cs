using System;

namespace CommonWebServiceLibrary.Attributes
{
    public class CanonicalAttribute : ResourceAttribute
    {
        public CanonicalAttribute(Type resourceType) : base("self", resourceType)
        {
        }
    }
}