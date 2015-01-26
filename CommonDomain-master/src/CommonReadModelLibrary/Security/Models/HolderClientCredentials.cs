using System;
using System.Collections.Generic;
using CommonReadModelLibrary.Models;
using NodaTime;

namespace CommonReadModelLibrary.Security.Models
{
    public class HolderClientCredentials : IViewDocument
    {
        public HolderClientCredentials()
        {
            HandledMessages = new List<Guid>();
            FieldChanges = new Dictionary<string, Instant>();
        }

        public string Id { get; set; }
        public Guid HolderId { get; set; }
        public string OwnerDocumentId { get; set; }
        public bool Deleted { get; set; }
        public IList<Guid> HandledMessages { get; set; }
        public IDictionary<string, Instant> FieldChanges { get; set; }
        public Instant LastChangeTime { get; set; }

        public byte[] AuthenticationKey { get; set; }
    }
}