using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Xml;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeConfiguration : AttributeXmlConfiguration
{
	private readonly Size VideoFormat = new Size(640, 480);

	private readonly string[] LegacyProperties = new string[13]
	{
		"Mode", "VideoFormat", "WorkingPath", "CaptureFrameRate", "CameraDeviceName", "CameraDriver", "CompressionCodecName", "VideoCaptureSpinWaitTime", "BitmapSelectionStrategy", "CenteredDiskSnapTime",
		"VideocapActiveXSpinTime", "CenterDiskBeforeRead", "CycleCameraOnUse"
	};

	private string m_workingDirectory;

	public static BarcodeConfiguration Instance => Singleton<BarcodeConfiguration>.Instance;

	[DisplayName("Delete Bitmap On Successful Read")]
	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "true")]
	[Description("If true, will delete the bitmap used to read the barcode, if successfully read.")]
	public bool DeleteBitmapOnSuccessfulRead { get; set; }

	[XmlConfiguration(DefaultValue = "DirectShowFrame")]
	[DisplayName("Camera Plugin")]
	[Category("Settings")]
	[Description("Sets the plugin to use in legacy scan mode.")]
	public string CameraPlugin { get; set; }

	[Description("The port the snap/decode device is configured.")]
	[DisplayName("Snap Decode Port")]
	[Category("Settings")]
	[XmlConfiguration(DefaultValue = "NONE")]
	public string SnapDecodePort { get; set; }

	[DisplayName("Write Pause")]
	[Category("Settings")]
	[Description("In ms.")]
	[XmlConfiguration(DefaultValue = "500")]
	public int WritePause { get; private set; }

	[Description("In ms.")]
	[DisplayName("Reboot On Start")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Settings")]
	public bool RebootOnStart { get; private set; }

	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Enable Infared")]
	[Category("Settings")]
	[Description("If true, will save images where a datamatrix was not read.")]
	public bool EnableInfared { get; private set; }

	[Category("Settings")]
	[Description("If true, starts/stops the camera; otherwise, keeps it on.")]
	[DisplayName("Cycle Camera On Use")]
	[XmlConfiguration(DefaultValue = "false")]
	public bool CycleCameraOnUse { get; private set; }

	[DisplayName("Use CI On Fail")]
	[Description("If true, uses CI to take a photo & try decode.")]
	[XmlConfiguration(DefaultValue = "false")]
	[Category("Settings")]
	public bool UseInliteOnFail { get; private set; }

	[XmlConfiguration(DefaultValue = "Legacy")]
	[DisplayName("Scanner Service")]
	[Category("Settings")]
	[Description("The configured scanner.")]
	public ScannerServices ScannerService { get; set; }

	[DisplayName("Log Detailed Scan")]
	[XmlConfiguration(DefaultValue = "false")]
	[Description("The configured scanner.")]
	[Category("Settings")]
	public bool LogDetailedScan { get; private set; }

	[Category("Settings")]
	[Description("If true, will insert read data.")]
	[XmlConfiguration(DefaultValue = "false")]
	[DisplayName("Insert Read Stats")]
	public bool InsertReadStats { get; private set; }

	[XmlConfiguration(DefaultValue = "4")]
	[Category("Settings")]
	[Description("For the scanner, the max number of barcodes.")]
	[DisplayName("Expected Read Codes")]
	public int ExpectedCodes { get; private set; }

	[Category("Settings")]
	[Description("In ms, the time to wait for a scan to complete.")]
	[XmlConfiguration(DefaultValue = "2000")]
	[DisplayName("Scan Timeout")]
	public int ScanTimeout { get; private set; }

	[XmlConfiguration(DefaultValue = "15000")]
	[DisplayName("Scanner Wakeup Pause")]
	[Category("Settings")]
	[Description("In ms, the time to wait for the scanner after boot.")]
	public int ScannerWakeupPause { get; private set; }

	[DisplayName("Cortex Wakeup Action")]
	[Category("Settings")]
	[Browsable(false)]
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
	[XmlConfiguration(DefaultValue = "50")]
	[Category("Settings")]
	[DisplayName("Cortex Port Open Wait")]
	[Description("Time to wait for the port after open (in ms).")]
	public int CortexPortOpenWait { get; private set; }

	protected override string FileNodeName => "Camera";

	internal ReadOnlyCollection<string> ComputeFilterList { get; private set; }

	internal IDictionary<string, object> Properties { get; private set; }

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

	public static bool Upgrade(TextWriter writer, string configurationFile)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			int num = 0;
			xmlDocument.Load(configurationFile);
			if (xmlDocument.DocumentElement == null)
			{
				writer.WriteLine("There is no document element present in the XML.");
				return false;
			}
			BarcodeConfiguration barcodeConfiguration = new BarcodeConfiguration();
			XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("Barcode");
			if (xmlNode != null)
			{
				xmlDocument.DocumentElement.RemoveChild(xmlNode);
				num++;
			}
			XmlNode xmlNode2 = xmlDocument.DocumentElement.SelectSingleNode(barcodeConfiguration.FileNodeName);
			if (xmlNode2 == null)
			{
				XmlElement newChild = xmlDocument.CreateElement(barcodeConfiguration.FileNodeName);
				xmlDocument.DocumentElement.AppendChild(newChild);
				num++;
				barcodeConfiguration.StoreProperties(xmlDocument, new ErrorList());
			}
			else
			{
				List<XmlNode> list = new List<XmlNode>();
				using (new DisposeableList<XmlNode>(list))
				{
					string[] legacyProperties = barcodeConfiguration.LegacyProperties;
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
					if (list.Count > 0)
					{
						num++;
					}
					foreach (XmlNode item in list)
					{
						xmlNode2.RemoveChild(item);
					}
				}
			}
			if (num > 0)
			{
				xmlDocument.Save(configurationFile);
				writer.WriteLine("SUCCESSFULLY upgraded decode configuration.");
			}
			return true;
		}
		catch (Exception ex)
		{
			writer.WriteLine("BarcodeConfiguration.Upgrade FAILED with an unhandled exception.");
			writer.WriteLine(ex.Message);
			return false;
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

	private BarcodeConfiguration()
		: base("barcodeConfig", typeof(BarcodeConfiguration))
	{
		string[] list = new string[1] { "redbox" };
		ComputeFilterList = new ReadOnlyCollection<string>(list);
		Properties = new Dictionary<string, object>();
		Properties["WorkingPath"] = WorkingPath;
		Properties["VideoFormat"] = VideoFormat;
	}
}
