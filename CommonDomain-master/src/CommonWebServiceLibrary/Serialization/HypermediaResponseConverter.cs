//using CommonWebServiceLibrary.Routing;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Linq;

//namespace CommonWebServiceLibrary.Serialization
//{
//    public class HypermediaResponseConverter : JsonConverter
//    {
//        private readonly JsonSerializer _serializer;
//        private readonly IRouteConstructor _routeConstructor;

//        public HypermediaResponseConverter(JsonSerializer serializer, IRouteConstructor routeConstructor)
//        {
//            _serializer = serializer;
//            _routeConstructor = routeConstructor;
//        }

//        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//        {
//            var jo = JObject.FromObject(value, _serializer);
//            var routes = ResourceRoutes.GetRoutesFor(value.GetType());
//            var tokens = jo.Properties().ToDictionary(p => p.Name, p => p.Value.Value<object>().ToString());
//            var links = new JArray();

//            foreach (var route in routes)
//            {
//                var href = _routeConstructor.ResolveResourceUrl(
//                                              route.UriTemplate,
//                                              _routeConstructor.ResolveParameterNames(route.UriTemplate).ToArray(),
//                                              tokens);
//                links.Add(new JObject(
//                                new JProperty("rel", route.Relation),
//                                new JProperty("method", route.Method),
//                                new JProperty("href", href)));
//            }

//            jo.Add("_links", links);
            
//            _serializer.Serialize(writer, jo);
//        }

//        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//        {
//            return _serializer.Deserialize(reader, objectType);
//        }

//        public override bool CanConvert(Type objectType)
//        {
//            return ResourceRoutes.GetRoutesFor(objectType).Any(r => r.IsCanonical);
//        }
//    }
//}