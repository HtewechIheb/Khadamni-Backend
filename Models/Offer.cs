using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project_X.Models
{
    public class Offer
    {
        public long Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Spots { get; set; }
        public string Type { get; set; }
        public string ExperienceLowerBound { get; set; }
        public string ExperienceUpperBound { get; set; }
        [JsonIgnore]
        public virtual Company Company { get; set; }
        [JsonIgnore]
        public virtual ICollection<Application> Applications { get; set; }
    }
}
