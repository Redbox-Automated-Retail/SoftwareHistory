using System;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class LegacyScanner : ScannerAdapter, IScannerDevice, IDisposable, IConfigurationObserver
{
	private enum CameraGeneration
	{
		Unknown,
		First,
		Second,
		Third,
		Fourth,
		Fifth
	}

	private ICameraPlugin Plugin;

	public bool RequiresExternalLighting => true;

	public bool IsConnected { get; private set; }

	public bool SupportsRestart => Plugin.SupportsReset;

	public bool InError { get; private set; }

	public string Name => Service.ToString();

	public bool Start()
	{
		return Start(test: false);
	}

	public bool Start(bool test)
	{
		if (IsConnected)
		{
			return true;
		}
		try
		{
			InError = false;
			IsConnected = Plugin.Start();
			if (!test)
			{
				return IsConnected;
			}
			ISnapResult snapResult = Snap();
			return snapResult.SnapOk;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Start() caught an exception.", e);
			return IsConnected = false;
		}
	}

	public ISnapResult Snap()
	{
		return Snap(GenerateSnapFileName());
	}

	public ISnapResult Snap(string snapFile)
	{
		Plugin.Snap(snapFile);
		SnapResult snapResult = new SnapResult(snapFile);
		if (!snapResult.SnapOk)
		{
			InError = true;
		}
		return snapResult;
	}

	public bool ValidateSettings()
	{
		return false;
	}

	public bool TestConfiguration(IMessageSink sink)
	{
		return true;
	}

	public bool Stop()
	{
		if (!IsConnected)
		{
			return false;
		}
		IsConnected = false;
		try
		{
			return Plugin.Stop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("LegacyCameraService.Stop() caught an exception.", e);
			return false;
		}
	}

	public bool Restart()
	{
		if (!Plugin.SupportsReset)
		{
			return false;
		}
		Stop();
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		service.SpinWait(500);
		LogHelper.Instance.Log("[Legacy Camera Service] Cycle camera driver.");
		IUsbDeviceService service2 = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		IDeviceDescriptor deviceDescriptor = service2.FindActiveCamera(matchDriver: false);
		if (deviceDescriptor != null)
		{
			LogHelper.Instance.Log("Found camera {0}", deviceDescriptor.Friendlyname);
			CameraGeneration cameraGeneration = From(deviceDescriptor);
			if (CameraGeneration.Third == cameraGeneration || CameraGeneration.Fourth == cameraGeneration)
			{
				LogHelper.Instance.Log("The running driver for the camera {0}", deviceDescriptor.MatchDriver() ? "is correct" : "is not correct.");
			}
			DeviceStatus deviceStatus = service2.FindDeviceStatus(deviceDescriptor);
			if ((deviceStatus & DeviceStatus.Found) != DeviceStatus.None && (deviceStatus & DeviceStatus.Enabled) != DeviceStatus.None)
			{
				bool flag = deviceDescriptor.ResetDriver();
				LogHelper.Instance.Log(" RESET camera returned {0}", flag ? "OK" : "FAILURE");
			}
		}
		return Start();
	}

	public IScanResult Scan()
	{
		string text = GenerateSnapFileName();
		ISnapResult snapResult = Snap(text);
		if (snapResult.SnapOk)
		{
			IBarcodeReader configuredReader = BarcodeReaderFactory.Instance.GetConfiguredReader();
			IScanResult scanResult = configuredReader.Scan(text);
			if (BarcodeConfiguration.Instance.DeleteBitmapOnSuccessfulRead && scanResult.DecodeResults.Count > 0)
			{
				try
				{
					File.Delete(text);
				}
				catch (Exception ex)
				{
					LogHelper.Instance.Log("[LegacyCameraService] Unable to delete file '{0}'", text);
					LogHelper.Instance.Log(ex.Message);
				}
			}
			return scanResult;
		}
		ScanResult scanResult2 = new ScanResult();
		scanResult2.DeviceError = true;
		return scanResult2;
	}

	public void NotifyConfigurationLoaded()
	{
		if (BarcodeConfiguration.Instance.ScannerService == Service && Plugin == null)
		{
			LoadPlugin(BarcodeConfiguration.Instance.CameraPlugin);
		}
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		if (BarcodeConfiguration.Instance.ScannerService == Service && Plugin == null)
		{
			LoadPlugin(BarcodeConfiguration.Instance.CameraPlugin);
		}
	}

	public string FindInstalledPort()
	{
		return "NONE";
	}

	protected override void DisposeInner()
	{
		Plugin.Stop();
	}

	internal LegacyScanner()
		: base(ScannerServices.Legacy)
	{
	}

	private void LoadPlugin(string driver)
	{
		CameraPluginDescriptor cameraPluginDescriptor = PluginLoader.Instance.LocatePlugin(driver, out var plugin);
		if (cameraPluginDescriptor == null)
		{
			LogHelper.Instance.Log($"Unable to locate plugin {driver}", LogEntryType.Error);
			Plugin = new NilCameraPlugin();
		}
		else
		{
			LogHelper.Instance.Log($"Successfully loaded camera plugin {driver}");
			Plugin = plugin;
			if (cameraPluginDescriptor.DescriptorPath != null)
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(cameraPluginDescriptor.DescriptorPath);
				if (xmlDocument.DocumentElement != null)
				{
					XmlNode xmlNode = xmlDocument.SelectSingleNode("Plugin/Properties");
					if (xmlNode != null)
					{
						foreach (XmlNode childNode in xmlNode.ChildNodes)
						{
							if (!childNode.Name.Equals("property"))
							{
								continue;
							}
							string value = childNode.Attributes["name"].Value;
							string value2 = childNode.Attributes["value"].Value;
							XmlAttribute xmlAttribute = childNode.Attributes["type"];
							if (xmlAttribute == null)
							{
								BarcodeConfiguration.Instance.Properties[value] = value2;
								continue;
							}
							try
							{
								Type type = Type.GetType(xmlAttribute.Value);
								BarcodeConfiguration.Instance.Properties[value] = Convert.ChangeType(value2, type);
							}
							catch
							{
								BarcodeConfiguration.Instance.Properties[value] = value2;
							}
						}
					}
				}
			}
		}
		Plugin.InitWithProperties(BarcodeConfiguration.Instance.Properties);
	}

	private CameraGeneration From(IDeviceDescriptor descriptor)
	{
		if (descriptor == null)
		{
			return CameraGeneration.Unknown;
		}
		if (descriptor.Vendor.Equals("1871") && (descriptor.Product.Equals("0d01", StringComparison.CurrentCultureIgnoreCase) || descriptor.Product.Equals("0f01", StringComparison.CurrentCultureIgnoreCase)))
		{
			return CameraGeneration.Fourth;
		}
		if (descriptor.Vendor.Equals("0c45", StringComparison.CurrentCultureIgnoreCase) && descriptor.Product.Equals("627b", StringComparison.CurrentCultureIgnoreCase))
		{
			return CameraGeneration.Third;
		}
		return CameraGeneration.Unknown;
	}
}
