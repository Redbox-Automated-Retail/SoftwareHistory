using System.Drawing;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Model.Marketing;

public class MarketingAd : IAd
{
	public string ImageName { get; set; }

	public int? MinimumDisplay { get; set; }

	public BitmapImage Image
	{
		get
		{
			Image image = ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetBitmap(ImageName);
			return ServiceLocator.Instance.GetService<IBitmapCacheService>()?.ToBitmapImage(image);
		}
	}

	public MarketingAdCampaign Campaign { get; set; }
}
