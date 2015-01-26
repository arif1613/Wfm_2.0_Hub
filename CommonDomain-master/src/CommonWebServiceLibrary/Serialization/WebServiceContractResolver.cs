using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommonReadModelLibrary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CommonWebServiceLibrary.Serialization
{
    public class WebServiceContractResolver : DefaultContractResolver
    {
        private static readonly Regex PropertyRegex = new Regex(@"([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", RegexOptions.Compiled);
        private static string[] _hiddenProperties;

        public WebServiceContractResolver()
        {
            _hiddenProperties = typeof (IViewDocument).GetProperties()
                                                      .Where(p => !p.Name.Equals("Id") && !p.Name.Equals("HolderId"))
                                                      .Select(p => ResolvePropertyName(p.Name)).ToArray();
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            return PropertyRegex.Replace(propertyName, "$1$3_$2$4").ToLower();
        }

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);
            contract.PropertyNameResolver = base.ResolvePropertyName;

            return contract;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (typeof (IViewDocument).IsAssignableFrom(type))
            {
                return properties.Where(p => !_hiddenProperties.Contains(p.PropertyName)).ToList();
            }

            return properties;
        }
    }
}