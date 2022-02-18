using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Ads;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskClientService.Ads;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class VsmViewController : BaseViewController
{
	private static IAd _cachedAd;

	private static Action<string> _updateVsmImpressionCallback;

	public static void ConfigureVsmView()
	{
		if (OrchestrationService.InvokeIsVendJobCompletedCallback())
		{
			OrchestrationService.ViewGoBack("VSM view  vend job completed.", popOnly: false);
			return;
		}
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			OrchestrationService.SetCurrentUICulture();
			VsmViewModel vsmViewModel = new VsmViewModel
			{
				TitleText = Resources.vsm_title_text
			};
			GetAdImage(vsmViewModel);
			vsmViewModel.ProcessGetSpeechControl = delegate
			{
				SpeechControl speechControl = new SpeechControl();
				SpeechPart item = new SpeechPart
				{
					Texts = { (ITextPart)new TextPart
					{
						Text = vsmViewModel?.TitleText
					} }
				};
				((ISpeechControl)speechControl).SpeechParts.Add(item);
				return speechControl;
			};
			viewUserControl.DataContext = vsmViewModel;
			vsmViewModel?.AdImpression?.Start();
		}
	}

	private static void GetAdImage(VsmViewModel vsmViewModel)
	{
		MeijerVsmKiosk meijerVsmKiosk = null;
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.EnableMeijerVSM)
		{
			meijerVsmKiosk = GetMeijerVsmKiosk();
		}
		if (meijerVsmKiosk != null && meijerVsmKiosk.NearestKiosk != null)
		{
			vsmViewModel.MinimumDisplayTime = 7000;
			vsmViewModel.AdImage = null;
			vsmViewModel.HeaderHeight = 192f;
			vsmViewModel.AdImpression = null;
			vsmViewModel.ClosingLocationGridVisibility = Visibility.Visible;
			vsmViewModel.ClosingLocationDateText = $"This location is\nclosing in {meijerVsmKiosk.DateText}";
			vsmViewModel.ClosingLocationText = "Keep renting at your nearest location:";
			vsmViewModel.ClosingLocationBannerVisibility = Visibility.Visible;
			vsmViewModel.ClosingLocationBannerText = meijerVsmKiosk.NearestKiosk.Banner;
			vsmViewModel.ClosingLocationAddressVisibility = Visibility.Visible;
			vsmViewModel.ClosingLocationAddressText = meijerVsmKiosk.NearestKiosk.Address;
			vsmViewModel.ClosingLocationCityVisibility = Visibility.Visible;
			vsmViewModel.ClosingLocationCityText = meijerVsmKiosk.NearestKiosk.City;
			vsmViewModel.ClosingLocationNoteVisibility = Visibility.Visible;
			return;
		}
		if (meijerVsmKiosk != null && meijerVsmKiosk.NearestKiosk == null)
		{
			vsmViewModel.MinimumDisplayTime = 7000;
			vsmViewModel.AdImage = null;
			vsmViewModel.HeaderHeight = 192f;
			vsmViewModel.AdImpression = null;
			vsmViewModel.ClosingLocationGridVisibility = Visibility.Visible;
			vsmViewModel.ClosingLocationDateText = $"This location is\nclosing in {meijerVsmKiosk.DateText}";
			vsmViewModel.ClosingLocationText = "Visit redbox.com to stream movies On Demand\nand watch Free Movies & Live TV!";
			return;
		}
		IAd ad = null;
		if (_cachedAd != null)
		{
			ad = _cachedAd;
			_cachedAd = null;
		}
		else
		{
			ad = BaseViewController.GetAd(AdLocation.VSM);
		}
		bool flag = ad is IVistarAd;
		vsmViewModel.MinimumDisplayTime = ((ad != null && ad.MinimumDisplay.HasValue) ? ad.MinimumDisplay.Value : 3000);
		vsmViewModel.AdImage = ad?.Image;
		vsmViewModel.HeaderHeight = (flag ? 192 : 136);
		vsmViewModel.AdImpression = ((ad != null) ? new AdImpression
		{
			Ad = ad,
			AdLocation = AdLocation.VSM
		} : null);
		if (ad is MarketingAd)
		{
			UpdateVSMImpression();
		}
	}

	public static void LeaveVsmView(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance == null)
		{
			return;
		}
		VsmViewModel viewModel = BaseViewController.GetViewModel<VsmViewModel>(viewFrameInstance.Id);
		if (viewModel == null)
		{
			return;
		}
		IAdImpression adImpression = viewModel.AdImpression;
		if (adImpression == null)
		{
			return;
		}
		IAd ad = adImpression.Ad;
		if (ad == null || !ad.MinimumDisplay.HasValue)
		{
			return;
		}
		try
		{
			int num = viewModel.AdImpression.Ad.MinimumDisplay.Value - viewModel.AdImpression.DisplayMilliseconds;
			if (num > 0)
			{
				ServiceLocator.Instance.GetService<ITimerService>()?.Wait(num);
			}
			BaseViewController.RecordAdImpression(viewModel.AdImpression);
			LogHelper.Instance.Log($"Leaving vsm_view.  Display duration: {viewModel.AdImpression.DisplayMilliseconds}");
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Exception in VsmViewController.LeaveVsmView. {arg}");
		}
	}

	public static void RegisterUpdateVsmImpressionCallback(Action<string> updateVsmImpressionCallback)
	{
		_updateVsmImpressionCallback = updateVsmImpressionCallback;
	}

	public static void InvokeUpdateVsmImpressionCallback(string vsmProgramName)
	{
		_updateVsmImpressionCallback?.Invoke(vsmProgramName);
	}

	public static bool IsVsmAdAvailable()
	{
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.EnableMeijerVSM && GetMeijerVsmKiosk() != null)
		{
			return true;
		}
		_cachedAd = BaseViewController.GetAd(AdLocation.VSM);
		return _cachedAd != null;
	}

	public static string GetVsmProgram()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>().GetResource("vsm_image")?["program_name"]?.ToString();
	}

	public static void UpdateVSMImpression()
	{
		string vsmProgram = GetVsmProgram();
		if (!string.IsNullOrEmpty(vsmProgram))
		{
			InvokeUpdateVsmImpressionCallback(vsmProgram);
		}
	}

	private static MeijerVsmKiosk GetMeijerVsmKiosk()
	{
		return MeijerVsmKioskCollection.GetMeijerVsmKiosk(ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId ?? 0);
	}
}
