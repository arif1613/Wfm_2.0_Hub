using System;

namespace CommonWebServiceLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ResourceAttribute : Attribute
    {
        public ResourceAttribute(string relation, Type resourceType)
        {
            Relation = relation;
            ResourceType = resourceType;
        }

        public string Relation { get; set; }
        public Type ResourceType { get; set; }
    }
}