using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project_X.Models
{
    public class Company
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string ContactNumber { get; set; }
        public string Category { get; set; }
        [JsonIgnore]
        public byte[] LogoFile { get; set; }
        [JsonIgnore]
        public string LogoFileName { get; set; }
        [JsonIgnore]
        public string AccountId { get; set; }
        [JsonIgnore]
        public virtual AppUser Account { get; set; }
        [JsonIgnore]
        public virtual ICollection<Offer> Offers { get; set; }
    }
}
