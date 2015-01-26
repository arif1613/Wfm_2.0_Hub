using System;
using System.Collections.Generic;
using CommonReadModelLibrary.Models;
using NodaTime;

namespace CommonReadModelLibrary.Security.Models
{
    public class UserWithHolder : IViewDocument
    {
        public UserWithHolder()
        {
            HandledMessages = new List<Guid>();
            FieldChanges = new Dictionary<string, Instant>();
            Roles = new List<string>();
            Password = new PasswordDetails();
        }

        public string Id { get; set; }
        public Guid HolderId { get; set; }
        public bool Deleted { get; set; }
        public IList<Guid> HandledMessages { get; set; }
        public IDictionary<string, Instant> FieldChanges { get; set; }
        public Instant LastChangeTime { get; set; }

        public string OwnerDocumentId { get; set; }
        public string Username { get; set; }
        public IList<string> Roles { get; set; }
        public PasswordDetails Password { get; set; }

    }
}