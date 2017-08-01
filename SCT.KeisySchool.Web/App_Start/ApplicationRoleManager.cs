using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspNet.Identity.MongoDB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace SCT.KeisySchool.Web.App_Start
{
	public class ApplicationRoleManager : RoleManager<IdentityRole>
	{
		#region -- Constructors, destructors and finalizers 

			public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore): base(roleStore) { }

		#endregion -- Constructors, destructors and finalizers --

		#region -- Methods --

			public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
			{
				return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<ApplicationIdentityContext>().Roles));
			}

		#endregion -- Methods --
	}
}