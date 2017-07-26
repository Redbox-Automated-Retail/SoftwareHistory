using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class ApplicationState : IApplicationState
{
	private bool _isADAAccessible;

	private static ApplicationState _applicationState;

	public static ApplicationState Instance
	{
		get
		{
			if (_applicationState == null)
			{
				_applicationState = new ApplicationState();
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
}
