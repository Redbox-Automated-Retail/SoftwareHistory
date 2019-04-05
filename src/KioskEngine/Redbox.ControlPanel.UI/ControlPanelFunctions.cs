using System;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Services;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Views;

namespace Redbox.ControlPanel.UI;

public class ControlPanelFunctions
{
	private static void ActivateDefaultBundle()
	{
		LogHelper.Instance.LogTo("UserLoginLog", "Control Panel | Login cancel button pressed");
		OrchestrationService.ViewGoBackSafe("Control Panel Login closed", popOnly: true);
		try
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>().ActivateDefaultBundle();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in Kernel Function activatedefaultbundle.", e);
		}
	}

	private static void AuthenticateUser(UserData userData)
	{
		bool flag = true;
		IAuthenticationServices service = ServiceLocator.Instance.GetService<IAuthenticationServices>();
		if (service != null)
		{
			ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
			IMachineSettingsStore service3 = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service2 != null && service3 != null)
			{
				userData.StoreNumber = service3.GetValue("Store", "ID", "N/A");
				RemoteServiceCallback completeCallback = delegate(IRemoteServiceResult result)
				{
					userData.AuthenticationResultCallback(result);
				};
				service.Authenticate(userData.StoreNumber, userData.Id, userData.Password, useLdapAuthentication: true, completeCallback);
				flag = false;
			}
		}
		if (flag)
		{
			LogHelper.Instance.LogTo("UserLoginLog", "Control Panel | Login IAuthenticationServices or ICallbackService failed");
			userData.AuthenticationResultCallback(null);
		}
	}

	public static void UpdateMcafee()
	{
		string arg = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.Market?.MarketName.RemoveString(" ").Replace(",", "_").ToLower() ?? "unknownmarket";
		string text = "c:\\Program Files\\McAfee\\Agent\\maconfig.exe";
		string text2 = "c:\\Program Files\\McAfee\\Agent\\cmdagent.exe";
		if (!File.Exists(text) || !File.Exists(text2))
		{
			LogHelper.Instance.Log($"UpdateMcafee - Forcing Reinstall. MarketName: {arg}");
			string text3 = "C:\\Program Files\\Redbox\\Reds\\Installers\\FramePkg.exe";
			if (File.Exists(text3))
			{
				string args = $"/install=agent /silent /forceinstall /CustomProps1={arg}";
				ProcessFunctions.LaunchProcess(text3, args, hiddenProcess: true);
			}
			else
			{
				LogHelper.Instance.Log("UpdateMcafee - Unable to update Mcafee. Mcafee is not installed and there is no installer available.");
			}
		}
		else
		{
			string arguments = $"-custom -prop1 {arg}";
			string args2 = "–p";
			LogHelper.Instance.Log($"UpdateMcafee - MarketName: {arg}");
			ProcessFunctions.StartProcessWaitForExit(text, arguments, 20000);
			ProcessFunctions.LaunchProcess(text2, args2, hiddenProcess: true);
		}
	}

	private static void DisplayControlPanelView()
	{
		OrchestrationService.ViewGoBackSafe("Control Panel Login closed", popOnly: true);
		OrchestrationService.ViewShow("control_panel_view");
	}

	public static void ShowLogin()
	{
		LogHelper.Instance.Log("Control Panel Login starting.");
		if (ServiceLocator.Instance.GetService<IViewService>() != null)
		{
			LoginInfo parameter = new LoginInfo
			{
				AuthenticateAction = delegate(UserData userData)
				{
					AuthenticateUser(userData);
				},
				CancelAction = delegate
				{
					ActivateDefaultBundle();
				},
				ContinueAction = delegate
				{
					DisplayControlPanelView();
				}
			};
			OrchestrationService.ViewShow("control_panel_login_view", parameter);
		}
	}
}
