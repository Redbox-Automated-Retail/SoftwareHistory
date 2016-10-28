using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Log.Framework;

namespace Redbox.KioskEngine.Bootstrap;

public class Program : IDisposable
{
	private static readonly ILogger m_logger = LogHelper.Instance.CreateLog4NetLogger(typeof(Program));

	private static readonly ILogger m_loggerCardReader = LogHelper.Instance.CreateMultiLog4NetLogger("CardReaderLog");

	private static readonly ILogger m_loggerUser = LogHelper.Instance.CreateMultiLog4NetLogger("UserLoginLog");

	private static readonly ILogger m_loggerTestSvcs = LogHelper.Instance.CreateMultiLog4NetLogger("TestServices");

	private static readonly ILogger m_loggerControlPanelAction = LogHelper.Instance.CreateMultiLog4NetLogger("ControlPanelActionLog");

	private static readonly ILogger m_loggerFieldMaintenanceAction = LogHelper.Instance.CreateMultiLog4NetLogger("FieldMaintenanceActionLog");

	public static bool SetTls11
	{
		get
		{
			try
			{
				string text = "SetTls11";
				string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Core";
				object value = Registry.GetValue(text2, text, null);
				if (value == null)
				{
					LogHelper.Instance.Log("SetTls11 is not set in the registry at {0}\\{1} - default is true", text2, text);
					return true;
				}
				LogHelper.Instance.Log("SetTls11 registry key is set to {0}", value);
				return Convert.ToBoolean(value);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unhandled exception occurred in Program.SetSecurityProtocolType", e);
			}
			return true;
		}
	}

	[STAThread]
	public static void Main()
	{
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		Application.ThreadException += Application_ThreadException;
		using Program program = new Program();
		program.Run();
	}

	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		try
		{
			Exception e2 = (Exception)e.ExceptionObject;
			LogHelper.Instance.Log("Unhandled Application Exception. Exiting Application", e2);
		}
		finally
		{
			Application.Exit();
		}
	}

	private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
	{
		try
		{
			LogHelper.Instance.Log("Unhandled Windows Forms Thread Exception. Exiting Application", e.Exception);
		}
		finally
		{
			Application.Exit();
		}
	}

	public void Run()
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "log4net.config");
		m_logger.Configure(path);
		m_loggerCardReader.Configure(path);
		m_loggerUser.Configure(path);
		m_loggerTestSvcs.Configure(path);
		LogHelper.Instance.Loggers.Add(m_logger);
		LogHelper.Instance.Loggers.Add(m_loggerCardReader);
		LogHelper.Instance.Loggers.Add(m_loggerUser);
		LogHelper.Instance.Loggers.Add(m_loggerControlPanelAction);
		LogHelper.Instance.Loggers.Add(m_loggerFieldMaintenanceAction);
		LogHelper.Instance.Loggers.Add(m_loggerTestSvcs);
		if (SetTls11)
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
		}
		try
		{
			ErrorList errorList = EngineApplication.Instance.Initialize();
			if (!errorList.ContainsCode("INSTANCE"))
			{
				if (errorList.ContainsError())
				{
					errorList.ToLogHelper();
					ErrorForm errorForm = new ErrorForm();
					errorForm.Errors = errorList;
					ErrorForm errorForm2 = errorForm;
					errorForm2.ShowDialog();
				}
				else if (EngineApplication.Instance.Run())
				{
					Application.Run(EngineApplication.Instance);
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occured in Program.Run. Exiting Application", e);
		}
	}

	public void Dispose()
	{
	}
}
