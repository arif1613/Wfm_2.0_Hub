using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonWebServiceLibrary.Security
{
    public class SecurityIdentity : ISecurityIdentity
    {
        public SecurityIdentity(Guid id, string name, Guid ownerId, Guid clientId, IEnumerable<Guid> holders, IEnumerable<string> claims)
        {
            Id = id;
            UserName = name;
            Name = name;
            OwnerId = ownerId;
            ClientId = clientId;
            Claims = claims;
            Holders = holders.ToList();
            AuthenticationType = "MAC";
            IsAuthenticated = true;
        }

        public string UserName { get; private set; }
        public IEnumerable<string> Claims { get; private set; }
        public Guid Id { get; private set; }
        public Guid OwnerId { get; private set; }
        public IEnumerable<Guid> Holders { get; private set; }
        public Guid ClientId { get; set; }
        public string Name { get; private set; }
        public string AuthenticationType { get; private set; }
        public bool IsAuthenticated { get; private set; }
    }
}