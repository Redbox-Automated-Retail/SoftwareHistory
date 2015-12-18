using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskData;

namespace Redbox.Rental.Data;

public class KioskData : IKioskData
{
	private IKioskDataInstance _instance;

	private IKernelService _kernelService;

	private string _runningPath;

	private string _dataPath;

	private string _versionsStagedPath;

	private string _versionsActivePath;

	private const string VersionsStagedName = "KioskDataVersions.staged";

	private const string VersionsActiveName = "KioskDataVersions.active";

	private string _collectionsStagedPath;

	private string _collectionsActivePath;

	private const string CollectionsStagedName = "KioskDataCollections.staged";

	private const string CollectionsActiveName = "KioskDataCollections.active";

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	public KioskData(IKernelService kernelService)
	{
		_kernelService = kernelService;
		_runningPath = Path.GetDirectoryName(typeof(KioskData).Assembly.Location);
		_dataPath = Path.Combine(_runningPath, "..\\data");
		_versionsStagedPath = Path.Combine(_dataPath, "KioskDataVersions.staged");
		_versionsActivePath = Path.Combine(_dataPath, "KioskDataVersions.active");
		_collectionsStagedPath = Path.Combine(_dataPath, "KioskDataCollections.staged");
		_collectionsActivePath = Path.Combine(_dataPath, "KioskDataCollections.active");
	}

	public bool HasStaged()
	{
		if (File.Exists(_versionsStagedPath))
		{
			return true;
		}
		if (File.Exists(_collectionsStagedPath))
		{
			return true;
		}
		return false;
	}

	public IKioskDataInstance GetData()
	{
		try
		{
			if (Activate())
			{
				LoadInstance();
				SendStatistic();
			}
			if (_instance == null)
			{
				LoadInstance();
			}
			return _instance;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskData.GetData - Unhandled exception occurred", e);
			return null;
		}
	}

	private void SendStatistic()
	{
		try
		{
			if (_kernelService != null)
			{
				string text = ((_instance == null) ? new DateTime(2000, 1, 1) : _instance.VersionTimeStamp).ToString("yyyy/MM/dd HH:mm:ss");
				LogHelper.Instance.Log("KioskData.SendStatistic - Sending KioskData new version: {0}.", text);
				_kernelService.ExecuteChunk(string.Format("kernel.healthsvcs_enqueuestatistic('{0}','{1}')", "KioskData", text));
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskData.SendStatistic - Unhandled exception occurred", e);
		}
	}

	private bool Activate()
	{
		if (!HasStaged())
		{
			return false;
		}
		try
		{
			return (byte)(0u | (Activate(_versionsStagedPath, _versionsActivePath) ? 1u : 0u) | (Activate(_collectionsStagedPath, _collectionsActivePath) ? 1u : 0u)) != 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskData.Activate - Unhandled exception occurred", e);
			return false;
		}
	}

	private bool Activate(string stagedPath, string activePath)
	{
		try
		{
			if (!File.Exists(stagedPath))
			{
				return false;
			}
			if (File.Exists(activePath))
			{
				File.Delete(activePath);
			}
			File.Move(stagedPath, activePath);
			LogHelper.Instance.Log("KioskData.Activate - Activated {0} to {1}", stagedPath, activePath);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskData.Activate - Unhandled exception occurred", e);
			return false;
		}
	}

	private void LoadInstance()
	{
		KioskVersionData kioskVersionData = LoadKioskVersionData();
		List<KioskCollectionData> list = LoadKioskCollectionData();
		LogHelper.Instance.Log("KioskData.LoadInstance - loading kioskdata version file.");
		LogHelper.Instance.Log("KioskData.LoadInstance - loading kioskdata collection file.");
		KioskDataInstance instance = new KioskDataInstance();
		if (kioskVersionData != null)
		{
			instance.VersionTimeStamp = kioskVersionData.Version;
			LogHelper.Instance.Log(kioskVersionData.ToJson());
		}
		else
		{
			LogHelper.Instance.Log("version is null");
		}
		if (list != null)
		{
			LogHelper.Instance.Log(list.ToJson());
			int sortOrder = 0;
			list.ForEach(delegate(KioskCollectionData c)
			{
				Collection collection = new Collection
				{
					Id = c.Id,
					Name = c.Name,
					SortOrder = sortOrder++,
					EffectiveStartDate = c.EffectiveStartDate,
					EffectiveEndDate = c.EffectiveEndDate,
					IsTop20 = c.IsTop20,
					LegalInfo = c.LegalInfo
				};
				if (c.Titles != null)
				{
					c.Titles.ForEach(delegate(long title)
					{
						collection.Titles.Add(title);
					});
				}
				instance.Collections.Add(collection);
			});
		}
		else
		{
			LogHelper.Instance.Log("collections is null");
		}
		_instance = instance;
	}

	private List<KioskCollectionData> LoadKioskCollectionData()
	{
		try
		{
			return Encoding.ASCII.GetString(File.ReadAllBytes(_collectionsActivePath)).ToObject<List<KioskCollectionData>>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskData.LoadKioskCollectionData - Unhandled exception occurred", e);
			return null;
		}
	}

	private KioskVersionData LoadKioskVersionData()
	{
		try
		{
			return Encoding.ASCII.GetString(File.ReadAllBytes(_versionsActivePath)).ToObject<KioskVersionData>();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("KioskData.LoadKioskVersionData - Unhandled exception occurred", e);
			return null;
		}
	}
}
