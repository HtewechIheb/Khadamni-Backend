using Project_X.Contracts.Enumerations;
using System;

namespace Project_X.Contracts.Requests
{
    public class UpdateApplicationRequest
    {
        public DateTime? Date { get; set; }
        public ApplicationStatus Status { get; set; }
        public long? CandidateId { get; set; }
        public long? OfferId { get; set; }
    }
}
