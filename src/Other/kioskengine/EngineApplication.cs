using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Microsoft.Win32;
using Redbox.BrokerServices.KernelExtension;
using Redbox.Core;
using Redbox.GetOpts;
using Redbox.HardwareServices.KernelExtension;
using Redbox.KioskEngine.Bootstrap.Properties;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Drawing;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.IDE;
using Redbox.KioskEngine.Kernel;
using Redbox.KioskServices.KernelExtension;
using Redbox.REDS.Framework;
using Redbox.Rental.Data;
using Redbox.Rental.Kernel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Marketing;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Services;
using Redbox.Rental.Services.KioskEvents;
using Redbox.Rental.Services.KioskHealth;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Marketing;
using Redbox.Rental.Services.Personalization;
using Redbox.Rental.Services.Pricing;
using Redbox.Rental.Services.ShoppingCart;
using Redbox.UpdateServices.KernelExtension;
using WindowsInput;

namespace Redbox.KioskEngine.Bootstrap;

public class EngineApplication : ApplicationContext, IInputService, IFileCacheService, IEngineApplication
{
	public enum MapType : uint
	{
		MAPVK_VK_TO_VSC,
		MAPVK_VSC_TO_VK,
		MAPVK_VK_TO_CHAR,
		MAPVK_VSC_TO_VK_EX
	}

	internal struct NativeMessage
	{
		public IntPtr hWnd;

		public uint msg;

		public IntPtr wParam;

		public IntPtr lParam;

		public uint time;

		public System.Drawing.Point p;
	}

	private ElementHost _elementHost;

	public string _dataPath;

	internal System.Timers.Timer _waitTimer;

	internal object _lockObject = new object();

	private string m_corruptDbName;

	private bool m_enableMaintenanceMode;

	private bool m_appStarting = true;

	private bool checkMainFormStatus;

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

	public bool HasCorruptedDatabase => !string.IsNullOrEmpty(m_corruptDbName);

