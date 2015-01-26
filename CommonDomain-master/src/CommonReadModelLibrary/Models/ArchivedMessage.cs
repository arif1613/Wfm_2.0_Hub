using System;
using CommonDomainLibrary;

namespace CommonReadModelLibrary.Models
{
    public class ArchivedMessage
    {
        public Guid Id { get; set; }
        public Type MessageType { get; set; }
        public IMessage Message { get; set; }

        public Guid documentId { get; set; }
    }
}
