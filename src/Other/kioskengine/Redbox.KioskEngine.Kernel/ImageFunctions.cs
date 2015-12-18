using System;
using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class ImageFunctions
{
	[KernelFunction(Name = "kernel.registerimagefromcache")]
	internal static bool RegisterImageFromCache(string name, string cacheName)
	{
		IDataCacheService service = ServiceLocator.Instance.GetService<IDataCacheService>();
		if (service == null)
		{
			throw new SystemException("IDataCacheService is not registered.");
		}
		IBitmapCacheService service2 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service2 == null)
		{
			throw new SystemException("IBitmapCacheService is not registered.");
		}
		if (service2.GetImage(name) != null)
		{
			return true;
		}
		byte[] content = service.GetContent(DataCacheType.Image, cacheName);
		if (content == null)
		{
			LogHelper.Instance.Log("*** IDataCacheService was not able to read any data for: '{0}'", cacheName);
			return false;
		}
		return service2.RegisterImage(name, content) != null;
	}

	[KernelFunction(Name = "kernel.createthumbnail")]
	internal static void CreateThumbnail(string name, string thumbName, int width, int height)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service == null)
		{
			throw new SystemException("IBitmapCacheService is not registered.");
		}
		if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(thumbName))
		{
			if (!name.StartsWith("cache:"))
			{
				ServiceLocator.Instance.GetService<IResourceBundleService>().GetBitmap(name);
			}
			else
			{
				name = name.Substring(6);
			}
			service.RegisterThumbnailImage(name, thumbName, width, height);
		}
	}

	[KernelFunction(Name = "kernel.trimbitmapcache")]
	internal static int TrimBitmapCache(int maximumItems)
	{
		return ServiceLocator.Instance.GetService<IBitmapCacheService>()?.TrimCache(maximumItems) ?? 0;
	}

	[KernelFunction(Name = "kernel.writetextonimage")]
	internal static void WriteTextOnImage(string textToDisplay, string baseImageName, string outputToImageName, string font, LuaTable rectangle)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service == null)
		{
			throw new SystemException("IBitmapCacheService is not registered.");
		}
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		Rectangle textLocation = default(Rectangle);
		try
		{
			textLocation.X = int.Parse(rectangle["X"].ToString());
			textLocation.Y = int.Parse(rectangle["Y"].ToString());
			textLocation.Width = int.Parse(rectangle["Width"].ToString());
			textLocation.Height = int.Parse(rectangle["Height"].ToString());
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("*** Unable to parse Lua Rectangle passed to kernel.writetextonimage: {0}", ex.Message);
		}
		service.RegisterImageWithText(outputToImageName, textToDisplay, service2.GetFont(font, out var _), service2.GetBitmap(baseImageName), textLocation);
	}
}
