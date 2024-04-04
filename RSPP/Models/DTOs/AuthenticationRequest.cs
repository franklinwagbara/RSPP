using System.ComponentModel.DataAnnotations;

namespace RSPP.Models.DTOs
{
    public class AuthenticationRequest
    {
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(20,ErrorMessage = "{0} length must be between {2} and {1}", MinimumLength =4)]
        public string Password { get; set; }
    }
}
