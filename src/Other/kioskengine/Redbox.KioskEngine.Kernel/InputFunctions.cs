using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class InputFunctions
{
	[KernelFunction(Name = "kernel.clearidlenotify")]
	internal static void ClearIdleNotify()
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.ClearIdleHandlers();
	}

	[KernelFunction(Name = "kernel.registeridlenotify")]
	internal static void RegisterIdleNotify(string name, IdleEventHandler idleHandler)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RegisterIdleHandler(name, idleHandler);
	}

	[KernelFunction(Name = "kernel.removeidlenotify")]
	internal static void RemoveIdleNotify(string name)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RemoveIdleHandler(name);
	}

	[KernelFunction(Name = "kernel.clearactivatednotify")]
	internal static void ClearActivatedNotify()
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.ClearActivatedHandlers();
	}

	[KernelFunction(Name = "kernel.removeactivatednotify")]
	internal static void RemoveActivatedNotify(string name)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RemoveActivatedHandler(name);
	}

	[KernelFunction(Name = "kernel.registeractivatednotify")]
	internal static void RegisterActivatedNotify(string name, ActivatedEventHandler activatedHandler)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RegisterActivatedHandler(name, activatedHandler);
	}

	[KernelFunction(Name = "kernel.clearclicknotify")]
	internal static void ClearClickNotify()
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.ClearMouseClickHandlers();
	}

	[KernelFunction(Name = "kernel.registerclicknotify")]
	internal static void RegisterMouseClickNotify(string name, MouseClickHandler mouseClickHandler)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RegisterMouseClickHandler(name, mouseClickHandler);
	}

	[KernelFunction(Name = "kernel.removeclicknotify")]
	internal static void RemoveMouseClickNotify(string name)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RemoveMouseClickHandler(name);
	}

	[KernelFunction(Name = "kernel.clearkeypressnotify")]
	internal static void ClearKeyPressNotify()
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.ClearKeyPressHandlers();
	}

	[KernelFunction(Name = "kernel.registerkeypressnotify")]
	internal static void RegisterKeyPressNotify(string name, KeyPressHandler keyPressHandler)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RegisterKeyPressHandler(name, keyPressHandler);
	}

	[KernelFunction(Name = "kernel.removekeypressnotify")]
	internal static void RemoveKeyPressNotify(string name)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RemoveKeyPressHandler(name);
	}

	[KernelFunction(Name = "kernel.cleardoubleclicknotify")]
	internal static void ClearDoubleClickNotify()
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.ClearMouseDoubleClickHandlers();
	}

	[KernelFunction(Name = "kernel.registerdoubleclicknotify")]
	internal static void RegisterDoubleClickNotify(string name, MouseClickHandler mouseClickHandler)
	{
		IInputService service = ServiceLocator.Instance.GetService<IInputService>();
		service.RegisterMouseDoubleClickHandler(name, mouseClickHandler);
	}

	[KernelFunction(Name = "kernel.showmouse")]
	internal static void ShowMouse()
	{
		Cursor.Show();
	}

	[KernelFunction(Name = "kernel.hidemouse")]
	internal static void HideMouse()
	{
		Cursor.Hide();
	}

	[KernelFunction(Name = "kernel.getmouseposscreen")]
	internal static LuaTable GetMousePositionInScreen()
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["x"] = Cursor.Position.X;
		luaTable["y"] = Cursor.Position.Y;
		KernelService.Instance.LuaRuntime["mouse_position"] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getmousepos")]
	internal static LuaTable GetMousePositionInClient()
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		Point point = Application.OpenForms[0].PointToClient(Cursor.Position);
		luaTable["x"] = point.X;
		luaTable["y"] = point.Y;
		KernelService.Instance.LuaRuntime["mouse_position"] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.setmousepos")]
	internal static void SetMousePosition(int x, int y)
	{
		Cursor.Position = Application.OpenForms[0].PointToScreen(new Point(x, y));
	}

	[KernelFunction(Name = "kernel.mousesingleclick")]
	internal static void PerformSingleClick()
	{
		ServiceLocator.Instance.GetService<IInputService>()?.PerformSingleClick();
	}

	[KernelFunction(Name = "kernel.mousedoubleclick")]
	internal static void PerformDoubleClick()
	{
		ServiceLocator.Instance.GetService<IInputService>()?.PerformDoubleClick();
	}

	[KernelFunction(Name = "kernel.confirm")]
	internal static string Confirm(string message)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		DialogResult dialogResult = MessageBox.Show(message, service.ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
		return dialogResult.ToString();
	}

	[KernelFunction(Name = "kernel.showmessage")]
	internal static void ShowMessage(string message)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		MessageBox.Show(message, service.ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	[KernelFunction(Name = "kernel.sendkeys")]
	internal static void SendKeys(string keys)
	{
		ServiceLocator.Instance.GetService<IInputService>()?.SimulateKeyStrokes(keys);
	}
}
