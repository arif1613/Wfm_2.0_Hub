using System;
using System.Collections.Generic;
using NodaTime;

namespace CommonReadModelLibrary.Models
{
    public class RequestedOperation : IViewDocument
    {
        public RequestedOperation()
        {
            HandledMessages = new List<Guid>();
            FieldChanges = new Dictionary<string, Instant>();
        }

        public Guid UserId { get; set; }
        public Guid HolderId { get; set; }
        public string OwnerDocumentId { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string Id { get; set; }
        public Guid AggregateId { get; set; }
        public bool Completed { get; set; }
        public bool Failed { get; set; }

        public bool Deleted { get; set; }
        public IList<Guid> HandledMessages { get; set; }
        public IDictionary<string, Instant> FieldChanges { get; set; }
        public Instant LastChangeTime { get; set; }
    }
}