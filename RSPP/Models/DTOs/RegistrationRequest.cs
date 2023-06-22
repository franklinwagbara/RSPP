using System.ComponentModel.DataAnnotations;

namespace RSPP.Models.DTOs
{
    public class RegistrationRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(20, ErrorMessage = "{0} length must be between {2} and {1}", MinimumLength = 4)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirmation Password is required.")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Mobile Phone Number is required")]
        [StringLength(11, ErrorMessage = "{0} length must be {1}")]
        [Display(Name = "Mobile Phone Number")]
        [RegularExpression(@"^0(7|8|9){1}(0|1){1}\d{8}$", ErrorMessage = "Invalid phone number provided")]
        public string MobilePhoneNumber { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(200, ErrorMessage = "{0} length must be between {2} and {1}", MinimumLength = 2)]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Company Address is required")]
        [StringLength(200, ErrorMessage = "{0} length must be between {2} and {1}", MinimumLength = 4)]
        public string CompanyAddress { get; set; }
    }
}
