using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TweenFunctions
{
	[KernelFunction(Name = "kernel.tweencreate")]
	internal static void CreateTween(string name, string type, float begin, float finish, float duration, TweenChange changeHandler, TweenEnd endHandler)
	{
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		service.CreateTween(name, Enum<TweenType>.ParseIgnoringCase(type, TweenType.Linear), begin, finish, duration, changeHandler, endHandler);
	}

	[KernelFunction(Name = "kernel.tweenstop")]
	internal static void StopTween(string name)
	{
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		service.StopTween(name);
	}

	[KernelFunction(Name = "kernel.tweenstart")]
	internal static void StartTween(string name)
	{
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		service.StartTween(name);
	}

	[KernelFunction(Name = "kernel.tweenremove")]
	internal static void RemoveTween(string name)
	{
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		service.RemoveTween(name);
	}

	internal static void Reset()
	{
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		service.Reset();
	}
}
