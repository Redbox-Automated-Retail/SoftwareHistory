using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services;

public class ApplicationState : IApplicationState
{
	private string _inCartTemplateName;

	private bool _isADAAccessible;

	private static ApplicationState _applicationState;

	public static ApplicationState Instance
	{
		get
		{
			if (_applicationState == null)
			{
				_applicationState = new ApplicationState();
				ServiceLocator.Instance.AddService(typeof(IApplicationState), _applicationState);
			}
			return _applicationState;
		}
	}

	public bool IsADAAccessible
	{
		get
		{
			return _isADAAccessible;
		}
		set
		{
			_isADAAccessible = value;
		}
	}

	public bool KeepSessionOnStartViewEnter { get; set; }

	public bool IsABEOn
	{
		get
		{
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (!IsADAAccessible)
			{
				return service?.TTSEnabled ?? false;
			}
			return true;
		}
	}

	public string InCartTemplateName
	{
		get
		{
			return _inCartTemplateName;
		}
		set
		{
			_inCartTemplateName = value;
		}
	}

	public bool VerboseLogging
	{
		get
		{
			string value = ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Store", "VerboseLogging", "False");
			bool result = false;
			bool.TryParse(value, out result);
			return result;
		}
	}

	public List<IKioskProduct> BrowseProducts { get; set; }
}
