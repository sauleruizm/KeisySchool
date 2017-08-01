
namespace SCT.KeisySchool.Web.App_Start
{
	public enum SignInStatus
	{
		Success,
		LockedOut,
		RequiresTwoFactorAuthentication,
		Failure
	}
}