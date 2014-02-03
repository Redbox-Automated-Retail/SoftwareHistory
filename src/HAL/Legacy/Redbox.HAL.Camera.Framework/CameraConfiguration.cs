using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraConfiguration : XmlConfiguration
{
	private string m_workingDirectory;

	public static CameraConfiguration Instance => Singleton<CameraConfiguration>.Instance;

	[DisplayName("Driver Mode")]
	[Category("Settings")]
	[ValidValueListProvider("GetValidModes")]
	[ExcludeType]
	[Description("The mode under which the camera driver runs. If set to Emulated, all camera driver functions will be emulated based on the configured emulation state.")]
	public CameraMode Mode { get; set; }

	[DisplayName("Video Format")]
	[Description("The size of the video capture frame.  Larger frame sizes yield a higher capture resolution at the cost of high CPU utilization, memory usage, and disk space.")]
	[Category("Settings")]
	public Size VideoFormat { get; set; }

	[Description("If true, starts/stops the camera; otherwise, keeps it on.")]
	[DisplayName("Cycle Camera On Use")]
	[Category("Settings")]
	public bool CycleCameraOnUse { get; set; }

	[Description("If true, will delete the bitmap used to read the barcode, if successfully read.")]
	[DisplayName("Delete Bitmap On Successful Read")]
	[Category("Settings")]
	public bool DeleteBitmapOnSuccessfulRead { get; set; }

	[Description("The plugin to use.")]
	[Category("Settings")]
	[DisplayName("Camera Driver")]
	public string CameraDriver { get; set; }

	[Category("Settings")]
	[Description("In ms, the time to wait for an image.")]
	[DisplayName("Camera Snap Timeout")]
	public int SnapTimeout { get; set; }

	[Category("Settings")]
	[Description("In ms, the time to sleep after start.")]
	[DisplayName("Camera Wakeup Sleep")]
	public int CameraWakeupSleep { get; set; }

	internal IDictionary<string, object> Properties { get; set; }

	internal string WorkingPath
	{
		get
		{
			if (!string.IsNullOrEmpty(m_workingDirectory))
			{
				return m_workingDirectory;
			}
			return m_workingDirectory = Path.GetFullPath(Path.Combine(RuntimeService.Instance.AssemblyDirectory, "..\\Video"));
		}
	}

	public string[] GetValidModes()
	{
		return new string[2]
		{
			CameraMode.Normal.ToString(),
			CameraMode.Emulated.ToString()
		};
	}

	protected override void LoadProperties(XmlDocument document, ErrorList errors)
	{
		Mode = GetNodeValue(document.DocumentElement, "Camera/Mode", CameraMode.Normal);
		int nodeValue = GetNodeValue(document.DocumentElement, "Camera/VideoFormat/Width", 640);
		int nodeValue2 = GetNodeValue(document.DocumentElement, "Camera/VideoFormat/Height", 480);
		VideoFormat = new Size(nodeValue, nodeValue2);
		DeleteBitmapOnSuccessfulRead = GetNodeValue(document.DocumentElement, "Camera/DeleteBitmapOnSuccessfulRead", defaultValue: true);
		SnapTimeout = GetNodeValue(document.DocumentElement, "Camera/SnapTimeout", 5000);
		CameraWakeupSleep = GetNodeValue(document.DocumentElement, "Camera/CameraWakeupSleep", 10000);
		CameraDriver = GetNodeValue(document.DocumentElement, "Camera/CameraDriver", "Vidcap");
		if (RuntimeService.Instance.GetPlatform() == Platform.Windows7 && CameraDriver == "Vidcap")
		{
			LogHelper.Instance.Log("Platform is Windows7; hijack control to DirectShow.");
			CameraDriver = "DirectShow";
		}
		CycleCameraOnUse = GetNodeValue(document.DocumentElement, "Camera/CycleCameraOnUse", defaultValue: false);
		if (CycleCameraOnUse)
		{
			LogHelper.Instance.Log("WARNING: CycleCameraOnUse is set to TRUE.");
		}
		SaveProperties();
	}

	protected override void StoreProperties(XmlDocument document, ErrorList errors)
	{
		XmlNode xmlNode = document.DocumentElement.SelectSingleNode("Camera/Mode");
		if (xmlNode != null)
		{
			xmlNode.InnerText = Mode.ToString();
		}
		XmlNode xmlNode2 = document.DocumentElement.SelectSingleNode("Camera/VideoFormat/Width");
		if (xmlNode2 != null)
		{
			xmlNode2.InnerText = XmlConvert.ToString(VideoFormat.Width);
		}
		XmlNode xmlNode3 = document.DocumentElement.SelectSingleNode("Camera/VideoFormat/Height");
		if (xmlNode3 != null)
		{
			xmlNode3.InnerText = XmlConvert.ToString(VideoFormat.Height);
		}
		XmlNode xmlNode4 = document.DocumentElement.SelectSingleNode("Camera/DeleteBitmapOnSuccessfulRead");
		if (xmlNode4 != null)
		{
			xmlNode4.InnerText = XmlConvert.ToString(DeleteBitmapOnSuccessfulRead);
		}
		XmlNode xmlNode5 = document.DocumentElement.SelectSingleNode("Camera/CameraDriver");
		if (xmlNode5 != null)
		{
			xmlNode5.InnerText = CameraDriver;
		}
		else
		{
			SetSingleNodeValue(document.DocumentElement, "Camera/CameraDriver", CameraDriver);
		}
		XmlNode xmlNode6 = document.DocumentElement.SelectSingleNode("Camera/CycleCameraOnUse");
		if (xmlNode6 != null)
		{
			xmlNode6.InnerText = XmlConvert.ToString(CycleCameraOnUse);
		}
		else
		{
			SetSingleNodeValue(document.DocumentElement, "Camera/CycleCameraOnUse", CycleCameraOnUse);
		}
		XmlNode xmlNode7 = document.DocumentElement.SelectSingleNode("Camera/SnapTimeout");
		if (xmlNode7 != null)
		{
			xmlNode7.InnerText = XmlConvert.ToString(SnapTimeout);
		}
		else
		{
			SetSingleNodeValue(document.DocumentElement, "Camera/SnapTimeout", SnapTimeout);
		}
		SetSingleNodeValue(document.DocumentElement, "Camera/CameraWakeupSleep", CameraWakeupSleep);
	}

	private CameraConfiguration()
		: base("cameraConfig")
	{
		SnapTimeout = 5000;
		Mode = CameraMode.Normal;
		VideoFormat = new Size(640, 480);
		CameraDriver = "DirectShow";
		CameraWakeupSleep = 5000;
		Properties = new Dictionary<string, object>();
		SaveProperties();
	}

	private void SaveProperties()
	{
		Properties["WorkingPath"] = WorkingPath;
		Properties["Mode"] = Mode;
		Properties["VideoFormat"] = VideoFormat;
		Properties["SnapTimeout"] = SnapTimeout;
		Properties["WakeupPause"] = CameraWakeupSleep;
	}
}
