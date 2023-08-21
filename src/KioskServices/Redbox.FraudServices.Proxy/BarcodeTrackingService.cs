using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Redbox.Core;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.FraudServices;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;

namespace Redbox.FraudServices.Proxy;

public class BarcodeTrackingService : IBarcodeTrackingService
{
	private List<IBarcodeTrackingInfo> _barcodeTrackingInfoCacheList = new List<IBarcodeTrackingInfo>();

	private readonly object _lockObject = new object();

	private readonly BarcodeTrackingInfoCacheFile _barcodeTrackingInfoCacheFile;

	private Timer _sendMessageTimer;

	private IQueueMessageProcessorService _queueMessageProcessorService;

	private bool _isActive;

	public static BarcodeTrackingService Instance => Singleton<BarcodeTrackingService>.Instance;

	public string CacheFileDataPath
	{
		get
		{
			return _barcodeTrackingInfoCacheFile.DataPath;
		}
		set
		{
			if (_barcodeTrackingInfoCacheFile != null)
			{
				_barcodeTrackingInfoCacheFile.DataPath = value;
			}
		}
	}

	public int TimerInterval { get; set; }

	public int MaxBatchSize { get; set; }

	public int BarcodeTrackingMessagePriority { get; set; }

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
				LogHelper.Instance.Log($"Setting BarcodeTrackingService.IsActive = {_isActive}");
				if (_isActive)
				{
					RegisterWithQueueService();
					StartSendMessageTimer();
				}
				else
				{
					StopSendMessageTimer();
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
			if (service2 != null && service2.AwsBarcodeTrackingEnabled)
			{
				_queueMessageProcessorService = new AWSBarcodeTrackingMessageProcessorService();
			}
			else
			{
				_queueMessageProcessorService = new OldBarcodeTrackingMessageProcessorService();
			}
			service.RegisterMessageProcessorService(_queueMessageProcessorService);
		}
		else
		{
			LogHelper.Instance.Log("Error in BarcodeTrackingServices.RegisterWithQueueService.  Unable to find QueueService.");
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

	public void StartSendMessageTimer()
	{
		if (!_isActive || _sendMessageTimer != null || TimerInterval <= 0)
		{
			return;
		}
		_sendMessageTimer = new Timer(delegate
		{
			LogHelper.Instance.Log("BarcodeTrackingServices send message timer executing.");
			try
			{
				if (_sendMessageTimer != null)
				{
					_sendMessageTimer.Change(-1, -1);
				}
				AddCachedBarcodeTrackingInfosToMessageQueue(null);
			}
			finally
			{
				if (_sendMessageTimer != null)
				{
					_sendMessageTimer.Change(TimerInterval, TimerInterval);
				}
			}
		}, null, 0, TimerInterval);
	}

	public void StopSendMessageTimer()
	{
		if (_sendMessageTimer != null)
		{
			_sendMessageTimer.Dispose();
			_sendMessageTimer = null;
		}
	}

	public void AddBarcodeTrackingInfoToQueue(IBarcodeTrackingInfo barcodeTrackingInfo, RemoteServiceCallback completeCallback)
	{
		if (_isActive)
		{
			if (barcodeTrackingInfo != null)
			{
				PerformAddBarcodeTrackingInfoToQueue(barcodeTrackingInfo, completeCallback);
			}
			else
			{
				LogHelper.Instance.Log("Error in AddBarcodeTrackingInfoToQueue.  barcodeTrackingInfo is null.");
			}
		}
		else
		{
			LogHelper.Instance.Log("Unable to add barcode tracking info to queue because BarcodeTrackingService is not active");
		}
	}

	public void AddCachedBarcodeTrackingInfosToMessageQueue(RemoteServiceCallback completeCallback)
	{
		if (_isActive)
		{
			PerformAddBarcodeTrackingInfoToQueue(null, completeCallback, forceMessageSend: true);
		}
		else
		{
			LogHelper.Instance.Log("Unable to add cached barcode tracking info to message queue because BarcodeTrackingService is not active");
		}
	}

	private void PerformAddBarcodeTrackingInfoToQueue(IBarcodeTrackingInfo barcodeTrackingInfo, RemoteServiceCallback completeCallback, bool forceMessageSend = false)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			ErrorList errorList = null;
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			try
			{
				lock (_lockObject)
				{
					List<IBarcodeTrackingInfo> barcodeTrackingInfoCacheList = _barcodeTrackingInfoCacheList;
					if (barcodeTrackingInfoCacheList == null || barcodeTrackingInfoCacheList.Count == 0)
					{
						_barcodeTrackingInfoCacheFile.ReadBarcodeTrackingInfoCacheFile(out _barcodeTrackingInfoCacheList);
					}
					if (barcodeTrackingInfo != null)
					{
						LogHelper.Instance.Log("Adding barcode tracking info to queue: barcode: " + barcodeTrackingInfo?.Barcode);
						_barcodeTrackingInfoCacheList.Add(barcodeTrackingInfo);
					}
					if (((_barcodeTrackingInfoCacheList.Count >= MaxBatchSize) | forceMessageSend) && _barcodeTrackingInfoCacheList.Count > 0)
					{
						List<IBarcodeTrackingInfo> list = new List<IBarcodeTrackingInfo>();
						list.AddRange(_barcodeTrackingInfoCacheList.Take(MaxBatchSize));
						errorList = AddBarcodeTrackingToMessageQueue(list);
						if (!errorList.ContainsError())
						{
							list.ForEach(delegate(IBarcodeTrackingInfo x)
							{
								_barcodeTrackingInfoCacheList.Remove(x);
							});
						}
						else
						{
							LogHelper.Instance.Log("Error in PerformAddBarcodeTrackingInfoToQueue.");
							errorList.ToLogHelper();
						}
					}
					_barcodeTrackingInfoCacheFile.WriteBarcodeTrackingInfoCacheFile(_barcodeTrackingInfoCacheList);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BarcodeTrackingServices.AddBarcodeTrackingInfoToMessageQueue.", e);
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
					Message = "Invoking completeCallback from BarcodeTrackingServices.AddBarcodeTrackingInfoToMessageQueue."
				});
			}
		});
	}

	private ErrorList AddBarcodeTrackingToMessageQueue(List<IBarcodeTrackingInfo> barcodeTrackingInfoList)
	{
		ErrorList result = new ErrorList();
		try
		{
			LogHelper.Instance.Log($"Adding barcode tracking message to message queue.  barcode tracking info count: {barcodeTrackingInfoList?.Count ?? 0}");
			IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
			ServiceLocator.Instance.GetService<IMacroService>();
			IBarcodeTrackingInfo barcodeTrackingInfo = barcodeTrackingInfoList.FirstOrDefault();
			int.TryParse(barcodeTrackingInfo?.KioskId, out var result2);
			AddBarcodeTrackingRequest addBarcodeTrackingRequest = new AddBarcodeTrackingRequest
			{
				KioskId = result2,
				EngineVersion = barcodeTrackingInfo.KioskClientSoftwareVersion,
				HalVersion = barcodeTrackingInfo.HALSoftwareVersion,
				BarcodeTrackingList = new List<AddBarcodeTracking>()
			};
			foreach (IBarcodeTrackingInfo barcodeTrackingInfo2 in barcodeTrackingInfoList)
			{
				addBarcodeTrackingRequest.BarcodeTrackingList.Add(new AddBarcodeTracking
				{
					SourceName = barcodeTrackingInfo2.SourceName,
					BarcodesRead = barcodeTrackingInfo2.BarcodesRead,
					SecureBarcodesRead = barcodeTrackingInfo2.SecureBarcodesRead,
					Barcode = barcodeTrackingInfo2.Barcode,
					CameraType = (int)barcodeTrackingInfo2.CameraType,
					ScanDateTime = barcodeTrackingInfo2.ScanDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
					DecoderUsed = (int)barcodeTrackingInfo2.DecoderUsed,
					MarkedAsFraud = barcodeTrackingInfo2.MarkedAsFraud,
					ScanAttempt = barcodeTrackingInfo2.ScanAttempt
				});
			}
			IConfiguration service2 = ServiceLocator.Instance.GetService<IConfiguration>();
			result = service?.Enqueue((byte)(service2?.BarcodeTrackingMessagePriority ?? 90), "BarcodeTracking", addBarcodeTrackingRequest);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BarcodeTrackingServices.AddBarcodeTrackingToMessageQueue.", e);
		}
		return result;
	}

	private BarcodeTrackingService()
	{
		_barcodeTrackingInfoCacheFile = new BarcodeTrackingInfoCacheFile();
	}
}
