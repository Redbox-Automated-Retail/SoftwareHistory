using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Drawing;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.IDE;
using Redbox.KioskEngine.Kernel;
using Redbox.REDS.Framework;
using WindowsInput;

namespace Redbox.KioskEngine.Bootstrap;

public class EngineApplication : ApplicationContext, IInputService, IFileCacheService, IEngineApplication
{
	internal struct NativeMessage
	{
		public IntPtr hWnd;

		public uint msg;

		public IntPtr wParam;

		public IntPtr lParam;

		public uint time;

		public Point p;
	}

	private NamedLock m_instanceLock;

	private CommandLine m_commandLine;

	private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(initialState: false);

	private static Guid m_applicationGuid = new Guid("{FB5E48FF-4677-4cfa-8ED1-265114D7564C}");

	private readonly IDictionary<string, IdleEventHandler> m_idleHandlers = new Dictionary<string, IdleEventHandler>();

	private readonly IDictionary<string, KeyPressHandler> m_keyPressHandlers = new Dictionary<string, KeyPressHandler>();

	private readonly IDictionary<string, MouseClickHandler> m_mouseClickHandlers = new Dictionary<string, MouseClickHandler>();

	private readonly IDictionary<string, MouseClickHandler> m_mouseDoubleClickHandlers = new Dictionary<string, MouseClickHandler>();

	private readonly IDictionary<string, ActivatedEventHandler> m_activatedHandlers = new Dictionary<string, ActivatedEventHandler>();

	public static EngineApplication Instance => Singleton<EngineApplication>.Instance;

	public CommandLine CommandLine
	{
		get
		{
			if (m_commandLine == null)
			{
				m_commandLine = CommandLine.ParseTo(Options.Instance);
			}
			return m_commandLine;
		}
	}

	internal static bool IsApplicationIdle
	{
		get
		{
			NativeMessage msg;
			return !PeekMessage(out msg, IntPtr.Zero, 0u, 0u, 0u);
		}
	}

	private bool checkMainFormStatus { get; set; }

	public void Reset()
	{
		LogHelper.Instance.Log("Reset input service.");
		ClearIdleHandlers();
		ClearActivatedHandlers();
		ClearKeyPressHandlers();
		ClearMouseClickHandlers();
		ClearMouseDoubleClickHandlers();
	}

