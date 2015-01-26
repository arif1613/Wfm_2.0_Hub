using System;
using System.Collections.Generic;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;

namespace CommonReadModelLibrary.Support
{
    public interface ISupportService
    {
        IEnumerable<ArchivedMessage> GetArchivedMessages(List<Type> messageTypes, ICommonIdentity identity, Guid clientId, byte[] authenticationKey);
    }
}