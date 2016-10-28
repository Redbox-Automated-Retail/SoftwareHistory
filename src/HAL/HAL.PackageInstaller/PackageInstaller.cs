using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using Microsoft.Win32;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;
using Redbox.HAL.Script.Framework;

namespace Redbox.HAL.PackageInstaller;

internal sealed class PackageInstaller : IDisposable
{
	private const int BackupsToKeep = 2;

	private const string KioskLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs";

	private const string DefaultConfiguration = "HAL.xml.default";

	private const string ConfigurationFileName = "hal.xml";

	private readonly string[] DefaultDatabases = new string[2] { "HALCounters.vdb3", "HALData.vdb3" };

	private readonly string ServiceInstallerBatchFile;

	private readonly TextWriter Writer;

	private readonly string InstallerLog;

	private readonly string InstallLogsDirectory;

	private readonly string ConfigurationFullPath;

	private readonly IRuntimeService RuntimeService;

	private readonly List<IPackageInstallHandler> PackageHandlers = new List<IPackageInstallHandler>();

	private readonly ErrorList InstallErrors = new ErrorList();

	private bool Disposed;

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		if (InstallErrors.ContainsError())
		{
			Writer.WriteLine("** Installation failed with the following errors **");
			foreach (Error installError in InstallErrors)
			{
				Writer.WriteLine("{0}: {1}: {2} ", installError.Code, installError.Description, installError.Details);
			}
		}
		Writer.Dispose();
	}

	private PackageInstaller()
	{
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		ConfigurationFullPath = RuntimeService.RuntimePath("hal.xml");
		InstallLogsDirectory = RuntimeService.InstallPath("InstallLogs");
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
		ServiceInstallerBatchFile = Path.Combine(RuntimeService.AssemblyDirectory, "register-service.bat");
	}

	private void CreateDirectories()
	{
		List<string> list = new List<string>();
		using (new DisposeableList<string>(list))
		{
			list.Add("C:\\Program Files\\Redbox\\KioskLogs");
			list.Add(Path.Combine(RuntimeService.AssemblyDirectory, "data"));
			list.Add("c:\\Program Files\\Redbox\\HALService\\Video");
			foreach (string item in list)
			{
				try
				{
					if (!Directory.Exists(item))
					{
						Directory.CreateDirectory(item);
					}
				}
				catch (Exception ex)
				{
					Writer.WriteLine("Unable to create directory: {0}", ex.Message);
				}
			}
		}
	}

	private void InstallDatabases()
	{
		string[] defaultDatabases = DefaultDatabases;
		foreach (string arg in defaultDatabases)
		{
			string text = RuntimeService.RuntimePath($"data\\{arg}");
			if (File.Exists(text))
			{
				Writer.WriteLine("The database {0} already exists - take no further action.", arg);
				continue;
			}
			string text2 = RuntimeService.RuntimePath($"{arg}.default");
			string arg2 = $"copy {text2} -> {text}";
			try
			{
				File.Copy(text2, text);
				Writer.WriteLine("{0}: SUCCESS", arg2);
			}
			catch (Exception ex)
			{
				InstallErrors.Add(Error.NewError("I001", "Copy failure", $"{arg2}: FAILURE reason = {ex.Message}"));
			}
		}
	}

	private void BackupConfiguration()
	{
		FileHelper.CreateBackup(ConfigurationFullPath, BackupAction.Copy);
		List<BackupEntry> list = new List<BackupEntry>();
		using (new DisposeableList<BackupEntry>(list))
		{
			string[] files = Directory.GetFiles(RuntimeService.AssemblyDirectory, "hal*.xml");
			string[] array = files;
			foreach (string fullPath in array)
			{
				BackupEntry backupEntry = new BackupEntry(fullPath, FileHelper.DefaultFormat);
				if (!backupEntry.FileName.Equals("hal.xml", StringComparison.CurrentCultureIgnoreCase) && !backupEntry.FileName.Equals("HAL.xml.default", StringComparison.CurrentCultureIgnoreCase) && backupEntry.BackupDate.HasValue)
				{
					list.Add(backupEntry);
				}
			}
			list.Sort((BackupEntry x, BackupEntry y) => x.BackupDate.Value.CompareTo(y.BackupDate.Value));
			if (list.Count > 2)
			{
				for (int num = 0; num < list.Count - 2; num++)
				{
					FileHelper.SafeDeleteFile(list[num].FullPath);
				}
			}
		}
	}

	private bool ServiceIsInstalled()
	{
		ServiceController[] services = ServiceController.GetServices();
		ServiceController[] array = services;
		foreach (ServiceController serviceController in array)
		{
			if (serviceController.ServiceName.Equals("halsvc$default", StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void InstallService()
	{
		if (!File.Exists(ServiceInstallerBatchFile))
		{
			InstallErrors.Add(Error.NewError("I003", "No service batch file.", "The service install batch file is missing."));
			return;
		}
		if (ServiceIsInstalled())
		{
			Writer.WriteLine("Service already installed - don't need to register.");
			return;
		}
		using Process process = new Process();
		process.StartInfo.FileName = ServiceInstallerBatchFile;
		process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		process.Start();
		process.WaitForExit();
		int exitCode = process.ExitCode;
		if (exitCode == 0)
		{
			Writer.WriteLine("Register service was successful.");
			return;
		}
		Writer.WriteLine("Register service returned with code {0}", exitCode);
		InstallErrors.Add(Error.NewError("I004", "Service install failed.", $"The service install returned error code {exitCode}"));
	}

	private void RegisterHandlers()
	{
		string[] files = Directory.GetFiles(RuntimeService.AssemblyDirectory, "*.dll");
		string[] array = files;
		foreach (string text in array)
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(text);
				Type typeFromHandle = typeof(IPackageInstallHandler);
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (typeFromHandle.IsAssignableFrom(type) && !type.IsInterface)
					{
						IPackageInstallHandler packageInstallHandler = (IPackageInstallHandler)Activator.CreateInstance(type);
						PackageHandlers.Add(packageInstallHandler);
						packageInstallHandler.OnRegister();
					}
				}
			}
			catch (Exception ex)
			{
				Writer.WriteLine("Unable to load assembly '{0}' to scan for installer handlers.", text);
				Writer.WriteLine("Exception: {0}", ex.Message);
			}
		}
		Writer.WriteLine("[Package Installer] there are {0} handlers registered.", PackageHandlers.Count);
	}

	private void OnUpgrade()
	{
		foreach (IPackageInstallHandler packageHandler in PackageHandlers)
		{
			packageHandler.OnUpgrade(InstallErrors, Writer);
		}
	}

	private void OnNewInstall()
	{
		foreach (IPackageInstallHandler packageHandler in PackageHandlers)
		{
			packageHandler.OnNewInstall(InstallErrors, Writer);
		}
	}

	private static void Main(string[] args)
	{
		ServiceLocator.Instance.AddService(typeof(ITableTypeFactory), new TableTypeFactory());
		ServiceLocator.Instance.AddService(typeof(IFormattedLogFactoryService), new FormattedLogFactory());
		ServiceLocator.Instance.AddService(typeof(IRuntimeService), new RuntimeService());
		using PackageInstaller packageInstaller = new PackageInstaller();
		if (packageInstaller.ServiceIsInstalled())
		{
			ServiceController serviceController = new ServiceController("halsvc$default");
			try
			{
				serviceController.Stop();
				serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30.0));
			}
			catch
			{
			}
		}
		packageInstaller.CreateDirectories();
		Platform platform = packageInstaller.RuntimeService.GetPlatform();
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
		packageInstaller.InstallDatabases();
		if (packageInstaller.InstallErrors.ContainsError())
		{
			packageInstaller.Writer.WriteLine("** There were problems copying databases - ABORTING install: **");
			{
				foreach (Error installError in packageInstaller.InstallErrors)
				{
					packageInstaller.Writer.WriteLine(installError.ToString());
				}
				return;
			}
		}
		ServiceLocator.Instance.AddService(typeof(IDataTableService), new DataTableService(exclusive: false));
		ServiceLocator.Instance.AddService(typeof(IPersistentMapService), new PersistentMapService());
		ConfigurationService configurationService = ConfigurationService.Make(packageInstaller.ConfigurationFullPath);
		ServiceLocator.Instance.AddService(typeof(IConfigurationService), configurationService);
		packageInstaller.RegisterHandlers();
		if (File.Exists(packageInstaller.ConfigurationFullPath))
		{
			packageInstaller.BackupConfiguration();
			packageInstaller.Writer.WriteLine("The configuration file is already present.");
			configurationService.LoadAndUpgrade(packageInstaller.InstallErrors);
			packageInstaller.OnUpgrade();
		}
		else
		{
			packageInstaller.Writer.WriteLine("Installing configuration from GAMP.");
			string text2 = packageInstaller.RuntimeService.RuntimePath("HAL.xml.default");
			try
			{
				File.Copy(text2, packageInstaller.ConfigurationFullPath);
			}
			catch (Exception ex)
			{
				packageInstaller.InstallErrors.Add(Error.NewError("I004", "Configuration install failed.", $"Unable to move file '{text2}' - error: {ex.Message}"));
				return;
			}
			configurationService.LoadAndImport(packageInstaller.InstallErrors);
			if (packageInstaller.InstallErrors.ErrorCount == 0)
			{
				packageInstaller.OnNewInstall();
			}
			else
			{
				packageInstaller.Writer.WriteLine("** Gamp data import failed - the configuration file may be incomplete. **");
			}
		}
		RegistryKey registryKey = null;
		string assemblyFile = Path.Combine(packageInstaller.RuntimeService.AssemblyDirectory, "halsvc-win32.exe");
		try
		{
			AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
			string subkey = "SOFTWARE\\Redbox\\HAL";
			registryKey = Registry.LocalMachine.CreateSubKey(subkey);
			registryKey?.SetValue("Product Version", assemblyName.Version.ToString());
		}
		catch (Exception arg)
		{
			packageInstaller.Writer.WriteLine("Unable to set service registry key.", arg);
		}
		finally
		{
			registryKey?.Close();
		}
		packageInstaller.InstallService();
	}
}
