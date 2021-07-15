using Project_X.Contracts.Enumerations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Contracts.Requests
{
    public class AddApplicationRequest
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public ApplicationStatus Status { get; set; }
        [Required]
        public long CandidateId { get; set; }
        [Required]
        public long OfferId { get; set; }
    }
}
