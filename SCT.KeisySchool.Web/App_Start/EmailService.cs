using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace SCT.KeisySchool.Web.App_Start
{
	public class EmailService:IIdentityMessageService
	{
		#region -- Methods --

			public Task SendAsync(IdentityMessage message) { return Task.FromResult(0); }

		#endregion -- Methods --
	}
}