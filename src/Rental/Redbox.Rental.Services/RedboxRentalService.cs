using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;
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
			RegisterTTSWithRentalShoppingCartService();
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

	private void RegisterTTSWithRentalShoppingCartService()
	{
		ServiceLocator.Instance.GetService<IRentalShoppingCartService>().OnRentalShoppingCartAdded += delegate(IRentalShoppingCart rentalShoppingCart)
		{
			if (rentalShoppingCart != null)
			{
				rentalShoppingCart.OnChange += RespondToRentalShoppingCartChange;
				RespondToRentalShoppingCartChange(rentalShoppingCart);
			}
		};
	}

	private void RespondToRentalShoppingCartChange(IRentalShoppingCart rentalShoppingCart)
	{
		IDataService dataService = ServiceLocator.Instance.GetService(typeof(IDataService)) as IDataService;
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
		if (!service.TTSEnabled)
		{
			return;
		}
		IList<IRentalShoppingCartItem> list = rentalShoppingCart.Items;
		if (list == null)
		{
			list = new List<IRentalShoppingCartItem>();
		}
		for (int i = 1; i <= rentalShoppingCart.MaximumCapacity; i++)
		{
			string text = null;
			IRentalShoppingCartItem rentalShoppingCartItem = ((i <= list.Count) ? list[i - 1] : null);
			if (rentalShoppingCartItem != null)
			{
				dataService.GetProduct(rentalShoppingCartItem.ProductId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct != null && kioskProduct is ITitleProduct)
				{
					text = ((ITitleProduct)kioskProduct).LongName;
				}
			}
			string macroName = $"TTS:CartTitle_{i}_Name";
			string macroValue = text;
			SetAndLogMacroValue(service2, macroName, macroValue);
			macroName = $"TTS:CartTitle_{i}_Action";
			macroValue = ((rentalShoppingCartItem != null) ? rentalShoppingCartItem.Action.ToString() : string.Empty);
			SetAndLogMacroValue(service2, macroName, macroValue);
		}
		service2["TTS:NumberOfTitlesInCart"] = ((list.Count > 0) ? list.Count.ToString() : string.Empty);
		service2["TTS:CartTitlePlurality"] = ((list.Count == 1) ? "Title" : "Titles");
		service.TTSRepeatSequence();
	}

	private void SetAndLogMacroValue(IMacroService macroService, string macroName, string macroValue)
	{
		LogHelper.Instance.Log(string.Format("{0} = {1}", macroName, string.IsNullOrEmpty(macroValue) ? "empty" : macroValue));
		macroService[macroName] = macroValue;
	}
}
