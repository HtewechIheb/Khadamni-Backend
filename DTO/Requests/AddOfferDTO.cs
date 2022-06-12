using System.ComponentModel.DataAnnotations;

namespace Project_X.DTO.Requests
{
    public class AddOfferDTO
    {
        [Required]
        public string Industry { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public int? Spots { get; set; }
        public double? Salary { get; set; }
        public string Degree { get; set; }
        public string Gender { get; set; }
        public string[] Skills { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string MinimumExperience { get; set; }
        public string RecommendedExperience { get; set; }
    }
}
