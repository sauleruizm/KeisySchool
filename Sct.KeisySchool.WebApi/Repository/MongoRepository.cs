using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using MongoDB.Driver;
using Sct.KeisySchool.WebApi.Repository;

namespace Sct.KeisySchool.WebApi.MongoRepository
{
	public class MongoRepository: IRepository
	{
		#region -- Constructors, destructors, and finalizers --

			/// <summary>
			/// Main Constructors 
			/// </summary>
			public MongoRepository()
			{
				_provider = new MongoClient( connectionString: ConnectionString);
			}

		#endregion -- Constructors, destructors, and finalizers --;

		#region -- Private member variables --

			/// <summary>
			/// Represent the mongo client provider
			/// </summary>
			private IMongoClient _provider;

		#endregion -- Private member variables --;

		#region -- Properties --

			/// <summary>
			/// Get the database instance.
			/// </summary>
			private IMongoDatabase _db { get { return this._provider.GetDatabase("keisyschooldb"); } }

			/// <summary>
			/// It's the mongodb connnection string
			/// </summary>
			protected string ConnectionString
			{
				get
				{
					return ConfigurationManager.ConnectionStrings["keisyschooldb"].ToString();
				}
			}

		#endregion -- Properties --;

		#region -- Methods --

			public void Delete<T>() where T : class, new()
			{
				_db.GetCollection(
			}

			public void Delete<T>(Expression<Func<T, bool>> expression ) where T : class, new()
			{
				_db.GetCollection<T>(this.GetType().Name).DeleteOneAsync(expression);
			}

			public void DeleteAll<T>() where T : class, new()
			{
				throw new NotImplementedException();
			}

			public T Single<T>(System.Linq.Expressions.Expression<Func<T, bool>> expression) where T : class, new()
			{
				throw new NotImplementedException();
			}

			public IQueryable<T> All<T>() where T : class, new()
			{
				throw new NotImplementedException();
			}

			public IQueryable<T> All<T>(int page, int pageSize) where T : class, new()
			{
				throw new NotImplementedException();
			}

			public void Add<T>(T item) where T : class, new()
			{
				throw new NotImplementedException();
			}

			public void Add<T>(IEnumerable<T> items) where T : class, new()
			{
				throw new NotImplementedException();
			}

			public void Dispose()
		{
			throw new NotImplementedException();
		}

		#endregion -- Methods --;		
	}
}