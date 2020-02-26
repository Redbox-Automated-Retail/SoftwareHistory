using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeConfiguration : AttributeXmlConfiguration
{
	internal readonly IDictionary<string, object> Properties;

	internal readonly string WorkingPath;

	private readonly string[] LegacyProperties = new string[15]
	{
		"Mode", "VideoFormat", "WorkingPath", "CaptureFrameRate", "CameraDeviceName", "CameraDriver", "CompressionCodecName", "VideoCaptureSpinWaitTime", "BitmapSelectionStrategy", "CenteredDiskSnapTime",
		"VideocapActiveXSpinTime", "CenterDiskBeforeRead", "CycleCameraOnUse", "DeleteBitmapOnSuccessfulRead", "IRHardwareInstallDate"
	};

	private static readonly object InstanceLock = new object();

	[Description("Sets the plugin to use in legacy scan mode.")]
	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "DirectShowFrame")]
	[DisplayName("Camera Plugin")]
	public string CameraPlugin { get; set; }

	[Category("Settings")]
	[Description("The port the snap/decode device is configured.")]
	[DisplayName("Snap Decode Port")]
	[XmlConfiguration(DefaultValue = "NONE")]
	public string SnapDecodePort { get; set; }

	[XmlConfiguration(DefaultValue = "500")]
	[DisplayName("Write Pause")]
	[Description("In ms.")]
	[Category("Settings")]
	public int WritePause { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Cycle Camera On Use")]
	[Description("If true, starts/stops the camera; otherwise, keeps it on.")]
	[Category("Settings")]
	public bool CycleCameraOnUse { get; private set; }

	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Use CI On Fail")]
	[Description("If true, uses CI to take a photo & try decode.")]
	public bool UseInliteOnFail { get; private set; }

	[XmlConfiguration(DefaultValue = "Legacy")]
	[Category("Settings")]
	[DisplayName("Scanner Service")]
	[Description("The configured scanner.")]
	public ScannerServices ScannerService { get; set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Category("Settings")]
	[DisplayName("Log Detailed Scan")]
	[Description("The configured scanner.")]
	public bool LogDetailedScan { get; private set; }

	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "4")]
	[DisplayName("Expected Read Codes")]
	[Description("For the scanner, the max number of barcodes.")]
	public int ExpectedCodes { get; private set; }

	[Description("In ms, the time to wait for a scan to complete.")]
	[XmlConfiguration(DefaultValue = "2000")]
	[Category("Settings")]
	[DisplayName("Scan Timeout")]
	public int ScanTimeout { get; private set; }

	[Category("Settings")]
	[DisplayName("Scanner Wakeup Pause")]
	[Description("In ms, the time to wait for the scanner after boot.")]
	[XmlConfiguration(DefaultValue = "15000")]
	public int ScannerWakeupPause { get; private set; }

	[Description("If true, will trim read code count to 4.")]
	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "true")]
	[DisplayName("Filter Excess Codes")]
	[Browsable(false)]
	public bool FilterExcessReadCodes { get; private set; }

	[Category("Settings")]
	[DisplayName("Cortex Port Open Wait")]
	[Description("Time to wait for the port after open (in ms).")]
	[XmlConfiguration(DefaultValue = "50")]
	[Browsable(false)]
	public int CortexPortOpenWait { get; private set; }

	[XmlConfiguration(DefaultValue = "false")]
	[Description("If true, will snap an image on decode failure.")]
	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Cortex Snap On Decode Failure")]
	public bool CortexSnapOnDecodeFailure { get; private set; }

	[Description("Size of the Cortex port buffer.")]
	[Browsable(false)]
	[DisplayName("Cortex Port Buffer Size")]
	[XmlConfiguration(DefaultValue = "8192")]
	[Category("Settings")]
	public int CortexPortBufferSize { get; private set; }

	[Category("Settings")]
	[Browsable(false)]
	[DisplayName("Use Runtime Path")]
	[Description("If true, uses runtime video path")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool UseRuntimePath { get; set; }

	[Category("Settings")]
	[DisplayName("Use Cortex HD Field")]
	[Browsable(false)]
	[Description("If true, uses HD field for images")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool UseCortexHDField { get; set; }

	[Category("Settings")]
	[DisplayName("IR HardwareInstallDate")]
	[Description("Timestamp of when the IR hardware was installed.")]
	[XmlConfiguration(DefaultValue = "NONE")]
	[Browsable(false)]
	public string IRHardwareInstallDate { get; set; }

	[Category("Settings")]
	[DisplayName("Use CI For Fraud Validation")]
	[Description("If true, uses CI to support fraud validation.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool UseInliteForFraudValidation { get; private set; }

	protected override string FileNodeName => "Camera";

	internal static BarcodeConfiguration Instance { get; private set; }

	public static BarcodeConfiguration NewInstance()
	{
		lock (InstanceLock)
		{
			if (Instance == null)
			{
				Instance = new BarcodeConfiguration();
				IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
				service.RegisterConfiguration(Configurations.Camera.ToString(), Instance);
				ScannerDeviceFactory scannerDeviceFactory = new ScannerDeviceFactory();
				ServiceLocator.Instance.AddService<IScannerDeviceService>(scannerDeviceFactory);
				Instance.AddObserver(scannerDeviceFactory);
			}
			return Instance;
		}
	}

	public static BarcodeConfiguration MakeNewInstance2()
	{
		lock (InstanceLock)
		{
			if (Instance == null)
			{
				Instance = new BarcodeConfiguration();
			}
			return Instance;
		}
	}

	public string[] GetModes()
	{
		return new string[0];
	}

	protected override void LoadPropertiesInner(XmlDocument document, ErrorList errors)
	{
	}

	protected override void StorePropertiesInner(XmlDocument document, ErrorList errors)
	{
	}

	protected override void ImportInner(ErrorList errors)
	{
	}

	protected override void UpgradeInner(XmlDocument document, ErrorList errors)
	{
		try
		{
			XmlNode xmlNode = document.DocumentElement.SelectSingleNode("Barcode");
			if (xmlNode != null)
			{
				document.DocumentElement.RemoveChild(xmlNode);
			}
			XmlNode cameraNode = document.DocumentElement.SelectSingleNode(FileNodeName);
			if (cameraNode == null)
			{
				XmlElement newChild = document.CreateElement(FileNodeName);
				document.DocumentElement.AppendChild(newChild);
				StoreProperties(document, new ErrorList());
				return;
			}
			List<XmlNode> toRemove = new List<XmlNode>();
			using (new DisposeableList<XmlNode>(toRemove))
			{
				Array.ForEach(LegacyProperties, delegate(string property)
				{
					foreach (XmlNode childNode in cameraNode.ChildNodes)
					{
						if (childNode.Name == property)
						{
							toRemove.Add(childNode);
						}
					}
				});
				XmlNode xmlNode2 = toRemove.Find((XmlNode x) => x.Name == "IRHardwareInstallDate");
				if (xmlNode2 != null && !string.IsNullOrEmpty(xmlNode2.InnerText))
				{
					Redbox.HAL.Controller.Framework.KioskConfiguration.Instance.SetConfig("IRHardwareInstallDate", xmlNode2.InnerText, saveValue: true);
				}
				toRemove.ForEach(delegate(XmlNode n)
				{
					cameraNode.RemoveChild(n);
				});
			}
		}
		catch (Exception e)
		{
			errors.Add(Error.NewError("U002", "Barcode configuration upgrade failure.", e));
		}
	}

	private BarcodeConfiguration()
		: base("barcodeConfig", typeof(BarcodeConfiguration))
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		WorkingPath = service.InstallPath("Video");
		Properties = new Dictionary<string, object>();
		Properties["WorkingPath"] = WorkingPath;
		Properties["VideoFormat"] = new Size(640, 480);
	}
}
