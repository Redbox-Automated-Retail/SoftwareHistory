using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.USB;

namespace Redbox.KioskEngine.Environment;

public class EnvironmentManager : IEnvironmentNotificationService
{
	private HardwareChangeCallback m_hardwareChangeCallback;

	private readonly IDictionary<string, ConfigurationCallback> m_configurationCallbacks = new Dictionary<string, ConfigurationCallback>();

	public static EnvironmentManager Instance => Singleton<EnvironmentManager>.Instance;

	public bool HasValidCardReader { get; internal set; }

	public bool CheckCardReader
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Core", "CheckCardReader", defaultValue: true) ?? false;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "CheckCardReader", value);
		}
	}

	public int NullKioskId
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Core", "NullKioskId", 1) ?? 1;
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "NullKioskId", value);
		}
	}

	public void Reset()
	{
		try
		{
			TouchScreenFactory.Instance.Initialize();
			LogHelper.Instance.Log("Reset environment manager:");
			GenericTouchScreen touchScreen = TouchScreenFactory.Instance.GetTouchScreen();
			if (touchScreen != null)
			{
				LogHelper.Instance.Log("...Touch screen state is: {0}.", touchScreen.State);
			}
			else
			{
				LogHelper.Instance.Log("...No touch screen instance available.");
			}
			LogHelper.Instance.Log("...Clear configuration callbacks.");
			m_configurationCallbacks.Clear();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.Reset", e);
		}
	}

	public ErrorList Initialize()
	{
		ErrorList result = new ErrorList();
		LogHelper.Instance.Log("Initialize environment manager.");
		ServiceLocator.Instance.AddService(typeof(IEnvironmentNotificationService), Instance);
		if (CheckCardReader)
		{
			LogHelper.Instance.Log("Check for valid credit card reader.");
			if (!CardReader.IsAttached())
			{
				LogHelper.Instance.Log("...No valid credit card reader found.");
				HasValidCardReader = false;
			}
			else
			{
				LogHelper.Instance.Log("...Valid credit card reader found.");
				HasValidCardReader = true;
			}
		}
		else
		{
			HasValidCardReader = true;
		}
		return result;
	}

	public void Register(IntPtr handle)
	{
		if (!CheckCardReader)
		{
			return;
		}
		try
		{
			UsbNotification.RegisterForUsbEvents(handle);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised by UsbNotification.RegisterForUsbEvents.", e);
		}
	}

	public void Unregister(IntPtr handle)
	{
		if (!CheckCardReader)
		{
			return;
		}
		try
		{
			UsbNotification.UnregisterForUsbEvents(handle);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised by UsbNotification.UnregisterForUsbEvents.", e);
		}
	}

	public void NotifyConfigurationCallbacks(string store, string action, string path, string key, string newValue)
	{
		ConfigurationCallback[] array = new ConfigurationCallback[m_configurationCallbacks.Count];
		m_configurationCallbacks.Values.CopyTo(array, 0);
		ConfigurationCallback[] array2 = array;
		foreach (ConfigurationCallback function in array2)
		{
			CallbackService.Instance.EnqueueCallback(new ConfigurationCallbackEntry
			{
				Store = store,
				Function = function,
				Key = key,
				Path = path,
				Action = action,
				NewValue = newValue
			});
		}
	}

	public void RegisterConfigurationCallback(string name, ConfigurationCallback callback)
	{
		m_configurationCallbacks[name] = callback;
	}

	public void UnregisterConfigurationCallback(string name)
	{
		m_configurationCallbacks.Remove(name);
	}

	public void UnregisterHardwareChangeCallback()
	{
		m_hardwareChangeCallback = null;
	}

	public void RegisterHardwareChangeCallback(HardwareChangeCallback callback)
	{
		m_hardwareChangeCallback = callback;
	}

	public EnvironmentNotificationType ProcessNotification(ref Message msg)
	{
		if (CheckCardReader)
		{
			NotificationEventType notificationEventType = UsbNotification.HandleMessage(ref msg);
			if (HasValidCardReader)
			{
				if (notificationEventType == NotificationEventType.DeviceRemoved && !CardReader.IsAttached())
				{
					return EnvironmentNotificationType.CreditCardReaderInvalid;
				}
			}
			else if (notificationEventType == NotificationEventType.DeviceArrived && CardReader.IsAttached())
			{
				return EnvironmentNotificationType.CreditCardReaderValid;
			}
		}
		return EnvironmentNotificationType.None;
	}

	public void ToggleTouchScreen()
	{
		TouchScreenFactory.Instance.GetTouchScreen()?.Toggle();
	}

	public TouchScreenState TouchScreenStatus()
	{
		return TouchScreenFactory.Instance.GetTouchScreen()?.State ?? TouchScreenState.Unavailable;
	}

	public string TouchScreenDriver()
	{
		return TouchScreenFactory.Instance.GetTouchScreen()?.Driver;
	}

	public void LaunchSecureBrowser(IDictionary<string, string> approvedUrls, NavigateUrlCallback navigateUrlCallback)
	{
		SecureBrowser secureBrowser = new SecureBrowser(approvedUrls, navigateUrlCallback);
		secureBrowser.Show();
	}

	public void RaiseValidReader()
	{
		try
		{
			HasValidCardReader = true;
			if (m_hardwareChangeCallback != null)
			{
				m_hardwareChangeCallback("Card Reader", "Connected", "High");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseValidReader.", e);
		}
	}

	public void RaiseInvalidReader()
	{
		try
		{
			HasValidCardReader = false;
			if (m_hardwareChangeCallback != null)
			{
				m_hardwareChangeCallback("Card Reader", "Disconnected", "High");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EnvironmentManager.RaiseInvalidReader.", e);
		}
	}

	private EnvironmentManager()
	{
	}
}
