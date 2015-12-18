using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.Data;

public class BlockListData : IBlockListData
{
	private IKernelService _kernelService;

	private string _runningPath;

	private string _dataPath;

	private string _blockListStagedPath;

	private string _blockListActivePath;

	private const string BlockListFileStagedName = "BlockList.staged";

	private const string BlockListFileActiveName = "BlockList.active";

	private DateTime? _version;

	private VistaDBDatabaseProvider _database;

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	public BlockListData(IKernelService kernelService)
	{
		_kernelService = kernelService;
		_runningPath = Path.GetDirectoryName(typeof(BlockListData).Assembly.Location);
		_dataPath = Path.Combine(_runningPath, "..\\data");
		_blockListStagedPath = Path.Combine(_dataPath, "BlockList.staged");
		_blockListActivePath = Path.Combine(_dataPath, "BlockList.active");
	}

	public IEnumerable<BlackList> GetBlackList(string pattern1, string pattern2)
	{
		try
		{
			CheckForStagedFile();
			if (!File.Exists(_blockListActivePath))
			{
				LogHelper.Instance.Log("BlockList at path: {0} does not exist.", _blockListActivePath);
				return new List<BlackList>();
			}
			OpenData();
			string sql = $"select b.* from BlockList b where b.pattern_hash in ('{pattern1}','{pattern2}')";
			DataTable sqlData = _database.GetSqlData(sql);
			return ConvertFromDataTable(sqlData);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("BlockListService.GetBlackList: An unhandled exception occurred.", e);
		}
		finally
		{
			CloseData();
		}
		return new List<BlackList>();
	}

	public DateTime? GetVersion()
	{
		try
		{
			CheckForStagedFile();
			if (!_version.HasValue)
			{
				UpdateVersion();
			}
			return _version;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("BlockListService.GetVersion: An unhandled exception occurred.", e);
		}
		finally
		{
			CloseData();
		}
		return null;
	}

	public void CheckForNewFile()
	{
		try
		{
			CheckForStagedFile();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("BlockListService.CheckForNewFile: An unhandled exception occurred.", e);
		}
	}

	private void UpdateVersion()
	{
		try
		{
			_version = null;
			if (!File.Exists(_blockListActivePath))
			{
				LogHelper.Instance.Log("BlockList at path: {0} does not exist.", _blockListActivePath);
				return;
			}
			OpenData();
			string sql = string.Format("select v.* from Version v where v.[Key] ='{0}'", "generated_on");
			DataTable sqlData = _database.GetSqlData(sql);
			if (sqlData.Rows.Count != 0 && DateTime.TryParseExact(Convert.ToString(sqlData.Rows[0]["Value"]), "yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
			{
				_version = result;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("BlockListService.GetVersion: An unhandled exception occurred.", e);
		}
		finally
		{
			CloseData();
		}
	}

	private void CheckForStagedFile()
	{
		if (File.Exists(_blockListStagedPath))
		{
			if (File.Exists(_blockListActivePath))
			{
				File.Delete(_blockListActivePath);
			}
			File.Move(_blockListStagedPath, _blockListActivePath);
			UpdateVersion();
			if (_version.HasValue && _kernelService != null)
			{
				_kernelService.ExecuteChunk(string.Format("kernel.healthsvcs_enqueuestatistic('{0}','{1}')", "OfflineBlocklistData", _version.Value.ToString("yyyy/MM/dd HH:mm:ss")));
			}
		}
	}

	private List<BlackList> ConvertFromDataTable(DataTable data)
	{
		List<BlackList> list = new List<BlackList>();
		foreach (DataRow row in data.Rows)
		{
			BlackList blackList = new BlackList();
			int num = 0;
			foreach (DataColumn column in data.Columns)
			{
				if (!DBNull.Value.Equals(row[num]))
				{
					if (column.ColumnName.Equals("Key"))
					{
						blackList.Key = Convert.ToString(row[num]);
					}
					else if (column.ColumnName.Equals("pattern_hash"))
					{
						blackList.PatternHash = Convert.ToString(row[num]);
					}
					else if (column.ColumnName.Equals("blocklist_type_id"))
					{
						blackList.BlacklistTypeID = (BlackListType)Convert.ToInt32(row[num]);
					}
					else if (column.ColumnName.Equals("value"))
					{
						blackList.Value = Convert.ToString(row[num]);
					}
				}
				num++;
			}
			list.Add(blackList);
		}
		return list;
	}

	private void OpenData()
	{
		LogHelper.Instance.Log("BlockListService.OpenData().  Opening profile - {0}", _blockListActivePath);
		_database = new VistaDBDatabaseProvider("BlockList", _blockListActivePath);
		_database.Open();
	}

	private void CloseData()
	{
		if (_database != null)
		{
			_database.Dispose();
			_database = null;
		}
		LogHelper.Instance.Log("BlockListService.CloseData()");
	}
}
