using System;
using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.StartView;

public class StartViewBanner
{
	private string BannerImageActorName = "marketing_banner";

	private string TopLineActorName = "offer_horz_line_top";

	private string BottomLineActorName = "offer_horz_line_bottom";

	public Image BannerImage { get; set; }

	public event EventHandler OnBannerClick;

	public void RenderActors()
	{
		RenderImage();
		RenderLines();
	}

	public void ClearActors()
	{
		ActorFunctions.RemoveActor(BannerImageActorName);
		ActorFunctions.RemoveActor(TopLineActorName);
		ActorFunctions.RemoveActor(BottomLineActorName);
	}

	private void RenderLines()
	{
		if (BannerImage != null)
		{
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			IActor actor = ActorFunctions.CreateActor(TopLineActorName, addToFront: false, 328, 0, 696, 3, null, null, null, null, null, null, null, null, null, "Image", null);
			if (service != null)
			{
				actor.Image = service.GetBitmap("image_kiosk_2_offer_horz_line");
			}
			actor = ActorFunctions.CreateActor(BottomLineActorName, addToFront: false, 328, 117, 696, 3, null, null, null, null, null, null, null, null, null, "Image", null);
			if (service != null)
			{
				actor.Image = service.GetBitmap("image_kiosk_2_offer_horz_line");
			}
		}
	}

	private void RenderImage()
	{
		if (BannerImage != null)
		{
			IActor actor = ActorFunctions.CreateActor(BannerImageActorName, addToFront: false, 328, 0, 695, 120, null, null, null, null, null, null, null, null, null, "Image", "Enabled");
			actor.Image = BannerImage;
			actor.Hit += OnClick;
		}
	}

	private void OnClick(object sender, EventArgs e)
	{
		if (OnBannerClick != null)
		{
			OnBannerClick(this, null);
		}
	}
}
