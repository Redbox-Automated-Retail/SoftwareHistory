using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Environment;

public class StoreInfoService : IStoreInfoService, IPollRequestReply
{
	private int _syncID;

	private StoreInfoChangeSetData _storeInfoChangeSet;

	private string _clientStoreInfoHash;

	public static StoreInfoService Instance => Singleton<StoreInfoService>.Instance;

	public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
	{
		ErrorList errorList = new ErrorList();
		pollRequestList = new List<PollRequest>();
		try
		{
			ErrorList clientStoreInfo = GetClientStoreInfo(out var clientStoreInfo2);
			errorList.AddRange(clientStoreInfo);
			string data = ObjectExtensions.ToJson(clientStoreInfo2);
			string hash = GetHash(clientStoreInfo2);
			if (!string.IsNullOrEmpty(_clientStoreInfoHash) && _clientStoreInfoHash != hash)
			{
				_syncID = 0;
			}
			SetHash(hash);
			if (_syncID == 0)
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.StoreInfo,
					SyncId = 0,
					Data = data
				});
			}
			else
			{
				pollRequestList.Add(new PollRequest
				{
					PollRequestType = PollRequestType.StoreInfo,
					SyncId = _syncID
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("SIS0091", "Unhandled exception occurred in StoreInfoModel.GetPollRequest.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			PollReply pollReply = (from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.UpdateStoreInfo
				select pr).FirstOrDefault();
			if (pollReply != null)
			{
				_syncID = 0;
				_clientStoreInfoHash = string.Empty;
				LogHelper.Instance.Log("Store Info server is clearing the sync id as requested by the server.");
				return new ErrorList();
			}
			PollReply pollReply2 = (from pr in pollReplyList.ToList()
				where pr.PollReplyType == PollReplyType.StoreInfoChangeSet
				select pr).FirstOrDefault();
			if (pollReply2 != null)
			{
				if (string.IsNullOrEmpty(pollReply2.Data))
				{
					errorList.Add(Error.NewError("CFS0092", "Error, A poll reply contained a type of store info changeset without the changeset.", ObjectExtensions.ToJson(pollReply2)));
				}
				else
				{
					StoreInfoChangeSetData changeSet = StringExtensions.ToObject<StoreInfoChangeSetData>(pollReply2.Data);
					ErrorList collection = ProcessChangeSet(changeSet);
					errorList.AddRange(collection);
				}
				_syncID = pollReply2.SyncId;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("CFS0091", "Unhandled exception occurred in StoreInfoService.ProcessPollReply.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private ErrorList ProcessChangeSet(StoreInfoChangeSetData changeSet)
	{
		ErrorList errorList = new ErrorList();
		LogHelper.Instance.Log("Processing StoreInfo ChangeSet: " + ObjectExtensions.ToJson(changeSet), LogEntryType.Debug);
		try
		{
			_storeInfoChangeSet = changeSet;
			if (_storeInfoChangeSet.CorrectTimeZone)
			{
				TimeZoneInfo timeZone = TimeZoneInfo.FromSerializedString(_storeInfoChangeSet.TimeZoneInfoString);
				if (!TimeZoneFunctions.SetTimeZone(timeZone))
				{
					errorList.Add(Error.NewError("SIS0100", "Setting timezone failed", "Check the log for further details."));
				}
			}
			if (_storeInfoChangeSet.CorrectDateTime && !TimeZoneFunctions.SetTime(_storeInfoChangeSet.CurrentUTCDateTime))
			{
				errorList.Add(Error.NewError("SIS0101", "Setting datetime failed", "Check the log for further details."));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("SIS0089", $"Unhandled exception occurred in StoreInfoService.ProcessChangeSet for store info", e));
		}
		return errorList;
	}

	private void SetHash(string hash)
	{
		_clientStoreInfoHash = hash;
	}

	private ErrorList GetClientStoreInfo(out ClientStoreInfo clientStoreInfo)
	{
		ErrorList errorList = new ErrorList();
		clientStoreInfo = new ClientStoreInfo
		{
			Info = new Dictionary<string, string>()
		};
		try
		{
			TimeZoneInfo.ClearCachedData();
			clientStoreInfo.TimeZoneInfoString = TimeZoneInfo.Local.ToSerializedString();
			clientStoreInfo.UTCDateTime = DateTime.UtcNow;
			clientStoreInfo.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
			clientStoreInfo.Info.Add("OS Version", System.Environment.OSVersion.Version.ToString());
			clientStoreInfo.Info.Add("OS VersionString", System.Environment.OSVersion.VersionString);
			clientStoreInfo.Info.Add("OS Platform", System.Environment.OSVersion.Platform.ToString());
			clientStoreInfo.Info.Add("OS ServicePack", System.Environment.OSVersion.ServicePack);
			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			string value = hostEntry.AddressList.FirstOrDefault((IPAddress a) => a.AddressFamily == AddressFamily.InterNetwork).ToString();
			clientStoreInfo.Info.Add("IP Address", value);
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			if (allNetworkInterfaces.Any())
			{
				string dnsSuffix = allNetworkInterfaces[0].GetIPProperties().DnsSuffix;
				clientStoreInfo.Info.Add("DNS Suffix", dnsSuffix);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("SIS0092", "Unhandled exception occurred in StoreInfoServieGetClientStoreInfo.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private string GetHash(ClientStoreInfo clientStoreInfo)
	{
		var obj = new
		{
			TimeZoneInfoString = clientStoreInfo.TimeZoneInfoString,
			Version = clientStoreInfo.Version,
			Info = new Dictionary<string, string>()
		};
		LinqExtensions.ForEach(clientStoreInfo.Info, delegate(KeyValuePair<string, string> info)
		{
			obj.Info.Add(info.Key, info.Value);
		});
		string s = ObjectExtensions.ToJson(obj);
		byte[] bytes = Encoding.ASCII.GetBytes(s);
		return ByteArrayExtensions.ToASCIISHA1Hash(bytes);
	}

	private StoreInfoService()
	{
		if (ServiceLocator.Instance.GetService<IStoreInfoService>() == null)
		{
			ServiceLocator.Instance.AddService(typeof(IStoreInfoService), this);
		}
	}
}
