using System;

namespace CommonSystemTestLibrary.Models
{
    public class RequestedOperationResponse : Response
    {
        public Guid User_Id { get; set; }
        public string Status { get; set; }
        public string Error_Message { get; set; }
        public Guid Aggregate_Id { get; set; }
        public bool Completed { get; set; }
        public bool Failed { get; set; }
    }
}
