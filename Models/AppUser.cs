using Microsoft.AspNetCore.Identity;
using Project_X.Enumerations;

namespace Project_X.Models
{
    public class AppUser : IdentityUser
    {
        public UserType Type { get; set; }
        public virtual Company Company { get; set; }
        public virtual Candidate Candidate { get; set; }
    }
}
