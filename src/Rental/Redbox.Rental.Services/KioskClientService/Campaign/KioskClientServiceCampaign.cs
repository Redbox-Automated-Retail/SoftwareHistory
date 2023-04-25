using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Campaign;

namespace Redbox.Rental.Services.KioskClientService.Campaign;

public class KioskClientServiceCampaign : BaseKioskClientService, IKioskClientServiceCampaign
{
	private const string _controllerPrefix = "api/campaign";

	private IConfiguration _configuration;

	private KioskInCartDetails _inCart;

	private KioskStartScreenDetails _startScreen;

	public bool IsKCSInCartEnabled => _configuration?.UseKCSInCart ?? false;

	public bool IsKCSStartScreenEnabled => _configuration?.UseKCSStartScreen ?? false;

	public int KCSLoadCampaignTimerDuration => _configuration?.KCSLoadCampaignTimerDuration ?? 1800000;

	public KioskClientServiceCampaign()
	{
		_configuration = ServiceLocator.Instance.GetService<IConfiguration>();
		LoadCampaign();
	}

	public void LoadCampaign()
	{
		LogHelper.Instance.Log("KCS.Campaign.KioskClientServiceCampaign.LoadCampaign");
		LogHelper.Instance.Log($"IsKCSInCartEnabled {IsKCSInCartEnabled}");
		_inCart = (IsKCSInCartEnabled ? GetKCSInCart() : null);
		LogHelper.Instance.Log(string.Format("InCart {0}", (_inCart != null) ? "loaded" : "null"));
		PromotionService.Instance?.LoadInCartData(_inCart);
		LogHelper.Instance.Log($"IsKCSStartScreenEnabled {IsKCSStartScreenEnabled}");
		_startScreen = (IsKCSStartScreenEnabled ? GetKCSStartScreen() : null);
		LogHelper instance = LogHelper.Instance;
		KioskStartScreenDetails startScreen = _startScreen;
		object arg;
		if (startScreen != null)
		{
			ControlList controls = startScreen.Controls;
			if (controls != null)
			{
				_ = controls.Count;
				if (true)
				{
					arg = $"loaded {_startScreen.Controls.Count}";
					goto IL_011f;
				}
			}
		}
		arg = "null";
		goto IL_011f;
		IL_011f:
		instance.Log($"StartScreen {arg}");
	}

	public KioskInCartDetails GetInCart()
	{
		return _inCart;
	}

	public KioskStartScreenDetails GetStartScreen()
	{
		return _startScreen;
	}

	public ICarouselControl GetCarouselControl()
	{
		return _startScreen?.Controls?.Where((IControl c) => c.ControlType == ControlType.Carousel && c is ICarouselControl).Select((IControl c) => c as ICarouselControl).FirstOrDefault();
	}

	public IList<IStartAssetControl> GetStartAssetControls()
	{
		return _startScreen?.Controls?.Where((IControl c) => c.ControlType == ControlType.StartAsset && c is IStartAssetControl).Select((IControl c) => c as IStartAssetControl).OrderBy((IStartAssetControl sac) => sac.Order)
			.ToList();
	}

	private KioskInCartDetails GetKCSInCart()
	{
		try
		{
			WebResult<GetInCartsResponse> webResult = ExecuteWebRequest<GetInCartsResponse>(new Uri(string.Format("{0}/{1}/incart", base.KioskClientServiceBaseUrl, "api/campaign")), null, "GET", 500);
			return (webResult == null || webResult.StatusCode != HttpStatusCode.OK) ? null : webResult?.Response?.InCart;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KCS.Campaign.KioskClientServiceCampaign.GetKCSInCart", e);
			return null;
		}
	}

	private KioskStartScreenDetails GetKCSStartScreen()
	{
		try
		{
			WebResult<GetStartScreenResponse> webResult = ExecuteWebRequest<GetStartScreenResponse>(new Uri(string.Format("{0}/{1}/startscreen", base.KioskClientServiceBaseUrl, "api/campaign")), null, "GET", 500);
			return (webResult == null || webResult.StatusCode != HttpStatusCode.OK) ? null : webResult?.Response?.StartScreen;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("An unhandled exception was raised in KCS.Campaign.KioskClientServiceCampaign.GetKCSStartScreen", e);
			return null;
		}
	}
}
