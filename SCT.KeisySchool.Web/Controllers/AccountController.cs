using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SCT.KeisySchool.Web.App_Start;
using SCT.KeisySchool.Web.Models;
using System.Linq;

namespace SCT.KeisySchool.Web.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		#region -- Private constants --

			// Used for XSRF protection when adding external logins
			private const string XsrfKey = "XsrfId";

		#endregion -- Private constants --

		#region -- Constructors and Destructors --

		public AccountController(){}

			public AccountController(ApplicationUserManager userManager) { UserManager = userManager; }

		#endregion -- Constructors and Destructors --

		#region -- Private member variables --

			private ApplicationUserManager _userManager;
			private SignInHelper _signInHelper;

		#endregion -- Private member variables --

		#region -- Properties --

			public ApplicationUserManager UserManager
			{
				get
				{
					return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
				}
				private set { _userManager = value; }
			}
			
			public SignInHelper SignInHelper
			{
				get {
					if (_signInHelper ==null)
					{
						_signInHelper = new SignInHelper(UserManager, AuthenticationManager);
					}
					return _signInHelper; }
				set { _signInHelper = value; }
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

			/// <summary>
			/// GET: /Account/Login
			/// </summary>
			/// <param name="returnUrl"></param>
			/// <returns></returns>
			[AllowAnonymous]
			public ActionResult Login(string returnUrl)
			{
				ViewBag.ReturnUrl = returnUrl;
				return View();
			}

			/// <summary>
			/// POST: /Account/Login
			/// </summary>
			/// <param name="model"></param>
			/// <param name="returnUrl"></param>
			/// <returns></returns>
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}

				// This doen't count login failures towards lockout only two factor authentication
				// To enable password failures to trigger lockout, change to shouldLockout: true
				var result = await SignInHelper.PasswordSignIn(model.PID, model.Password, model.RememberMe, shouldLockout: false);
				switch (result)
				{
					case SCT.KeisySchool.Web.App_Start.SignInStatus.Success:
						return RedirectToLocal(returnUrl);
					case SCT.KeisySchool.Web.App_Start.SignInStatus.LockedOut:
						return View("Lockout");
					case SCT.KeisySchool.Web.App_Start.SignInStatus.RequiresTwoFactorAuthentication:
						return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
					case SCT.KeisySchool.Web.App_Start.SignInStatus.Failure:
					default: 
						ModelState.AddModelError("", "Invalid login attempt.");
						return View(model);
				}
			}

			/// <summary>
			/// GET: /Account/VerifyCode
			/// </summary>
			/// <param name="provider"></param>
			/// <param name="returnUrl"></param>
			/// <returns></returns>
			[AllowAnonymous]
			public async Task<ActionResult> VerifyCode(string provider, string returnUrl)
			{
				if (!await SignInHelper.HasBeenVerified())
				{
					return View("Error");
				}
				var user = await UserManager.FindByIdAsync(await SignInHelper.GetVerifiedUserIdAsync());
				if (user==null)
				{
					// To exercise the flow without actually sending codes, uncomment the following line
					ViewBag.Status = "For DEMO purposes the current " + provider + " code is: " 
						+ await UserManager.GenerateTwoFactorTokenAsync(user.Id, provider);
				}
				return View(new VerifyCodeViewModel { Provider= provider, ReturnUrl = returnUrl });
			}

			/// <summary>
			/// POST: /Account/VerifyCode
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns>
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}

				var result = await SignInHelper.TwoFactorSignIn(model.Provider, model.Code, isPersistent: false, rememberBrowser: model.RememberBrowser);
				switch (result)
				{
					case SCT.KeisySchool.Web.App_Start.SignInStatus.Success:
						return RedirectToLocal(model.ReturnUrl);
					case SCT.KeisySchool.Web.App_Start.SignInStatus.LockedOut:
						return View("Lockedout");					
					case SCT.KeisySchool.Web.App_Start.SignInStatus.Failure:		
					default:
						ModelState.AddModelError("", "Invalid code.");
						return View(model);
				}
			}

			/// <summary>
			/// GET: /Account/Register
			/// </summary>
			/// <returns></returns>
			[AllowAnonymous]
			public ActionResult Register() { return View(); }

			/// <summary>
			/// POST: /Account/Register
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns>
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> Register(RegisterViewModel model)
			{
				if (ModelState.IsValid)
				{
					var user = new ApplicationUser { UserName = model.PID, Email = model.Email, TwoFactorEnabled = false };
					var result = await UserManager.CreateAsync(user, model.Password);
					if (result.Succeeded)
					{
						var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
						var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
						await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
						ViewBag.Link = callbackUrl;
						return View("DisplayEmail");
					}
					AddErrors(result);
				}

				// If we got this far, something failed, redisplay form
				return View(model);
			}

			/// <summary>
			/// GET: /Account/ConfirmEmail
			/// </summary>
			/// <param name="userId"></param>
			/// <param name="code"></param>
			/// <returns></returns>
			[AllowAnonymous]
			public async Task<ActionResult> ConfirmEmail(string userId, string code)
			{
				if (userId==null || code == null)
				{
					return View("Error");
				}
				var result = await UserManager.ConfirmEmailAsync(userId, code);
				if (result.Succeeded)
				{
					return View("ConfirmEmail");
				}
				AddErrors(result);
				return View();
			}


			/// <summary>
			/// GET: /Account/ForgotPassword
			/// </summary>
			/// <returns></returns>
			[AllowAnonymous]
			public ActionResult ForgotPassword()
			{
				return View();
			}

			/// <summary>
			///  POST: /Account/ForgotPassword
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns>
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
			{
				if (ModelState.IsValid)
				{
					var user = await UserManager.FindByNameAsync(model.Email);
					if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
					{
						// Don't reveal that the user does not exist or is not confirmed
						return View("ForgotPasswordConfirmation");
					}

					var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
					var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
					await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
					ViewBag.Link = callbackUrl;
					return View("ForgotPasswordConfirmation");
				}

				// If we got this far, something failed, redisplay form
				return View(model);
			}

			/// <summary>
			/// GET: /Account/ForgotPasswordConfirmation
			/// </summary>
			/// <returns></returns>
			[AllowAnonymous]
			public ActionResult ForgotPasswordConfirmation()
			{
				return View();
			}

			/// <summary>
			/// GET: /Account/ResetPassword
			/// </summary>
			/// <param name="code"></param>
			/// <returns></returns>
			[AllowAnonymous]
			public ActionResult ResetPassword(string code)
			{
				return code == null ? View("Error") : View();
			}

			/// <summary>
			/// POST: /Account/ResetPassword
			/// </summary>
			/// <param name="model"></param>
			/// <returns></returns
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
			{
				if (!ModelState.IsValid)
				{
					return View(model);
				}
				var user = await UserManager.FindByNameAsync(model.Email);
				if (user == null)
				{
					// Don't reveal that the user does not exist
					return RedirectToAction("ResetPasswordConfirmation", "Account");
				}
				var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
				if (result.Succeeded)
				{
					return RedirectToAction("ResetPasswordConfirmation", "Account");
				}
				AddErrors(result);
				return View();
			}

			//
			/// <summary>
			///  GET: /Account/ResetPasswordConfirmation
			/// </summary>
			/// <returns></returns>
			[AllowAnonymous]
			public ActionResult ResetPasswordConfirmation()
			{
				return View();
			}

			// POST: /Account/ExternalLogin
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public ActionResult ExternalLogin(string provider, string returnUrl)
			{
				// Request a redirect to the external login provider
				return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
			}

			//
			// GET: /Account/SendCode
			[AllowAnonymous]
			public async Task<ActionResult> SendCode(string returnUrl)
			{
				var userId = await SignInHelper.GetVerifiedUserIdAsync();
				if (userId == null)
				{
					return View("Error");
				}
				var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
				var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
				return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl });
			}

			//
			// POST: /Account/SendCode
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> SendCode(SendCodeViewModel model)
			{
				// Generate the token and send it
				if (!ModelState.IsValid)
				{
					return View();
				}

				if (!await SignInHelper.SendTwoFactorCode(model.SelectedProvider))
				{
					return View("Error");
				}
				return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl });
			}

			//
			// GET: /Account/ExternalLoginCallback
			[AllowAnonymous]
			public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
			{
				var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
				if (loginInfo == null)
				{
					return RedirectToAction("Login");
				}

				// Sign in the user with this external login provider if the user already has a login
				var result = await SignInHelper.ExternalSignIn(loginInfo, isPersistent: false);
				switch (result)
				{
					case SCT.KeisySchool.Web.App_Start.SignInStatus.Success:
						return RedirectToLocal(returnUrl);
					case SCT.KeisySchool.Web.App_Start.SignInStatus.LockedOut:
						return View("Lockout");
					case SCT.KeisySchool.Web.App_Start.SignInStatus.RequiresTwoFactorAuthentication:
						return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
					case SCT.KeisySchool.Web.App_Start.SignInStatus.Failure:
					default:
						// If the user does not have an account, then prompt the user to create an account
						ViewBag.ReturnUrl = returnUrl;
						ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
						return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
				}
			}

			//
			// POST: /Account/ExternalLoginConfirmation
			[HttpPost]
			[AllowAnonymous]
			[ValidateAntiForgeryToken]
			public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
			{
				if (User.Identity.IsAuthenticated)
				{
					return RedirectToAction("Index", "Manage");
				}

				if (ModelState.IsValid)
				{
					// Get the information about the user from the external login provider
					var info = await AuthenticationManager.GetExternalLoginInfoAsync();
					if (info == null)
					{
						return View("ExternalLoginFailure");
					}
					var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
					var result = await UserManager.CreateAsync(user);
					if (result.Succeeded)
					{
						result = await UserManager.AddLoginAsync(user.Id, info.Login);
						if (result.Succeeded)
						{
							await SignInHelper.SignInAsync(user, isPersistent: false, rememberBrowser: false);
							return RedirectToLocal(returnUrl);
						}
					}
					AddErrors(result);
				}

				ViewBag.ReturnUrl = returnUrl;
				return View(model);
			}

			//
			// POST: /Account/LogOff
			[HttpPost]
			[ValidateAntiForgeryToken]
			public ActionResult LogOff()
			{
				AuthenticationManager.SignOut();
				return RedirectToAction("Index", "Home");
			}

			//
			// GET: /Account/ExternalLoginFailure
			[AllowAnonymous]
			public ActionResult ExternalLoginFailure()
			{
				return View();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && _userManager != null)
				{
					_userManager.Dispose();
					_userManager = null;
				}
				base.Dispose(disposing);
			}

			private ActionResult RedirectToLocal(string returnUrl)
			{
				if (Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}
				return RedirectToAction("Index", "Home");
			}

			private void AddErrors(IdentityResult result)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("", error);
				}
			}

		#endregion -- Methods --

		internal class ChallengeResult : HttpUnauthorizedResult
		{
			public ChallengeResult(string provider, string redirectUri)
				: this(provider, redirectUri, null)
			{
			}

			public ChallengeResult(string provider, string redirectUri, string userId)
			{
				LoginProvider = provider;
				RedirectUri = redirectUri;
				UserId = userId;
			}

			public string LoginProvider { get; set; }
			public string RedirectUri { get; set; }
			public string UserId { get; set; }

			public override void ExecuteResult(ControllerContext context)
			{
				var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
				if (UserId != null)
				{
					properties.Dictionary[XsrfKey] = UserId;
				}
				context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
			}
		}

	}


}