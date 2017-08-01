using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Owin;
using SCT.KeisySchool.Web.App_Start;

namespace SCT.KeisySchool.Web
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth(app);
		}
	}
}