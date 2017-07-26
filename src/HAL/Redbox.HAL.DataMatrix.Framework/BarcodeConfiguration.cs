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

	[Category("Settings")]
	[DisplayName("Delete Bitmap On Successful Read")]
	[Description("If true, will delete the bitmap used to read the barcode, if successfully read.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool DeleteBitmapOnSuccessfulRead { get; set; }

	[Category("Settings")]
	[DisplayName("Camera Plugin")]
	[Description("Sets the plugin to use in legacy scan mode.")]
	[XmlConfiguration(DefaultValue = "DirectShowFrame")]
	public string CameraPlugin { get; set; }

	[Category("Settings")]
	[DisplayName("Snap Decode Port")]
	[Description("The port the snap/decode device is configured.")]
	[XmlConfiguration(DefaultValue = "NONE")]
	public string SnapDecodePort { get; set; }

	[Category("Settings")]
	[DisplayName("Write Pause")]
	[Description("In ms.")]
	[XmlConfiguration(DefaultValue = "500")]
	public int WritePause { get; set; }

	[Category("Settings")]
	[DisplayName("Reboot On Start")]
	[Description("In ms.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool RebootOnStart { get; private set; }

	[Category("Settings")]
	[DisplayName("Cycle Camera On Use")]
	[Description("If true, starts/stops the camera; otherwise, keeps it on.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool CycleCameraOnUse { get; private set; }

	[Category("Settings")]
	[DisplayName("Use CI On Fail")]
	[Description("If true, uses CI to take a photo & try decode.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool UseInliteOnFail { get; private set; }

	[Category("Settings")]
	[DisplayName("Scanner Service")]
	[Description("The configured scanner.")]
	[XmlConfiguration(DefaultValue = "Legacy")]
	public ScannerServices ScannerService { get; set; }

	[Category("Settings")]
	[DisplayName("Log Detailed Scan")]
	[Description("The configured scanner.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool LogDetailedScan { get; private set; }

	[Category("Settings")]
	[DisplayName("Insert Read Stats")]
	[Description("If true, will insert read data.")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool InsertReadStats { get; private set; }

	[Category("Settings")]
	[DisplayName("Expected Read Codes")]
	[Description("For the scanner, the max number of barcodes.")]
	[XmlConfiguration(DefaultValue = "4")]
	public int ExpectedCodes { get; private set; }

	[Category("Settings")]
	[DisplayName("Scan Timeout")]
	[Description("In ms, the time to wait for a scan to complete.")]
	[XmlConfiguration(DefaultValue = "2000")]
	public int ScanTimeout { get; private set; }

	[Category("Settings")]
	[DisplayName("Scanner Wakeup Pause")]
	[Description("In ms, the time to wait for the scanner after boot.")]
	[XmlConfiguration(DefaultValue = "15000")]
	public int ScannerWakeupPause { get; private set; }

	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Cortex Wakeup Action")]
	[Description("The action to take on startup of the Cortex camera.")]
	[XmlConfiguration(DefaultValue = "Decode")]
	public CortexStartupAction CortexStartupAction { get; private set; }

	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Filter Excess Codes")]
	[Description("If true, will trim read code count to 4.")]
	[XmlConfiguration(DefaultValue = "true")]
	public bool FilterExcessReadCodes { get; private set; }

	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Cortex Port Open Wait")]
	[Description("Time to wait for the port after open (in ms).")]
	[XmlConfiguration(DefaultValue = "50")]
	public int CortexPortOpenWait { get; private set; }

	[Browsable(false)]
	[Category("Settings")]
	[DisplayName("Cortex Snap On Decode Failure")]
	[Description("If true, will snap an image on decode failure.")]
	[XmlConfiguration(DefaultValue = "false")]
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
				ServiceLocator.Instance.GetService<IConfigurationService>().RegisterConfiguration(Configurations.Camera.ToString(), Instance);
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
