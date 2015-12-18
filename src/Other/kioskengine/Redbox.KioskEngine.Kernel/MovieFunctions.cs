using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class MovieFunctions
{
	[KernelFunction(Name = "kernel.peekmovie")]
	internal static string PeekMovie()
	{
		return ServiceLocator.Instance.GetService<IFlashService>()?.PeekFlashMovie();
	}

	[KernelFunction(Name = "kernel.pushmovie")]
	internal static void PushMovie(string name, LuaTable location)
	{
		IFlashService service = ServiceLocator.Instance.GetService<IFlashService>();
		if (service == null || location == null)
		{
			return;
		}
		Rectangle? rectangle = location.ToRectangle();
		if (rectangle.HasValue)
		{
			string movie = ServiceLocator.Instance.GetService<IResourceBundleService>().GetMovie(name);
			if (movie != null)
			{
				service.PushFlashMovie(movie, rectangle.Value);
				location.Dispose();
			}
		}
	}

	[KernelFunction(Name = "kernel.popmovie")]
	internal static void PopMovie()
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.PopFlashMovie();
	}

	[KernelFunction(Name = "kernel.switchmovie")]
	internal static void SwitchMovie(string name)
	{
		IFlashService service = ServiceLocator.Instance.GetService<IFlashService>();
		if (service != null)
		{
			string movie = ServiceLocator.Instance.GetService<IResourceBundleService>().GetMovie(name);
			if (movie != null)
			{
				service.SwitchTopMovie(movie);
			}
		}
	}

	[KernelFunction(Name = "kernel.moviecacheclear")]
	internal static void ClearMovieCache()
	{
		ServiceLocator.Instance.GetService<IFlashService>()?.ClearCache();
	}
}
