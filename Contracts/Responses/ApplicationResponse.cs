using System;

namespace Project_X.Contracts.Responses
{
    public class ApplicationResponse
    {
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public long CandidateId { get; set; }
        public long OfferId { get; set; }
    }
}
