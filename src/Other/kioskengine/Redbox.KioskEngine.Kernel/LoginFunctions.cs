using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class LoginFunctions
{
	[KernelFunction(Name = "kernel.showlogin")]
	internal static void ShowLogin(string title, int timeout, LuaFunction loginCallback, LuaFunction cancelCallback)
	{
		LoginForm loginForm = new LoginForm();
		if (!string.IsNullOrEmpty(title))
		{
			loginForm.Text = title;
		}
		loginForm.SetTimeout(timeout);
		loginForm.IsValid = true;
		loginForm.LoginCallback = loginCallback;
		loginForm.CancelLoginCallback = cancelCallback;
		loginForm.Show();
	}

	[KernelFunction(Name = "kernel.setloginresult")]
	internal static void SetLoginResult(string result)
	{
		LoginForm loginForm = (LoginForm)Application.OpenForms["LoginForm"];
		if (loginForm != null)
		{
			if (string.Compare(result, "OK", ignoreCase: true) == 0)
			{
				loginForm.CloseAsLoggedIn();
			}
			else
			{
				loginForm.ShowError(result);
			}
		}
	}

	internal static void Reset()
	{
		LoginForm loginForm = (LoginForm)Application.OpenForms["LoginForm"];
		if (loginForm != null)
		{
			loginForm.IsValid = false;
			loginForm.LoginCallback.Dispose();
			loginForm.LoginCallback = null;
			loginForm.CancelLoginCallback.Dispose();
			loginForm.CancelLoginCallback = null;
			loginForm.CloseAsCancel();
			while (Application.OpenForms["LoginForm"] != null)
			{
				Application.DoEvents();
			}
		}
	}
}
