using System.ComponentModel.DataAnnotations;

namespace SCT.KeisySchool.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}