using System;

namespace CommonSystemTestLibrary.Models
{
    public abstract class Response
    {
        public string Id { get; set; }
        public Guid Holder_Id { get; set; }
    }
}
