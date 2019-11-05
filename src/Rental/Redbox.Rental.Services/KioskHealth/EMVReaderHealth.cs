using System;
using System.Threading;
using Redbox.Core;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.Services.KioskHealth;

public class EMVReaderHealth : IKioskHealthItem, IEMVReaderHealth
{
	private readonly IKioskHealthConfiguration _configuration;

	private readonly IKioskEventService _kioskEvents;

	private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private readonly int _lockTimeout = 5000;

	private bool _EMVReaderActive;

	private bool _EMVReaderTrackMultipleFallbacks;

	private int _EMVReaderMultipleFallbacksThreshold;

	private int _technicalFallbackSessionCount;

	private Guid? _firstTechnicalFallbackEventId;

	private string KioskId => ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId.ToString() ?? "0";

	private string TechnicalFallbackSessionEventData => new
	{
		TechnicalFallbackSessionCount = 1 + _technicalFallbackSessionCount,
		TechnicalFallbackSessionThreshold = _EMVReaderMultipleFallbacksThreshold
	}.ToJson();

	private string TechnicalFallbackSessionResetEventData => new
	{
		TechnicalFallbackSessionTrackingReset = true
	}.ToJson();

	public EMVReaderHealth(IKioskHealthConfiguration configuration, IKioskEventService kioskEvents)
	{
		_configuration = configuration;
		_kioskEvents = kioskEvents;
	}

	public void Start()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.Start", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.Start - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void Stop()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.Stop", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			InternalStop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.Stop - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void InternalStart()
	{
		try
		{
			ReadConfiguration();
			if (!_EMVReaderActive)
			{
				LogHelper.Instance.Log("EMVReaderHealth is not active");
				return;
			}
			LogHelper.Instance.Log("EMVReaderHealth.InternalStart");
			Reset();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.InternalStart - exception occurred", e);
		}
	}

	private void InternalStop()
	{
		try
		{
			LogHelper.Instance.Log("EMVReaderHealth.InternalStop");
			_EMVReaderActive = false;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.InternalStop - exception occurred", e);
		}
	}

	public void Restart()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.Restart", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LogHelper.Instance.Log("EMVReaderHealth.ReStart");
			InternalStop();
			InternalStart();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.Restart - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void SessionWasInTechnicalFallback(bool inFallback)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("EMVReaderHealth.SessionWasInTechnicalFallback", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (!_EMVReaderActive || !_EMVReaderTrackMultipleFallbacks)
			{
				return;
			}
			if (inFallback)
			{
				KioskEvent kioskEvent;
				if (_firstTechnicalFallbackEventId.HasValue)
				{
					kioskEvent = KioskEvent.NewEvent(KioskEventType.KioskHealthEMVReader, 700, success: true, null, data: TechnicalFallbackSessionEventData, relatedId: _firstTechnicalFallbackEventId);
				}
				else
				{
					kioskEvent = KioskEvent.NewEvent(KioskEventType.KioskHealthEMVReader, 700, success: true, null, null, TechnicalFallbackSessionEventData);
					kioskEvent.RelatedId = kioskEvent.Id;
					_firstTechnicalFallbackEventId = kioskEvent.Id;
				}
				_technicalFallbackSessionCount++;
				if (_technicalFallbackSessionCount % _EMVReaderMultipleFallbacksThreshold == 0)
				{
					SendAlert(kioskEvent, "MultipleTechnicalFallbackTransactions");
				}
				_kioskEvents.AddEvent(kioskEvent);
			}
			else
			{
				if (_technicalFallbackSessionCount > 0)
				{
					_kioskEvents.AddEvent(KioskEvent.NewEvent(KioskEventType.KioskHealthEMVReader, 701, success: true, null, data: TechnicalFallbackSessionResetEventData, relatedId: _firstTechnicalFallbackEventId));
				}
				Reset();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("EMVReaderHealth.SessionWasInTechnicalFallback - exception occurred", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	private void SendAlert(KioskEvent kioskEvent, string subType)
	{
		ServiceLocator.Instance.GetService<IHealthServices>()?.SendAlert(KioskId, "HardwareError", subType, kioskEvent.Data, DateTime.Now, null);
	}

	private void Reset()
	{
		_technicalFallbackSessionCount = 0;
		_firstTechnicalFallbackEventId = null;
	}

	private void ReadConfiguration()
	{
		_EMVReaderActive = _configuration.EMVReaderActive;
		_EMVReaderTrackMultipleFallbacks = _configuration.EMVReaderTrackMultipleFallbacks;
		_EMVReaderMultipleFallbacksThreshold = _configuration.EMVReaderMultipleFallbacksThreshold;
		LogHelper.Instance.Log($"EMVReaderHealth Active {_EMVReaderActive}");
		LogHelper.Instance.Log($"EMVReaderHealth Track Multiple Technical Fallback Sessions Over Threshold {_EMVReaderTrackMultipleFallbacks}");
		LogHelper.Instance.Log($"EMVReaderHealth Multiple Technical Fallback Sessions Threshold {_EMVReaderMultipleFallbacksThreshold}");
	}
}
