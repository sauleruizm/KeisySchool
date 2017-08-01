using System.Collections.Generic;

namespace SCT.KeisySchool.Web.Models.Teachers
{
    public interface IUser
    {
        string Id { get; set; }

        string UserName { get; set; }

        string Name { get; set; }

        string LastName { get; set; }

        IEnumerable<RoleViewModel> Roles { get;  set;}

        string PhoneNumber { get; set; }
    }
}
