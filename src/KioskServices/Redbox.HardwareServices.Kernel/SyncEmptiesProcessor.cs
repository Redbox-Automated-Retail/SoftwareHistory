using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.HardwareServices.Kernel;

internal class SyncEmptiesProcessor
{
	private IHardwareService _hardwareService;

	private bool _isRunning;

	private bool _isEnabled;

	private object _lockObject = new object();

	internal static SyncEmptiesProcessor Instance => Singleton<SyncEmptiesProcessor>.Instance;

	private SyncEmptiesProcessor()
	{
		_hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (_hardwareService == null)
		{
			LogHelper.Instance.Log("SyncEmptiesProcessor: Unable to locate Hardware Service.");
		}
		else if (!ServiceLocator.Instance.GetService<IConfigurationService>().TryGetValue<bool>(ConfigFile.System, ConfigCategory.General, ConfigSetting.SyncEmptyEnabled, out _isEnabled))
		{
			_isEnabled = true;
		}
	}

	internal void StartJob()
	{
		if (!_isEnabled)
		{
			return;
		}
		lock (_lockObject)
		{
			if (!_isRunning)
			{
				StartSync();
			}
		}
	}

	private void StartSync()
	{
		_isRunning = true;
		LogErrors(_hardwareService.SoftSync(null, EmptySoftSyncStatusChangedHander, out var _), "SoftSync");
	}

	private void EmptySoftSyncStatusChangedHander(string jobId, string status)
	{
		if (!_hardwareService.IsComplete(status))
		{
			return;
		}
		ErrorList programResults = _hardwareService.GetProgramResults(jobId, out var hardwareResults);
		if (programResults.ErrorCount == 0)
		{
			List<int> list = new List<int>();
			foreach (IHardwareResult item in hardwareResults)
			{
				if (item.Code == "SoftSync" && item.ItemID == "EMPTY")
				{
					list.Add(item.Slot);
					list.Add(item.Deck);
				}
			}
			if (list.Count > 1)
			{
				ErrorList source = _hardwareService.SyncLocations(null, null, "SyncEmptyLocations", DateTime.Now.AddMinutes(20.0), list, out var _);
				string text = ((!string.IsNullOrEmpty(jobId) && !source.Any()) ? "Successfully" : "Unsuccessfully");
				LogHelper.Instance.Log(string.Format("{0} scheduled job: {1} - {2} with {3} locations.", text, jobId ?? string.Empty, "SyncEmptyLocations", list.Count() / 2));
				return;
			}
			LogHelper.Instance.Log("No Empty locations found to sync.");
			lock (_lockObject)
			{
				_isRunning = false;
				return;
			}
		}
		LogErrors(programResults, "GetProgramResults");
		lock (_lockObject)
		{
			_isRunning = false;
		}
	}

	private static void LogErrors(ErrorList errors, string jobName)
	{
		if (errors.ErrorCount <= 0)
		{
			return;
		}
		LogHelper.Instance.Log($"{jobName} encountered the following errors:");
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.LogError(error.Code, error.Description);
		}
	}
}
