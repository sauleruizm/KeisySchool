using System.Web.Mvc;

namespace SCT.KeisySchool.Web.App_Start
{
	public class FilterConfig
	{
		#region -- Methods --

			public static void RegisteredGlobalFilters(GlobalFilterCollection filters)
			{
				filters.Add(new HandleErrorAttribute());
			}

		#endregion
	}
}