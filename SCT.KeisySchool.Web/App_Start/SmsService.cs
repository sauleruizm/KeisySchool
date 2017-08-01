using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace SCT.KeisySchool.Web.App_Start
{
	public class SmsService:IIdentityMessageService
	{
		#region -- Methods --

			public Task SendAsync(IdentityMessage message)
			{
				// Plug in your sms service here to send a text message.
				return Task.FromResult(0);
			}

		#endregion -- Methods --
	}
}