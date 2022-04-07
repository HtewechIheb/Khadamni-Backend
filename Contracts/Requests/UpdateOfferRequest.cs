using Project_X.Contracts.Enumerations;

namespace Project_X.Contracts.Requests
{
    public class UpdateOfferRequest
    {
        public OfferCategory Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Spots { get; set; }
        public OfferType Type { get; set; }
        public string ExperienceLowerBound { get; set; }
        public string ExperienceUpperBound { get; set; }
        public long? CompanyId { get; set; }
    }
}
