using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class SettingsFunctions
{
	[KernelFunction(Name = "kernel.getuservalue")]
	internal static string GetUserSettingValue(string path, string key)
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		return service.GetValue<string>(path, key);
	}

	[KernelFunction(Name = "kernel.getuservalueordefault")]
	internal static string GetUserSettingValue(string path, string key, string defaultValue)
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		return service.GetValue(path, key, defaultValue);
	}

	[KernelFunction(Name = "kernel.setuservalue")]
	internal static void SetUserSettingValue(string path, string key, string value)
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		service.SetValue(path, key, value);
	}

	[KernelFunction(Name = "kernel.removeuserkey")]
	internal static void RemoveUserSettingKey(string path)
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		service.RemoveKey(path);
	}

	[KernelFunction(Name = "kernel.removeuservalue")]
	internal static void RemoveUserSettingValue(string path, string key)
	{
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		service.RemoveValue(path, key);
	}

	[KernelFunction(Name = "kernel.getmachinevalue")]
	internal static string GetMachineSettingValue(string path, string key)
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		return service.GetValue<string>(path, key);
	}

	[KernelFunction(Name = "kernel.getmachinevalueordefault")]
	internal static string GetMachineSettingValue(string path, string key, string defaultValue)
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		return service.GetValue(path, key, defaultValue);
	}

	[KernelFunction(Name = "kernel.setmachinevalue")]
	internal static void SetMachineSettingValue(string path, string key, string value)
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		service.SetValue(path, key, value);
	}

	[KernelFunction(Name = "kernel.removemachinekey")]
	internal static void RemoveMachineSettingKey(string path)
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		service.RemoveKey(path);
	}

	[KernelFunction(Name = "kernel.removemachinevalue")]
	internal static void RemoveMachineSettingValue(string path, string key)
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		service.RemoveValue(path, key);
	}

	[KernelFunction(Name = "kernel.registerconfigurationcallback")]
	internal static void RegisterConfigurationCallback(string name, ConfigurationCallback callback)
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		service.RegisterConfigurationCallback(name, callback);
	}

	[KernelFunction(Name = "kernel.removeconfigurationcallback")]
	internal static void RemoveConfigurationCallback(string name)
	{
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		service.UnregisterConfigurationCallback(name);
	}
}
