using System;
using System.Collections.Generic;
using CommonReadModelLibrary.Models;
using NodaTime;

namespace CommonReadModelLibrary.Security.Models
{
    public class HolderWithClients : IViewDocument
    {
        public HolderWithClients()
        {
            HandledMessages = new List<Guid>();
            FieldChanges = new Dictionary<string, Instant>();
            Clients = new List<string>();
            Children = new List<string>();
        }

        public string Id { get; set; }
        public Guid HolderId { get; set; }
        public string OwnerDocumentId { get; set; }
        public bool Deleted { get; set; }
        public IList<Guid> HandledMessages { get; set; }
        public IDictionary<string, Instant> FieldChanges { get; set; }
        public Instant LastChangeTime { get; set; }

        public IList<string> Clients { get; set; }
        public IList<string> Children { get; set; }
    }
}