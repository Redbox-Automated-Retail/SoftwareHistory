using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
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
		try
		{
			ErrorList errorList = EngineApplication.Instance.Initialize();
			if (!errorList.ContainsCode("INSTANCE"))
			{
				if (errorList.ContainsError())
				{
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
