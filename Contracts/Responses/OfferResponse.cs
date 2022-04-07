namespace Project_X.Contracts.Responses
{
    public class OfferResponse
    {
        public long Id { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Spots { get; set; }
        public string Type { get; set; }
        public string ExperienceLowerBound { get; set; }
        public string ExperienceUpperBound { get; set; }
    }
}
