using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Models
{
    public class Application
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public long CandidateId { get; set; }
        public long OfferId { get; set; }
        public Candidate Candidate { get; set; }
        public Offer Offer { get; set; }
    }
}
