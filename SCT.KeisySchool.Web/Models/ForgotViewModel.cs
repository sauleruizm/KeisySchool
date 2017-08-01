using System.ComponentModel.DataAnnotations;

namespace SCT.KeisySchool.Web.Models
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name="Email")]
        public string Email { get; set; }
    }
}