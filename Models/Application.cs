using System;
using System.Text.Json.Serialization;

namespace Project_X.Models
{
    public class Application
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public long CandidateId { get; set; }
        public long OfferId { get; set; }
        [JsonIgnore]
        public virtual Candidate Candidate { get; set; }
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
    }
}
