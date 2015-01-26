using System;
using System.Collections.Generic;
using NodaTime;

namespace CommonReadModelLibrary.Models
{
    public interface IViewDocument
    {
        string Id { get; set; }
        bool Deleted { get; set; }
        Guid HolderId { get; set; }
        IList<Guid> HandledMessages { get; set; }   
        IDictionary<string, Instant> FieldChanges { get; set; }
        Instant LastChangeTime { get; set; }
    }
}