	public void PerformSingleClick()
	{
		try
		{
			InputSimulator.SimulateLeftMouseDown(Cursor.Position.X, Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(Cursor.Position.X, Cursor.Position.Y);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EngineApplication.PerformSingleClick.", e);
		}
	}

	public void PerformDoubleClick()
	{
		try
		{
			InputSimulator.SimulateLeftMouseDown(Cursor.Position.X, Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(Cursor.Position.X, Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseDown(Cursor.Position.X, Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(Cursor.Position.X, Cursor.Position.Y);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EngineApplication.PerformDoubleClick.", e);
		}
	}

	public void SimulateKeyStrokes(string input)
	{
		try
		{
			InputSimulator.SimulateTextEntry(input);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in EngineApplication.SimulateKeyStrokes.", e);
		}
	}

	public void RegisterIdleHandler(string name, IdleEventHandler handler)
	{
		if (!m_idleHandlers.ContainsKey(name))
		{
			m_idleHandlers[name] = handler;
		}
	}

	public void RemoveIdleHandler(string name)
	{
		m_idleHandlers.Remove(name);
	}

	public void ClearIdleHandlers()
	{
		m_idleHandlers.Clear();
	}

	public void RegisterActivatedHandler(string name, ActivatedEventHandler handler)
	{
		if (!m_activatedHandlers.ContainsKey(name))
		{
			m_activatedHandlers[name] = handler;
		}
	}

	public void RemoveActivatedHandler(string name)
	{
		m_activatedHandlers.Remove(name);
	}

	public void ClearActivatedHandlers()
	{
		m_activatedHandlers.Clear();
	}

	public void RegisterKeyPressHandler(string name, KeyPressHandler handler)
	{
		if (!m_keyPressHandlers.ContainsKey(name))
		{
			m_keyPressHandlers[name] = handler;
		}
	}

	public void RegisterMouseClickHandler(string name, MouseClickHandler handler)
	{
		if (!m_mouseClickHandlers.ContainsKey(name))
		{
			m_mouseClickHandlers[name] = handler;
		}
	}

	public void RemoveMouseClickHandler(string name)
	{
		m_mouseClickHandlers.Remove(name);
	}

	public void RegisterMouseDoubleClickHandler(string name, MouseClickHandler handler)
	{
		m_mouseDoubleClickHandlers.ContainsKey(name);
	}

	public void RemoveMouseDoubleClickHandler(string name)
	{
		m_mouseDoubleClickHandlers.Remove(name);
	}

	public void ClearKeyPressHandlers()
	{
		m_keyPressHandlers.Clear();
	}

	public void ClearMouseClickHandlers()
	{
		m_mouseClickHandlers.Clear();
	}

	public void ClearMouseDoubleClickHandlers()
	{
		m_mouseDoubleClickHandlers.Clear();
	}

	public void RemoveKeyPressHandler(string name)
	{
		m_keyPressHandlers.Remove(name);
	}

	public void Remove(string pattern)
	{
		try
		{
			string cachePath = GetCachePath();
			string[] files = Directory.GetFiles(cachePath, pattern);
			foreach (string path in files)
			{
				File.Delete(path);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in IFlashService.ClearCache().", e);
		}
	}

	public void FlushCache()
	{
		LogHelper.Instance.Log("Flush file cache service.");
		try
		{
			string cachePath = GetCachePath();
			string[] files = Directory.GetFiles(cachePath, "*.*");
			foreach (string text in files)
			{
				LogHelper.Instance.Log("...Delete file '{0}'", text);
				File.Delete(text);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in IFlashService.ClearCache().", e);
		}
	}

	public void SetFileContent(string fileName, byte[] data, bool overwrite)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (!File.Exists(path) || overwrite)
		{
			File.WriteAllBytes(path, data);
		}
	}

	public void SetFileContent(string fileName, string data, bool overwrite)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (!File.Exists(path) || overwrite)
		{
			File.WriteAllText(path, data);
		}
	}

	public string GetCachePath()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		string text = Path.Combine(service.ExpandProperties("${RunningPath}"), "..\\cache");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public byte[] GetFileContent(string fileName)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}
		return null;
	}

	public string GetFileContentAsString(string fileName)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (File.Exists(path))
		{
			return File.ReadAllText(path);
		}
		return null;
	}

	public void ShutDown()
	{
		Application.Exit();
	}

	public bool Run()
	{
		HostForm instance = (HostForm)(base.MainForm = new HostForm());
		base.MainForm.BackColor = RenderingService.Instance.BackgroundColor;
		BufferedGraphicsManager.Current.MaximumBuffer = base.MainForm.DisplayRectangle.Size;
		ServiceLocator.Instance.AddService(typeof(IFlashService), instance);
		ServiceLocator.Instance.AddService(typeof(ITrackDataService), instance);
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		service.Register(base.MainForm.Handle);
		IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
		IScene activeScene = service2.CreateScene("Default", base.MainForm.Width, base.MainForm.Height, RenderingService.Instance.BackgroundColor);
		service2.ActiveScene = activeScene;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (!ResourceBundleService.Instance.IsPendingBundleSwitch() && ResourceBundleService.Instance.DefaultBundleName == null)
		{
			errorList.AddRange(SelectBundle());
		}
		if (!errorList.ContainsError())
		{
			if (checkMainFormStatus)
			{
				return false;
			}
			base.MainForm.Show();
			return true;
		}
		ErrorForm errorForm = new ErrorForm();
		errorForm.Errors = errorList;
		ErrorForm errorForm2 = errorForm;
		errorForm2.ShowDialog();
		return false;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		FirewallHelper.CreateFirewallExemptions();
		ConfigureDefaultMacros();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		LogHelper.Instance.Log("Engine start up.");
		LogHelper.Instance.Log(service.ExpandProperties("Engine: ${ProductName} ${ProductVersion}"));
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		m_instanceLock = new NamedLock(m_applicationGuid.ToString(), LockScope.Local);
		if (!m_instanceLock.Exists())
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("INSTANCE", service.ExpandProperties("Only one instance of ${ProductName} ${ProductVersion} may be run at a time."), service.ExpandProperties("Another instance of ${ProductName} ${ProductVersion} is already running.")));
			return errorList;
		}
		if (CommandLine.Errors.Count > 0)
		{
			CommandLine.WriteUsage(includeHelp: false);
			while (CommandLine.Errors.Count > 0)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E005", CommandLine.Errors.Pop(), string.Empty));
			}
			return errorList;
		}
		CommandHelper.Initialize();
		UserSettingsStore.Instance.RootPath = "SOFTWARE\\Redbox\\REDS\\Kiosk Engine";
		MachineSettingsStore.Instance.RootPath = "SOFTWARE\\Redbox\\REDS\\Kiosk Engine";
		errorList.AddRange(KernelExtensionService.Instance.Initialize(Application.ExecutablePath));
		errorList.AddRange(InitializeConfiguration());
		ServiceLocator.Instance.AddService(typeof(IEngineApplication), this);
		ServiceLocator.Instance.AddService(typeof(IObjectCacheService), ObjectCacheService.Instance);
		ServiceLocator.Instance.AddService(typeof(IIdeService), IdeService.Instance);
		ServiceLocator.Instance.AddService(typeof(IInputService), this);
		ServiceLocator.Instance.AddService(typeof(IFileCacheService), this);
		ServiceLocator.Instance.AddService(typeof(IViewService), ViewService.Instance);
		ServiceLocator.Instance.AddService(typeof(ITimerService), TimerService.Instance);
		ServiceLocator.Instance.AddService(typeof(ISoundService), SoundService.Instance);
		ServiceLocator.Instance.AddService(typeof(ITweenService), TweenService.Instance);
		ServiceLocator.Instance.AddService(typeof(ICallbackService), CallbackService.Instance);
		ServiceLocator.Instance.AddService(typeof(ISecurityService), SecurityService.Instance);
		ServiceLocator.Instance.AddService(typeof(IDataCacheService), DataCacheService.Instance);
		ServiceLocator.Instance.AddService(typeof(IUserSettingsStore), UserSettingsStore.Instance);
		ServiceLocator.Instance.AddService(typeof(IMachineSettingsStore), MachineSettingsStore.Instance);
		ServiceLocator.Instance.AddService(typeof(IPreferenceService), PreferenceService.Instance);
		ServiceLocator.Instance.AddService(typeof(IScreenSaverService), ScreenSaverService.Instance);
		ServiceLocator.Instance.AddService(typeof(IStyleSheetService), StyleSheetService.Instance);
		ServiceLocator.Instance.AddService(typeof(IKernelFunctionRegistryService), KernelFunctionRegistryService.Instance);
		IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service2 != null && service2.TryGetValue<bool>("system", "General", "KioskAnalytics", out var value) && value)
		{
			ServiceLocator.Instance.AddService(typeof(ISessionAnalyticsService), SessionAnalyticsService.Instance);
		}
		List<BinRange> redboxGiftCardBinRanges = GetRedboxGiftCardBinRanges();
		if (redboxGiftCardBinRanges != null)
		{
			CardTypeHelper.Initialize(redboxGiftCardBinRanges);
		}
		InitializePreferencePages();
		errorList.AddRange(SchedulerService.Instance.Initialize());
		errorList.AddRange(DataStoreService.Instance.Initialize());
		errorList.AddRange(FontCacheService.Instance.Initialize());
		errorList.AddRange(BitmapCacheService.Instance.Initialize());
		errorList.AddRange(RenderingService.Instance.Initialize());
		errorList.AddRange(KernelService.Instance.Initialize());
		errorList.AddRange(EnvironmentManager.Instance.Initialize());
		errorList.AddRange(QueueService.Instance.Initialize(Path.Combine(service["RunningPath"], "message.data")));
		errorList.AddRange(ShoppingSessionService.Instance.Initialize(Path.Combine(service["RunningPath"], "session.data")));
		errorList.AddRange(DebugService.Instance.Initialize());
		errorList.AddRange(KernelExtensionService.Instance.LoadAllExtensions());
		Redbox.KioskEngine.ComponentModel.ErrorList errorList2 = ResourceBundleService.Instance.Initialize(Application.ExecutablePath);
		if (errorList2.ContainsError())
		{
			errorList.AddRange(errorList2);
		}
		errorList2 = ResourceBundleService.Instance.LoadBundles();
		if (errorList2.ContainsError())
		{
			errorList.AddRange(errorList2);
		}
		string text = Options.Instance.BundleFile;
		if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(ResourceBundleService.Instance.DefaultBundleName))
		{
			text = ResourceBundleService.Instance.DefaultBundleName;
		}
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				BundleSpecifier bundleSpecifier = new BundleSpecifier(text);
				errorList.AddRange(ResourceBundleService.Instance.Activate(bundleSpecifier.Name, bundleSpecifier.Version.ToString()));
			}
			catch (Exception ex)
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B110", ex.Message, "Correct the specifier and try again."));
			}
		}
		else
		{
			LogHelper.Instance.Log("No Default Bundle specified in System.Config");
		}
		return errorList;
	}

	public void ThreadSafeHostUpdate(MethodInvoker invoker)
	{
		if (base.MainForm.InvokeRequired)
		{
			base.MainForm.Invoke(invoker);
		}
		else
		{
			invoker();
		}
	}

	public bool CanRestart()
	{
		return EnvironmentFunctions.CanRestart();
	}

	public bool CanShutDown()
	{
		return EnvironmentFunctions.CanShutdown();
	}

	public void FlagForSafeShutdown()
	{
		EnvironmentFunctions.FlagForSafeShutdown();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
			CommandLine.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnMainFormClosed(object sender, EventArgs e)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
		service2.Flush();
		IEnvironmentNotificationService service3 = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		IQueueService service4 = ServiceLocator.Instance.GetService<IQueueService>();
		IKernelService service5 = ServiceLocator.Instance.GetService<IKernelService>();
		IRenderingService service6 = ServiceLocator.Instance.GetService<IRenderingService>();
		IFontCacheService service7 = ServiceLocator.Instance.GetService<IFontCacheService>();
		IDataCacheService service8 = ServiceLocator.Instance.GetService<IDataCacheService>();
		IBitmapCacheService service9 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IShoppingSessionService service10 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IDataStoreService service11 = ServiceLocator.Instance.GetService<IDataStoreService>();
		ISchedulerService service12 = ServiceLocator.Instance.GetService<ISchedulerService>();
		IStyleSheetService service13 = ServiceLocator.Instance.GetService<IStyleSheetService>();
		service13.Reset();
		service.Reset();
		service12.Shutdown();
		service3.Unregister(base.MainForm.Handle);
		service3.Reset();
		if (service6.ActiveScene != null)
		{
			service6.ActiveScene.Clear();
		}
		service9.DropCache();
		service7.DropCache();
		service5.Dispose();
		service8.Shutdown();
		service2.Reset();
		service4.StopQueueWorker();
		service10.Dispose();
		service4.Dispose();
		service11.CloseAllStores();
		ServiceLocator.Instance.GetService<IKernelExtensionService>()?.DeactivateAllExtensions();
		IConfigurationService service14 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service14 != null)
		{
			Redbox.KioskEngine.ComponentModel.ErrorList errorList = service14.Stop();
			errorList.ForEach(delegate(Redbox.KioskEngine.ComponentModel.Error error)
			{
				LogHelper.Instance.Log("Code: {0} Desc: {1} Details: {2}", error.Code, error.Description, error.Details);
			});
		}
		base.OnMainFormClosed(sender, e);
	}

	[DllImport("User32.dll")]
	internal static extern bool PeekMessage(out NativeMessage msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);

	internal Redbox.KioskEngine.ComponentModel.ErrorList SelectBundle()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		SelectBundleForm selectBundleForm = new SelectBundleForm();
		selectBundleForm.SearchPath = EnumerableExtensions.Join(ResourceBundleService.Instance.SearchPath, ";");
		SelectBundleForm selectBundleForm2 = selectBundleForm;
		selectBundleForm2.ShowDialog();
		if (selectBundleForm2.SelectedBundle != null)
		{
			if (ResourceBundleService.Instance.SetSwitchToBundle(selectBundleForm2.SelectedBundle))
			{
				checkMainFormStatus = false;
			}
			ResourceBundleService.Instance.ShowDebugger = selectBundleForm2.ShowDebugger;
		}
		else
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			if (service != null && service.GetValue<string>("Core", "DefaultBundle") != "NONE")
			{
				ResourceBundleService.Instance.ActivateDefaultBundle();
				checkMainFormStatus = false;
			}
			else
			{
				checkMainFormStatus = true;
				m_keyPressHandlers.Clear();
				base.MainForm.Close();
			}
		}
		return result;
	}

	internal void SwitchBundleIfNecessary()
	{
		if (!ResourceBundleService.Instance.IsPendingBundleSwitch())
		{
			return;
		}
		IKernelExtensionService kernelExtensionService = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		MethodInvoker invoker = delegate
		{
			Redbox.KioskEngine.ComponentModel.ErrorList errorList = ResourceBundleService.Instance.Switch(out var previousBundle);
			if (!errorList.ContainsError())
			{
				kernelExtensionService.ActivateAllExtensions();
				ServiceLocator.Instance.GetService<IQueueService>()?.StartQueueWorker();
			}
			else
			{
				ErrorForm errorForm = new ErrorForm
				{
					Errors = errorList
				};
				errorForm.ShowDialog();
				ResourceBundleService.Instance.SetSwitchToBundle(previousBundle);
			}
		};
		ThreadSafeHostUpdate(invoker);
	}

	internal void ConfigureDefaultMacros()
	{
		MacroService macroService = new MacroService();
		ServiceLocator.Instance.AddService(typeof(IMacroService), macroService);
		macroService["EngineVersion"] = AssemblyInfoHelper.GetVersion(typeof(Program).Assembly);
		macroService["RunningPath"] = Path.GetDirectoryName(typeof(EngineApplication).Assembly.Location);
		macroService["ProductName"] = "Redbox Kiosk Engine";
		macroService["ProductVersion"] = AssemblyInfoHelper.GetVersion(typeof(Program).Assembly);
	}

	internal void OnIdle(object sender, EventArgs e)
	{
		try
		{
			ActivateIdleHandlers();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.Idle handler.", e2);
		}
		while (IsApplicationIdle)
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			SwitchBundleIfNecessary();
			TimerService.Instance.UpdateTimers();
			TweenService.Instance.UpdateTweens();
			while (executionTimer.ElapsedMilliseconds < 10 && CallbackService.Instance.InvokeNextCallback())
			{
			}
			RenderScene();
			int num = (int)(33 - executionTimer.ElapsedMilliseconds);
			if (num > 0)
			{
				m_resetEvent.WaitOne(num);
			}
		}
	}

	internal bool RenderScene()
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service == null)
		{
			return false;
		}
		if (service.ActiveScene == null)
		{
			return false;
		}
		using (BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(base.MainForm.CreateGraphics(), base.MainForm.DisplayRectangle))
		{
			service.ActiveScene.Render(bufferedGraphics.Graphics, out var changed);
			if (!changed)
			{
				return false;
			}
			bufferedGraphics.Render();
		}
		return true;
	}

	internal void ActivateIdleHandlers()
	{
		if (m_idleHandlers.Count != 0)
		{
			KeyValuePair<string, IdleEventHandler>[] array = new KeyValuePair<string, IdleEventHandler>[m_idleHandlers.Count];
			m_idleHandlers.CopyTo(array, 0);
			KeyValuePair<string, IdleEventHandler>[] array2 = array;
			foreach (KeyValuePair<string, IdleEventHandler> keyValuePair in array2)
			{
				keyValuePair.Value();
			}
		}
	}

	internal void ActivateActivatedHandlers()
	{
		if (m_activatedHandlers.Count != 0)
		{
			KeyValuePair<string, ActivatedEventHandler>[] array = new KeyValuePair<string, ActivatedEventHandler>[m_activatedHandlers.Count];
			m_activatedHandlers.CopyTo(array, 0);
			KeyValuePair<string, ActivatedEventHandler>[] array2 = array;
			foreach (KeyValuePair<string, ActivatedEventHandler> keyValuePair in array2)
			{
				keyValuePair.Value();
			}
		}
	}

	internal void ActivateKeyPressHandlers(KeyPressEventArgsEx e)
	{
		if (m_keyPressHandlers.Count != 0)
		{
			string key = e.KeyChar.ToString();
			int keyChar = e.KeyChar;
			KeyValuePair<string, KeyPressHandler>[] array = new KeyValuePair<string, KeyPressHandler>[m_keyPressHandlers.Count];
			m_keyPressHandlers.CopyTo(array, 0);
			KeyValuePair<string, KeyPressHandler>[] array2 = array;
			foreach (KeyValuePair<string, KeyPressHandler> keyValuePair in array2)
			{
				keyValuePair.Value(key, keyChar, e.ModifierKeys.ToString());
			}
		}
	}

	internal void ActivateMouseClickHandlers(MouseEventArgs e)
	{
		if (m_mouseClickHandlers.Count != 0)
		{
			KeyValuePair<string, MouseClickHandler>[] array = new KeyValuePair<string, MouseClickHandler>[m_mouseClickHandlers.Count];
			m_mouseClickHandlers.CopyTo(array, 0);
			KeyValuePair<string, MouseClickHandler>[] array2 = array;
			foreach (KeyValuePair<string, MouseClickHandler> keyValuePair in array2)
			{
				keyValuePair.Value(e.Button.ToString(), e.X, e.Y);
			}
		}
	}

	internal void ActivateMouseDoubleClickHandlers(MouseEventArgs e)
	{
		if (m_mouseDoubleClickHandlers.Count != 0)
		{
			KeyValuePair<string, MouseClickHandler>[] array = new KeyValuePair<string, MouseClickHandler>[m_mouseDoubleClickHandlers.Count];
			m_mouseDoubleClickHandlers.CopyTo(array, 0);
			KeyValuePair<string, MouseClickHandler>[] array2 = array;
			foreach (KeyValuePair<string, MouseClickHandler> keyValuePair in array2)
			{
				keyValuePair.Value(e.Button.ToString(), e.X, e.Y);
			}
		}
	}

	private EngineApplication()
	{
		SessionEndingEventHandler value = delegate
		{
			if (base.MainForm != null)
			{
				base.MainForm.Close();
			}
		};
		SystemEvents.SessionEnding += value;
		Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
		{
			try
			{
				ServiceLocator.Instance.GetService<IKernelExtensionService>()?.NotifyOfHostCrash(e.Exception);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("Unable to enqueue application crash alert.", e2);
			}
			try
			{
				ShoppingSessionService.Instance.AbandonAll("Application crash.");
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("Unable to save the active shopping session.", e3);
			}
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.ThreadException handler.", e.Exception);
			MessageBox.Show(e.Exception.ToString());
		};
		Application.Idle += OnIdle;
	}

	private static void InitializePreferencePages()
	{
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("EngineCore_General", "Store", "Engine Core/General", PreferencePageTarget.LocalSystem, new GeneralPreferencePage());
			service.AddPreferencePage("EngineCore_Ide", "Ide", "Engine Core/IDE", PreferencePageTarget.LocalSystem, new IdePreferencePage());
			service.AddPreferencePage("EngineCore_Resource", "Resource", "Engine Core/Resource", PreferencePageTarget.LocalSystem, new ResourcePreferencePage());
		}
	}

	private static Redbox.KioskEngine.ComponentModel.ErrorList InitializeConfiguration()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		ServiceLocator.Instance.GetService<IMacroService>();
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null)
		{
			LogHelper.Instance.Log("Starting Update Manager Configuration Service.");
			errorList.AddRange(service.Start());
			string directoryName = Path.GetDirectoryName(typeof(DataStoreService).Assembly.Location);
			string path = Path.Combine(directoryName, "..\\data");
			DirectoryInfo directoryInfo = new DirectoryInfo(path);
			try
			{
				FileInfo[] files = directoryInfo.GetFiles("*.config");
				FileInfo[] array = files;
				foreach (FileInfo fileInfo in array)
				{
					LogHelper.Instance.Log("> adding " + fileInfo.Name);
					errorList.AddRange(service.Add(Path.GetFileNameWithoutExtension(fileInfo.Name), fileInfo.FullName, "default"));
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("'data' folder issue.", e);
				System.Environment.Exit(0);
			}
		}
		return errorList;
	}

	private static List<BinRange> GetRedboxGiftCardBinRanges()
	{
		List<BinRange> list = null;
		try
		{
			IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
			if (service != null && service.TryGetValue<string>("system", "General", "RedboxGiftCardBinRanges", out var value))
			{
				string[] array = value.Split(',');
				if (array.Length % 2 == 0)
				{
					list = new List<BinRange>();
					for (int i = 0; i < array.Length; i += 2)
					{
						list.Add(new BinRange(int.Parse(array[i]), int.Parse(array[i + 1])));
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Invalid BIN range.", e);
			list = null;
		}
		return list;
	}
}
