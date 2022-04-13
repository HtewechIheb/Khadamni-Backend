using System.Collections.Generic;

namespace Project_X.Models
{
    public class AuthResult
    {
        public AppUser User { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Succeeded { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
