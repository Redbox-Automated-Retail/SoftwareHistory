using System;
using Microsoft.Win32;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataStorage;

internal sealed class RegistryStore : IPersistentMap
{
	private readonly string RegKey;

	public T GetValue<T>(string option, T def)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(RegKey);
			if (registryKey == null)
			{
				return def;
			}
			object value = registryKey.GetValue(option);
			if (value == null)
			{
				return def;
			}
			return (T)ConversionHelper.ChangeType(value, typeof(T));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to read secret option {option}", e);
			return def;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public void SetValue<T>(string option, T value)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.CreateSubKey(RegKey);
			registryKey?.SetValue(option, value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to set secret option {option}", e);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	public void Remove(string option)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey(RegKey, writable: true);
			registryKey?.DeleteValue(option);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to delete secret option {option}", e);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	internal RegistryStore(string key)
	{
		RegKey = key;
	}
}
