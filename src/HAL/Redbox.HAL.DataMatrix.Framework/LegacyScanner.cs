using System;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class LegacyScanner : ScannerAdapter, IScannerDevice, IDisposable
{
	private int m_cycleCount;

	public bool RequiresExternalLighting => true;

	public bool IsConnected { get; private set; }

	public bool SupportsRestart => Plugin.SupportsReset;

	public bool InError { get; private set; }

	internal static LegacyScanner Instance => Singleton<LegacyScanner>.Instance;

	private ICameraPlugin Plugin { get; set; }

	public bool Start()
	{
		if (IsConnected)
		{
			return true;
		}
		try
		{
			InError = false;
			return IsConnected = Plugin.Start();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Start() caught an exception.", e);
			return IsConnected = false;
		}
	}

	public SnapResult Snap()
	{
		return Snap(GenerateSnapFileName());
	}

	public SnapResult Snap(string snapFile)
	{
		Plugin.Snap(snapFile);
		SnapResult snapResult = TestSnap(snapFile);
		if (!snapResult.SnapOk)
		{
			InError = true;
		}
		return snapResult;
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

	public void TurnOff()
	{
		if (!IsConnected)
		{
			return;
		}
		IsConnected = false;
		try
		{
			Plugin.TurnOff();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("TurnOffCamera encountered an exception.", e);
		}
	}

	public bool Restart()
	{
		if (!Plugin.SupportsReset)
		{
			return false;
		}
		TurnOff();
		PerfFunctions.SpinWait(500);
		if (++m_cycleCount == 1)
		{
			m_cycleCount = 0;
			LogHelper.Instance.Log("[Legacy Camera Service] Cycle camera driver.");
			using DeviceHelper helper = new DeviceHelper(debug: false);
			CameraInfo cameraInfo = new CameraInfo(helper);
			if (cameraInfo.FindActiveCamera())
			{
				LogHelper.Instance.Log("Found camera {0}", cameraInfo.Revision.ToString());
				if (cameraInfo.Revision == KnownCameras.ThirdGen || cameraInfo.Revision == KnownCameras.FourthGen)
				{
					LogHelper.Instance.Log("The running driver for the camera {0}", cameraInfo.CorrectDriverRunning() ? "is correct" : "is not correct.");
				}
				if (DeviceStatus.Enabled == cameraInfo.Status)
				{
					bool flag = cameraInfo.ResetDriver();
					LogHelper.Instance.Log(" RESET camera returned {0}", flag ? "OK" : "FAILURE");
				}
			}
		}
		return Start();
	}

	public ScanResult Scan()
	{
		string snapFile = GenerateSnapFileName();
		ScanResult scanResult = new ScanResult(Snap(snapFile));
		if (!scanResult.SnapOk)
		{
			scanResult.ResetOnException();
			return scanResult;
		}
		BarcodeReader.Instance.Scan(scanResult.SnapResult.Path, scanResult);
		return scanResult;
	}

	public string FindInstalledPort()
	{
		return "NONE";
	}

	protected override void DisposeInner()
	{
		Plugin.TurnOff();
	}

	private LegacyScanner()
		: base(ScannerServices.Legacy)
	{
		LoadPluginsInner();
	}

	private void LoadPluginsInner()
	{
		string cameraPlugin = BarcodeConfiguration.Instance.CameraPlugin;
		CameraPluginDescriptor cameraPluginDescriptor = PluginLoader.Instance.LocatePlugin(cameraPlugin, out var plugin);
		if (cameraPluginDescriptor == null)
		{
			LogHelper.Instance.Log($"Unable to locate plugin {cameraPlugin}", LogEntryType.Error);
			Plugin = new NilCameraPlugin();
		}
		else
		{
			LogHelper.Instance.Log($"Successfully loaded camera plugin {cameraPlugin}");
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
}
