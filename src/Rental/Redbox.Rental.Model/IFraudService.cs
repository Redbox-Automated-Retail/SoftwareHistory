using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public interface IFraudService
{
	bool IsIRSecurityEnabled { get; }

	bool HardwareSupportsIRScan { get; }

	bool IsFraudImageEnabled { get; }

	bool SupportsPaperFraud { get; }

	IEnumerable<TitleType> FraudPeelIncludeFormats { get; }

	IEnumerable<TitleType> FraudPaperExcludedFormats { get; }

	string ImageFolderPath { get; }

	void SendFraudAlert(string barcode, bool isFraud, string readTime, DateTime? returnDate = null);

	void AddToFraudStatistics(string count);

	bool IsPaperFraudCandidate(string barcode);

	void ProcessJobResultsForFraud(string jobId, bool sendStatistic = false);

	void ProcessReturnUnknownEvent(string barcode, string jobId);

	void CleanImageFolder();

	void HandleBeforeReturnMessageProcessing(IMessage message, out object clientData);

	void ProcessReturnResponse(IDictionary<string, object> result, object clientData);

	bool ScheduleFraudSync(DateTime startTime, string userName);

	void CreateFraudSyncTimer();

	void UploadDiscImageFromSync(string imageName, string barcode);
}
