using System.Web.Mvc;

namespace SCT.KeisySchool.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
		[AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

		[AllowAnonymous]
		public ActionResult About()
		{
			ViewBag.Message = "Your app description page.";
			return View();
		}

		[AllowAnonymous]
		public ActionResult Contact()
		{
			ViewBag.Message = "Your contact page.";
			return View();
		}
	}
}