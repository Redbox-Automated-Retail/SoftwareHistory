using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class RebalanceService : IRebalance
{
	private string _runningPath;

	private string _dataPath;

	private int _kioskId;

	public const string InFileName = "RebalanceIn";

	public const string OutFileName = "RebalanceOut";

	public const string UnThinFileName = "RebalanceUnthin";

	public const string VersionFileName = "RebalanceVersion";

	public const string SourceExtension = ".json";

	public const string StagedExtension = ".data";

	public const string HashExtension = ".hash";

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	public RebalanceService(string kioskIdFilter)
	{
		IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
		if (service == null)
		{
			_runningPath = Path.GetDirectoryName(typeof(RebalanceService).Assembly.Location);
			_dataPath = Path.Combine(_runningPath, "..\\data");
		}
		else
		{
			_runningPath = service.RunningPath;
			_dataPath = service.DataPath;
		}
		_kioskId = Convert.ToInt32(kioskIdFilter);
	}

	public DateTime? GetRebalanceVersion()
	{
		try
		{
			if (!File.Exists(GetPath("RebalanceVersion", ".json")))
			{
				return null;
			}
			if (DateTime.TryParseExact(File.ReadAllText(GetPath("RebalanceVersion", ".json"), Encoding.ASCII).ToObject<RebalanceVersion>().VersionTimeStampUTC, "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AssumeUniversal, out var result))
			{
				return result.ToLocalTime();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("RebalanceService.GetRebalanceVersion: An unhandled exception occurred.", e);
		}
		return null;
	}

	public void CheckNewRebalance()
	{
		try
		{
			RebalanceData data = GetData();
			if (!data.IsVersionNew)
			{
				return;
			}
			File.WriteAllBytes(GetPath("RebalanceVersion", ".data"), Encoding.ASCII.GetBytes(data.Version.ToJson()));
			File.WriteAllText(GetPath("RebalanceVersion", ".hash"), data.VersionHash);
			if (data.IsOutNew)
			{
				File.WriteAllBytes(GetPath("RebalanceOut", ".data"), Encoding.ASCII.GetBytes(data.Out.ToJson()));
				File.WriteAllText(GetPath("RebalanceOut", ".hash"), data.OutHash);
				if (data.IsUnThinNew)
				{
					File.WriteAllBytes(GetPath("RebalanceUnthin", ".data"), Encoding.ASCII.GetBytes(data.UnThin.ToJson()));
					File.WriteAllText(GetPath("RebalanceUnthin", ".hash"), data.UnThinHash);
				}
			}
			if (data.IsInNew)
			{
				File.WriteAllBytes(GetPath("RebalanceIn", ".data"), Encoding.ASCII.GetBytes(data.In.ToJson()));
				File.WriteAllText(GetPath("RebalanceIn", ".hash"), data.InHash);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("RebalanceService.CheckNewRebalance: An unhandled exception occurred.", e);
		}
	}

	private RebalanceData GetData()
	{
		RebalanceData rebalanceData = new RebalanceData();
		if (File.Exists(GetPath("RebalanceVersion", ".json")))
		{
			rebalanceData.Version = File.ReadAllText(GetPath("RebalanceVersion", ".json"), Encoding.ASCII).ToObject<RebalanceVersion>();
			rebalanceData.VersionHash = Encoding.ASCII.GetBytes(rebalanceData.Version.ToJson()).ToASCIISHA1Hash();
		}
		rebalanceData.LastVersionHash = GetHash("RebalanceVersion");
		if (!string.IsNullOrEmpty(rebalanceData.VersionHash))
		{
			rebalanceData.IsVersionNew = string.IsNullOrEmpty(rebalanceData.LastVersionHash) || rebalanceData.LastVersionHash != rebalanceData.VersionHash;
		}
		if (File.Exists(GetPath("RebalanceIn", ".json")))
		{
			rebalanceData.In = File.ReadAllText(GetPath("RebalanceIn", ".json"), Encoding.ASCII).ToObject<List<RebalanceIn>>();
			rebalanceData.In = rebalanceData.In.Where((RebalanceIn r) => r.KioskId == _kioskId).ToList();
			if (rebalanceData.In.Count > 0)
			{
				rebalanceData.InHash = Encoding.ASCII.GetBytes(rebalanceData.In.ToJson()).ToASCIISHA1Hash();
			}
		}
		rebalanceData.LastInHash = GetHash("RebalanceIn");
		if (!string.IsNullOrEmpty(rebalanceData.InHash))
		{
			rebalanceData.IsInNew = string.IsNullOrEmpty(rebalanceData.LastInHash) || rebalanceData.LastInHash != rebalanceData.InHash;
		}
		if (File.Exists(GetPath("RebalanceOut", ".json")))
		{
			rebalanceData.Out = File.ReadAllText(GetPath("RebalanceOut", ".json"), Encoding.ASCII).ToObject<List<RebalanceOut>>();
			rebalanceData.Out = rebalanceData.Out.Where((RebalanceOut r) => r.KioskId == _kioskId).ToList();
			if (rebalanceData.Out.Count > 0)
			{
				rebalanceData.OutHash = Encoding.ASCII.GetBytes(rebalanceData.Out.ToJson()).ToASCIISHA1Hash();
			}
		}
		rebalanceData.LastOutHash = GetHash("RebalanceOut");
		if (!string.IsNullOrEmpty(rebalanceData.OutHash))
		{
			rebalanceData.IsOutNew = string.IsNullOrEmpty(rebalanceData.LastOutHash) || rebalanceData.LastOutHash != rebalanceData.OutHash;
		}
		if (File.Exists(GetPath("RebalanceUnthin", ".json")))
		{
			rebalanceData.UnThin = File.ReadAllText(GetPath("RebalanceUnthin", ".json"), Encoding.ASCII).ToObject<List<RebalanceUnthin>>();
			rebalanceData.UnThin = rebalanceData.UnThin.Where((RebalanceUnthin r) => r.KioskId == _kioskId).ToList();
			if (rebalanceData.UnThin.Count > 0)
			{
				rebalanceData.UnThinHash = Encoding.ASCII.GetBytes(rebalanceData.UnThin.ToJson()).ToASCIISHA1Hash();
			}
		}
		rebalanceData.LastUnThinHash = GetHash("RebalanceUnthin");
		if (!string.IsNullOrEmpty(rebalanceData.UnThinHash))
		{
			rebalanceData.IsUnThinNew = string.IsNullOrEmpty(rebalanceData.LastUnThinHash) || rebalanceData.LastUnThinHash != rebalanceData.UnThinHash;
		}
		return rebalanceData;
	}

	public string GetHash(string fileName)
	{
		string path = Path.Combine(_dataPath, Path.ChangeExtension(fileName, ".hash"));
		if (!File.Exists(path))
		{
			return null;
		}
		return File.ReadAllText(path, Encoding.ASCII);
	}

	public string GetPath(string fileName, string extension)
	{
		return Path.Combine(_dataPath, Path.ChangeExtension(fileName, extension));
	}
}
