using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using SCT.KeisySchool.Web.Models;
using  Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using AspNet.Identity.MongoDB;
using System.Threading.Tasks;

namespace SCT.KeisySchool.Web.App_Start
{
	public class ApplicationUserManager: UserManager<ApplicationUser>
	{
		#region -- Constructors, destructors and finalizers --

			public ApplicationUserManager(IUserStore<ApplicationUser> store)
				: base(store)
			{
			}

		#endregion -- Constructors, destructors and finalizers --

		#region -- Methods --

			public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
			{
				var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationIdentityContext>().Users));

				// Configure validation logic for usernames
				manager.UserValidator = new UserValidator<ApplicationUser>(manager)
				{
					AllowOnlyAlphanumericUserNames= false, 
					RequireUniqueEmail=false				
				};

				// Configure validation logic for passwords
				manager.PasswordValidator = new PasswordValidator
				{
					RequiredLength = 6,
					RequireNonLetterOrDigit = true,
					RequireDigit = true,
					RequireLowercase = true,
					RequireUppercase = true,
				};
				// Configure user lockout defaults
				manager.UserLockoutEnabledByDefault = true;
				manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
				manager.MaxFailedAccessAttemptsBeforeLockout = 5;

				// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
				// You can write your own provider and plug in here.
				manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
				{
					MessageFormat = "Your security code is: {0}"
				});
				manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
				{
					Subject = "SecurityCode",
					BodyFormat = "Your security code is {0}"
				});
				manager.EmailService = new EmailService();
				manager.SmsService = new SmsService();
				var dataProtectionProvider = options.DataProtectionProvider;
				if (dataProtectionProvider != null)
				{
					manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
				}
				return manager;

			}
			/// <summary>
			/// Method to add user to multiple roles
			/// </summary>
			/// <param name="userId">user id</param>
			/// <param name="roles">list of role names</param>
			/// <returns></returns>
			public virtual async Task<IdentityResult> AddUserToRolesAsync(string userId, IList<string> roles)
			{
				var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

				var user = await FindByIdAsync(userId).ConfigureAwait(false);
				if (user == null)
				{
					throw new InvalidOperationException("Invalid user Id");
				}

				var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
				// Add user to each role using UserRoleStore
				foreach (var role in roles.Where(role => !userRoles.Contains(role)))
				{
					await userRoleStore.AddToRoleAsync(user, role).ConfigureAwait(false);
				}

				// Call update once when all roles are added
				return await UpdateAsync(user).ConfigureAwait(false);
			}

			/// <summary>
			/// Remove user from multiple roles
			/// </summary>
			/// <param name="userId">user id</param>
			/// <param name="roles">list of role names</param>
			/// <returns></returns>
			public virtual async Task<IdentityResult> RemoveUserFromRolesAsync(string userId, IList<string> roles)
			{
				var userRoleStore = (IUserRoleStore<ApplicationUser, string>)Store;

				var user = await FindByIdAsync(userId).ConfigureAwait(false);
				if (user == null)
				{
					throw new InvalidOperationException("Invalid user Id");
				}

				var userRoles = await userRoleStore.GetRolesAsync(user).ConfigureAwait(false);
				// Remove user to each role using UserRoleStore
				foreach (var role in roles.Where(userRoles.Contains))
				{
					await userRoleStore.RemoveFromRoleAsync(user, role).ConfigureAwait(false);
				}

				// Call update once when all roles are removed
				return await UpdateAsync(user).ConfigureAwait(false);
			}

		#endregion -- Methods --
	}
}