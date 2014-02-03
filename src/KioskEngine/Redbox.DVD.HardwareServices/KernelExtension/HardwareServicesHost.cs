using System;
using Redbox.Core;
using Redbox.HardwareServices.Kernel;
using Redbox.HardwareServices.Proxy;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class HardwareServicesHost : IKernelExtensionHost, IDisposable
{
	public IKernelExtension Extension { get; set; }

	public void Dispose()
	{
	}

	public bool CanSwitch(out ErrorList errors)
	{
		errors = new ErrorList();
		bool flag = HardwareService.Instance.CanSwitch();
		LogHelper.Instance.Log("HardwareService CanSwitch: {0}", flag);
		if (!flag)
		{
			errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E246", "Bundle Switch Error", "Can not switch bundles while a return is running, try again after the return has finished."));
			return false;
		}
		return true;
	}

	public ErrorList PreLoad()
	{
		return new ErrorList();
	}

	public ErrorList Load()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Load:");
		ErrorList errorList = new ErrorList();
		errorList.AddRange(HardwareService.Instance.Initialize());
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("HardwareServices_General", "Local Services\\Hardware Services", "Hardware Services/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
			service.AddPreferencePage("HardwareServices_Mock", "Local Services\\Hardware Services", "Hardware Services/Mock Scripts", PreferencePageTarget.LocalSystem, new MockPreferencePage());
		}
		LogHelper.Instance.Log("...Loaded.");
		return errorList;
	}

	public ErrorList Reset()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Reset:");
		ErrorList result = new ErrorList();
		HardwareService.Instance.Reset();
		ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>()?.RegisterKernelFunctions(typeof(HardwareFunctions).Assembly, "Hardware Services");
		ServiceLocator.Instance.GetService<IFileCacheService>()?.Remove("*.hs");
		ErrorList errorList = HardwareService.Instance.CompileScriptResources();
		if (errorList.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm();
			errorForm.Errors = errorList;
			errorForm.Text = "HAL Script Compilation Errors";
			errorForm.Message = "One or more HAL Script resources failed to compile:";
			ErrorForm errorForm2 = errorForm;
			errorForm2.Show();
		}
		return result;
	}

	public ErrorList Activate()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Activate:");
		return new ErrorList();
	}

	public ErrorList Deactivate()
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Deactivate:");
		return new ErrorList();
	}

	public void HandleHostCrash(Exception e)
	{
		LogHelper.Instance.Log("Hardware Services Kernel Extension Host Crash.");
	}
}
