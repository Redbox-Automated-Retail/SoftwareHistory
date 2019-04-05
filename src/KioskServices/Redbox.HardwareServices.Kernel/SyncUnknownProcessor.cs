using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;

namespace Redbox.HardwareServices.Kernel;

internal class SyncUnknownProcessor
{
	private IHardwareService _hardwareService;

	private IDataFileStoreService _fileService;

	private IMachineSettingsStore _machineSettings;

	private string _dataPath;

	private int _maxSyncCount;

	private bool _isEnabled;

	private const string DATAFILENAME = "SyncCounts";

	private bool _isRunning;

	private int? _qlmDeck;

	private object _lockObject = new object();

	internal static SyncUnknownProcessor Instance => Singleton<SyncUnknownProcessor>.Instance;

	private SyncUnknownProcessor()
	{
		_machineSettings = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		_hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (_hardwareService == null)
		{
			LogHelper.Instance.Log("SyncUnknownProcessor: Unable to locate Hardware Service.");
			return;
		}
		IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
		_dataPath = Path.Combine(service.DataPath, "SyncUnknowns");
		_fileService = new DataFileStoreService();
		_fileService.Initialize("SyncUnknownCounts", _dataPath, "json", "corrupt", useEncryption: false);
		IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (!service2.TryGetValue<bool>(ConfigFile.System, ConfigCategory.General, ConfigSetting.SyncUnknownEnabled, out _isEnabled))
		{
			_isEnabled = true;
		}
		service2.TryGetValue<int>(ConfigFile.System, ConfigCategory.General, ConfigSetting.MaxUnknownSyncs, out _maxSyncCount);
		if (_maxSyncCount == 0)
		{
			_maxSyncCount = 3;
		}
	}

	internal void StartJob(int? qlm_deck, bool forceSync = false)
	{
		if (!_isEnabled)
		{
			return;
		}
		lock (_lockObject)
		{
			bool flag = forceSync;
			if (!forceSync && !_isRunning)
			{
				_qlmDeck = qlm_deck;
				if (DateTime.TryParse(_machineSettings.GetValue("Local Services\\Hardware Services", "LastUknownSyncDate", string.Empty), out var result))
				{
					flag = DateTime.Now.Date > result.Date;
					LogHelper.Instance.Log($"HardwareFunctions.SyncUnknowns: Previous Uknown Sync run was {result.ToString()}. Run Sync = {flag}");
				}
				else
				{
					flag = true;
					LogHelper.Instance.Log("HardwareFunctions.SyncUnknowns: No previous Unknown Sync run found. Running Sync");
				}
			}
			if (flag)
			{
				StartSync();
			}
		}
	}

	private void StartSync()
	{
		_isRunning = true;
		LogErrors(_hardwareService.SoftSync(null, UnknownSoftSyncStatusChangedHander, out var _), "SoftSync");
	}

	private void UnknownSoftSyncStatusChangedHander(string jobId, string status)
	{
		if (!_hardwareService.IsComplete(status))
		{
			return;
		}
		ErrorList programResults = _hardwareService.GetProgramResults(jobId, out var hardwareResults);
		if (programResults.ErrorCount == 0)
		{
			Dictionary<int, SyncCount> locationCounts = GetLocationCounts();
			List<int> locationsToSync = GetLocationsToSync(hardwareResults, locationCounts);
			if (locationsToSync.Count > 1)
			{
				if (QueueSyncLocations(jobId, locationsToSync))
				{
					SaveLocationCounts(locationCounts);
				}
				return;
			}
			LogHelper.Instance.Log("SyncUnknowns: No Unknown locations found to sync.");
			SaveLocationCounts(locationCounts);
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

	private List<int> GetLocationsToSync(ReadOnlyCollection<IHardwareResult> hardwareResults, Dictionary<int, SyncCount> locationCounts)
	{
		List<int> list = new List<int>();
		foreach (IHardwareResult hardwareResult in hardwareResults)
		{
			if (!(hardwareResult.Code == "SoftSync"))
			{
				continue;
			}
			int key = LocationKey(hardwareResult.Deck, hardwareResult.Slot);
			if (hardwareResult.ItemID == "UNKNOWN" && (!_qlmDeck.HasValue || hardwareResult.Deck != _qlmDeck))
			{
				bool flag = true;
				if (locationCounts.ContainsKey(key))
				{
					if (locationCounts[key].TimesSyncedUnknown >= _maxSyncCount)
					{
						flag = false;
					}
					else
					{
						locationCounts[key].TimesSyncedUnknown++;
					}
				}
				else
				{
					locationCounts.Add(key, new SyncCount
					{
						Deck = hardwareResult.Deck,
						Slot = hardwareResult.Slot,
						TimesSyncedUnknown = 1
					});
				}
				if (flag)
				{
					list.Add(hardwareResult.Slot);
					list.Add(hardwareResult.Deck);
				}
			}
			else if (locationCounts.ContainsKey(key))
			{
				locationCounts.Remove(key);
			}
		}
		return list;
	}

	private bool QueueSyncLocations(string jobId, List<int> syncLocations)
	{
		ErrorList source = _hardwareService.SyncLocations(null, UnknownSyncLocationStatusChangedHander, "SyncUnknown", DateTime.Now, syncLocations, out var _);
		bool flag = !string.IsNullOrEmpty(jobId) && !source.Any();
		string text = (flag ? "Successfully" : "Unsuccessfully");
		LogHelper.Instance.Log(string.Format("{0} scheduled job: {1} - {2} with {3} locations.", text, jobId ?? string.Empty, "SyncUnknown", syncLocations.Count() / 2));
		return flag;
	}

	private void UnknownSyncLocationStatusChangedHander(string jobId, string status)
	{
		if (_hardwareService.IsComplete(status))
		{
			if (_hardwareService.GetProgramResults(jobId, out var _).ErrorCount == 0)
			{
				string value = DateTime.Now.ToString();
				ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Local Services\\Hardware Services", "LastUknownSyncDate", value);
			}
			lock (_lockObject)
			{
				_isRunning = false;
			}
		}
	}

	private void SaveLocationCounts(Dictionary<int, SyncCount> locationCounts)
	{
		if (!_fileService.Set("SyncCounts", new SyncCounts
		{
			Counts = locationCounts
		}))
		{
			LogHelper.Instance.Log("SyncUnknownProcessor failed to save sync counts.");
		}
	}

	private Dictionary<int, SyncCount> GetLocationCounts()
	{
		if (_fileService.Get<SyncCounts>("SyncCounts", out var deserializeObject))
		{
			return deserializeObject.Counts;
		}
		return new Dictionary<int, SyncCount>();
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

	private static int LocationKey(int Deck, int Slot)
	{
		return Deck * 1000 + Slot;
	}
}
