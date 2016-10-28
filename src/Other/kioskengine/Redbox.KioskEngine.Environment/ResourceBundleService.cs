using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Xps.Packaging;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Environment;

public class ResourceBundleService : IResourceBundleService
{
	private string m_applicationPath;

	private BundleCollection m_innerBundles;

	private Dictionary<string, Assembly> _asseblies = new Dictionary<string, Assembly>();

	public static ResourceBundleService Instance => Singleton<ResourceBundleService>.Instance;

	public IEnumerable<IBundleSpecifier> Bundles => InnerBundles.GetAllBundleVersions();

	public IResourceBundleFilter Filter { get; internal set; }

	public IResourceBundleSet ActiveBundleSet { get; internal set; }

	public IResourceBundle ActiveBundle { get; internal set; }

	public string SwitchStatusMessage { get; set; }

	public bool ShowDebugger { get; set; }

	public string[] SearchPath
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return (service != null) ? service.GetValue("Core", "BundleSearchPath", new string[1] { "..\\bundles" }) : new string[1] { "..\\bundles" };
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "BundleSearchPath", value);
		}
	}

	public string Environment
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Store", "Environment", "Production");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Store", "Environment", value);
		}
	}

	public string DefaultBundleName
	{
		get
		{
			IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
			if (service != null && service.TryGetObject("system", "Store", "DefaultBundle", out var value))
			{
				return value.ToString();
			}
			return "Rental Application,*";
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.SetValue("Core", "DefaultBundle", value);
		}
	}

	internal BundleCollection InnerBundles
	{
		get
		{
			if (m_innerBundles == null)
			{
				m_innerBundles = new BundleCollection();
			}
			return m_innerBundles;
		}
	}

	internal IResourceBundle SwitchToBundle { get; set; }

	public bool IsPendingBundleSwitch()
	{
		return SwitchToBundle != null;
	}

	public bool SetSwitchToBundle(IResourceBundle bundle)
	{
		if (bundle == ActiveBundle && MachineSettingsStore.Instance.GetValue("Store", "Environment", "MISSING") != "Integration")
		{
			return false;
		}
		SwitchToBundle = bundle;
		return true;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Switch(out IResourceBundle previousBundle)
	{
		previousBundle = ActiveBundle;
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (SwitchToBundle == null)
		{
			return errorList;
		}
		SwitchStatusForm switchStatusForm = new SwitchStatusForm();
		if (!string.IsNullOrEmpty(SwitchStatusMessage))
		{
			switchStatusForm.StatusLabel = SwitchStatusMessage;
		}
		switchStatusForm.Show();
		Application.DoEvents();
		ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
		IKernelExtensionService service = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		try
		{
			DateTime now = DateTime.Now;
			Redbox.KioskEngine.ComponentModel.ErrorList errors;
			while (!service.CanSwitch(out errors))
			{
				LogHelper.Instance.Log("Unable to switch resource bundles due to switch error from kernel extension(s):", LogEntryType.Error);
				foreach (Redbox.KioskEngine.ComponentModel.Error item in errors)
				{
					LogHelper.Instance.Log("...{0}", item);
				}
				LogHelper.Instance.Log("Wait for 15 seconds before next check.");
				manualResetEvent.WaitOne(15000);
				if ((DateTime.Now - now).TotalMinutes >= 5.0)
				{
					return errorList;
				}
			}
		}
		finally
		{
			manualResetEvent.Close();
		}
		try
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			errorList.AddRange(Deactivate());
			LogHelper.Instance.Log("Activate resource bundle: {0}", SwitchToBundle.Storage.BundlePath);
			ActiveBundle = SwitchToBundle;
			Filter = ActiveBundle.CreateFilter();
			Filter["runtime"] = "Client";
			Filter["platform"] = ".NET/Windows";
			Filter["environment"] = Environment;
			IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
			List<string> list = new List<string>();
			service2.GetSettingNames("system", "Filters", out list);
			foreach (string item2 in list)
			{
				ConfigurationData value = new ConfigurationData();
				if (service2.TryGetConfigurationData("system", "Filters", item2, out value) && value.Value.GetType() == typeof(string))
				{
					Filter[item2] = value.Value;
					LogHelper.Instance.Log("Added filter: {0} = {1}", item2, value.Value);
				}
			}
			SwitchToBundle = null;
			Redbox.REDS.Framework.ErrorList source = ActiveBundle.Activate(Filter, out var manifestInfo);
			source.CopyToLocalCollection(errorList);
			List<IResourceBundle> list2 = new List<IResourceBundle>();
			List<string> list3 = new List<string>();
			list3.Add(manifestInfo.StartupScript);
			List<string> list4 = list3;
			foreach (IBundleSpecifier require in manifestInfo.Requires)
			{
				Redbox.REDS.Framework.ErrorList bundle = InnerBundles.GetBundle(require.Name, require.Version.ToString(), out var bundle2, out var manifestInfo2);
				bundle.CopyToLocalCollection(errorList);
				if (manifestInfo2 == null)
				{
					continue;
				}
				if (manifestInfo2.BundleType == BundleType.Application)
				{
					errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B122", "Only Library and Content bundles may be referenced in the Requires property.", "Specify a bundle that has a type of Library or Content."));
					continue;
				}
				if (!string.IsNullOrEmpty(manifestInfo2.StartupScript) && !list4.Contains(manifestInfo2.StartupScript))
				{
					list4.Add(manifestInfo2.StartupScript);
				}
				list2.Add(bundle2);
			}
			if (!errorList.ContainsError())
			{
				ActiveBundleSet = new ResourceBundleSet($"Bundle Set: {manifestInfo.ProductName}, {manifestInfo.ProductVersion}", ActiveBundle, list2);
				IMacroService service3 = ServiceLocator.Instance.GetService<IMacroService>();
				service3["ProductName"] = manifestInfo.ProductName;
				service3["ProductVersion"] = manifestInfo.ProductVersion;
				errorList.AddRange(service.ResetAllExtensions());
				IKernelService service4 = ServiceLocator.Instance.GetService<IKernelService>();
				foreach (string item3 in list4)
				{
					if (!service4.LoadStartupScript(item3))
					{
						errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B033", $"Unable to load the startup script resource: '{item3}'", "Review the startup script resource for syntax errors."));
					}
				}
				if (!errorList.ContainsError())
				{
					IKernelFunctionRegistryService kernelFunctionRegistryService = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
					if (kernelFunctionRegistryService != null)
					{
						_asseblies.ForEach(delegate(KeyValuePair<string, Assembly> pair)
						{
							kernelFunctionRegistryService.RegisterKernelFunctions(pair.Value, pair.Key);
						});
					}
					service4.RegisterFunctions(kernelFunctionRegistryService.GetKernelFunctions());
					if (ShowDebugger)
					{
						ShowDebugger = false;
						IDebugService service5 = ServiceLocator.Instance.GetService<IDebugService>();
						if (service5 != null)
						{
							service5.IsApplicationRunning = false;
							service5.ActivateDebugger();
						}
					}
					else
					{
						string testControl = manifestInfo.TestControl;
						if (testControl != null && MachineSettingsStore.Instance.GetValue<string>("Store", "Environment") == "Integration")
						{
							if (!service4.IsTestPlanExecuting)
							{
								service4.ExecuteTestPlan(testControl);
							}
						}
						else
						{
							service4.ExecuteApplication();
						}
					}
					ICallbackService service6 = ServiceLocator.Instance.GetService<ICallbackService>();
					service6.Resume();
				}
			}
			LogHelper.Instance.Log("Time to switch bundle: {0}", executionTimer.Elapsed);
			return errorList;
		}
		finally
		{
			switchStatusForm.Close();
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList LoadBundles()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		InnerBundles.Clear();
		List<string> list = new List<string>();
		string[] searchPath = SearchPath;
		foreach (string text in searchPath)
		{
			if (!Path.IsPathRooted(text))
			{
				list.Add(Path.Combine(Path.GetDirectoryName(m_applicationPath), text));
			}
			else
			{
				list.Add(text);
			}
		}
		foreach (string item in list)
		{
			LogHelper.Instance.Log("...Search path: {0}", item);
			if (!Directory.Exists(item))
			{
				errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewWarning("B001", $"Bundle directory does not exist: {item}", "Correct the bundle search path and try again."));
				return errorList;
			}
			string[] files = Directory.GetFiles(item, "*.bundle");
			if (files.Length != 0)
			{
				string[] array = files;
				foreach (string text2 in array)
				{
					ResourceBundle resourceBundle = new ResourceBundle(text2, ResourceBundleStorageType.Zipped);
					Redbox.REDS.Framework.ErrorList errorList2 = resourceBundle.LoadResources();
					if (errorList2.ContainsError())
					{
						foreach (Redbox.REDS.Framework.Error item2 in errorList2)
						{
							errorList.Add(item2.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(item2.Code, item2.Description, item2.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(item2.Code, item2.Description, item2.Details));
						}
						continue;
					}
					Redbox.REDS.Framework.ErrorList source = InnerBundles.AddBundleVersion(resourceBundle);
					source.CopyToLocalCollection(errorList);
					if (!errorList.ContainsError())
					{
						LogHelper.Instance.Log("......Loaded bundle '{0}'.", text2);
					}
				}
				continue;
			}
			string[] files2 = Directory.GetFiles(item, "manifest.resource");
			if (files2.Length == 1)
			{
				ResourceBundle resourceBundle2 = new ResourceBundle(item, ResourceBundleStorageType.FileSystem);
				resourceBundle2.LoadResources();
				Redbox.REDS.Framework.ErrorList source2 = InnerBundles.AddBundleVersion(resourceBundle2);
				source2.CopyToLocalCollection(errorList);
				if (!errorList.ContainsError())
				{
					LogHelper.Instance.Log("......Loaded loose bundle '{0}'.", item);
				}
			}
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize(string applicationPath)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList result = new Redbox.KioskEngine.ComponentModel.ErrorList();
		LogHelper.Instance.Log("Initialize resource bundle service:");
		ServiceLocator.Instance.AddService(typeof(IResourceBundleService), Instance);
		m_applicationPath = applicationPath;
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ActivateDefaultBundle()
	{
		string defaultBundleName = DefaultBundleName;
		BundleSpecifier bundleSpecifier = new BundleSpecifier(defaultBundleName);
		return Activate(bundleSpecifier.Name, bundleSpecifier.Version.ToString());
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Activate(string productName, string productVersion)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		Redbox.REDS.Framework.ErrorList bundle = InnerBundles.GetBundle(productName, productVersion, out var bundle2, out var manifestInfo);
		bundle.CopyToLocalCollection(errorList);
		if (manifestInfo != null && manifestInfo.BundleType != BundleType.Application)
		{
			errorList.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("B122", "Only Application bundles may be activated.", "Specify a bundle that has a type of Application."));
			return errorList;
		}
		if (!errorList.ContainsError())
		{
			SwitchToBundle = bundle2;
		}
		return errorList;
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Deactivate()
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		if (ActiveBundle != null)
		{
			LogHelper.Instance.Log("Deactivate resource bundle: {0}", ActiveBundle.Storage.BundlePath);
		}
		ITweenService service = ServiceLocator.Instance.GetService<ITweenService>();
		if (service != null)
		{
			LogHelper.Instance.Log("Resetting tween service.");
			service.Reset();
		}
		ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
		service2.Flush();
		service2.Suspend();
		Filter = null;
		IKernelExtensionService service3 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		errorList.AddRange(service3.DeactivateAllExtensions());
		LogHelper.Instance.Log("Deactivate all kernel extensions", LogEntryType.Info);
		IMacroService service4 = ServiceLocator.Instance.GetService<IMacroService>();
		service4.Clear(new string[2] { "RunningPath", "EngineVersion" });
		ResetServices(errorList);
		ActiveBundle = null;
		ActiveBundleSet = null;
		return errorList;
	}

	public void Restart()
	{
		IResource manifest = GetManifest(out var manifestInfo);
		if (manifest != null)
		{
			Activate(manifestInfo.ProductName, manifestInfo.ProductVersion);
		}
	}

	public bool HasActiveBundle()
	{
		return ActiveBundle != null;
	}

	public IResourceBundle GetBundle(IBundleSpecifier bundleSpecifier)
	{
		Redbox.REDS.Framework.ErrorList bundle = InnerBundles.GetBundle(bundleSpecifier.Name, bundleSpecifier.Version.ToString(), out var bundle2, out var _);
		return bundle2;
	}

	public IResource GetResource(string resourceName)
	{
		return (ActiveBundleSet != null) ? ActiveBundleSet.GetResource(resourceName, Filter) : null;
	}

	public ReadOnlyCollection<IResource> GetResources(string typeName)
	{
		return (ActiveBundleSet != null) ? ActiveBundleSet.GetResources(typeName, Filter) : new List<IResource>().AsReadOnly();
	}

	public ReadOnlyCollection<IResource> GetResourcesUnfiltered(string typeName)
	{
		return (ActiveBundleSet != null) ? ActiveBundleSet.GetResourcesUnfiltered(typeName) : new List<IResource>().AsReadOnly();
	}

	public IDictionary<string, IResourceType> GetResourceTypes()
	{
		IDictionary<string, IResourceType> result;
		if (ActiveBundleSet == null)
		{
			IDictionary<string, IResourceType> dictionary = new Dictionary<string, IResourceType>();
			result = dictionary;
		}
		else
		{
			result = ActiveBundleSet.GetResourceTypes();
		}
		return result;
	}

	public Font GetFont(string resourceName, out bool resourceFound)
	{
		resourceFound = false;
		IFontCacheService service = ServiceLocator.Instance.GetService<IFontCacheService>();
		IResource resource = GetResource(resourceName);
		if (resource != null)
		{
			float size = 10f;
			FontStyle style = FontStyle.Regular;
			if (resource["point_size"] != null)
			{
				size = Convert.ToSingle(resource["point_size"]);
			}
			if (resource["font_style"] != null)
			{
				style = (FontStyle)resource["font_style"];
			}
			IAspect aspect = resource.GetAspect("content");
			if (aspect != null)
			{
				object content = aspect.GetContent();
				if (content != null)
				{
					resourceFound = true;
					return service.RegisterFont(resource.GetFullName(), (string)resource["font_family"], size, style, (byte[])content);
				}
			}
			if (resource["font_family"] != null)
			{
				resourceFound = true;
				return service.RegisterFont(resource.GetFullName(), (string)resource["font_family"], size, style);
			}
		}
		return service.RegisterFont("default", "Arial", 12f, FontStyle.Bold);
	}

	public IResource GetManifest(out IManifestInfo manifestInfo)
	{
		manifestInfo = null;
		return (ActiveBundle != null) ? ActiveBundle.GetManifest(Filter, out manifestInfo) : null;
	}

	public XmlNode GetXml(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		return (XmlNode)resource.GetAspect("content").GetContent();
	}

	public string GetMovie(string resourceName)
	{
		IFileCacheService service = ServiceLocator.Instance.GetService<IFileCacheService>();
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		byte[] array = (byte[])resource.GetAspect("content").GetContent();
		if (array == null)
		{
			return null;
		}
		string text = Path.Combine(service.GetCachePath(), Path.ChangeExtension(resourceName, ".movie"));
		service.SetFileContent(text, array, overwrite: false);
		return text;
	}

	public byte[] GetSound(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		return (byte[])resource.GetAspect("content").GetContent();
	}

	public XpsDocument GetXpsDocument(string resourceName)
	{
		XpsDocument result = null;
		IResource resource = GetResource(resourceName);
		if (resource != null)
		{
			IAspect aspect = resource.GetAspect("content");
			byte[] array = (byte[])resource.GetAspect("content").GetContent();
			if (array != null)
			{
				string text = string.Format("memorystream://{0}", aspect.Properties["file"]);
				Uri uri = new Uri(text);
				Package package = PackageStore.GetPackage(uri);
				if (package == null)
				{
					MemoryStream stream = new MemoryStream(array);
					package = Package.Open(stream);
					PackageStore.AddPackage(new Uri(text), package);
				}
				result = new XpsDocument(package, CompressionOption.Fast, text);
			}
		}
		return result;
	}

	public string GetScript(string resourceName)
	{
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		object content = resource.GetAspect("content").GetContent();
		if (content is byte[])
		{
			return Encoding.ASCII.GetString((byte[])content);
		}
		return (string)content;
	}

	public Image GetBitmap(string resourceName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IResource resource = GetResource(resourceName);
		if (resource == null)
		{
			return null;
		}
		return service.RegisterImage(resource.GetFullName(), (byte[])resource.GetAspect("content").GetContent());
	}

	public ReadOnlyCollection<object> ExecuteScript(string resourceName)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		return service.ExecuteScript(resourceName);
	}

	private ResourceBundleService()
	{
	}

	private static void ResetServices(List<Redbox.KioskEngine.ComponentModel.Error> errors)
	{
		ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		ITimerService service3 = ServiceLocator.Instance.GetService<ITimerService>();
		IInputService service4 = ServiceLocator.Instance.GetService<IInputService>();
		IFlashService service5 = ServiceLocator.Instance.GetService<IFlashService>();
		ISoundService service6 = ServiceLocator.Instance.GetService<ISoundService>();
		IKernelService service7 = ServiceLocator.Instance.GetService<IKernelService>();
		ISchedulerService service8 = ServiceLocator.Instance.GetService<ISchedulerService>();
		IRenderingService service9 = ServiceLocator.Instance.GetService<IRenderingService>();
		IDataStoreService service10 = ServiceLocator.Instance.GetService<IDataStoreService>();
		IFontCacheService service11 = ServiceLocator.Instance.GetService<IFontCacheService>();
		IStyleSheetService service12 = ServiceLocator.Instance.GetService<IStyleSheetService>();
		IBitmapCacheService service13 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		IShoppingSessionService service14 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		IEnvironmentNotificationService service15 = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		IKernelFunctionRegistryService service16 = ServiceLocator.Instance.GetService<IKernelFunctionRegistryService>();
		service8?.Reset();
		service3?.Reset();
		if (service9.ActiveScene != null)
		{
			service9.ActiveScene.Clear();
		}
		service2.Reset();
		service12?.Reset();
		service13.DropCache();
		service11.DropCache();
		service7.Reset();
		service16.Reset();
		service5.Reset();
		service4.Reset();
		service15.Reset();
		service14.Reset();
		service.Reset();
		service10.CloseAllStores();
	}

	public void RegisterAssemblyFunctions(string name, Assembly assembly)
	{
		_asseblies[name] = assembly;
	}
}
