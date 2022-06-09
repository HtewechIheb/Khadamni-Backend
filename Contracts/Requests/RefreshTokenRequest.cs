using System.ComponentModel.DataAnnotations;

namespace Project_X.Contracts.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}
