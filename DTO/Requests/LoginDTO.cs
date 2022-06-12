using System.ComponentModel.DataAnnotations;

namespace Project_X.DTO.Requests
{
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
