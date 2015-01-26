using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CommonSystemTestLibrary.Extensions;
using CommonSystemTestLibrary.Models;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace CommonSystemTestLibrary.Context
{
    public static class ExtendedScenarioContext
    {
        public static WebException WebException
        {
            get
            {
                if(ScenarioContext.Current.ContainsKey("webException"))
                    return (WebException) ScenarioContext.Current["webException"];
                return null;
            }
            set { ScenarioContext.Current.Add("webException", value); }
        }

        public static RequestedOperationResponse RequestedOperation
        {
            get { return (RequestedOperationResponse)ScenarioContext.Current["requestedOperation"]; }
            set { ScenarioContext.Current.Add("requestedOperation", value); }            
        }

        public static UserInfo CurrentUser
        {
            get
            {
                return ScenarioContext.Current.ContainsKey("user") ? (UserInfo)ScenarioContext.Current["user"] : null;
            }
            set { ScenarioContext.Current.Add("user", value); }
        }

        public static string GetScenarioTitleAsName(string entity)
        {
            var parts = new string[3];

            parts[0] = ScenarioContext.Current.ScenarioInfo.Title.ToCamelCase();

            parts[1] = CurrentUser != null
                ? "As" + CurrentUser.Username
                : String.Empty;

            parts[2] = "Test" + entity;

            return String.Join(String.Empty, parts);
        }

        private static Dictionary<string, EntityInstance> EntityInstances
        {
            get
            {
                const string key = "entityInstances";
                if(ScenarioContext.Current.ContainsKey(key))
                    return (Dictionary<string, EntityInstance>)ScenarioContext.Current[key];
                return null;
            }
            set { ScenarioContext.Current.Add("entityInstances", value); }  
        }

        private sealed class EntityInstance
        {
            public Guid Id { get; set; }
            public Guid HolderId { get; set; }
            public string EntityType { get; set; }
        }

        internal static void Add(string name, Guid holderId, Guid id, string entityType)
        {
            if (EntityInstances == null)
                EntityInstances = new Dictionary<string, EntityInstance>();

            EntityInstances.Add(name, new EntityInstance { HolderId = holderId, Id = id, EntityType = entityType });
        }

        internal static void Remove(string name)
        {
            if (EntityInstances != null)
                EntityInstances.Remove(name);
        }

        internal static void Remove(string entityType, IEnumerable<Guid> ids)
        {
            var removeList =
                EntityInstances.Where(
                    entityInstance =>
                        entityInstance.Value.EntityType.Equals(entityType) && ids.Contains(entityInstance.Value.Id))
                    .Select(entityInstance => entityInstance.Key)
                    .ToList();

            Remove(removeList);
        }

        internal static void Remove(IEnumerable<string> names)
        {
            foreach (var name in names)
                Remove(name);
        }

        internal static Guid GetId(string name)
        {
            return EntityInstances[name].Id;
        }

        internal static Guid GetHolderId(string name)
        {
            return EntityInstances[name].HolderId;
        }

        internal static string GetName(string entityType, int index)
        {
            return EntityInstances.Where(e => e.Value.EntityType.Equals(entityType)).ToList()[index].Key;
        }

        internal static Guid GetId(string entityType, int index)
        {
            return EntityInstances.Where(e => e.Value.EntityType.Equals(entityType)).ToList()[index].Value.Id;
        }

        internal static Guid GetHolderId(string entityType, int index)
        {
            return EntityInstances.Where(e => e.Value.EntityType.Equals(entityType)).ToList()[index].Value.HolderId;
        }

        internal static List<Guid> GetByHolderId(string entityType, Guid holderId)
        {
            return
                EntityInstances.Where(e => e.Value.EntityType.Equals(entityType) && e.Value.HolderId.Equals(holderId))
                    .Select(e => e.Value.Id).ToList();
        }

        internal static void Cleanup()
        {
            if (EntityInstances == null)
                return;

            foreach (var entityInstance in EntityInstances.Reverse())
            {
                var holderId = entityInstance.Value.HolderId;
                var id = entityInstance.Value.Id;
                var username = SystemTestContext.Instance.SuperAdminUsername;

                dynamic entityService = SystemTestContext.Instance.GetSeriveForEntityType(entityInstance.Value.EntityType);

                try
                {
                    entityService.Get(holderId, id, username, RetryMode.RetryOnAllExceptions);
                }
                catch (Exception exception)
                {
                    Assert.Fail("Exception when trying to retrieve {0} {1}: {2}", entityInstance.Value.EntityType,
                        entityInstance.Key, exception.Message);
                }

                try
                {
                    entityService.Delete(holderId, id, username);
                }
                catch (Exception exception)
                {
                    Assert.Fail("Exception when trying to remove {0} {1}: {2}", entityInstance.Value.EntityType,
                        entityInstance.Key, exception.Message);
                }
            }
        }
    }
}
