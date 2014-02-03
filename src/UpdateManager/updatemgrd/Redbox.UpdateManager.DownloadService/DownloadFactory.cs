using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.DownloadService;

internal class DownloadFactory
{
	public const string DownloadExtension = ".dldat";

	public const string DownloadLabel = ".dl";

	private IDataStoreService _dataStoreService;

	private object _lock = new object();

	public static DownloadFactory Instance => Singleton<DownloadFactory>.Instance;

	private IDataStoreService DataStoreService
	{
		get
		{
			if (_dataStoreService == null)
			{
				_dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			}
			return _dataStoreService;
		}
	}

	public List<IDownloader> GetDownloads()
	{
		lock (_lock)
		{
			List<IDownloader> list = new List<IDownloader>();
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			List<Guid> downLoadList = GetDownLoadList();
			foreach (Guid item in downLoadList)
			{
				try
				{
					DownloadData downloadData = service.Get<DownloadData>(string.Concat(item, ".dldat"));
					if (downloadData == null)
					{
						DeleteDownload(item);
						continue;
					}
					IDownloader downloaderFromData = GetDownloaderFromData(downloadData);
					if (downloadData != null)
					{
						list.Add(downloaderFromData);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log($"An unhandled exception occurred in DownloadFactory.GetDownloads for Guid: {item}", e);
				}
			}
			return list;
		}
	}

	internal IDownloader GetDownloaderFromData(DownloadData downloadData)
	{
		switch (downloadData.DownloadType)
		{
		case DownloadType.Bits:
			return new BitsDownloader(downloadData);
		case DownloadType.WebClient:
			return new WebClientDownloader(downloadData);
		default:
			downloadData.DownloadType = DownloadType.Bits;
			return new BitsDownloader(downloadData);
		}
	}

	internal DownloadData GetDownloadData(string key, string hash, string url, DownloadPriority downloadPriority)
	{
		DownloadData downloadData = new DownloadData();
		downloadData.Key = key;
		downloadData.Url = url;
		downloadData.DownloadState = DownloadState.None;
		downloadData.RetryCount = 0;
		downloadData.FileGuid = Guid.NewGuid();
		downloadData.Hash = hash;
		downloadData.DownloadType = DownloadType.Bits;
		downloadData.DownloadPriority = downloadPriority;
		return downloadData;
	}

	internal void SaveDownload(DownloadData downloadData)
	{
		lock (_lock)
		{
			DataStoreService.Set(string.Concat(downloadData.FileGuid, ".dldat"), downloadData);
			List<Guid> downLoadList = GetDownLoadList();
			if (!downLoadList.Contains(downloadData.FileGuid))
			{
				downLoadList.Add(downloadData.FileGuid);
				DataStoreService.Set(".dl", downLoadList);
			}
		}
	}

	internal void DeleteDownload(DownloadData downloadData)
	{
		lock (_lock)
		{
			if (File.Exists(downloadData.Path))
			{
				File.Delete(downloadData.Path);
			}
			DataStoreService.Delete(string.Concat(downloadData.FileGuid, ".dldat"));
			List<Guid> downLoadList = GetDownLoadList();
			if (downLoadList.Contains(downloadData.FileGuid))
			{
				downLoadList.Remove(downloadData.FileGuid);
				if (downLoadList.Count > 0)
				{
					DataStoreService.Set(".dl", downLoadList);
					return;
				}
				DataStoreService.CleanUp(".dldat");
				DataStoreService.Delete(".dl");
			}
		}
	}

	internal void DeleteDownload(Guid guid)
	{
		lock (_lock)
		{
			DataStoreService.Delete(string.Concat(guid, ".dldat"));
			List<Guid> downLoadList = GetDownLoadList();
			if (downLoadList.Contains(guid))
			{
				downLoadList.Remove(guid);
				if (downLoadList.Count > 0)
				{
					DataStoreService.Set(".dl", downLoadList);
					return;
				}
				DataStoreService.CleanUp(".dldat");
				DataStoreService.Delete(".dl");
			}
		}
	}

	internal List<Guid> GetDownLoadList()
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		List<Guid> list = service.Get<List<Guid>>(".dl");
		if (list == null)
		{
			list = new List<Guid>();
		}
		return list;
	}

	private DownloadFactory()
	{
	}
}
