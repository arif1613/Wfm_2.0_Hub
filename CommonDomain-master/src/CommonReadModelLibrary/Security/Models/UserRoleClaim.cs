namespace CommonReadModelLibrary.Security.Models
{
    public class UserRoleClaim
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
        public string Claim { get; set; }
    }
}