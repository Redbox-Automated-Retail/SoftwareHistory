using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.Core;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;

namespace Redbox.FraudServices.Proxy;

public class UploadDiscImageService : IUploadDiscImageService
{
	private IQueueMessageProcessorService _queueMessageProcessorService;

	private bool _isActive;

	public static UploadDiscImageService Instance => Singleton<UploadDiscImageService>.Instance;

	public int TimerInterval { get; set; }

	public int MaxBatchSize { get; set; }

	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (_isActive != value)
			{
				_isActive = value;
				LogHelper.Instance.Log($"Setting UploadDiscImageService.IsActive = {_isActive}");
				if (_isActive)
				{
					RegisterWithQueueService();
				}
				else
				{
					UnRegisterWithQueueService();
				}
			}
		}
	}

	private void RegisterWithQueueService()
	{
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service != null)
		{
			IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
			if (service2 != null && service2.AwsUploadDiscImageEnabled)
			{
				_queueMessageProcessorService = new AWSUploadDiscImageMessageProcessorService();
			}
			else
			{
				_queueMessageProcessorService = new OldUploadDiscImageMessageProcessorService();
			}
			service.RegisterMessageProcessorService(_queueMessageProcessorService);
		}
		else
		{
			LogHelper.Instance.Log("Error in UploadDiscImageService.RegisterWithQueueService.  Unable to find QueueService.");
		}
	}

	private void UnRegisterWithQueueService()
	{
		if (_queueMessageProcessorService != null)
		{
			ServiceLocator.Instance.GetService<IQueueService>()?.UnRegisterMessageProcessorService(_queueMessageProcessorService);
			_queueMessageProcessorService = null;
		}
	}

	public void UploadDiscImage(int kioskId, string barcode, ImageType imageType, long? transactionId, byte[] image, bool? hardwareSupportsIRScan, string cpAccountNumber, RemoteServiceCallback completeCallback)
	{
		if (_isActive)
		{
			if (barcode != null)
			{
				AddDiscImageToQueue(kioskId, barcode, imageType, transactionId, image, hardwareSupportsIRScan, cpAccountNumber, completeCallback);
			}
			else
			{
				LogHelper.Instance.Log("Error in UploadDiscImage.  barcode is null.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Unable to add disc image info to queue because UploadDiscImageService is not active");
		}
	}

	private void AddDiscImageToQueue(int kioskId, string barcode, ImageType imageType, long? transactionId, byte[] image, bool? hardwareSupportsIRScan, string cpAccountNumber, RemoteServiceCallback completeCallback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				errorList = AddDiscImageToMessageQueue(kioskId, barcode, imageType, transactionId, image, hardwareSupportsIRScan, cpAccountNumber);
				if (!errorList.ContainsError())
				{
					LogHelper.Instance.Log("Added barcode " + barcode + " image to the message queue.");
				}
				else
				{
					LogHelper.Instance.Log("Error in AddDiscImageToQueue.");
					errorList.ToLogHelper();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in UploadDiscImageService.AddDiscImageToMessageQueue.", e);
			}
			finally
			{
				ICallbackService service = ServiceLocator.Instance.GetService<ICallbackService>();
				if (errorList != null)
				{
					remoteServiceResult.Errors.AddRange(errorList);
					remoteServiceResult.Success = !remoteServiceResult.Errors.ContainsError();
				}
				service.EnqueueCallback(new RemoteServiceCallbackEntry
				{
					Callback = completeCallback,
					Result = remoteServiceResult,
					Message = "Invoking completeCallback from UploadDiscImageService.AddDiscImageToMessageQueue."
				});
			}
		});
	}

	private ErrorList AddDiscImageToMessageQueue(int kioskId, string barcode, ImageType imageType, long? transactionId, byte[] image, bool? hardwareSupportsIRScan, string cpAccountNumber)
	{
		ErrorList result = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Adding barcode " + barcode + " image to message queue.");
			IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
			ServiceLocator.Instance.GetService<IMacroService>();
			IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
			byte priority = (byte)service2.UploadDiscImageMessagePriority;
			bool awsUploadDiscImageEnabled = service2.AwsUploadDiscImageEnabled;
			IHardwareState service3 = ServiceLocator.Instance.GetService<IHardwareState>();
			int? num = service3?.CurrentCameraGeneration;
			int? num2 = service3?.BarcodeDecoder;
			string image2 = Convert.ToBase64String(image);
			UploadDiscImageRequest uploadDiscImageRequest = new UploadDiscImageRequest
			{
				KioskId = kioskId,
				Barcode = barcode,
				TransactionId = transactionId,
				Image = image2,
				ImageType = (byte)imageType,
				PropertyBag = new Dictionary<string, string>(),
				SupportsCameraFraudScan = hardwareSupportsIRScan
			};
			if (num.HasValue && num >= 3)
			{
				uploadDiscImageRequest.PropertyBag.Add("CameraType", num.ToString());
			}
			if (num2.HasValue && num2 >= 1)
			{
				uploadDiscImageRequest.PropertyBag.Add("DecoderType", num2.ToString());
			}
			if (awsUploadDiscImageEnabled && transactionId.HasValue)
			{
				uploadDiscImageRequest.PropertyBag.Add("CPAccountNumber", cpAccountNumber?.ToString());
			}
			result = service?.Enqueue(priority, "UploadDiscImage", uploadDiscImageRequest);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in UploadDiscImageService.AddDiscImageToMessageQueue.", e);
		}
		return result;
	}

	private UploadDiscImageService()
	{
	}
}
