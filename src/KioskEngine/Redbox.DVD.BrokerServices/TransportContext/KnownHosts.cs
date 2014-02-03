using System;
using System.Collections;
using System.Threading;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.TransportContext;

internal class KnownHosts : MarshalByRefObject, ITimerConsumer
{
	private class HostInformationAndReason
	{
		public Exception Reason;

		public HostInformation HostInformation;

		public HostInformationAndReason(HostInformation hostInformation, Exception reason)
		{
			HostInformation = hostInformation;
			Reason = reason;
		}
	}

	public ITransportContext ITransportContext;

	private Hashtable _hashtable = new Hashtable();

	private object _syncRoot = new object();

	public HostInformation this[string uri]
	{
		get
		{
			lock (SyncRoot)
			{
				HostInformation hostInformation = Get(uri);
				if (hostInformation == null)
				{
					hostInformation = new HostInformation(uri, ITransportContext);
					UpdateHost(uri, hostInformation);
					int timeSpan = GenuineUtility.ConvertToMilliseconds(ITransportContext.IParameterProvider[GenuineParameter.ConnectTimeout]);
					hostInformation.Renew(timeSpan, canMakeShorter: true);
					BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
					{
						binaryLogWriter.WriteEvent(LogCategory.HostInformation, "KnownHosts.this[string]", LogMessageType.HostInformationCreated, null, null, hostInformation, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The HostInformation has been created for the remote host: {0}.", uri);
					}
				}
				return hostInformation;
			}
		}
	}

	public int TotalHosts
	{
		get
		{
			lock (SyncRoot)
			{
				return _hashtable.Count;
			}
		}
	}

	public object SyncRoot => _syncRoot;

	public KnownHosts(ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
		TimerProvider.Attach(this);
	}

	public HostInformation Get(string uri)
	{
		return GetHostInformation(uri);
	}

	internal void UpdateHost(string uriOrUrl, HostInformation hostInformation)
	{
		SetHostInformation(uriOrUrl, hostInformation);
	}

	public void Renew(string uriOrUrl, int timeSpan, bool canMakeShorter)
	{
		HostInformation hostInformation = null;
		Get(uriOrUrl)?.Renew(timeSpan, canMakeShorter);
	}

	public void TimerCallback()
	{
		int tickCount = GenuineUtility.TickCount;
		HostInformation hostInformation = null;
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		lock (SyncRoot)
		{
			foreach (object item in _hashtable)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
				ArrayList arrayList3 = (ArrayList)dictionaryEntry.Value;
				int num = 0;
				while (num < arrayList3.Count)
				{
					hostInformation = (HostInformation)arrayList3[num];
					if (GenuineUtility.IsTimeoutExpired(hostInformation.ExpireTime, tickCount) || hostInformation.IsDisposed)
					{
						arrayList3.RemoveAt(num);
						arrayList.Add(hostInformation);
						if (arrayList3.Count <= 0)
						{
							arrayList2.Add(dictionaryEntry.Key);
						}
					}
					else
					{
						num++;
					}
				}
			}
			foreach (string item2 in arrayList2)
			{
				_hashtable.Remove(item2);
			}
			foreach (HostInformation item3 in arrayList)
			{
				BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.HostInformation] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.HostInformation, "KnownHosts.this[string]", LogMessageType.HostInformationReferencesDisassociated, GenuineExceptions.Get_Debugging_GeneralWarning("The association between HostInformation and its URL or URI has been broken."), null, item3, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, item3.Uri, item3.Url, null, null, "The current HostInformation does not refer to \"{0}\" and \"{1}\" any longer.", (item3.Uri == null) ? string.Empty : item3.Uri, (item3.Url == null) ? string.Empty : item3.Url);
				}
				GenuineThreadPool.QueueUserWorkItem(ReleaseHostResources, new HostInformationAndReason(item3, GenuineExceptions.Get_Channel_ClientDidNotReconnectWithinTimeOut(item3.ToString())), longDuration: true);
			}
		}
	}

	public ArrayList GetKnownHosts()
	{
		lock (SyncRoot)
		{
			ArrayList arrayList = new ArrayList(_hashtable.Count);
			foreach (object item in _hashtable)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
				ArrayList arrayList2 = (ArrayList)dictionaryEntry.Value;
				foreach (HostInformation item2 in arrayList2)
				{
					if (item2.Uri == null || item2.Url == null || (item2.Uri != null && item2.Url != null && (string)dictionaryEntry.Key == item2.Uri))
					{
						arrayList.Add(item2);
					}
				}
			}
			return arrayList;
		}
	}

	public bool ReleaseHostResources(HostInformation hostInformation, Exception reason)
	{
		if (hostInformation.Uri != null)
		{
			RemoveHostInformation(hostInformation.Uri, hostInformation);
		}
		if (hostInformation.Url != null)
		{
			RemoveHostInformation(hostInformation.Url, hostInformation);
		}
		bool flag = hostInformation.Dispose(reason);
		if (flag)
		{
			ITransportContext.ConnectionManager.ReleaseConnections(hostInformation, GenuineConnectionType.All, reason);
			ITransportContext.IIncomingStreamHandler.DispatchException(hostInformation, reason);
			ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.HostResourcesReleased, reason, hostInformation, null));
		}
		return flag;
	}

	private void ReleaseHostResources(object hostInformationAndReasonAsObject)
	{
		HostInformationAndReason hostInformationAndReason = (HostInformationAndReason)hostInformationAndReasonAsObject;
		ReleaseHostResources(hostInformationAndReason.HostInformation, hostInformationAndReason.Reason);
	}

	private void SetHostInformation(string key, HostInformation hostInformation)
	{
		lock (SyncRoot)
		{
			if (!(_hashtable[key] is ArrayList arrayList))
			{
				ArrayList arrayList2 = new ArrayList();
				_hashtable[key] = arrayList2;
				arrayList2.Add(hostInformation);
				return;
			}
			for (int i = 0; i < arrayList.Count; i++)
			{
				if (object.ReferenceEquals(arrayList[i], hostInformation))
				{
					return;
				}
				HostInformation hostInformation2 = (HostInformation)arrayList[i];
				if (hostInformation.Url != null && hostInformation2.Url == null && hostInformation.Uri == hostInformation2.Uri && hostInformation2.RemoteHostUniqueIdentifier == -1)
				{
					arrayList.RemoveAt(i);
					arrayList.Add(hostInformation);
					hostInformation2.Dispose(GenuineExceptions.Get_Receive_ConflictOfConnections());
					return;
				}
			}
			arrayList.Add(hostInformation);
		}
	}

	public HostInformation GetHostInformation(string key)
	{
		lock (SyncRoot)
		{
			if (!(_hashtable[key] is ArrayList arrayList))
			{
				return null;
			}
			return (HostInformation)arrayList[0];
		}
	}

	private void RemoveHostInformation(string key, HostInformation hostInformation)
	{
		lock (SyncRoot)
		{
			if (!(_hashtable[key] is ArrayList arrayList))
			{
				return;
			}
			for (int i = 0; i < arrayList.Count; i++)
			{
				if (object.ReferenceEquals(arrayList[i], hostInformation))
				{
					arrayList.RemoveAt(i);
					break;
				}
			}
			if (arrayList.Count <= 0)
			{
				_hashtable.Remove(key);
			}
		}
	}

	private void CollectExpiredHostInformation(ArrayList collectedHosts, int now)
	{
		lock (SyncRoot)
		{
			foreach (object item in _hashtable)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)item;
				ArrayList arrayList = (ArrayList)dictionaryEntry.Value;
				int num = 0;
				while (num < arrayList.Count)
				{
					HostInformation hostInformation = (HostInformation)arrayList[num];
					if (GenuineUtility.IsTimeoutExpired(hostInformation.ExpireTime, now) || hostInformation.IsDisposed)
					{
						collectedHosts.Add(hostInformation);
						arrayList.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
			}
		}
	}
}
