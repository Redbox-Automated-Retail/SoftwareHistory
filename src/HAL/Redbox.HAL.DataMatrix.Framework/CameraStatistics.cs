using System;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.DataMatrix.Framework;

public static class CameraStatistics
{
	public static int RemoveCameraStats(ScannerServices key)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return 0;
		}
		try
		{
			string deleteQuery = string.Format("DELETE FROM [{0}] where CameraType = {1}", "CameraData", (int)key);
			return dataStore.Delete(deleteQuery);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete CameraData info.", e);
			return 0;
		}
	}

	public static void DumpStats(string file, ScannerServices key)
	{
		if (File.Exists(file))
		{
			FileHelper.CreateBackup(file, BackupAction.Move);
		}
		StreamWriter writer = new StreamWriter(file);
		try
		{
			IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
			if (dataStore == null)
			{
				LogHelper.Instance.Log("Unable to find counters store. EPIC FAILURE.");
				return;
			}
			string query = string.Format("SELECT SecureCount, ClearImageScan, ReadOk, ScanDate, ScanTime FROM [{0}] WHERE CameraType = '{1}'", "CameraData", (int)key);
			dataStore.ExectuteSelectQuery(delegate(IDataReader reader)
			{
				writer.WriteLine("{0},{1},{2},{3},{4}", reader.ReadInt(0), reader.ReadBoolean(1), reader.ReadBoolean(2), reader.ReadDateTime(3).Value.ToString(), reader.ReadDecimal(4));
			}, query);
		}
		finally
		{
			if (writer != null)
			{
				((IDisposable)writer).Dispose();
			}
		}
	}

	internal static bool InsertCameraStat(bool readOk, int secureCount, bool clearImage, int key, double decodeTime)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return false;
		}
		string insert = string.Format("INSERT INTO [{0}] (ReadOk, SecureCount, ClearImageScan, CameraType, ScanDate, ScanTime) VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}')", "CameraData", readOk ? 1 : 0, secureCount, clearImage ? 1 : 0, key, DateTime.Now, decodeTime);
		return dataStore.Insert(insert);
	}
}
