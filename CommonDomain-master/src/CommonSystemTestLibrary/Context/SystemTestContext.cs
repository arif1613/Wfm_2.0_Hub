using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomainLibrary.Common;
using CommonSystemTestLibrary.Models;
using CommonSystemTestLibrary.Services;

namespace CommonSystemTestLibrary.Context
{
    public class SystemTestContext
    {
        private readonly List<UserInfo> _users;
        private readonly Guid _holderId;
        private readonly Dictionary<string, EntityService> _services; 

        public Guid RootHolderId
        {
            get { return _holderId; }
        }

        public static SystemTestContext Instance { get; private set; }

        private SystemTestContext(string resourceUrl, Guid holderId, Guid clientId, byte[] authorizationKey)
        {
            _users = new List<UserInfo>();
            _services = new Dictionary<string, EntityService>();
            _holderId = holderId;
            RequestFactory.Initialize(resourceUrl, clientId, authorizationKey);
            RequestedOperationsService.Initialize(resourceUrl);
        }

        public static void Initialize(string resourceUrl, Guid holderId, Guid clientId, byte[] authorizationKey)
        {
            Instance = new SystemTestContext(resourceUrl, holderId, clientId, authorizationKey);
        }

        public void RegisterUser(UserInfo user)
        {
            _users.Add(user);
        }

        public List<UserInfo> GetUsers()
        {
            return _users.ToList();
        }

        public UserInfo GetUserWithRole(string role)
        {
            foreach (var user in _users)
            {
                if (!string.IsNullOrEmpty(role) && !role.Equals("None"))
                {
                    if (user.User_Roles.Contains(role))
                        return user;
                }
                else
                {
                    if (user.User_Roles.Count == 0)
                        return user;
                }
            }

            throw new KeyNotFoundException();
        }

        public string SuperAdminUsername { get { return GetUserWithRole(UserRoles.SuperAdmin).Username; } }

        public void RegisterService(string entityType, EntityService service)
        {
            _services.Add(entityType, service);
        }

        internal EntityService GetSeriveForEntityType(string entityType)
        {
            return _services[entityType];
        }

        public bool HasUser(string username)
        {
            return _users.Any(user => user.Username.Equals(username));
        }
    }
}
