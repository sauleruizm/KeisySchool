using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SCT.KeisySchool.Web.App_Start;

namespace SCT.KeisySchool.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
			EnsureAuthIndexes.Exists();
			AreaRegistration.RegisterAllAreas();
			FilterConfig.RegisteredGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
