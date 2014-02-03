using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class ViewFunctions
{
	[KernelFunction(Name = "kernel.pushview")]
	internal static void PushView(string viewName)
	{
		ServiceLocator.Instance.GetService<IViewService>()?.Push(viewName);
	}

	[KernelFunction(Name = "kernel.popview")]
	internal static string PopView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Pop();
	}

	[KernelFunction(Name = "kernel.popviewdiscard")]
	internal static string PopViewDiscard()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.PopDiscard();
	}

	[KernelFunction(Name = "kernel.peekview")]
	internal static string PeekView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.Peek();
	}

	[KernelFunction(Name = "kernel.getviewframe")]
	internal static LuaTable GetViewFrame()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service == null)
		{
			return null;
		}
		IViewFrame viewFrame = service.PeekViewFrame();
		if (viewFrame == null)
		{
			return null;
		}
		if (viewFrame.ViewWindow.HasValue)
		{
			LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable["x"] = viewFrame.ViewWindow.Value.X;
			luaTable["y"] = viewFrame.ViewWindow.Value.Y;
			luaTable["width"] = viewFrame.ViewWindow.Value.Width;
			luaTable["height"] = viewFrame.ViewWindow.Value.Height;
			return luaTable;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.showview")]
	internal static void ShowView()
	{
		ServiceLocator.Instance.GetService<IViewService>()?.Show();
	}

	[KernelFunction(Name = "kernel.reenterview")]
	internal static void ReenterView()
	{
		ServiceLocator.Instance.GetService<IViewService>()?.EnterTopOfStock();
	}

	[KernelFunction(Name = "kernel.getlastshownview")]
	internal static string GetLastShownView()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.GetLastShownView();
	}
}
