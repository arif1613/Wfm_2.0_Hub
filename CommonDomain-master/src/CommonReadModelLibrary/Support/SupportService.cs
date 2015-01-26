using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomainLibrary;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Http;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.RavenDB;
using NodaTime;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Linq;

namespace CommonReadModelLibrary.Support
{
    public class SupportService : ISupportService
    {
        private readonly IRequestHelper _requestHelper;
        private readonly string _supportResourceUrl;

        private class ArchivedMessageResponse
        {
            public Guid Id { get; set; }
            public dynamic Message { get; set; }
            public Type Message_Type { get; set; }
        }

        private sealed class Converter
        {
            public static T Deserialize<T>(string json) where T : IMessage
            {
                var settings = new JsonSerializerSettings();
                settings.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

                var message = JsonConvert.DeserializeObject<T>(json, settings);
                return message;
            }

            public object ConvertDynamicJsonObject(dynamic dynamicObject, Type type)
            {
                var jObj = (JObject)dynamicObject;

                RenameProperties(jObj, type);

                var json = JsonConvert.SerializeObject(dynamicObject, new NodaDateTimeZoneConverter(DateTimeZoneProviders.Tzdb));

                var castMethod = GetType().GetMethod("Deserialize").MakeGenericMethod(type);
                var output = castMethod.Invoke(null, new object[] { json });

                return output;
            }

            private static void RenameProperties(JObject jObject, Type type)
            {
                var newProperties = new List<JProperty>();
                var oldProperties = new List<string>();

                foreach (var property in jObject.Properties())
                {
                    var newName = ConvertToCamelCase(property.Name);
                    if (type.GetProperty(newName) == null && type.GetMember(newName).Length == 0) continue;
                    newProperties.Add(new JProperty(newName, property.Value));
                    oldProperties.Add(property.Name);
                }

                foreach (var newProperty in newProperties)
                {
                    var value = newProperty.Value as JObject;
                    if (value != null)
                    {
                        var propertyType = type.GetProperty(newProperty.Name).PropertyType;
                        RenameProperties(value, propertyType);
                    }
                    jObject.Add(newProperty);
                }

                foreach (var oldProperty in oldProperties)
                    jObject.Remove(oldProperty);
            }

            private static string ConvertToCamelCase(string str)
            {
                var output = string.Empty;
                var upperCase = true;
                foreach (var c in str)
                {
                    if (upperCase)
                    {
                        output += char.ToUpper(c);
                        upperCase = false;
                    }
                    else if (c == '_')
                        upperCase = true;
                    else
                        output += c;
                }
                return output;
            }
        }

        public SupportService(IRequestHelper requestHelper, string supportResourceUrl)
        {
            _requestHelper = requestHelper;
            _supportResourceUrl = supportResourceUrl;
        }

        public IEnumerable<ArchivedMessage> GetArchivedMessages(List<Type> messageTypes, ICommonIdentity identity, Guid clientId, 
            byte[] authenticationKey)
        {
            var arcihvedMessages = new List<ArchivedMessage>();

            foreach (var messageType in messageTypes)
                arcihvedMessages.AddRange(GetArchivedMessages(messageType, identity, clientId, authenticationKey));

            return arcihvedMessages.OrderBy(am => am.Message.Timestamp).ToList();
        }

        private IEnumerable<ArchivedMessage> GetArchivedMessages(Type messageType, ICommonIdentity identity, Guid clientId,
            byte[] authenticationKey)
        {
            var resource = string.Format("/{0}/archivedMessages/{1}/{2}", identity.OwnerId.ToString("N"),
                messageType.Assembly.GetName().Name.Replace(".", "-"), messageType.FullName.Replace(".", "-"));

            var json = _requestHelper.GET(_supportResourceUrl, resource, identity.Name, clientId, authenticationKey);

            var messages = JsonConvert.DeserializeObject<List<ArchivedMessageResponse>>(json);

            return (from archivedMessageResponse in messages
                    let message = new Converter().ConvertDynamicJsonObject(archivedMessageResponse.Message, archivedMessageResponse.Message_Type)
                    select new ArchivedMessage
                    {
                        Id = archivedMessageResponse.Id,
                        Message = message,
                        MessageType = archivedMessageResponse.Message_Type
                    }).ToList(); 
        }
    }
}
