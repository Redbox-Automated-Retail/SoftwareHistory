using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Win32;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.PackageInstaller;

internal sealed class PackageInstaller : IDisposable
{
	private const string KioskLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs";

	private bool Disposed;

	private readonly string ServiceInstallerBatchFile;

	private readonly TextWriter Writer;

	private readonly string InstallerLog;

	private readonly int MachineConfiguration;

	private readonly string InstallLogsDirectory;

	private readonly string ConfigurationFile;

	private readonly string QRStateFile;

	private readonly string DeprecatedQRStateFile;

	private readonly ErrorList InstallErrors = new ErrorList();

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Writer.Dispose();
		}
	}

	private PackageInstaller()
	{
		ConfigurationFile = RuntimeService.Instance.RuntimePath("hal.xml");
		QRStateFile = RuntimeService.Instance.RuntimePath("qr.state");
		DeprecatedQRStateFile = RuntimeService.Instance.RuntimePath("qr.defs");
		string fullPath = Path.GetFullPath(Path.Combine(RuntimeService.Instance.AssemblyDirectory, ".."));
		InstallLogsDirectory = Path.Combine(fullPath, "InstallLogs");
		if (!Directory.Exists(InstallLogsDirectory))
		{
			try
			{
				Directory.CreateDirectory(InstallLogsDirectory);
			}
			catch
			{
			}
		}
		InstallerLog = Path.Combine(InstallLogsDirectory, "Package-installer.log");
		try
		{
			Writer = new StreamWriter(InstallerLog, append: true);
		}
		catch
		{
			Writer = StreamWriter.Null;
		}
		ServiceInstallerBatchFile = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "register-service.bat");
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
			if (registryKey == null)
			{
				MachineConfiguration = -1;
				return;
			}
			object value = registryKey.GetValue("MaxSlots");
			MachineConfiguration = ((int?)Convert.ChangeType(value, typeof(int))) ?? (-1);
		}
		catch
		{
			MachineConfiguration = -1;
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private void CreateDirectories()
	{
		try
		{
			if (!Directory.Exists("C:\\Program Files\\Redbox\\KioskLogs"))
			{
				Directory.CreateDirectory("C:\\Program Files\\Redbox\\KioskLogs");
			}
		}
		catch (Exception ex)
		{
			Writer.WriteLine("Unable to create directory: {0}", ex.Message);
		}
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
		catch (Exception ex2)
		{
			Writer.WriteLine("Unable to create data directory: {0}", ex2.Message);
		}
		string path2 = "c:\\Program Files\\Redbox\\HALService\\Video";
		try
		{
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
			}
		}
		catch (Exception ex3)
		{
			Writer.WriteLine("Unable to create images directory: {0}", ex3.Message);
		}
	}

	private void InstallDatabase(string fileName)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"data\\{fileName}");
		if (File.Exists(text))
		{
			Writer.WriteLine("The database {0} already exists - take no further action.", fileName);
			return;
		}
		string text2 = RuntimeService.Instance.RuntimePath($"{fileName}.default");
		string arg = $"copy {text2} -> {text}";
		try
		{
			File.Copy(text2, text);
			Writer.WriteLine("{0}: SUCCESS", arg);
		}
		catch (Exception ex)
		{
			InstallErrors.Add(Error.NewError("I001", "Copy failure", $"{arg}: FAILURE reason = {ex.Message}"));
		}
	}

	private bool InstallService()
	{
		bool flag = true;
		RegistryKey registryKey = null;
		try
		{
			string name = "SYSTEM\\CurrentControlSet\\Services\\halsvc$default";
			registryKey = Registry.LocalMachine.OpenSubKey(name);
			if (registryKey != null)
			{
				string text = (string)registryKey.GetValue("ImagePath");
				if (text.Contains(RuntimeService.Instance.AssemblyDirectory))
				{
					flag = false;
					Writer.WriteLine("Service already installed - don't need to register.");
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
		if (!flag)
		{
			return true;
		}
		Process process = new Process();
		process.StartInfo.FileName = ServiceInstallerBatchFile;
		process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		process.Start();
		process.WaitForExit();
		int exitCode = process.ExitCode;
		process.Close();
		Writer.WriteLine("Register service returned with code {0}", exitCode);
		return exitCode == 0;
	}

	private void CleanupQuickReturn(string configuration)
	{
		XmlDocument xmlDocument = new XmlDocument();
		string path = FileHelper.CreateBackup(configuration, BackupAction.Copy);
		bool flag = true;
		try
		{
			xmlDocument.Load(configuration);
			if (xmlDocument.DocumentElement == null)
			{
				return;
			}
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("InstalledDevices/Device");
			if (xmlNodeList.Count == 0)
			{
				return;
			}
			string text = null;
			foreach (XmlNode item in xmlNodeList)
			{
				XmlNode namedItem = item.Attributes.GetNamedItem("Name");
				if (namedItem.Value.Equals("QRTerminal", StringComparison.CurrentCultureIgnoreCase))
				{
					XmlNodeList xmlNodeList2 = item.SelectNodes("Property");
					foreach (XmlNode item2 in xmlNodeList2)
					{
						XmlNode namedItem2 = item2.Attributes.GetNamedItem("name");
						if (namedItem2.Value.Equals("Port"))
						{
							text = item2.Attributes.GetNamedItem("value").Value;
							break;
						}
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					break;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Console.WriteLine("InstalledPort: {0}", text);
				XmlNodeExtensions.SelectSingleNodeAndSetValue(xmlDocument.DocumentElement, "Controller/QuickReturnComPort", text);
				xmlDocument.Save(configuration);
			}
		}
		catch (Exception)
		{
			flag = false;
		}
		finally
		{
			if (flag)
			{
				FileHelper.SafeDeleteFile(path);
			}
		}
	}

	private bool InstallConfiguration()
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "HAL.xml");
		if (File.Exists(text))
		{
			Writer.WriteLine("The configuration file is already present - no further action.");
			return true;
		}
		try
		{
			File.Move(Path.Combine(RuntimeService.Instance.AssemblyDirectory, "HAL.xml.default"), text);
		}
		catch
		{
			Writer.WriteLine("Unable to move default HAL.xml file over.");
			return false;
		}
		ErrorList errorList = new ErrorList();
		ControllerConfiguration.Instance.Load(text, errorList);
		ControllerConfiguration.Instance.Import(Writer, errorList);
		ControllerConfiguration.Instance.Save(text, errorList);
		if (errorList.ContainsError())
		{
			Writer.WriteLine("There were problems importing the data from gamp - errors:");
			foreach (Error item in errorList)
			{
				Writer.WriteLine(item.ToString());
			}
			return false;
		}
		Writer.WriteLine("Successfully imported data from the Gamp folder.");
		return true;
	}

	private static void Main(string[] args)
	{
		ServiceHelper.StopHALService(check: false);
		using PackageInstaller packageInstaller = new PackageInstaller();
		packageInstaller.CreateDirectories();
		Platform platform = RuntimeService.Instance.GetPlatform();
		string text = "1.0.0.x";
		try
		{
			text = typeof(PackageInstaller).Assembly.GetName().Version.ToString();
		}
		catch
		{
			text = "1.0.0.x";
		}
		packageInstaller.Writer.WriteLine("HAL package installer version {0} running on {1} at {2} (platform = {3})", text, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), platform.ToString());
		if (Platform.Windows7 != platform)
		{
			packageInstaller.Writer.WriteLine("** ERROR ** This version is not supported on this platform ( {0} )", platform.ToString());
			return;
		}
		packageInstaller.InstallDatabase("HALCounters.vdb3");
		packageInstaller.InstallDatabase("HALData.vdb3");
		if (packageInstaller.InstallErrors.ContainsError())
		{
			packageInstaller.Writer.WriteLine("There were problems copying databases - ABORTING install:");
			{
				foreach (Error installError in packageInstaller.InstallErrors)
				{
					packageInstaller.Writer.WriteLine(installError.ToString());
				}
				return;
			}
		}
		StoreManager.Instance.Initialize();
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		try
		{
			StoreManager.Instance.UpgradeTables(packageInstaller.Writer);
			string text2 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "HAL.xml");
			if (File.Exists(text2))
			{
				packageInstaller.CleanupQuickReturn(text2);
				FileHelper.CreateBackup(text2, BackupAction.Copy);
				BarcodeConfiguration.Upgrade(packageInstaller.Writer, text2);
				InventoryManager.UpdateInventoryDatabaseOnUpgrade(packageInstaller.Writer);
				if (dataStore.GetValue("IceQubeExchanger", def: false))
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.Load(text2);
					XmlNodeExtensions.SelectSingleNodeAndSetValue(xmlDocument.DocumentElement, "Controller/EnableIceQubePolling", true.ToString());
					xmlDocument.Save(text2);
					packageInstaller.Writer.WriteLine("Migrated air exchanger configuration.");
					dataStore.Remove("IceQubeExchanger");
				}
			}
			else
			{
				int platterSlots = GampHelper.Instance.GetPlatterSlots();
				if (platterSlots == packageInstaller.MachineConfiguration)
				{
					packageInstaller.InstallConfiguration();
					InventoryManager.UpdateInventoryDatabaseOnUpgrade(packageInstaller.Writer);
				}
				else
				{
					InventoryManager.Instance.CreateUnknownDatabase(packageInstaller.MachineConfiguration);
					packageInstaller.Writer.WriteLine("The machine is configured for {0} slots; the gamp data says there are {1} slots - mark db UNKNOWN with {2} slots", packageInstaller.MachineConfiguration, platterSlots, packageInstaller.MachineConfiguration);
				}
			}
		}
		finally
		{
			StoreManager.Instance.CloseStores();
		}
		RegistryKey registryKey = null;
		string assemblyFile = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "halsvc-win32.exe");
		try
		{
			AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
			packageInstaller.Writer.WriteLine("Service has version {0}", assemblyName.Version.ToString());
			string subkey = "SOFTWARE\\Redbox\\HAL";
			registryKey = Registry.LocalMachine.CreateSubKey(subkey);
			registryKey?.SetValue("Product Version", assemblyName.Version.ToString());
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to set service registry key.", e);
		}
		finally
		{
			registryKey?.Close();
		}
		if (File.Exists(packageInstaller.ServiceInstallerBatchFile))
		{
			packageInstaller.InstallService();
		}
	}
}
