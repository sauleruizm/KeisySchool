using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SCT.KeisySchool.Web.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Role Name")]
        public string Name { get; set; }
        //[Display(Name = "Role Name")]
        //public IEnumerable<SelectListItem> RolesList { get; set; } 

    }
}