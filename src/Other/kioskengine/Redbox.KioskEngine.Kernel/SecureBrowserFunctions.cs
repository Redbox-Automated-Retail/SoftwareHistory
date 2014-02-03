using System.Collections.Generic;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class SecureBrowserFunctions
{
	[KernelFunction(Name = "kernel.launchsecurebrowser")]
	internal static void LaunchSecureBrowser(LuaTable approvedUrls, NavigateUrlCallback navigateUrlCallback)
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 1; i <= approvedUrls.Keys.Count; i++)
		{
			LuaTable luaTable = (LuaTable)approvedUrls[i];
			dictionary[(string)luaTable["title"]] = (string)luaTable["url"];
		}
		service.LaunchSecureBrowser(dictionary, navigateUrlCallback);
		approvedUrls.Dispose();
	}

	[KernelFunction(Name = "kernel.closesecurebrowser")]
	internal static void CloseSecureBrowser()
	{
		while (Application.OpenForms["SecureBrowser"] != null)
		{
			Application.OpenForms["SecureBrowser"].Close();
		}
	}

	internal static void Reset()
	{
		Application.OpenForms["SecureBrowser"]?.Close();
	}
}
