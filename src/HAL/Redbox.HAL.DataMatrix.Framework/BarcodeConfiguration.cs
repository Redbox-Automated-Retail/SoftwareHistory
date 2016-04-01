using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeConfiguration : AttributeXmlConfiguration
{
	internal readonly ReadOnlyCollection<string> ComputeFilterList;

	internal readonly IDictionary<string, object> Properties;

	internal readonly string WorkingPath;

	private readonly string[] LegacyProperties = new string[13]
	{
		"Mode", "VideoFormat", "WorkingPath", "CaptureFrameRate", "CameraDeviceName", "CameraDriver", "CompressionCodecName", "VideoCaptureSpinWaitTime", "BitmapSelectionStrategy", "CenteredDiskSnapTime",
		"VideocapActiveXSpinTime", "CenterDiskBeforeRead", "CycleCameraOnUse"
	};

	private static readonly object InstanceLock = new object();

	[XmlConfiguration(DefaultValue = "true")]
	[Category("Settings")]
	[DisplayName("Delete Bitmap On Successful Read")]
	[Description("If true, will delete the bitmap used to read the barcode, if successfully read.")]
	public bool DeleteBitmapOnSuccessfulRead { get; set; }

	[DisplayName("Camera Plugin")]
	[Description("Sets the plugin to use in legacy scan mode.")]
	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "DirectShowFrame")]
	public string CameraPlugin { get; set; }

	[XmlConfiguration(DefaultValue = "NONE")]
	[DisplayName("Snap Decode Port")]
	[Category("Settings")]
	[Description("The port the snap/decode device is configured.")]
	public string SnapDecodePort { get; set; }

	[DisplayName("Write Pause")]
	[XmlConfiguration(DefaultValue = "500")]
	[Category("Settings")]
	[Description("In ms.")]
	public int WritePause { get; set; }

	[Category("Settings")]
	[DisplayName("Reboot On Start")]
	[Description("In ms.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool RebootOnStart { get; private set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Cycle Camera On Use")]
	[Category("Settings")]
	[Description("If true, starts/stops the camera; otherwise, keeps it on.")]
	public bool CycleCameraOnUse { get; private set; }

	[Description("If true, uses CI to take a photo & try decode.")]
	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Use CI On Fail")]
	public bool UseInliteOnFail { get; private set; }

	[DisplayName("Scanner Service")]
	[XmlConfiguration(DefaultValue = "Legacy")]
	[Category("Settings")]
	[Description("The configured scanner.")]
	public ScannerServices ScannerService { get; set; }

	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("The configured scanner.")]
	[DisplayName("Log Detailed Scan")]
	public bool LogDetailedScan { get; private set; }

	[Category("Settings")]
	[DisplayName("Insert Read Stats")]
	[Description("If true, will insert read data.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool InsertReadStats { get; private set; }

	[Description("For the scanner, the max number of barcodes.")]
	[XmlConfiguration(DefaultValue = "4")]
	[DisplayName("Expected Read Codes")]
	[Category("Settings")]
	public int ExpectedCodes { get; private set; }

	[Category("Settings")]
	[Description("In ms, the time to wait for a scan to complete.")]
	[XmlConfiguration(DefaultValue = "2000")]
	[DisplayName("Scan Timeout")]
	public int ScanTimeout { get; private set; }

	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "15000")]
	[DisplayName("Scanner Wakeup Pause")]
	[Description("In ms, the time to wait for the scanner after boot.")]
	public int ScannerWakeupPause { get; private set; }

	[XmlConfiguration(DefaultValue = "Decode")]
	[Description("The action to take on startup of the Cortex camera.")]
	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Cortex Wakeup Action")]
	public CortexStartupAction CortexStartupAction { get; private set; }

	[Browsable(false)]
	[Description("If true, will trim read code count to 4.")]
	[DisplayName("Filter Excess Codes")]
	[XmlConfiguration(DefaultValue = "true")]
	[Category("Settings")]
	public bool FilterExcessReadCodes { get; private set; }

	[XmlConfiguration(DefaultValue = "50")]
	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Cortex Port Open Wait")]
	[Description("Time to wait for the port after open (in ms).")]
	public int CortexPortOpenWait { get; private set; }

	[DisplayName("Cortex Snap On Decode Failure")]
	[XmlConfiguration(DefaultValue = "false")]
	[Browsable(false)]
	[Category("Settings")]
	[Description("If true, will snap an image on decode failure.")]
	public bool CortexSnapOnDecodeFailure { get; private set; }

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
				ServiceLocator.Instance.AddService(typeof(IScannerDeviceService), scannerDeviceFactory);
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
			XmlNode xmlNode2 = document.DocumentElement.SelectSingleNode(FileNodeName);
			if (xmlNode2 == null)
			{
				XmlElement newChild = document.CreateElement(FileNodeName);
				document.DocumentElement.AppendChild(newChild);
				StoreProperties(document, new ErrorList());
				return;
			}
			List<XmlNode> list = new List<XmlNode>();
			using (new DisposeableList<XmlNode>(list))
			{
				string[] legacyProperties = LegacyProperties;
				foreach (string text in legacyProperties)
				{
					foreach (XmlNode childNode in xmlNode2.ChildNodes)
					{
						if (childNode.Name == text)
						{
							list.Add(childNode);
						}
					}
				}
				foreach (XmlNode item in list)
				{
					xmlNode2.RemoveChild(item);
				}
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
		string[] list = new string[1] { "redbox" };
		ComputeFilterList = new ReadOnlyCollection<string>(list);
		Properties = new Dictionary<string, object>();
		Properties["WorkingPath"] = WorkingPath;
		Properties["VideoFormat"] = new Size(640, 480);
	}
}
