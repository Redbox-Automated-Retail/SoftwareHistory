using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

internal static class HardwareCorrectionStats
{
	internal static List<HardwareCorrectionStat> LoadStats()
	{
		string query = string.Format("SELECT Type, Job, CorrectionTime, Success FROM [{0}]", "HardwareCorrection");
		return Load(query);
	}

	internal static List<HardwareCorrectionStat> LoadStats(CorrectionType type)
	{
		List<HardwareCorrectionStat> rv = new List<HardwareCorrectionStat>();
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return rv;
		}
		string query = string.Format("SELECT Job, CorrectionTime, Success FROM [{0}] WHERE Type = '{1}'", "HardwareCorrection", (int)type);
		dataStore.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			string job = reader.ReadString(0);
			DateTime? dateTime = reader.ReadDateTime(1);
			bool successful = reader.ReadBoolean(2);
			rv.Add(new HardwareCorrectionStat(type, dateTime.Value, successful, job));
		}, query);
		return rv;
	}

	internal static void DumpStats(CorrectionType type, string fileName)
	{
		string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service");
		string path2 = Path.Combine(path, fileName);
		if (File.Exists(path2))
		{
			FileHelper.CreateBackup(path2, BackupAction.Move);
		}
		using StreamWriter streamWriter = new StreamWriter(path2);
		List<HardwareCorrectionStat> list = LoadStats(CorrectionType.Arcus);
		using (new DisposeableList<HardwareCorrectionStat>(list))
		{
			if (list.Count == 0)
			{
				streamWriter.WriteLine("There are no stats.");
				return;
			}
			foreach (HardwareCorrectionStat item in list)
			{
				streamWriter.WriteLine("{0},{1},{2}", item.CorrectionDate.ToString(), item.JobID, item.Successful ? "SUCCESS" : "FAILURE");
			}
		}
	}

	internal static int RemoveStats(CorrectionType type)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return 0;
		}
		try
		{
			string deleteQuery = string.Format("DELETE FROM [{0}] where Type = {1}", "HardwareCorrection", (int)type);
			return dataStore.Delete(deleteQuery);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete HardwareCorrection info.", e);
			return 0;
		}
	}

	internal static bool InsertStat(CorrectionType type, bool correctionOk, string job)
	{
		if (!ControllerConfiguration.Instance.TrackHardwareCorrections)
		{
			return true;
		}
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return false;
		}
		string insert = string.Format("INSERT INTO [{0}] (Type, Job, CorrectionTime, Success) VALUES ('{1}', '{2}', '{3}', '{4}')", "HardwareCorrection", (int)type, job, DateTime.Now, correctionOk ? 1 : 0);
		return dataStore.Insert(insert);
	}

	private static List<HardwareCorrectionStat> Load(string query)
	{
		List<HardwareCorrectionStat> rv = new List<HardwareCorrectionStat>();
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return rv;
		}
		dataStore.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			CorrectionType type = (CorrectionType)reader.ReadInt(0);
			string job = reader.ReadString(1);
			DateTime? dateTime = reader.ReadDateTime(2);
			bool successful = reader.ReadBoolean(3);
			rv.Add(new HardwareCorrectionStat(type, dateTime.Value, successful, job));
		}, query);
		return rv;
	}
}
