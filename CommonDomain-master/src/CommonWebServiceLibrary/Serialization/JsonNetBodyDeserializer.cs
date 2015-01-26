using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace CommonWebServiceLibrary.Serialization
{
    public class JsonNetBodyDeserializer : IBodyDeserializer
    {
        private readonly JsonSerializer _serializer;

        /// <summary>
        /// Empty constructor if no converters are needed
        /// </summary>
        public JsonNetBodyDeserializer(JsonSerializer jsonSerializer)
        {
            this._serializer = jsonSerializer;
        }

        /// <summary>
        /// Whether the deserializer can deserialize the content type
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <returns>True if supported, false otherwise</returns>
        public bool CanDeserialize(string contentType, BindingContext context)
        {
            return contentType.StartsWith("application/json", StringComparison.InvariantCultureIgnoreCase) ||
                   contentType.StartsWith("text/json", StringComparison.InvariantCultureIgnoreCase) ||
                   (contentType.StartsWith("application/vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    contentType.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Deserialize the request body to a model
        /// </summary>
        /// <param name="contentType">Content type to deserialize</param>
        /// <param name="bodyStream">Request body stream</param>
        /// <param name="context">Current context</param>
        /// <returns>Model instance</returns>
        public object Deserialize(string contentType, Stream bodyStream, BindingContext context)
        {
            var deserializedObject =
                this._serializer.Deserialize(new StreamReader(bodyStream), context.DestinationType);

            if (context.DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Except(context.ValidModelProperties).Any())
            {
                return CreateObjectWithBlacklistExcluded(context, deserializedObject);
            }

            return deserializedObject;
        }

        private static object ConvertCollection(object items, Type destinationType, BindingContext context)
        {
            var returnCollection = Activator.CreateInstance(destinationType);

            var collectionAddMethod =
                destinationType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);

            foreach (var item in (IEnumerable)items)
            {
                collectionAddMethod.Invoke(returnCollection, new[] { item });
            }

            return returnCollection;
        }

        private static object CreateObjectWithBlacklistExcluded(BindingContext context, object deserializedObject)
        {
            var returnObject = Activator.CreateInstance(context.DestinationType);

            if (context.DestinationType.IsCollection())
            {
                return ConvertCollection(deserializedObject, context.DestinationType, context);
            }

            foreach (var property in context.ValidModelProperties)
            {
                CopyPropertyValue(property, deserializedObject, returnObject);
            }

            return returnObject;
        }

        private static void CopyPropertyValue(PropertyInfo property, object sourceObject, object destinationObject)
        {
            property.SetValue(destinationObject, property.GetValue(sourceObject, null), null);
        }
    }
}