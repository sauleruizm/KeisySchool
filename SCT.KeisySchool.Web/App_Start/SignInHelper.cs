using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SCT.KeisySchool.Web.Models;

namespace SCT.KeisySchool.Web.App_Start
{
	public class SignInHelper
	{
		#region -- Constructors, destructors and finalizers --

			public SignInHelper(ApplicationUserManager userManager, IAuthenticationManager authManager)
			{
				UserManager = userManager;
				AuthenticationManager = authManager;
			}

		#endregion -- Constructors, destructors and finalizers --

		#region -- Properties --

			public ApplicationUserManager UserManager { get; private set; }
			public IAuthenticationManager AuthenticationManager { get; private set; }

		#endregion -- Properties --

		#region -- Methods --

			public async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
			{
				AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
				var userIdentity = await user.GenerateUserIdentityAsync(UserManager);

				if (rememberBrowser)
				{
					var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(user.Id);
					AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity, rememberBrowserIdentity);
				}
				else
				{
					AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
				}
			}

			public async Task<bool> SendTwoFactorCode(string provider)
			{
				var userId = await GetVerifiedUserIdAsync();
				if (userId== null)
				{
					return false;
				}
				var token = await UserManager.GenerateTwoFactorTokenAsync(userId, provider);
				await UserManager.NotifyTwoFactorTokenAsync(userId, provider, token);
				return true;
			}

			public async Task<string> GetVerifiedUserIdAsync()
			{
				var result = await AuthenticationManager.AuthenticateAsync(DefaultAuthenticationTypes.TwoFactorCookie);
				if (result!= null && result.Identity !=null && !string.IsNullOrEmpty(result.Identity.GetUserId()))
				{
					return result.Identity.GetUserId();
				}

				return null;
			}

			public async Task<bool> HasBeenVerified()
			{
				return await GetVerifiedUserIdAsync() != null;
			}

			public async Task<SignInStatus> TwoFactorSignIn(string provider, string code, bool isPersistent , bool rememberBrowser)
			{
				var userId = await GetVerifiedUserIdAsync();
				if (userId == null)
				{
					return SignInStatus.Failure;
				}
				var user = await UserManager.FindByIdAsync(userId);
				if (user == null)
				{
					return SignInStatus.Failure;
				}
				if (await UserManager.IsLockedOutAsync(user.Id))
				{
					return SignInStatus.LockedOut;
				}
				if (await UserManager.VerifyTwoFactorTokenAsync(user.Id,provider, code))
				{
					await UserManager.ResetAccessFailedCountAsync(user.Id);
					await SignInAsync(user, isPersistent, rememberBrowser);
					return SignInStatus.Success;
				}

				return SignInStatus.Failure;
			}

			public async Task<SignInStatus> ExternalSignIn(ExternalLoginInfo login, bool isPersistent)
			{
				var user = await UserManager.FindAsync(login.Login);
				if (user == null)
				{
					return SignInStatus.Failure;
				}
				if (await UserManager.IsLockedOutAsync(user.Id))
				{
					return SignInStatus.LockedOut;
				}
				return await SignInOrTwoFactor(user, isPersistent);
			}

			private async Task<SignInStatus> SignInOrTwoFactor(ApplicationUser user, bool isPersistent)
			{
				if (await UserManager.GetTwoFactorEnabledAsync(user.Id) &&
					!await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id))
				{
					var identity = new System.Security.Claims.ClaimsIdentity(DefaultAuthenticationTypes.TwoFactorCookie);
					identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
					AuthenticationManager.SignIn(identity);
					return SignInStatus.RequiresTwoFactorAuthentication;
				}
				await SignInAsync(user, isPersistent, false);
				return SignInStatus.Success;

			}

			public async Task<SignInStatus> PasswordSignIn(string userName, string password, bool isPersistent, bool shouldLockout)
			{
				var user = await UserManager.FindByNameAsync(userName);
				if (user == null)
				{
					return SignInStatus.Failure;
				}
				if (await UserManager.IsLockedOutAsync(user.Id))
				{
					return SignInStatus.LockedOut;
				}
				if (await UserManager.CheckPasswordAsync(user, password))
				{
					return await SignInOrTwoFactor(user, isPersistent);
				}
				if (shouldLockout)
				{
					// If lockout is requested, increment access failed count which might lock out the user
					await UserManager.AccessFailedAsync(user.Id);
					if (await UserManager.IsLockedOutAsync(user.Id))
					{
						return SignInStatus.LockedOut;
					}
				}
				return SignInStatus.Failure;
			}
		#endregion -- Methods --
	}
}