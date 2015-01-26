using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using Nancy.IO;
using Newtonsoft.Json;

namespace CommonWebServiceLibrary.Serialization
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonNetSerializer(JsonSerializer jsonSerializer)//, HypermediaResponseConverter hypermediaResponseConverter)
        {
            _serializer = jsonSerializer;
            //_serializer.Converters.Add(hypermediaResponseConverter);
        }

        public bool CanSerialize(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                return false;
            }

            string contentMimeType = contentType.Split(';')[0];

            return contentMimeType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase) ||
                   contentMimeType.Equals("text/json", StringComparison.InvariantCultureIgnoreCase) ||
                   (contentMimeType.StartsWith("application/vnd", StringComparison.InvariantCultureIgnoreCase) &&
                    contentMimeType.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> Extensions
        {
            get { yield return "json"; }
        }

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
            {
                _serializer.Serialize(writer, model);
                writer.Flush();
            }
        }
    }
}