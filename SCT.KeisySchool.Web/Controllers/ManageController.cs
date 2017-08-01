using System.Linq;
using System.Web;
using System.Web.Mvc;
using SCT.KeisySchool.Web.App_Start;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using SCT.KeisySchool.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace SCT.KeisySchool.Web.Controllers
{
    public class ManageController : Controller
	{
		#region -- Private contants --

			// Used for XSRF protection when adding external logins
			private const string XsrfKey = "XsrfId";

		#endregion -- Private contants --

		#region -- Constructors, destructors and finalizers --

		public ManageController() { }

			public ManageController(ApplicationUserManager userManager) { }

		#endregion -- Constructors, destructors and finalizers --

		#region -- Private member variables --

			private ApplicationUserManager _userManager;

		#endregion -- Private member variables --

		#region -- Properties --

			public ApplicationUserManager UserManager
			{
				get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
				set { _userManager = value; }
			}

			private IAuthenticationManager AuthenticationManager
			{
				get
				{
					return HttpContext.GetOwinContext().Authentication;
				}
			}

		#endregion -- Properties --

		#region -- Methods --

			public async Task<ActionResult> Index(ManageMessageId? message)
			{
				ViewBag.StatusMessage =
					message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
					: message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
					: message == ManageMessageId.SetTwoFactorSuccess ? "Your two factor provider has been set."
					: message == ManageMessageId.Error ? "An error has occurred."
					: message == ManageMessageId.AddPhoneSuccess ? "The phone number was added."
					: message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
					: "";
				var model = new IndexViewModel
				{
					HasPassword = HasPassword(),
					PhoneNumber = await UserManager.GetPhoneNumberAsync(User.Identity.GetUserId()),
					TwoFactor = await UserManager.GetTwoFactorEnabledAsync(User.Identity.GetUserId()),
					Logins = await UserManager.GetLoginsAsync(User.Identity.GetUserId()),
					BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(User.Identity.GetUserId())
				};

				return View(model);
			}

			/// <summary>
			/// GET: /Account/RemoveLogin
			/// </summary>
			/// <returns></returns>
			public ActionResult RemoveLogin()
			{
				var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
				ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
				return View(linkedAccounts);
			}

			/// <summary>
			/// POST: /Manage/RemoveLogin
			/// </summary>
			/// <param name="loginProvider"></param>
			/// <param name="providerKey"></param>
			/// <returns></returns>
			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
			{
				ManageMessageId? message;
				var result = UserManager.RemoveLogin(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}
					message = ManageMessageId.RemoveLoginSuccess;
				}
				else
				{
					message = ManageMessageId.Error;
				}

				return RedirectToAction("ManageLogins", new { Message = message } );
			}

			/// <summary>
			/// GET: /Account/AddPhoneNumber
			/// </summary>
			/// <returns></returns>
			public ActionResult AddPhoneNumber()
			{
				return View();
			}

			/// <summary>
			/// GET: /Manage/RememberBrowser
			/// </summary>
			/// <returns></returns>
			public ActionResult RememberBrowser()
			{
				var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(User.Identity.GetUserId());
				AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent= true }, rememberBrowserIdentity);
				return RedirectToAction("Index", "Manage");
			}

			/// <summary>
			/// GET: /Manage/ForgetBrowser
			/// </summary>
			/// <returns></returns>
			public ActionResult ForgetBrowser()
			{
				AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
				return RedirectToAction("Index", "Manage");
			}

			/// <summary>
			/// GET: /Manage/EnableTFA
			/// </summary>
			/// <returns></returns>
			public async Task<ActionResult> EnableTFA()
			{
				await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(),true);
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user!=null)
				{
					await SignInAsync(user, isPersistent: false);
				}
				return RedirectToAction("Index", "Manager");
			}

			/// <summary>
			///  GET: /Manage/DisableTFA
			/// </summary>
			/// <returns></returns>
			public async Task<ActionResult> DisableTFA()
			{
				await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user != null)
				{
					await SignInAsync(user, isPersistent: false);
				}
				return RedirectToAction("Index", "Manage");
			}

			/// <summary>
			/// POST: /Account/AddPhoneNumber
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns>
			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}
				// Send result of: UserManager.GetPhoneNumberCodeAsync(User.Identity.GetUserId(), phoneNumber);
				// Generate the token and send it
				var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
				if (UserManager.SmsService != null)
				{
					var message = new IdentityMessage
					{
						Destination = model.Number,
						Body = "Your security code is: " + code
					};
					await UserManager.SmsService.SendAsync(message);
				}
				return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
			}

			/// <summary>
			///  GET: /Account/VerifyPhoneNumber
			/// </summary>
			/// <param name="phoneNumber"></param>
			/// <returns></returns>
			public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
			{
				// This code allows you exercise the flow without actually sending codes
				// For production use please register a SMS provider in IdentityConfig and generate a code here.
				var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
				ViewBag.Status = "For DEMO purposes only, the current code is " + code;
				return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
			}

			/// <summary>
			/// POST: /Account/VerifyPhoneNumber
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns>
			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}
				var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}
					return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
				}
				// If we got this far, something failed, redisplay form
				ModelState.AddModelError("", "Failed to verify phone");
				return View(model);
			}

			/// <summary>
			/// GET: /Account/RemovePhoneNumber
			/// </summary>
			/// <returns></returns>
			public async Task<ActionResult> RemovePhoneNumber()
			{
				var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
				if (!result.Succeeded)
				{
					RedirectToAction("Index", new { Message = ManageMessageId.Error }); 
				}
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user == null)
				{
					await SignInAsync(user, isPersistent: false);
				}

				return RedirectToAction("Index", ManageMessageId.RemovePhoneSuccess);
			}

			/// <summary>
			/// GET: /Manage/ChangePassword
			/// </summary>
			/// <returns></returns>
			public ActionResult ChangePassword()
			{
				return View();
			}

			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}
				var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
				if (result.Succeeded)
				{
					var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
					if (user != null)
					{
						await SignInAsync(user, isPersistent: false);
					}
					return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
				}
				AddErrors(result);
				return View(model);
			}

			/// <summary>
			/// GET: /Manage/SetPassword
			/// </summary>
			/// <returns></returns>
			public ActionResult SetPassword()
			{
				return View();
			}

			/// <summary>
			/// POST: /Manage/SetPassword
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns>
			[HttpPost]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
			{
				if (ModelState.IsValid)
				{
					var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
					if (result.Succeeded)
					{
						var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
						if (user!=null)
						{
							await SignInAsync(user, isPersistent: false);
						}
						RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
					}
					AddErrors(result);
				}
				// If we got this far, something failed, redisplay form
				return View(model);
			}
			
			/// <summary>
			/// GET: /Account/Manage
			/// </summary>
			/// <param name="message"></param>
			/// <returns></returns>
			public async Task<ActionResult> ManageLogins(ManageMessageId? message)
			{
				ViewBag.StatusMessage =
					message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
					: message == ManageMessageId.Error ? "An error has occurred."
					: "";
				var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
				if (user == null)
				{
					return View("Error");
				}
				var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
				var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
				ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
				return View(new ManageLoginsViewModel
				{
					CurrentLogins = userLogins,
					OtherLogins = otherLogins
				});
			}
			/// <summary>
			/// POST: /Manage/LinkLogin
			/// </summary>
			/// <param name="provider"></param>
			/// <returns></returns>
			[HttpPost]
			[ValidateAntiForgeryToken]
			public ActionResult LinkLogin(string provider)
			{
				// Request a redirect to the external login provider to link a login for the current user
				return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
			}

			/// <summary>
			///  GET: /Manage/LinkLoginCallback
			/// </summary>
			/// <returns></returns>
			public async Task<ActionResult> LoginCallback()
			{
				var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
				if (loginInfo == null)
				{
					RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
				}
				var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
				return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
			}

			private async Task SignInAsync(ApplicationUser user, bool isPersistent)
			{
				AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
				AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent} , await user.GenerateUserIdentityAsync(UserManager));
			}

			private bool HasPassword()
			{
				var user = UserManager.FindById(User.Identity.GetUserId());
				if (user != null)
				{
					return user.PasswordHash != null;
				}
				return false;
			}

			private bool HasPhoneNumber()
			{
				var user = UserManager.FindById(User.Identity.GetUserId());
				if (user !=null)
				{
					return user.PhoneNumber != null;
				}
				return false;
			}

			private void AddErrors(IdentityResult result)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

		#endregion -- Methods --
	}
}