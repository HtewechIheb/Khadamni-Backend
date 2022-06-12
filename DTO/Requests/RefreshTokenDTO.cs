using System.ComponentModel.DataAnnotations;

namespace Project_X.DTO.Requests
{
    public class RefreshTokenDTO
    {
        [Required]
        public string Token { get; set; }
    }
}
