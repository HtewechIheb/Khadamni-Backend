using Microsoft.AspNetCore.Identity;
using Project_X.Enumerations;
using System.Text.Json.Serialization;

namespace Project_X.Models
{
    public class AppUser : IdentityUser
    {
        public UserType Type { get; set; }
        [JsonIgnore]
        public virtual Company Company { get; set; }
        [JsonIgnore]
        public virtual Candidate Candidate { get; set; }
    }
}
