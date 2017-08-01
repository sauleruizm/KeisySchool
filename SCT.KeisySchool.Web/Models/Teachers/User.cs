using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SCT.KeisySchool.Web.Models.Teachers
{
    public class User : IUser
    {
        public string Id
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public IEnumerable<RoleViewModel> Roles
        {
            get;
            set;
        }

        public string PhoneNumber
        {
            get;
            set;
        }
    }
}
