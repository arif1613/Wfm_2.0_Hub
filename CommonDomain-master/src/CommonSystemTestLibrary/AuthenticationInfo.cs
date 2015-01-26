using System;

namespace CommonSystemTestLibrary
{
    public class AuthenticationInfo
    {
        public string Username { get; set; }
        public Guid ClientId { get; set; }
        public byte[] AuthenticationKey { get; set; }
    }
}
