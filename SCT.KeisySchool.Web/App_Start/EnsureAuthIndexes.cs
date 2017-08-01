namespace SCT.KeisySchool.Web.App_Start
{
	using AspNet.Identity.MongoDB;

	public class EnsureAuthIndexes
	{
		#region -- Methods --

			public static void Exists()
			{
				var context = ApplicationIdentityContext.Create();
				IndexChecks.EnsureUniqueIndexOnUserName(context.Users);
				IndexChecks.EnsureUniqueIndexOnRoleName(context.Roles);
			}

		#endregion -- Methods --
	}
}