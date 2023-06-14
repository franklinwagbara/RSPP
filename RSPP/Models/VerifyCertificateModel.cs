using System.ComponentModel.DataAnnotations;

namespace RSPP.Models
{
    public class VerifyCertificateModel
    {
        [Required]
        [StringLength(30, ErrorMessage ="Please enter a valid certificate id")]
        public string CertificateId { get; set; }
        public string ErrorMessage { get; set; } = null;
    }
}
