using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

public class FraudServiceFunctions
{
	[KernelFunction(Name = "kernel.is_ir_security_enabled")]
	internal static bool IsIRSecurityEnabled()
	{
		return ServiceLocator.Instance.GetService<IFraudService>()?.IsIRSecurityEnabled ?? false;
	}

	[KernelFunction(Name = "kernel.is_paper_fraud")]
	internal static bool IsPaperFraudExcluded(string barcode)
	{
		return ServiceLocator.Instance.GetService<IFraudService>()?.IsPaperFraudCandidate(barcode) ?? false;
	}

	[KernelFunction(Name = "kernel.send_fraud_alert")]
	internal static void SendFraudAlertPaper(string barcode, bool isFraud, string readTime)
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.SendFraudAlert(barcode, isFraud, readTime);
	}

	[KernelFunction(Name = "kernel.send_fraud_clean_statistic")]
	internal static void SendFraudCleanStatistic(string count)
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.AddToFraudStatistics(count);
	}

	[KernelFunction(Name = "kernel.process_job_for_frauds")]
	internal static void ProcessJobResultsForFraudDiscs(string jobId, bool sendStatistic = false)
	{
		IFraudService fs = ServiceLocator.Instance.GetService<IFraudService>();
		Task.Run(delegate
		{
			fs?.ProcessJobResultsForFraud(jobId, sendStatistic);
		});
	}

	[KernelFunction(Name = "kernel.process_return_unknown_event")]
	internal static void ProcessReturnUnknownEvent(string barcode, string jobId)
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.ProcessReturnUnknownEvent(barcode, jobId);
	}

	[KernelFunction(Name = "kernel.clean_fraud_images")]
	internal static void CleanFraudImages()
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.CleanImageFolder();
	}

	[KernelFunction(Name = "kernel.supports_paper_fraud")]
	internal static bool RegisterBarcodeToTitleTypeCallback()
	{
		return ServiceLocator.Instance.GetService<IFraudService>().SupportsPaperFraud;
	}

	[KernelFunction(Name = "kernel.upload_disc_image_from_sync")]
	internal static void UploadDiscImage(string barcode, string filename)
	{
		ServiceLocator.Instance.GetService<IFraudService>().UploadDiscImageFromSync(filename, barcode);
	}

	[KernelFunction(Name = "kernel.peeled_fraud_image_upload_enabled")]
	internal static bool IsPeeledFraudEnabled()
	{
		return ServiceLocator.Instance.GetService<IFraudService>().IsFraudImageEnabled;
	}
}
