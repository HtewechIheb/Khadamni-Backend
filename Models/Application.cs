using System;

namespace Project_X.Models
{
    public class Application
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public long CandidateId { get; set; }
        public long OfferId { get; set; }
        public virtual Candidate Candidate { get; set; }
        public virtual Offer Offer { get; set; }
    }
}
