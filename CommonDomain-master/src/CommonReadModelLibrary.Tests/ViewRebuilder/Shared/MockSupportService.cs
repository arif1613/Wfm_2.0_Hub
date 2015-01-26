using System;
using System.Collections.Generic;
using System.Linq;
using CommonDomainLibrary.Security;
using CommonReadModelLibrary.Models;
using CommonReadModelLibrary.Support;

namespace CommonReadModelLibrary.Tests.ViewRebuilder.Shared
{
    class MockSupportService : ISupportService
    {
        public List<ArchivedMessage> ArchivedMessages;

        public MockSupportService()
        {
            ArchivedMessages = new List<ArchivedMessage>();
        }

        public IEnumerable<ArchivedMessage> GetArchivedMessages(List<Type> messageTypes, ICommonIdentity identity, Guid clientId, byte[] authorizationKey)
        {
            return ArchivedMessages.Where(archivedMessage => messageTypes.Contains(archivedMessage.MessageType)).ToList();
        }
    }
}
