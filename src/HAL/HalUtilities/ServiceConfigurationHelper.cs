using System;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities;

internal sealed class ServiceConfigurationHelper : IDisposable
{
	private readonly ClientHelper Helper;

	public void Dispose()
	{
	}

	internal void BackupConfiguration(string kioskID)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = service.RuntimePath("backup-log.txt");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		string text = service.RuntimePath(kioskID);
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
			}
			catch
			{
				streamWriter.WriteLine("Unable to create backup directory.");
				return;
			}
		}
		try
		{
			File.Copy("c:\\gamp\\SystemData.dat", Path.Combine(text, "SystemData.dat"), overwrite: true);
			File.Copy("c:\\gamp\\SlotData.dat", Path.Combine(text, "SlotData.dat"), overwrite: true);
			File.Copy("c:\\Program Files\\Redbox\\HALService\\bin\\HAL.xml", Path.Combine(text, "HAL.xml"), overwrite: true);
		}
		catch
		{
			streamWriter.WriteLine("Error backing up file.");
		}
	}

	internal bool UpdateOption(string updateConfigurationString)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		char c = ':';
		int num = updateConfigurationString.IndexOf('|');
		if (-1 != num)
		{
			c = '|';
		}
		string[] array = updateConfigurationString.Split(c);
		if (array.Length < 3)
		{
			Console.WriteLine("Unable to parse option {0}", updateConfigurationString);
			return false;
		}
		string path = service.RuntimePath("ConfigurationUpdateError.log");
		string text = array[0];
		string arg = array[1];
		string text2 = array[2];
		bool flag = array.Length == 4;
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
			}
			catch
			{
			}
		}
		HardwareService hardwareService = Helper.AutoConnect();
		if (hardwareService == null)
		{
			using (StreamWriter streamWriter = new StreamWriter(path))
			{
				streamWriter.WriteLine("Unable to talk to HAL.");
			}
			return false;
		}
		HardwareCommandResult hardwareCommandResult = hardwareService.ExecuteImmediate($"SETCFG \"{arg}\" \"{text2}\" TYPE={text}", out var job);
		if (!hardwareCommandResult.Success)
		{
			using (StreamWriter streamWriter2 = new StreamWriter(path))
			{
				streamWriter2.WriteLine("The configuration update failed.");
				foreach (Error error in hardwareCommandResult.Errors)
				{
					streamWriter2.WriteLine(error.ToString());
				}
			}
			return false;
		}
		if (!flag)
		{
			return true;
		}
		hardwareCommandResult = hardwareService.ExecuteImmediate($"GETCFG \"{arg}\" TYPE={text}", out job);
		string path2 = service.RuntimePath("ConfigurationUpdateValidateError.log");
		FileHelper.SafeDeleteFile(path2);
		bool result = false;
		using (StreamWriter streamWriter3 = new StreamWriter(path2))
		{
			if (!hardwareCommandResult.Success)
			{
				streamWriter3.WriteLine("The configuration update failed.");
				foreach (Error error2 in hardwareCommandResult.Errors)
				{
					streamWriter3.WriteLine(error2.ToString());
				}
				return false;
			}
			HardwareCommandResult stack = job.GetStack(out var stack2);
			if (!stack.Success)
			{
				streamWriter3.WriteLine("Couldn't validate: unable to get stack.");
			}
			else
			{
				string text3 = stack2.Pop();
				if (!text3.Equals(text2, StringComparison.CurrentCultureIgnoreCase))
				{
					streamWriter3.WriteLine("The new value {0} doesn't match {1}", text3, text2);
				}
				else
				{
					result = true;
				}
			}
		}
		return result;
	}

	internal ServiceConfigurationHelper(ClientHelper helper)
	{
		Helper = helper;
	}
}
