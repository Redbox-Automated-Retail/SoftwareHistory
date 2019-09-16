using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Redbox.Core;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.FraudServices;
using Redbox.KioskEngine.Environment.FraudServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;

namespace Redbox.Rental.Services;

public class HardwareHelper
{
	public static void ProcessResults(string jobId, string jobName)
	{
		ReadOnlyCollection<IHardwareResult> results = GetResults(jobId);
		ProcessResults(jobName, results);
	}

	public static void ProcessResults(string jobName, ReadOnlyCollection<IHardwareResult> results)
	{
		if (results == null || !results.Any())
		{
			LogHelper.Instance.Log("No Results to process.");
			return;
		}
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		IBarcodeTrackingService service2 = ServiceLocator.Instance.GetService<IBarcodeTrackingService>();
		BarcodeServices decoderUsed = (Enum.TryParse<BarcodeServices>(service?.BarcodeDecoder.ToString(), out var result) ? result : BarcodeServices.None);
		CameraGeneration cameraType = (Enum.TryParse<CameraGeneration>(service?.CurrentCameraGeneration.ToString(), out var result2) ? result2 : CameraGeneration.Unknown);
		string kioskId = ServiceLocator.Instance.GetService<IDataService>()?.GetStore().StoreId.ToString() ?? null;
		IMacroService service3 = ServiceLocator.Instance.GetService<IMacroService>();
		string kioskClientSoftwareVersion = service3["EngineVersion"] ?? "Not Found";
		string hALSoftwareVersion = service3["HALVersion"] ?? "Not Found";
		foreach (IHardwareResult item in results.Where((IHardwareResult x) => x.Code == "CameraStatsJson"))
		{
			IDictionary<string, object> messageData = item.MessageData;
			string itemID = item.ItemID;
			int scanAttempt = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "ReadAttempts").Value ?? ((object)0)).ToString());
			int barcodesRead = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "BarcodesRead").Value ?? ((object)0)).ToString());
			int num = int.Parse((messageData?.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "SecureCodesRead").Value ?? ((object)0)).ToString());
			DateTime result3 = DateTime.MinValue;
			DateTime.TryParse(item.Timestamp, out result3);
			bool markedAsFraud = ServiceLocator.Instance.GetService<IFraudService>().IsPaperFraudCandidate(itemID) && num == 0;
			service2.AddBarcodeTrackingInfoToQueue(new BarcodeTrackingInfo
			{
				KioskId = kioskId,
				Barcode = itemID,
				BarcodesRead = barcodesRead,
				SecureBarcodesRead = num,
				ScanDateTime = result3,
				CameraType = cameraType,
				DecoderUsed = decoderUsed,
				KioskClientSoftwareVersion = kioskClientSoftwareVersion,
				HALSoftwareVersion = hALSoftwareVersion,
				MarkedAsFraud = markedAsFraud,
				ScanAttempt = scanAttempt,
				SourceName = (jobName ?? "none")
			}, null);
		}
	}

	private static ReadOnlyCollection<IHardwareResult> GetResults(string jobId)
	{
		if (string.IsNullOrWhiteSpace(jobId))
		{
			LogHelper.Instance.Log("JobId wasn't provided.");
			return null;
		}
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		ReadOnlyCollection<IHardwareResult> hardwareResults = null;
		service?.GetProgramResults(jobId, out hardwareResults);
		return hardwareResults;
	}
}
