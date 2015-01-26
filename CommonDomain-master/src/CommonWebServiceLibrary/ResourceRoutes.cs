using System;
using System.Collections.Generic;
using System.Linq;
using CommonWebServiceLibrary.Attributes;
using Nancy;

namespace CommonWebServiceLibrary
{
    public class ResourceRoutes
    {
        private static readonly IDictionary<Type, IList<ResourceRoute>> Routes = new Dictionary<Type, IList<ResourceRoute>>();
 
        public static void Configure(IEnumerable<INancyModule> modules)
        {
            if (Routes.Count == 0)
            {
                lock (Routes)
                {
                    if (Routes.Count == 0)
                    {
                        PopulateRoutes(modules);
                    }
                }
            }
        }

        public static IEnumerable<ResourceRoute> GetRoutesFor(Type resourceType)
        {
            if (Routes.ContainsKey(resourceType))
            {
                return Routes[resourceType];
            }

            return new List<ResourceRoute>();
        }

        private static void PopulateRoutes(IEnumerable<INancyModule> modules)
        {
            foreach (var module in modules)
            {
                var resource =
                    (ResourceAttribute)module.GetType().GetCustomAttributes(typeof(ResourceAttribute), false).FirstOrDefault();

                if (resource != null)
                {
                    if (!Routes.ContainsKey(resource.ResourceType))
                    {
                        Routes.Add(resource.ResourceType, new List<ResourceRoute>());
                    }

                    foreach (var route in module.Routes)
                    {

                        Routes[resource.ResourceType].Add(new ResourceRoute
                        {
                            Method = route.Description.Method,
                            Relation = resource.Relation,
                            UriTemplate = route.Description.Path,
                            IsCanonical = resource is CanonicalAttribute,
                            Segments = route.Description.Segments
                        });
                    }
                }
            }
        }
    }
}