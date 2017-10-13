using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.USB;

namespace Redbox.KioskEngine.Environment;

public class CardReader
{
	public static bool IsAttached()
	{
		using (HidDeviceManager hidDeviceManager = new HidDeviceManager())
		{
			List<string> cardReaders = GetCardReaders();
			HidDevice[] array = hidDeviceManager.EnumerateDevices();
			foreach (HidDevice hidDevice in array)
			{
				string devicePath = hidDevice.DevicePath.ToLower();
				string s = string.Format("{0},{1}", GetIdFromPath(devicePath, "vid_"), GetIdFromPath(devicePath, "pid_"));
				SHA256Managed sHA256Managed = new SHA256Managed();
				string item = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(s))));
				if (cardReaders.Contains(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static string GetIdFromPath(string devicePath, string name)
	{
		return devicePath.Substring(devicePath.IndexOf(name) + 4, 4);
	}

	private static List<string> GetCardReaders()
	{
		List<string> list = new List<string>();
		using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Redbox\\CRList"))
		{
			if (registryKey != null)
			{
				object value = registryKey.GetValue("List");
				if (value != null)
				{
					string[] array = value.ToString().Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						list.Add((string)registryKey.GetValue(array[i]));
					}
				}
				registryKey.Close();
			}
		}
		if (list.Count == 0)
		{
			LogHelper.Instance.Log("...No card readers have been defined for this kiosk.", LogEntryType.Info);
		}
		return list;
	}
}
