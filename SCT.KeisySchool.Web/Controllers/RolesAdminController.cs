
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using AspNet.Identity.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using SCT.KeisySchool.Web.Models;
using SCT.KeisySchool.Web.App_Start;

namespace SCT.KeisySchool.Web.Controllers
{
	[Authorize(Roles = "Admin")]
    public class RolesAdminController : Controller
	{
		#region -- Constructors, destructors and finalizers --

			public RolesAdminController() { }

			public RolesAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
			{
				UserManager = userManager; RoleManager = roleManager;
			}

		#endregion -- Constructors, destructos and finalizers ---

		#region -- Private member variables --

			private ApplicationUserManager _userManager;

			private ApplicationRoleManager _roleManager;

		#endregion -- Private member variables --

		#region -- Properties --

			/// <summary>
			/// 
			/// </summary>
			public ApplicationIdentityContext IdentityContext { get { return HttpContext.GetOwinContext().GetUserManager<ApplicationIdentityContext>(); } }

			public ApplicationUserManager UserManager
			{
				get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
				set { _userManager = value; }
			}

			public ApplicationRoleManager RoleManager
			{
				get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
				set { _roleManager = value; }
			}

		#endregion -- Properties --

		#region -- Methods --

			/// <summary>
			/// GET: /Roles/
			/// </summary>
			/// <returns></returns>
			public async Task<ActionResult> Index()
			{
				var roles = await IdentityContext.AllRolesAsync();
				return View(roles);
			}

			/// <summary>
			/// GET: /Roles/Details/5
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
			public async Task<ActionResult> Details(string id)
			{
				if (id == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
				}
				var role = await RoleManager.FindByIdAsync(id);

				// Get the list of Users in this Role
				var users = await IdentityContext.Users.Find(u => u.Roles.Contains(role.Name)).ToListAsync();

				ViewBag.Users = users;
				ViewBag.UserCount = users.Count();
				return View(role);

			}

			/// <summary>
			/// GET: /Roles/Create
			/// </summary>
			/// <returns></returns>
			public ActionResult Create()
			{
                return View();
			}

			/// <summary>
			/// POST: /Roles/Create
			/// </summary>
			/// <param name="roleViewModel"></param>
			/// <returns></returns>
			[HttpPost]
			public async Task<ActionResult> Create(RoleViewModel roleViewModel)
			{
				if (ModelState.IsValid)
				{
					var role = new IdentityRole(roleViewModel.Name);
					var roleResult = await RoleManager.CreateAsync(role);
					if (!roleResult.Succeeded)
					{
						ModelState.AddModelError("", roleResult.Errors.First());

					}
					return RedirectToAction("Index");
				}
				return View();
			}

			/// <summary>
			/// GET: /Roles/Edit/Admin
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
			public async Task<ActionResult> Edit(string id)
			{
				if (id==null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
				}
				var role = await RoleManager.FindByIdAsync(id);
				if (role==null)
				{
					return HttpNotFound();
				}
				RoleViewModel roleModel = new RoleViewModel { Id = role.Id, Name = role.Name };
				return View(roleModel);
			}

			/// <summary>
			///  POST: /Roles/Edit/5
			/// </summary>
			/// <param name="roleModel"></param>
			/// <returns></returns>
			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> Edit([Bind(Include="Name,Id")] RoleViewModel roleModel)
			{
				if(ModelState.IsValid)
				{
					var role =await  RoleManager.FindByIdAsync(roleModel.Id);
					role.Name = roleModel.Name;
					await RoleManager.UpdateAsync(role);
					RedirectToAction("Index");
				}
				return View();
			}

			/// <summary>
			/// GET: /Roles/Delete/5
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
			public async Task<ActionResult> Delete(string id)
			{
				if (id == null)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
				}
				var role = await RoleManager.FindByIdAsync(id);
				if (role == null)
				{
					return HttpNotFound();
				}
				return View(role);
			}
		
			/// <summary>
			/// POST: /Roles/Delete/5
			/// </summary>
			/// <param name="id"></param>
			/// <param name="deleteUser"></param>
			/// <returns></returns>
			[HttpPost, ActionName("Delete")]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> DeleteConfirmed(string id, string deleteUser)
			{
				if (ModelState.IsValid)
				{
					if (id == null)
					{
						return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
					}
					var role = await RoleManager.FindByIdAsync(id);
					if (role == null)
					{
						return HttpNotFound();
					}
					IdentityResult result;
					if (deleteUser != null)
					{
						result = await RoleManager.DeleteAsync(role);
					}
					else
					{
						result = await RoleManager.DeleteAsync(role);
					}
					if (!result.Succeeded)
					{
						ModelState.AddModelError("", result.Errors.First());
						return View();
					}
					return RedirectToAction("Index");
				}
				return View();
			}

		#endregion -- Methods --
	}
}