using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SCT.KeisySchool.Web.Models.Teachers;

namespace SCT.KeisySchool.Web.Controllers
{
    //[Authorize(Roles="Admin, TeacherSupervisor, Teacher")]
    public class TeacherController : Controller
    {
        // GET: Teacher
        public ActionResult Index()
        {
            if (User.IsInRole("Teacher"))
            {
                //TODO: Mostrar solo los datos del actual usuario porque es profesor
                return View();
            }
            else
            {
				List<User> teachers = new List<User>
				{
					new User{ UserName ="4-100-200", LastName="Cedeño", Name="Keisy"},
					new User{ UserName ="4-200-300", LastName="Ruíz", Name="Saúl"},
					new User{ UserName ="10-100-100", LastName="Green", Name="Pedro"}
				};

                return View(teachers);
            }
            
        }

        // GET: Teacher/Details/5
        public ActionResult Details(string id)
        {
			if (!string.IsNullOrEmpty(id))
			{
				var teacher = new User
				{
					UserName = "4-100-200",
					LastName = "Cedeño",
					Name = "Keisy"
				};
				return View(teacher);
			}

            return View();
        }

        // GET: Teacher/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Teacher/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Teacher/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Teacher/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Teacher/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Teacher/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
