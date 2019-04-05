using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.FraudServices;
using Redbox.KioskEngine.Environment.FraudServices;

namespace Redbox.FraudServices.Proxy;

public class BarcodeTrackingInfoCacheFile
{
	private string _barcodeTrackingInfoCacheFileName = "BarcodeTrackingInfoCache.json";

	public string DataPath { get; set; }

	public string BarcodeTrackingInfoCacheFileNameFullPath => Path.Combine(DataPath, _barcodeTrackingInfoCacheFileName);

	public string BarcodeTrackingInfoCacheFileName
	{
		get
		{
			return _barcodeTrackingInfoCacheFileName;
		}
		set
		{
			_barcodeTrackingInfoCacheFileName = value;
		}
	}

	public bool WriteBarcodeTrackingInfoCacheFile(List<IBarcodeTrackingInfo> barcodeTrackingInfos)
	{
		bool result = false;
		try
		{
			if (barcodeTrackingInfos == null || barcodeTrackingInfos.Count == 0)
			{
				if (File.Exists(BarcodeTrackingInfoCacheFileNameFullPath))
				{
					File.Delete(BarcodeTrackingInfoCacheFileNameFullPath);
				}
			}
			else
			{
				File.WriteAllText(BarcodeTrackingInfoCacheFileNameFullPath, barcodeTrackingInfos.ToJson());
			}
			result = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in BarcodeTrackingInfoCacheFile.WriteBarcodeTrackingInfoCacheFile.", e);
		}
		return result;
	}

	public bool ReadBarcodeTrackingInfoCacheFile(out List<IBarcodeTrackingInfo> barcodeTrackingInfos)
	{
		barcodeTrackingInfos = new List<IBarcodeTrackingInfo>();
		bool result = false;
		try
		{
			if (File.Exists(BarcodeTrackingInfoCacheFileNameFullPath))
			{
				string text = File.ReadAllText(BarcodeTrackingInfoCacheFileNameFullPath);
				if (string.IsNullOrEmpty(text))
				{
					LogHelper.Instance.Log("Error in ReadBarcodeTrackingInfoCacheFile: file is empty");
				}
				else
				{
					List<BarcodeTrackingInfo> list = text.ToObject<List<BarcodeTrackingInfo>>();
					if (list != null)
					{
						foreach (BarcodeTrackingInfo item in list)
						{
							barcodeTrackingInfos.Add(item);
						}
					}
					if (barcodeTrackingInfos != null)
					{
						result = true;
					}
					else
					{
						LogHelper.Instance.Log("Error in ReadBarcodeTrackingInfoCacheFile: Unable to deserialize file.");
					}
				}
			}
			else
			{
				result = true;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error in BarcodeTrackingService.ReadBarcodeTrackingInfoCacheFile.", e);
		}
		return result;
	}
}
