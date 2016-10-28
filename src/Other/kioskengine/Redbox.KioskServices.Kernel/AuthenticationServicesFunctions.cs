using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskServices.Kernel;

internal static class AuthenticationServicesFunctions
{
	[KernelFunction(Name = "kernel.authsvcs_authenticateuser")]
	internal static void AuthenticateUser(string storeNumber, string userName, string password, bool useLdapAuthentication, LuaFunction authenticationCallback)
	{
		IAuthenticationServices service = ServiceLocator.Instance.GetService<IAuthenticationServices>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.Authenticate(storeNumber, userName, password, useLdapAuthentication, delegate(IRemoteServiceResult r)
			{
				callbackService.EnqueueCallback(new ResultCallbackEntry
				{
					Function = authenticationCallback,
					Parameters = new LuaTable[1] { r.AsTable() }
				});
			});
		}
	}
}
