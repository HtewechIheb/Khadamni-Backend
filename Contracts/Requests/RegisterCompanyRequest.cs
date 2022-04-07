using Microsoft.AspNetCore.Http;
using Project_X.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Project_X.Contracts.Requests
{
    public class RegisterCompanyRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Description { get; set; }
        public string ContactNumber { get; set; }
        public string Category { get; set; }
        [Required]
        [MaxFileSize(2 * 1024 * 1024)]
        [AllowedFileExtensions("jpg", "jpeg", "png")]
        public IFormFile LogoFile { get; set; }
    }
}
