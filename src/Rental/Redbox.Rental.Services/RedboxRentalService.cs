using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class RedboxRentalService : IRedboxRental
{
	public ErrorList Initialize()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			InitializeDesktop();
		}
		catch (Exception ex)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("EngineApplication.InitializeRedboxRental", ex.Message, ""));
		}
		return errorList;
	}

	private void InitializeDesktop()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string defaultCulture = ServiceLocator.Instance.GetService<IStoreManager>().GetDefaultCulture();
		if (!string.IsNullOrEmpty(defaultCulture))
		{
			service[ConfigSetting.DefaultCulture] = defaultCulture;
		}
	}
}
