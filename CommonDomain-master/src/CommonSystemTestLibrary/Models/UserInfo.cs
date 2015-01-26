using System;
using System.Collections.Generic;

namespace CommonSystemTestLibrary.Models
{
    public class UserInfo
    {
        public string Username { get; set; }
        public Guid Holder_Id { get; set; }
        public Guid Id { get; set; }
        public List<string> User_Roles { get; set; }
    }
}
