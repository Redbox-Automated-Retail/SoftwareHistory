using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeConfiguration : XmlConfiguration
{
	private readonly string[] SplitString = new string[1] { ";" };

	public static BarcodeConfiguration Instance => Singleton<BarcodeConfiguration>.Instance;

	[DisplayName("Scan Timeout (in milliseconds)")]
	[Category("Settings")]
	[Description("The amount of time (in milliseconds) the barcode decoder will attempt to find a valid Data Matrix barcode.")]
	public int ScanTimeout { get; set; }

	[Description("A semi-colon separated list of identifiers to exclude in an image scan.")]
	[DisplayName("Scan Filter Codes")]
	[Category("Settings")]
	public string ScanFilterCodes { get; set; }

	[Category("Settings")]
	[ExcludeType]
	[DisplayName("Barcode Reader Mode")]
	[Description("The mode under which the Data Matrix barcode reader runs.  If set to Emulated, all barcode reader functions will be emulated based on the configured emulation state.")]
	[ValidValueListProvider("GetModes")]
	public BarcodeReaderMode Mode { get; set; }

	[Description("Some cameras vertically invert frames as they are captured. If set to Yes, the barcode reader will vertically flip each frame before scanning begins.")]
	[Category("Behavior")]
	[DisplayName("Vertically Flip Image")]
	public bool VerticallyFlipImage { get; set; }

	[Description("Continue with frame ranking while looking at the image.")]
	[Category("Behavior")]
	[DisplayName("ClearImageFrameRanking")]
	public bool ClearImageFrameRanking { get; set; }

	public string[] GetModes()
	{
		return new string[2]
		{
			BarcodeReaderMode.ClearImage.ToString(),
			BarcodeReaderMode.Emulated.ToString()
		};
	}

	internal List<string> ComputeFilterList()
	{
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(ScanFilterCodes))
		{
			return list;
		}
		string[] array = ScanFilterCodes.Split(SplitString, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string item in array2)
		{
			list.Add(item);
		}
		return list;
	}

	protected override void LoadProperties(XmlDocument document, ErrorList errors)
	{
		XmlElement documentElement = document.DocumentElement;
		ScanFilterCodes = GetNodeValue(documentElement, "Barcode/ScanFilterCodes", "redbox");
		Mode = GetNodeValue(documentElement, "Barcode/Mode", BarcodeReaderMode.ClearImage);
		ScanTimeout = GetNodeValue(documentElement, "Barcode/ScanTimeout", 500);
		VerticallyFlipImage = GetNodeValue(documentElement, "Barcode/VerticallyFlipImage", defaultValue: false);
		ClearImageFrameRanking = GetNodeValue(documentElement, "Barcode/ClearImageFrameRanking", defaultValue: false);
	}

	protected override void StoreProperties(XmlDocument document, ErrorList errors)
	{
		SetSingleNodeValue(document.DocumentElement, "Barcode/ScanFilterCodes", ScanFilterCodes);
		SetSingleNodeValue(document.DocumentElement, "Barcode/Mode", Mode.ToString());
		SetSingleNodeValue(document.DocumentElement, "Barcode/ScanTimeout", XmlConvert.ToString(ScanTimeout));
		SetSingleNodeValue(document.DocumentElement, "Barcode/VerticallyFlipImage", XmlConvert.ToString(VerticallyFlipImage));
		SetSingleNodeValue(document.DocumentElement, "Barcode/ClearImageFrameRanking", XmlConvert.ToString(ClearImageFrameRanking));
	}

	private BarcodeConfiguration()
		: base("barcodeConfig")
	{
		ScanTimeout = 500;
		VerticallyFlipImage = false;
		Mode = BarcodeReaderMode.ClearImage;
		ClearImageFrameRanking = false;
		ScanFilterCodes = "redbox";
	}
}
