using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project_X.Models
{
    public class Candidate
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        [JsonIgnore]
        public byte[] ResumeFile { get; set; }
        [JsonIgnore]
        public string ResumeFileName { get; set; }
        [JsonIgnore]
        public byte[] PhotoFile { get; set; }
        [JsonIgnore]
        public string PhotoFileName { get; set; }
        public DateTime? Birthdate { get; set; }
        [JsonIgnore]
        public string AccountId { get; set; }
        [JsonIgnore]
        public virtual AppUser Account { get; set; }
        [JsonIgnore]
        public virtual ICollection<Application> Applications { get; set; }
    }
}
