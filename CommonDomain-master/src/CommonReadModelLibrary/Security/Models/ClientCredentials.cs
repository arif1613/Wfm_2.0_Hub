using System.Collections.Generic;
namespace CommonReadModelLibrary.Security.Models
{
    public class ClientCredentials
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string HolderId { get; set; }
        public string ClientId { get; set; }
        public string OwnerId { get; set; }
        public byte[] AuthenticationKey { get; set; }
        public IList<string> Roles { get; set; }
        public PasswordDetails Password { get; set; }   
    }
}