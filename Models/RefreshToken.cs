using System;

namespace Project_X.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public virtual AppUser User { get; set; }
    }
}
