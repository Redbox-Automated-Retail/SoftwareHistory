using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Services;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views;

public class ComingSoonViewFunctions
{
	[KernelFunction(Name = "kernel.configure_coming_soon_view")]
	internal static void ConfigureComingSoonView(LuaTable products)
	{
		ComingSoonViewController.ConfigureComingSoonView(CreateComingSoonTitles(products));
	}

	private static List<IComingSoonTitle> CreateComingSoonTitles(LuaTable products)
	{
		List<IComingSoonTitle> list = new List<IComingSoonTitle>();
		foreach (object key in products.Keys)
		{
			if (string.IsNullOrEmpty(products[key].ToString()) || !(products[key] is LuaTable luaTable))
			{
				continue;
			}
			string titleName = luaTable["long_name"].ToString();
			string text = luaTable["merchandise_date"].ToString();
			if (text != null && text.Length > 7)
			{
				text = $"{text.Substring(4, 2)}/{text.Substring(6, 2)}/{text.Substring(0, 4)}";
			}
			string text2 = luaTable["image_file"].ToString();
			string text3 = null;
			if (!string.IsNullOrEmpty(text2) && DataServiceFunctions.DataServiceRegisterImage(text2, text2))
			{
				text3 = $"cache:{text2}";
			}
			BitmapImage image = null;
			if (!string.IsNullOrEmpty(text3) && text3.StartsWith("cache:"))
			{
				string text4 = text3.Substring(6);
				if (OrchestrationService.DataServiceRegisterBitmapImage(text4, text4))
				{
					image = ServiceLocator.Instance.GetService<IBitmapCacheService>()?.GetBitmapImage(text4);
				}
			}
			IComingSoonTitle item = new ComingSoonTitle
			{
				TitleName = titleName,
				ReleaseDate = text,
				Image = image
			};
			list.Add(item);
		}
		return list;
	}
}
