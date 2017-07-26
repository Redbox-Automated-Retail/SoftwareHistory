using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Session;

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
		ITextToSpeechService service2 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service2 != null)
		{
			service2.OnAudioDeviceConnectionChanged += delegate(ITextToSpeechService textToSpeachService, bool isConnected)
			{
				if (Redbox.Rental.Services.Session.Session.Instance != null && Redbox.Rental.Services.Session.Session.Instance.BrowseState != null)
				{
					ApplicationState.Instance.IsADAAccessible = isConnected;
				}
			};
		}
		IThemeService service3 = ServiceLocator.Instance.GetService<IThemeService>();
		if (service3 != null)
		{
			service3.RegisterThemedControls("Redbox.Rental.UI.exe");
			ITheme theme = service3.Themes.Find((ITheme x) => x.Name == "Redbox Classic");
			if (theme != null)
			{
				theme.IsDefault = true;
				LogHelper.Instance.Log($"Set default theme to {theme.Name}", LogEntryType.Info);
				service3.CurrentTheme = theme;
				LogHelper.Instance.Log($"Set current theme to {theme.Name}", LogEntryType.Info);
			}
			else
			{
				LogHelper.Instance.Log("Unable to set default theme to Redbox Classic", LogEntryType.Info);
			}
		}
	}
}
