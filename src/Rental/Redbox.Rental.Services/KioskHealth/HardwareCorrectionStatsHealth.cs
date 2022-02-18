using System;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskEvents;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.Services.KioskHealth;

public class HardwareCorrectionStatsHealth : IKioskHealthItem
{
	private IKioskHealthConfiguration _configuration;

	private IKioskEventService _kioskEvents;

	private bool _hcsActive;

	private bool _hcsReporting;

	public HardwareCorrectionStatsHealth(IKioskHealthConfiguration configuration, IKioskEventService kioskEvents)
	{
		_configuration = configuration;
		_kioskEvents = kioskEvents;
	}

	public void Start()
	{
		try
		{
			LogHelper.Instance.Log("HardwareCorrectionStats.Start");
			ReadConfiguration();
			if (_hcsActive)
			{
				SendStatsToEvents();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("HardwareCorrectionStats.Start - exception occurred", e);
		}
	}

	public void Stop()
	{
		LogHelper.Instance.Log("HardwareCorrectionStats.Stop");
		_hcsActive = false;
		_hcsReporting = false;
	}

	public void Restart()
	{
		try
		{
			LogHelper.Instance.Log("HardwareCorrectionStats.Restart");
			ReadConfiguration();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("HardwareCorrectionStats.Restart - exception occurred", e);
		}
	}

	private void SendStatsToEvents()
	{
		if (!_hcsActive)
		{
			return;
		}
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			LogHelper.Instance.Log("HardwareCorrectionStats.SendStatsToEvents - IHardwareService is not available.");
			return;
		}
		ErrorList hardwareCorrectionStats = service.GetHardwareCorrectionStats(out var stats);
		if (hardwareCorrectionStats == null)
		{
			return;
		}
		if (hardwareCorrectionStats.Count > 0)
		{
			hardwareCorrectionStats.ToLogHelper();
		}
		int num = 0;
		if (stats != null)
		{
			Guid relatedId = Guid.NewGuid();
			num = stats.Count;
			LogHelper.Instance.Log("HardwareCorrectionStats.SendStatsToEvents - Found {0} Stats.", num);
			if (_hcsReporting)
			{
				stats.ForEach(delegate(IHardwareCorrectionStatistic s)
				{
					KioskEvent kioskEvent = KioskEvent.NewEvent(4, ToSubType(s.StatisticType), s.CorrectionOk, null, relatedId, new { s.ProgramName }.ToJson());
					kioskEvent.EventTime = s.CorrectionTime.ToUniversalTime();
					_kioskEvents.AddEvent(kioskEvent);
				});
			}
		}
		if (num > 0)
		{
			LogHelper.Instance.Log("HardwareCorrectionStats.SendStatsToEvents - Removing {0} Stats.", num);
			service.RemoveHardwareCorrectionStats(delegate
			{
			}, delegate(string jobIdentity, string status)
			{
				LogHelper.Instance.Log($"Message: {status}", LogEntryType.Info);
			}, out var _);
		}
	}

	private int ToSubType(HardwareCorrectionStatisticType statType)
	{
		return (int)(400 + statType);
	}

	private void ReadConfiguration()
	{
		_hcsActive = _configuration.HardwareCorrectionStatsActive;
		_hcsReporting = _configuration.HardwareCorrectionStatsReporting;
		LogHelper.Instance.Log("Hardware Correction Stats Active {0}", _hcsActive);
		LogHelper.Instance.Log("Hardware Correction Stats Reporting {0}", _hcsReporting);
	}
}
