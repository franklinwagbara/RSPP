using System.ComponentModel.DataAnnotations;

namespace RSPP.Models.DTOs
{
    public class AuthenticationRequest
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        [MinLength(4)]
        public string Password { get; set; }
    }
}
