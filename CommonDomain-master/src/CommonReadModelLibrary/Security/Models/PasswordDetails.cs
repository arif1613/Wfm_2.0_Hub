using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CommonReadModelLibrary.Security.Models
{
    public class PasswordDetails
    {
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public int Iterations { get; set; }
        public byte[] PasswordKey { get; set; }
        public byte[] PasswordInitVector { get; set; }
    }
}
