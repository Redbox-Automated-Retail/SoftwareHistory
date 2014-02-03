using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.Log.Framework;

namespace Redbox.UpdateManager.Daemon;

public class Program
{
	private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(UpdateManagerService));

	public static void Main(string[] args)
	{
		bool console;
		using (CommandLine commandLine = CommandLine.ParseTo(Options.Instance))
		{
			if (commandLine.HelpRequested || commandLine.Errors.Count > 0)
			{
				return;
			}
			using DefaultConsole defaultConsole = new DefaultConsole();
			if (Options.Instance.Register)
			{
				if (ExecuteInstaller(install: true, defaultConsole))
				{
					defaultConsole.WriteLine("Service successfully registered.");
				}
				else
				{
					defaultConsole.WriteLine("Service was not registered.");
				}
				return;
			}
			if (Options.Instance.Unregister)
			{
				if (ExecuteInstaller(install: false, defaultConsole))
				{
					defaultConsole.WriteLine("Service successfully unregistered.");
				}
				else
				{
					defaultConsole.WriteLine("Service was not unregistered.");
				}
				return;
			}
			console = Options.Instance.Console;
		}
		m_logger.Configure(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "updatemgrd-log4net.config"));
		LogHelper.Instance.Logger = m_logger;
		UpdateManagerService updateManagerService = new UpdateManagerService();
		if (Debugger.IsAttached || console)
		{
			updateManagerService.StartService();
			return;
		}
		ServiceBase.Run(new UpdateManagerService[1] { updateManagerService });
	}

	private static bool ExecuteInstaller(bool install, IConsole console)
	{
		try
		{
			using AssemblyInstaller assemblyInstaller = new AssemblyInstaller(typeof(Program).Assembly, new string[0]);
			assemblyInstaller.UseNewContext = true;
			IDictionary dictionary = new Hashtable();
			try
			{
				if (install)
				{
					assemblyInstaller.Install(dictionary);
				}
				else
				{
					assemblyInstaller.Uninstall(dictionary);
				}
				assemblyInstaller.Commit(dictionary);
				return true;
			}
			catch (Exception value)
			{
				assemblyInstaller.Rollback(dictionary);
				console.WriteLine(value);
				return false;
			}
		}
		catch (Exception value2)
		{
			console.WriteLine(value2);
			return false;
		}
	}
}
