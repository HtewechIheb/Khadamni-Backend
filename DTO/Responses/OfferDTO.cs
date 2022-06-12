namespace Project_X.DTO.Responses
{
    public class OfferDTO
    {
        public long Id { get; set; }
        public string Industry { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Spots { get; set; }
        public double? Salary { get; set; }
        public string Degree { get; set; }
        public string Gender { get; set; }
        public string[] Skills { get; set; }
        public string Type { get; set; }
        public string MinimumExperience { get; set; }
        public string RecommendedExperience { get; set; }
        public long CompanyId { get; set; }
    }
}
