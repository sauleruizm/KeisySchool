using AspNet.Identity.MongoDB;
using Microsoft.Win32.SafeHandles;
using MongoDB.Driver;
using SCT.KeisySchool.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SCT.KeisySchool.Web.App_Start
{
	public class ApplicationIdentityContext: IDisposable
    {
        #region -- Private member variables --

            // Flag: Has Dispose already been called?
            private bool disposed = false;

            // Instantiate a SafeHandle instance.
            private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        #endregion -- Private member variables --

        #region -- Constructor, destructors and finalizer --

        private ApplicationIdentityContext(IMongoCollection<ApplicationUser> users, IMongoCollection<IdentityRole> roles)
			{
				Users = users;
				Roles = roles;
			}

		#endregion -- Constructor, destructors and finalizer --

		#region -- Properties --

			public IMongoCollection<IdentityRole> Roles { get; set; }

			public IMongoCollection<ApplicationUser> Users { get; set; }


		#endregion -- Properties --

		#region -- Methods --

			/// <summary>
			/// Get all roles
			/// </summary>
			/// <returns></returns>
			public Task<List<IdentityRole>> AllRolesAsync()
			{
				return Roles.Find(r => true).ToListAsync();
			}

			/// <summary>
			/// Create database or obtain data,
			/// about the users and roles
			/// </summary>
			/// <returns></returns>
			public static ApplicationIdentityContext Create()
			{
				var client = new MongoClient(ConfigurationManager.ConnectionStrings["keisyschooldb"].ConnectionString);
				var database = client.GetDatabase("keisyschooldb");
				var users = database.GetCollection<ApplicationUser>("users");
				var roles = database.GetCollection<IdentityRole>("roles");
				return new ApplicationIdentityContext(users, roles);
			}

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // Protected implementation of Dispose pattern.
            protected virtual void Dispose(bool disposing)
            {
                if (disposed)
                    return;

                if (disposing)
                {
                    handle.Dispose();
                    // Free any other managed objects here.
                    //
                }

                // Free any unmanaged objects here.
                //
                disposed = true;
            }

		#endregion -- Methods --
	}
}