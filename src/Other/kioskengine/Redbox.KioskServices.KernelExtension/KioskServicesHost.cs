using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Kernel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class KioskServicesHost : IKernelExtensionHost, IDisposable
{
	public IKernelExtension Extension { get; set; }

	public void Dispose()
	{
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool flag = InventoryServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> InventoryService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH666", "Bundle Switch Error", "Can not switch while inventory service is performing work."));
			return false;
		}
		flag = MerchantServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> MerchantService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH667", "Bundle Switch Error", "Can not switch while merchant service is performing work."));
			return false;
		}
		flag = StoreServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> StoreService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH666", "Bundle Switch Error", "Can not switch while store service is performing work."));
			return false;
		}
		flag = PromotionServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> PromotionService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH665", "Bundle Switch Error", "Can not switch while promotion service is performing work."));
			return false;
		}
		flag = HealthServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> HealthService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH664", "Bundle Switch Error", "Can not switch while health service is performing work."));
			return false;
		}
		flag = AuthenticationServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> AuthenticationService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH663", "Bundle Switch Error", "Can not switch while authentication service is performing work."));
			return false;
		}
		flag = DigitalServicesProxy.Instance.CanSwitch();
		LogHelper.Instance.Log("KioskService -> DigitalService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("KSH662", "Bundle Switch Error", "Can not switch while digital service is performing work."));
			return false;
		}
		LogHelper.Instance.Log("KioskService CanSwitch: True");
		return true;
	}

	public ErrorList PreLoad()
	{
		return new ErrorList();
	}

	public ErrorList Load()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Load.");
		ServiceLocator.Instance.GetService<IObjectCacheService>()?.SetObject("Kiosk Services Proxy", new ServiceProxy());
		ServiceLocator.Instance.AddService(typeof(IStoreServices), StoreServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IHealthServices), HealthServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IMerchantServices), MerchantServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IInventoryServices), InventoryServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IPromotionServices), PromotionServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IAuthenticationServices), AuthenticationServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IDigitalServices), DigitalServicesProxy.Instance);
		ServiceLocator.Instance.AddService(typeof(IMarketingOptinServices), MarketingOptinServicesProxy.Instance);
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("KioskServices_General", "Store", "Kiosk Services/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
			service.AddPreferencePage("KioskServices_AuthenticationServices", "Remote Services\\Authentication Services", "Kiosk Services/Authentication Services", PreferencePageTarget.LocalSystem, new AuthSvcsPreferencePage());
			service.AddPreferencePage("KioskServices_HealthServices", "Remote Services\\Health Services", "Kiosk Services/Health Services", PreferencePageTarget.LocalSystem, new HealthServicesPreferencePage());
			service.AddPreferencePage("KioskServices_InventoryServices", "Remote Services\\Inventory Services", "Kiosk Services/Inventory Services", PreferencePageTarget.LocalSystem, new InventoryServicesPreferencePage());
			service.AddPreferencePage("KioskServices_MerchantServices", "Remote Services\\Merchant Services", "Kiosk Services/Merchant Services", PreferencePageTarget.LocalSystem, new MerchantServicesPreferencePage());
			service.AddPreferencePage("KioskServices_PromotionServices", "Remote Services\\Promotion Services", "Kiosk Services/Promotion Services", PreferencePageTarget.LocalSystem, new PromotionServicesPreferencePage());
			service.AddPreferencePage("KoiskServices_DigitalServices", "Remote Services\\Digital Services", "Kiosk Services/Digital Services", PreferencePageTarget.LocalSystem, new DigitalServicesPreferencePage());
			service.AddPreferencePage("KoiskServices_MarketingOptinServices", "Remote Services\\Marketing Optin Services", "Kiosk Services/Marketing Optin Services", PreferencePageTarget.LocalSystem, new MarketingOptinPreferencesPage());
			service.AddPreferencePage("KoiskServices_ReservationAuthServices", "Remote Services\\Reservation Auth Services", "Kiosk Services/Reservation Auth Services", PreferencePageTarget.LocalSystem, new ReservationAuthPreferencePage());
			service.AddPreferencePage("KioskServices_GiftCardAuth", "Remote Services\\Merchant Services", "Kiosk Services/Gift Card Auth", PreferencePageTarget.LocalSystem, new GiftCardAuthPreferencePage());
		}
		return result;
	}

	public ErrorList Reset()
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Reset:");
		ErrorList result = new ErrorList();
		IKernelFunctionRegistryService service = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		if (service != null)
		{
			LogHelper.Instance.Log("...Register Kiosk Services kernel functions.");
			service.RegisterKernelFunctions(typeof(IRemoteServiceResultExtensions).Assembly, "Kiosk Services");
		}
		LogHelper.Instance.Log("...Reset Kiosk Services Proxy.");
		AuthenticationServicesProxy.Instance.Reset();
		PromotionServicesProxy.Instance.Reset();
		DigitalServicesProxy.Instance.Reset();
		InventoryServicesProxy.Instance.Reset();
		return result;
	}

	public ErrorList Activate()
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Activate:");
		ErrorList result = new ErrorList();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.ProcessMessage -= OnProcessMessage;
			service.ProcessMessage += OnProcessMessage;
		}
		HealthServicesProxy.Instance.StartPingTimer();
		return result;
	}

	public ErrorList Deactivate()
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Deactivate:");
		ErrorList result = new ErrorList();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			service.ProcessMessage -= OnProcessMessage;
		}
		InventoryServicesProxy.Instance.Deactivate();
		HealthServicesProxy.Instance.StopPingTimer();
		return result;
	}

	public void HandleHostCrash(Exception e)
	{
		LogHelper.Instance.Log("Kiosk Services Kernel Extension Host Crash:", e);
		HealthServicesProxy.Instance.SendAlert(StoreServicesProxy.Instance.StoreNumber, "ApplicationCrash", null, e.ToString(), DateTime.Now, null);
	}

	private static bool OnProcessMessage(IMessage message)
	{
		if (!HealthServicesProxy.Instance.IsOnline || HealthServicesProxy.Instance.IsServerBusy || StoreServicesProxy.Instance.IsStandAlone)
		{
			return false;
		}
		try
		{
			string jsonRequest = Encoding.ASCII.GetString(message.Data);
			IDictionary<string, object> dictionary = KioskServicesProxy.Instance.ExecuteMessage(jsonRequest);
			return dictionary.ContainsKey("Success") && (bool)dictionary["Success"];
		}
		catch
		{
			return false;
		}
	}
}
