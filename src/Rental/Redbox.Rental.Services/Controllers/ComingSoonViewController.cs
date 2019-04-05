using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ComingSoonViewController : BaseViewController
{
	public static Func<LuaTable> GetComingSoonProductsCallback;

	public static void ConfigureComingSoonView(List<IComingSoonTitle> comingSoonTitles)
	{
		if (OrchestrationService.InvokeIsVendJobCompletedCallback())
		{
			OrchestrationService.ViewGoBack("Coming Soon View: Vend job finished.", popOnly: false);
		}
		else
		{
			if (!(BaseViewController.GetViewUserControl() is ComingSoonViewUserControl comingSoonViewUserControl))
			{
				return;
			}
			if (BundleFunctions.GetCulture() == "es-MX")
			{
				comingSoonViewUserControl.ComingSoonTextStyle = "font_target_larger";
				comingSoonViewUserControl.RentalsAreOnTheirWayStyle = "font_target_large_overlays";
			}
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "coming_soon_coming_soon");
			string dictionaryStringValue2 = BundleFunctions.GetDictionaryStringValue("strings", "rentals_coming_soon");
			comingSoonViewUserControl.ComingSoonText = dictionaryStringValue;
			comingSoonViewUserControl.RentalsAreOnTheirWayText = dictionaryStringValue2;
			foreach (IComingSoonTitle comingSoonTitle in comingSoonTitles)
			{
				comingSoonViewUserControl.AddComingSoonTitle(comingSoonTitle);
			}
		}
	}

	public static List<IComingSoonTitle> GetComingSoonTitles()
	{
		return CreateComingSoonTitles(GetComingSoonProductsCallback?.Invoke());
	}

	private static List<IComingSoonTitle> CreateComingSoonTitles(LuaTable products)
	{
		List<IComingSoonTitle> list = new List<IComingSoonTitle>();
		if (products != null)
		{
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
				if (!string.IsNullOrEmpty(text2) && OrchestrationService.DataServiceRegisterImage(text2, text2))
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
		}
		return list;
	}
}