	public string DataPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_dataPath))
			{
				return _dataPath;
			}
			string text = string.Empty;
			if (Debugger.IsAttached)
			{
				text = Settings.Default.DevDataPath;
			}
			if (string.IsNullOrEmpty(text))
			{
				text = Path.Combine(RunningPath, "..\\data");
			}
			else if (!Path.IsPathRooted(text))
			{
				text = Path.Combine(RunningPath, text);
			}
			_dataPath = Path.GetFullPath(text);
			return _dataPath;
		}
	}

	public string RunningPath => Path.GetDirectoryName(typeof(EngineApplication).Assembly.Location);

	internal static bool IsApplicationIdle
	{
		get
		{
			NativeMessage msg;
			return !PeekMessage(out msg, IntPtr.Zero, 0u, 0u, 0u);
		}
	}

	public bool AppStarting
	{
		get
		{
			return m_appStarting;
		}
		set
		{
			m_appStarting = value;
		}
	}

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
			InputSimulator.SimulateLeftMouseDown(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
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
			InputSimulator.SimulateLeftMouseDown(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseDown(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
			InputSimulator.SimulateLeftMouseUp(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
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
		if (!m_mouseDoubleClickHandlers.ContainsKey(name))
		{
		}
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
			LogHelper.Instance.Log("An unhandled exception was raised in IFileCacheService.Remove().", e);
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
			LogHelper.Instance.Log("An unhandled exception was raised in IFileCacheService.FlushCache().", e);
		}
	}

	public void SetFileContent(string fileName, byte[] data, bool overwrite)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (!File.Exists(path) | overwrite)
		{
			File.WriteAllBytes(path, data);
		}
	}

	public void SetFileContent(string fileName, string data, bool overwrite)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		if (!File.Exists(path) | overwrite)
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
		return (!File.Exists(path)) ? null : File.ReadAllBytes(path);
	}

	public string GetFileContentAsString(string fileName)
	{
		string path = Path.Combine(GetCachePath(), fileName);
		return (!File.Exists(path)) ? null : File.ReadAllText(path);
	}

	public void ShutDown()
	{
		System.Windows.Forms.Application.Exit();
	}

	public bool Run()
	{
		HostForm instance = (HostForm)(base.MainForm = new HostForm());
		base.MainForm.BackColor = RenderingService.Instance.BackgroundColor;
		BufferedGraphicsManager.Current.MaximumBuffer = base.MainForm.DisplayRectangle.Size;
		ServiceLocator.Instance.AddService(typeof(ITrackDataService), instance);
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		service.Register(base.MainForm.Handle);
		IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
		ElementHost elementHost = new ElementHost
		{
			Parent = base.MainForm,
			Dock = DockStyle.Fill,
			Visible = false
		};
		elementHost.BackColor = Color.Red;
		_elementHost = elementHost;
		IScene scene = service2.CreateScene("Default", base.MainForm.ClientSize.Width, base.MainForm.ClientSize.Height, RenderingService.Instance.BackgroundColor, elementHost);
		scene.OnWPFHit += Scene_OnWPFHit;
		scene.OnWPFKeyDown += Scene_OnWPFKeyDown;
		scene.OnWPFKeyUp += Scene_OnWPFKeyUp;
		service2.ActiveScene = scene;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			if (System.Windows.Application.Current == null)
			{
				new System.Windows.Application();
			}
			Uri resourceLocator = new Uri("Redbox.Rental.UI;component/Assets/StyleDictionary.xaml", UriKind.Relative);
			System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(resourceLocator) as ResourceDictionary);
			Uri resourceLocator2 = new Uri("Redbox.ControlPanel.UI;component/Assets/StyleDictionary.xaml", UriKind.Relative);
			System.Windows.Application.Current.Resources.MergedDictionaries.Add(System.Windows.Application.LoadComponent(resourceLocator2) as ResourceDictionary);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("E006", "Unable to load WPF resource dictionary.", e));
		}
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
		ErrorForm errorForm = new ErrorForm
		{
			Errors = errorList
		};
		errorForm.ShowDialog();
		return false;
	}

	[DllImport("user32.dll")]
	public static extern bool GetKeyboardState(byte[] lpKeyState);

	[DllImport("user32.dll")]
	public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

	[DllImport("user32.dll")]
	public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags);

	public static char? GetCharFromKey(Key key)
	{
		char? result = null;
		int num = KeyInterop.VirtualKeyFromKey(key);
		byte[] lpKeyState = new byte[256];
		GetKeyboardState(lpKeyState);
		uint wScanCode = MapVirtualKey((uint)num, MapType.MAPVK_VK_TO_VSC);
		StringBuilder stringBuilder = new StringBuilder(2);
		int num2 = ToUnicode((uint)num, wScanCode, lpKeyState, stringBuilder, stringBuilder.Capacity, 0u);
		int num3 = num2;
		if (num3 != -1 && num3 != 0)
		{
			result = stringBuilder[0];
		}
		return result;
	}

	private void Scene_OnWPFKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
	{
		char? charFromKey = GetCharFromKey(e.Key);
		if (charFromKey.HasValue)
		{
			KeyPressEventArgs e2 = new KeyPressEventArgs(charFromKey.Value);
			((HostForm)base.MainForm).OnKeyPress(sender, e2);
		}
	}

	private void Scene_OnWPFKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		Keys keyData = (Keys)KeyInterop.VirtualKeyFromKey(e.Key);
		System.Windows.Forms.KeyEventArgs e2 = new System.Windows.Forms.KeyEventArgs(keyData);
		((HostForm)base.MainForm).OnKeyDown(sender, e2);
	}

	private void Scene_OnWPFHit(IActor actor)
	{
		ActivateMouseClickHandlers(new System.Windows.Forms.MouseEventArgs(MouseButtons.Left, 1, System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y, 0));
		if (!base.MainForm.Focused)
		{
			base.MainForm.Focus();
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		FirewallHelper.CreateFirewallExemptions();
		m_enableMaintenanceMode = false;
		m_corruptDbName = "";
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
		ServiceLocator.Instance.AddService(typeof(IEngineApplication), this);
		errorList.AddRange(KernelExtensionService.Instance.Initialize(new List<Type>
		{
			typeof(BrokerServicesHost),
			typeof(HardwareServicesHost),
			typeof(KioskServicesHost),
			typeof(UpdateServicesHost)
		}));
		errorList.AddRange(InitializeConfiguration());
		ServiceLocator.Instance.AddService(typeof(IObjectCacheService), ObjectCacheService.Instance);
		ServiceLocator.Instance.AddService(typeof(IIdeService), IdeService.Instance);
		ServiceLocator.Instance.AddService(typeof(IInputService), this);
		ServiceLocator.Instance.AddService(typeof(IFileCacheService), this);
		ServiceLocator.Instance.AddService(typeof(IViewService), ViewService.Instance);
		ServiceLocator.Instance.AddService(typeof(ITimerService), TimerService.Instance);
		ServiceLocator.Instance.AddService(typeof(IIdleTimerService), IdleTimerService.Instance);
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
		ServiceLocator.Instance.AddService(typeof(IProductLookupCatalogService), new ProductLookupCatalogService());
		ServiceLocator.Instance.AddService(typeof(IThemeService), ThemeService.Instance);
		IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service2 != null)
		{
			if (service2.TryGetValue<bool>("system", "General", "KioskAnalytics", out var value) & value)
			{
				ServiceLocator.Instance.AddService(typeof(ISessionAnalyticsService), SessionAnalyticsService.Instance);
			}
			if (service2.TryGetValue<bool>("system", "General", "TextToSpeechEnabled", out var value2) & value2)
			{
				ServiceLocator.Instance.AddService(typeof(ITextToSpeechService), TextToSpeechService.Instance);
			}
		}
		InitializePreferencePages();
		errorList.AddRange(SchedulerService.Instance.Initialize());
		errorList.AddRange(DataStoreService.Instance.Initialize());
		errorList.AddRange(FontCacheService.Instance.Initialize());
		errorList.AddRange(BitmapCacheService.Instance.Initialize());
		errorList.AddRange(RenderingService.Instance.Initialize());
		errorList.AddRange(KernelService.Instance.Initialize());
		errorList.AddRange(EnvironmentManager.Instance.Initialize());
		errorList.AddRange(QueueService.Instance.Initialize(Path.Combine(RunningPath, "message.data")));
		errorList.AddRange(ShoppingSessionService.Instance.Initialize(Path.Combine(RunningPath, "session.data")));
		errorList.AddRange(DebugService.Instance.Initialize());
		errorList.AddRange(KernelExtensionService.Instance.LoadAllExtensions());
		Redbox.KioskEngine.ComponentModel.ErrorList errorList2 = ResourceBundleService.Instance.Initialize(System.Windows.Forms.Application.ExecutablePath, Settings.Default.DevBundlePath);
		if (errorList2.ContainsError())
		{
			errorList.AddRange(errorList2);
		}
		errorList.AddRange(InitializeRedboxRental());
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

	public Redbox.KioskEngine.ComponentModel.ErrorList InitializeRedboxRental()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		try
		{
			IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
			IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
			ControlPanelService instance = new ControlPanelService();
			ServiceLocator.Instance.AddService(typeof(IControlPanelService), instance);
			MarketingConfigurationManager marketingConfigurationManager = new MarketingConfigurationManager(service2);
			ServiceLocator.Instance.AddService(typeof(IMarketingConfigurationManager), marketingConfigurationManager);
			StoreManagerService storeManagerService = new StoreManagerService(Country.US, service2);
			ServiceLocator.Instance.AddService(typeof(IStoreManager), storeManagerService);
			KioskHealthConfiguration kioskHealthConfiguration = new KioskHealthConfiguration(service2);
			ServiceLocator.Instance.AddService(typeof(IKioskHealthConfiguration), kioskHealthConfiguration);
			KioskEventService kioskEventService = new KioskEventService(storeManagerService, kioskHealthConfiguration);
			kioskEventService.Start();
			ServiceLocator.Instance.AddService(typeof(IKioskEventService), kioskEventService);
			RentalHealthService instance2 = new RentalHealthService();
			ServiceLocator.Instance.AddService(typeof(IRentalHealthService), instance2);
			KernelHelperService kernelHelperService = new KernelHelperService();
			ServiceLocator.Instance.AddService(typeof(IKernelHelperService), kernelHelperService);
			BlockListData blockListData = new BlockListData(service);
			ServiceLocator.Instance.AddService(typeof(IBlockListData), blockListData);
			BlockListService instance3 = new BlockListService(storeManagerService, blockListData);
			ServiceLocator.Instance.AddService(typeof(IBlockList), instance3);
			InventoryData inventoryData = new InventoryData(kernelHelperService);
			inventoryData.Initialize();
			ServiceLocator.Instance.AddService(typeof(IInventoryData), inventoryData);
			InventoryService instance4 = new InventoryService(inventoryData);
			ServiceLocator.Instance.AddService(typeof(IInventory), instance4);
			PricingService pricingService = new PricingService(service, storeManagerService);
			ServiceLocator.Instance.AddService(typeof(IPricingService), pricingService);
			pricingService.Start();
			DataService dataService = new DataService(service, new DataServiceInstance(), new ProfileData(DataPath, "ProfileImages\\"), new CacheData(), new KioskData(service), this);
			ServiceLocator.Instance.AddService(typeof(IDataService), dataService);
			dataService.Initialize();
			dataService.Start();
			dataService.RefreshIfNeeded();
			IResourceBundleService service3 = ServiceLocator.Instance.GetService<IResourceBundleService>();
			service3.RegisterAssemblyFunctions("Rental Kernel", typeof(RentalDataFunctions).Assembly);
			MarketingAdImpressionService marketingAdImpressionService = new MarketingAdImpressionService(storeManagerService);
			ServiceLocator.Instance.AddService(typeof(IMarketingAdImpressionService), marketingAdImpressionService);
			marketingAdImpressionService.Start();
			MarketingAdService instance5 = new MarketingAdService(marketingConfigurationManager, storeManagerService, marketingAdImpressionService, service3);
			ServiceLocator.Instance.AddService(typeof(IMarketingAdService), instance5);
			RentalShoppingCartService.Instance.Initialize();
			ServiceLocator.Instance.AddService(typeof(IPersonalizationService), new PersonalizationService());
			ServiceLocator.Instance.AddService(typeof(ILoyaltyService), new LoyaltyService());
			RedboxRentalService redboxRentalService = new RedboxRentalService();
			ServiceLocator.Instance.AddService(typeof(IRedboxRental), redboxRentalService);
			errorList.AddRange(redboxRentalService.Initialize());
			KioskHealthService kioskHealthService = new KioskHealthService(kioskHealthConfiguration, kioskEventService);
			ServiceLocator.Instance.AddService(typeof(IKioskHealthService), kioskHealthService);
			TouchScreenHealth touchScreenHealth = new TouchScreenHealth(kioskHealthConfiguration, kioskEventService);
			ServiceLocator.Instance.AddService(typeof(ITouchScreenHealth), touchScreenHealth);
			kioskHealthService.AddHealthItem(touchScreenHealth);
			RouterHealth routerHealth = new RouterHealth(kioskHealthConfiguration, kioskEventService);
			ServiceLocator.Instance.AddService(typeof(IRouterHealth), routerHealth);
			kioskHealthService.AddHealthItem(routerHealth);
			ArcusHealth arcusHealth = new ArcusHealth(kioskHealthConfiguration, kioskEventService);
			ServiceLocator.Instance.AddService(typeof(IArcusHealth), arcusHealth);
			kioskHealthService.AddHealthItem(arcusHealth);
			HardwareCorrectionStatsHealth healthItem = new HardwareCorrectionStatsHealth(kioskHealthConfiguration, kioskEventService);
			kioskHealthService.AddHealthItem(healthItem);
			kioskHealthService.Start();
		}
		catch (Exception ex)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("EngineApplication.InitializeRedboxRental", ex.Message, ""));
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

	public void CorruptDbDetected(string dbName)
	{
		m_corruptDbName = dbName;
		m_enableMaintenanceMode = true;
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
		IIdleTimerService service2 = ServiceLocator.Instance.GetService<IIdleTimerService>();
		ICallbackService service3 = ServiceLocator.Instance.GetService<ICallbackService>();
		service3.Flush();
		IEnvironmentNotificationService service4 = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		IQueueService service5 = ServiceLocator.Instance.GetService<IQueueService>();
		IKernelService service6 = ServiceLocator.Instance.GetService<IKernelService>();
		IRenderingService service7 = ServiceLocator.Instance.GetService<IRenderingService>();
		IFontCacheService service8 = ServiceLocator.Instance.GetService<IFontCacheService>();
		IDataCacheService service9 = ServiceLocator.Instance.GetService<IDataCacheService>();
		IBitmapCacheService service10 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IShoppingSessionService service11 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IDataStoreService service12 = ServiceLocator.Instance.GetService<IDataStoreService>();
		ISchedulerService service13 = ServiceLocator.Instance.GetService<ISchedulerService>();
		IStyleSheetService service14 = ServiceLocator.Instance.GetService<IStyleSheetService>();
		service14.Reset();
		service2?.Reset();
		service.Reset();
		service13.Shutdown();
		service4.Unregister(base.MainForm.Handle);
		service4.Reset();
		if (service7.ActiveScene != null)
		{
			service7.ActiveScene.Clear();
		}
		service10.DropCache();
		service8.DropCache();
		service6.Dispose();
		service9.Shutdown();
		service3.Reset();
		service5.StopQueueWorker();
		service11.Dispose();
		service5.Dispose();
		service12.CloseAllStores();
		IKioskHealthService service15 = ServiceLocator.Instance.GetService<IKioskHealthService>();
		service15.Stop();
		IMarketingAdImpressionService service16 = ServiceLocator.Instance.GetService<IMarketingAdImpressionService>();
		service16.Stop();
		IPricingService service17 = ServiceLocator.Instance.GetService<IPricingService>();
		service17.Stop();
		IKioskEventService service18 = ServiceLocator.Instance.GetService<IKioskEventService>();
		service18.Stop();
		ServiceLocator.Instance.GetService<IKernelExtensionService>()?.DeactivateAllExtensions();
		IConfigurationService service19 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service19 != null)
		{
			Redbox.KioskEngine.ComponentModel.ErrorList errorList = service19.Stop();
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
		SelectBundleForm selectBundleForm = new SelectBundleForm
		{
			SearchPath = ResourceBundleService.Instance.SearchPath.Join(";")
		};
		selectBundleForm.ShowDialog();
		if (selectBundleForm.SelectedBundle != null)
		{
			if (ResourceBundleService.Instance.SetSwitchToBundle(selectBundleForm.SelectedBundle))
			{
				checkMainFormStatus = false;
			}
			ResourceBundleService.Instance.ShowDebugger = selectBundleForm.ShowDebugger;
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
		ServiceLocator.Instance.GetService<IPricingService>()?.ClearOnPricingSetChange();
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
		macroService["RunningPath"] = RunningPath;
		macroService["ProductName"] = "Redbox Kiosk Engine";
		macroService["ProductVersion"] = AssemblyInfoHelper.GetVersion(typeof(Program).Assembly);
	}

	internal void IdleLoop()
	{
		lock (_lockObject)
		{
			while (IsApplicationIdle && !_waitTimer.Enabled)
			{
				using ExecutionTimer executionTimer = new ExecutionTimer();
				SwitchBundleIfNecessary();
				TimerService.Instance.UpdateTimers();
				TweenService.Instance.UpdateTweens();
				while (executionTimer.ElapsedMilliseconds < 10 && CallbackService.Instance.InvokeNextCallback())
				{
				}
				if (m_enableMaintenanceMode)
				{
					IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
					if (service != null && m_corruptDbName.Length > 0 && !service.RaiseCorruptDb(m_corruptDbName))
					{
					}
				}
				RenderScene();
				_waitTimer.Start();
			}
		}
	}

	private void _waitTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		_elementHost.Child.Dispatcher.Invoke(delegate
		{
			IdleLoop();
		});
	}

	internal void OnIdle(object sender, EventArgs e)
	{
		if (_waitTimer == null)
		{
			_waitTimer = new System.Timers.Timer(5.0);
			_waitTimer.Elapsed += _waitTimer_Elapsed;
			_waitTimer.AutoReset = false;
		}
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		try
		{
			ActivateIdleHandlers();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied in the Application.Idle handler.", e2);
		}
		IdleLoop();
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
			if (service.ActiveScene.SceneRenderType == RenderType.WPF)
			{
				return true;
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

	internal void ActivateMouseClickHandlers(System.Windows.Forms.MouseEventArgs e)
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

	internal void ActivateMouseDoubleClickHandlers(System.Windows.Forms.MouseEventArgs e)
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
		SystemEvents.SessionEnding += delegate
		{
			if (base.MainForm != null)
			{
				base.MainForm.Close();
			}
		};
		System.Windows.Forms.Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
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
			System.Windows.Forms.MessageBox.Show(e.Exception.ToString());
		};
		System.Windows.Forms.Application.Idle += OnIdle;
	}

	private static void InitializePreferencePages()
	{
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service != null)
		{
			service.AddPreferencePage("EngineCore_General", "Store", "Engine Core/General", PreferencePageTarget.LocalSystem, new Redbox.KioskEngine.IDE.GeneralPreferencePage());
			service.AddPreferencePage("EngineCore_Ide", "Ide", "Engine Core/IDE", PreferencePageTarget.LocalSystem, new IdePreferencePage());
			service.AddPreferencePage("EngineCore_Resource", "Resource", "Engine Core/Resource", PreferencePageTarget.LocalSystem, new ResourcePreferencePage());
		}
	}

	private Redbox.KioskEngine.ComponentModel.ErrorList InitializeConfiguration()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service2 != null)
		{
			LogHelper.Instance.Log("Starting Update Manager Configuration Service.");
			errorList.AddRange(service2.Start());
			DirectoryInfo directoryInfo = new DirectoryInfo(DataPath);
			try
			{
				FileInfo[] files = directoryInfo.GetFiles("*.config");
				FileInfo[] array = files;
				foreach (FileInfo file in array)
				{
					LogHelper.Instance.Log("> adding " + file.Name);
					string name = Path.GetFileNameWithoutExtension(file.Name);
					service2.Add(name, file.FullName, "default", null, null, delegate
					{
						LogHelper.Instance.Log("A new {0} file was loaded.", Path.GetFileName(file.Name));
						if (name.Equals(ConfigFile.KioskHealth, StringComparison.CurrentCultureIgnoreCase))
						{
							IKioskEventService service3 = ServiceLocator.Instance.GetService<IKioskEventService>();
							if (service3 == null)
							{
								LogHelper.Instance.Log("Event service was not found when trying to trigger a new health config file.");
							}
							else
							{
								service3.Restart();
							}
							IKioskHealthService service4 = ServiceLocator.Instance.GetService<IKioskHealthService>();
							if (service4 == null)
							{
								LogHelper.Instance.Log("Health service was not found when trying to trigger a new health config file.");
							}
							else
							{
								service4.Restart();
							}
						}
					});
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
}
