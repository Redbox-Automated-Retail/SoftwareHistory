using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model;

internal class StoreCacheData
{
	private List<ClientConfigFile> _clientConfigFiles = new List<ClientConfigFile>();

	private List<long> _downloadFileList = new List<long>();

	private object _lock = new object();

	public StoreData Store { get; set; }

	public long KioskClient_ID { get; set; }

	public string City { get; set; }

	public string County { get; set; }

	public string State { get; set; }

	public string Zip { get; set; }

	public Identifier KioskStatus { get; set; }

	public Identifier Vendor { get; set; }

	public Identifier Market { get; set; }

	public Identifier Banner { get; set; }

	public TimeZoneData TimeZoneData { get; set; }

	public List<ClientRepositoryData> ClientRepositoryData { get; set; }

	public int ClientRepositoryDataSyncID { get; set; }

	public List<ClientConfigFile> ClientConfigFiles => _clientConfigFiles;

	public int ClientConfigFileSyncID { get; set; }

	public ClientStoreInfo ClientStoreInfo { get; set; }

	public int ClientStoreInfoSyncID { get; set; }

	public List<long> DownloadFileList
	{
		get
		{
			lock (_lock)
			{
				List<long> list = new List<long>();
				_downloadFileList.ForEach(delegate(long l)
				{
					list.Add(l);
				});
				return list;
			}
		}
	}

	public void UpdateClientConfigFile(List<ClientConfigFile> clientConfigFile)
	{
		lock (_lock)
		{
			ClientConfigFiles.Clear();
			clientConfigFile.ForEach(delegate(ClientConfigFile data)
			{
				ClientConfigFiles.Add(data);
			});
			ClientConfigFileSyncID++;
		}
	}

	public void UpdateClientConfigFile(ConfigFileArgs args)
	{
		lock (_lock)
		{
			ClientConfigFiles.Clear();
			args.ClientConfigFilesData.ForEach(delegate(ClientConfigFile data)
			{
				ClientConfigFiles.Add(data);
			});
			ClientConfigFileSyncID = args.ClientConfigFileSyncID;
		}
	}

	public void UpdateClientStoreInfo(ClientStoreInfo clientStoreInfo)
	{
		lock (_lock)
		{
			ClientStoreInfo = clientStoreInfo;
			ClientStoreInfoSyncID++;
		}
	}

	public void UpdateClientStoreInfo(StoreInfoArgs args)
	{
		lock (_lock)
		{
			ClientStoreInfo = args.ClientStoreInfo;
			ClientStoreInfoSyncID = args.ClientStoreInfoSyncID;
		}
	}

	public void UpdateDownloadFileList(List<long> downloadFileList)
	{
		lock (_lock)
		{
			_downloadFileList.Clear();
			downloadFileList.ForEach(delegate(long data)
			{
				_downloadFileList.Add(data);
			});
		}
	}

	public void UpdateDownloadFileList(DownloadFileArgs args)
	{
		lock (_lock)
		{
			_downloadFileList.Clear();
			args.DownloadFileList.ForEach(delegate(long data)
			{
				_downloadFileList.Add(data);
			});
		}
	}

	public void UpdateStoreData(StoreData sd)
	{
		lock (_lock)
		{
			Store.Number = sd.Number;
			Store.ID = sd.ID;
			Store.LastCheckIn = sd.LastCheckIn;
		}
	}

	public void UpdateKioskClientData(KioskClientData kc)
	{
		lock (_lock)
		{
			KioskClient_ID = kc.KioskClientID;
			City = kc.City;
			County = kc.County;
			State = kc.State;
			Zip = kc.Zip;
			if (kc.KioskStatusId.HasValue)
			{
				KioskStatus = new Identifier
				{
					ID = kc.KioskStatusId.Value,
					Reference = kc.KioskStatusName
				};
			}
			else
			{
				KioskStatus = null;
			}
			if (kc.BannerId.HasValue)
			{
				Banner = new Identifier
				{
					ID = kc.BannerId.Value,
					Reference = kc.BannerName
				};
			}
			else
			{
				Banner = null;
			}
			if (kc.MarketId.HasValue)
			{
				Market = new Identifier
				{
					ID = kc.MarketId.Value,
					Reference = kc.MarketName
				};
			}
			else
			{
				Market = null;
			}
			if (kc.VendorId.HasValue)
			{
				Vendor = new Identifier
				{
					ID = kc.VendorId.Value,
					Reference = kc.VendorName
				};
			}
			else
			{
				Vendor = null;
			}
			if (TimeZoneData != null && !TimeZoneData.Equals(kc.TimeZoneData))
			{
				ClientStoreInfoSyncID = 0;
			}
			TimeZoneData = kc.TimeZoneData;
		}
	}

