using Microsoft.Win32.SafeHandles;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Runtime.InteropServices;
using SCT.KeisySchool.Web.Models.Teachers;
using MongoDB.Driver;
using System.Configuration;

namespace SCT.KeisySchool.Web.App_Start
{
    public class MongoDbSchoolContext:   IDisposable
    {
        #region -- Private member variables --

            /// <summary>
            /// Flag: Has Dispose already been called?
            /// </summary>
            private bool disposed = false;

            /// <summary>
            /// Instantiate a SafeHandle instance.
            /// 
            /// </summary>
            private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

            private IMongoClient _iClient;

            private IMongoDatabase _iDatabase;

            private IMongoCollection<User> _iUsers;


        #endregion -- Private member variables --

        #region -- Properties --

        
            public IMongoCollection<User> Users 
            { 
                get 
                { 
                    return _iUsers = _iUsers ??  IDatabase.GetCollection<User>("users"); 
                } 
                set { _iUsers = value; }
            }

	        public IMongoClient IClient
	        {
		        get { return _iClient = _iClient ?? new MongoClient(ConfigurationManager.ConnectionStrings["keisyschooldb"].ConnectionString);;}
		        set { _iClient = value;}
	        }

	        public IMongoDatabase IDatabase
	        {
		        get { return _iDatabase = _iDatabase ?? IClient.GetDatabase("keisyschooldb");}
		        set { _iDatabase = value;}
	        }

      
        #endregion -- Properties --

        #region -- Constructors, destructors and finalizers --


        #endregion -- Constructors, destructors and finalizers --

        #region -- Methods --

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Protected implementation of Dispose pattern.
            /// </summary>
            /// <param name="disposing"></param>
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