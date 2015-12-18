using System;
using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel.Views.StartView;

public static class StartViewFunctions
{
	[KernelFunction(Name = "kernel.draw_start_view_banner")]
	internal static void DrawStartViewBanner(Action<string, string> onBannerClicked, bool hide)
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		bool flag = service?.ShowBlurayBrowse ?? false;
		bool flag2 = service?.ShowGameBanner ?? false;
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		string collectionId = null;
		StartViewBannerType bannerType = StartViewBannerType.none;
		Image image = service2.GetBitmap("banner_image");
		if (image != null)
		{
			IResource resource = service2.GetResource("banner_image");
			if (resource != null)
			{
				IAspect aspect = resource.GetAspect("content");
				if (aspect != null)
				{
					collectionId = (aspect.Properties.ContainsKey("collectionid") ? aspect.Properties["collectionid"].ToUpper() : null);
					if (collectionId == string.Empty)
					{
						collectionId = null;
					}
				}
			}
			if (string.IsNullOrEmpty(collectionId) || RentalDataFunctions.RentalIsCollectionsEnabled())
			{
				bannerType = StartViewBannerType.collections;
			}
		}
		if (bannerType == StartViewBannerType.none)
		{
			if (flag)
			{
				bannerType = StartViewBannerType.bluray;
				image = service2.GetBitmap("image_bluray_banner");
			}
			else if (flag2)
			{
				bannerType = StartViewBannerType.game;
				image = service2.GetBitmap("image_game_banner");
			}
			else
			{
				image = null;
			}
		}
		StartViewBanner startViewBanner = new StartViewBanner
		{
			BannerImage = image
		};
		startViewBanner.OnBannerClick += delegate
		{
			if (onBannerClicked != null)
			{
				onBannerClicked($"{bannerType}_banner", collectionId);
			}
		};
		startViewBanner.ClearActors();
		if (!hide)
		{
			startViewBanner.RenderActors();
		}
	}
}