	public void UpdateClientRepositoryData(List<ClientRepositoryData> clientRepositoryData, int syncId)
	{
		lock (_lock)
		{
			if (ClientRepositoryData == null)
			{
				ClientRepositoryData = new List<ClientRepositoryData>();
			}
			foreach (ClientRepositoryData clientRepositoryDatum in clientRepositoryData)
			{
				ClientRepositoryData data = clientRepositoryDatum;
				ClientRepositoryData clientRepositoryData2 = ClientRepositoryData.FirstOrDefault((ClientRepositoryData c) => string.Equals(data.RepositoryName, c.RepositoryName, StringComparison.CurrentCultureIgnoreCase));
				if (clientRepositoryData2 == null)
				{
					ClientRepositoryData.Add(clientRepositoryDatum);
					continue;
				}
				clientRepositoryData2.InTransit = clientRepositoryDatum.InTransit;
				clientRepositoryData2.Staged = clientRepositoryDatum.Staged;
				clientRepositoryData2.Active = clientRepositoryDatum.Active;
				clientRepositoryData2.Head = clientRepositoryDatum.Head;
			}
			ClientRepositoryDataSyncID = syncId;
		}
	}

	public void UpdateClientRepositoryData(ClientRepositoryArgs args)
	{
		UpdateClientRepositoryData(args.ClientRepositoryData, args.ClientRepositoryDataSyncID);
	}

	public void UpdateClientRepositoryData(Dictionary<string, ClientRepositoryData.Revlog> assigned)
	{
		lock (_lock)
		{
			if (ClientRepositoryData == null)
			{
				ClientRepositoryData = new List<ClientRepositoryData>();
			}
			foreach (KeyValuePair<string, ClientRepositoryData.Revlog> item2 in assigned)
			{
				KeyValuePair<string, ClientRepositoryData.Revlog> repository1 = item2;
				ClientRepositoryData clientRepositoryData = ClientRepositoryData.FirstOrDefault((ClientRepositoryData c) => string.Equals(repository1.Key, c.RepositoryName, StringComparison.CurrentCultureIgnoreCase));
				if (clientRepositoryData != null)
				{
					clientRepositoryData.Assigned = item2.Value;
					continue;
				}
				ClientRepositoryData clientRepositoryData2 = new ClientRepositoryData();
				clientRepositoryData2.RepositoryName = item2.Key;
				clientRepositoryData2.Assigned = item2.Value;
				clientRepositoryData2.InTransit = new ClientRepositoryData.Revlog
				{
					Hash = "0000000000000000000000000000000000000000"
				};
				clientRepositoryData2.Staged = new ClientRepositoryData.Revlog
				{
					Hash = "0000000000000000000000000000000000000000"
				};
				clientRepositoryData2.Active = new ClientRepositoryData.Revlog
				{
					Hash = "0000000000000000000000000000000000000000"
				};
				clientRepositoryData2.Head = new ClientRepositoryData.Revlog
				{
					Hash = "0000000000000000000000000000000000000000"
				};
				ClientRepositoryData item = clientRepositoryData2;
				ClientRepositoryData.Add(item);
			}
		}
	}

	public void ClearClientConfigFileSyncId()
	{
		lock (_lock)
		{
			ClientConfigFileSyncID = 0;
		}
	}

	public void ClearClientStoreInfoSyncId()
	{
		lock (_lock)
		{
			ClientStoreInfoSyncID = 0;
		}
	}

	public void ClearClearClientRepositoryDataSyncId()
	{
		lock (_lock)
		{
			ClientRepositoryDataSyncID = 0;
		}
	}
}
