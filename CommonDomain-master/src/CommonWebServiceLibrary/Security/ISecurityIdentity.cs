using System;
using System.Collections.Generic;
using CommonDomainLibrary.Security;
using Nancy.Security;

namespace CommonWebServiceLibrary.Security
{
    public interface ISecurityIdentity : IUserIdentity, ICommonIdentity
    {
        IEnumerable<Guid> Holders { get; }
        Guid ClientId { get; }
    }
}