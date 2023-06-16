using System.ComponentModel.DataAnnotations;

namespace RSPP.Models.DTOs
{
    public class AuthenticationRequest
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
