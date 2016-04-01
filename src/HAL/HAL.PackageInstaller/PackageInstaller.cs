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
	private const string KioskLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs";

	private bool Disposed;

	private readonly string ServiceInstallerBatchFile;

	private readonly TextWriter Writer;

	private readonly string InstallerLog;

	private readonly string InstallLogsDirectory;

	private readonly string ConfigurationFile;

	private readonly string QRStateFile;

	private readonly string DeprecatedQRStateFile;

	private readonly IRuntimeService RuntimeService;

	private readonly List<IPackageInstallHandler> PackageHandlers = new List<IPackageInstallHandler>();

	private readonly ErrorList InstallErrors = new ErrorList();

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
		ConfigurationFile = RuntimeService.RuntimePath("hal.xml");
		QRStateFile = RuntimeService.RuntimePath("qr.state");
		DeprecatedQRStateFile = RuntimeService.RuntimePath("qr.defs");
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

	private void InstallDatabase(string fileName)
	{
		string text = RuntimeService.RuntimePath($"data\\{fileName}");
		if (File.Exists(text))
		{
			Writer.WriteLine("The database {0} already exists - take no further action.", fileName);
			return;
		}
		string text2 = RuntimeService.RuntimePath($"{fileName}.default");
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

	private void InstallService()
	{
		if (!File.Exists(ServiceInstallerBatchFile))
		{
			InstallErrors.Add(Error.NewError("I003", "No service batch file.", "The service install batch file is missing."));
			return;
		}
		bool flag = true;
		ServiceController[] services = ServiceController.GetServices();
		ServiceController[] array = services;
		foreach (ServiceController serviceController in array)
		{
			if (serviceController.ServiceName.Equals("halsvc$default", StringComparison.CurrentCultureIgnoreCase))
			{
				flag = false;
				Writer.WriteLine("Service already installed - don't need to register.");
				break;
			}
		}
		if (!flag)
		{
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
		ServiceLocator.Instance.AddService(typeof(IFormattedLogFactoryService), new FormattedLogFactory());
		ServiceLocator.Instance.AddService(typeof(IRuntimeService), new RuntimeService());
		ServiceController serviceController = new ServiceController("halsvc$default");
		try
		{
			serviceController.Stop();
			serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30.0));
		}
		catch
		{
		}
		using PackageInstaller packageInstaller = new PackageInstaller();
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
		packageInstaller.InstallDatabase("HALCounters.vdb3");
		packageInstaller.InstallDatabase("HALData.vdb3");
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
		StoreManager instance = new StoreManager();
		ServiceLocator.Instance.AddService(typeof(IDataStoreService), instance);
		ConfigurationService configurationService = ConfigurationService.Make(packageInstaller.ConfigurationFile);
		ServiceLocator.Instance.AddService(typeof(IConfigurationService), configurationService);
		packageInstaller.RegisterHandlers();
		if (File.Exists(packageInstaller.ConfigurationFile))
		{
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
				File.Copy(text2, packageInstaller.ConfigurationFile);
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
