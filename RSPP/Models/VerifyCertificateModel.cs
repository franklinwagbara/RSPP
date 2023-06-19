using System.ComponentModel.DataAnnotations;

namespace RSPP.Models
{
    public class VerifyCertificateModel
    {
        [Required]
        //[RegularExpression(@"^(NSC\/((RRPSPU)|(RPRSPU))\/)\d{3}\/\d{4}$", ErrorMessage ="Invalid certificate id provided")]
        [StringLength(19, ErrorMessage= "Invalid certificate id provided")]
        public string LicenseReference { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
