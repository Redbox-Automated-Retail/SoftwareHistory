using System;
using System.Collections;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.TransportContext;

internal class UriStorage : ITimerConsumer
{
	private static UriStorage _sigleton = new UriStorage();

	private static Hashtable _knownUris = Hashtable.Synchronized(new Hashtable());

	public UriStorage()
	{
		TimerProvider.Attach(this);
	}

	public static ITransportContext GetTransportContext(string remoteUri)
	{
		ITransportContext result = null;
		if (_knownUris[remoteUri] is WeakReference weakReference)
		{
			result = weakReference.Target as ITransportContext;
		}
		return result;
	}

	public static void RegisterConnection(string remoteUri, ITransportContext iTransportContext)
	{
		_knownUris[remoteUri] = new WeakReference(iTransportContext);
	}

	public static void UnregisterUnavailableTransportContexts()
	{
		try
		{
			ArrayList arrayList = new ArrayList();
			lock (_knownUris.SyncRoot)
			{
				foreach (object knownUri in _knownUris)
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)knownUri;
					if (!(dictionaryEntry.Value is WeakReference { IsAlive: not false }))
					{
						arrayList.Add(dictionaryEntry.Key);
					}
				}
				for (int i = 0; i < arrayList.Count; i++)
				{
					_knownUris.Remove(arrayList[i]);
				}
			}
		}
		catch (Exception exception)
		{
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.MessageProcessing] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.MessageProcessing, "UriStorage.UnregisterUnavailableTransportContexts", LogMessageType.Error, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Unexpected exception occurred while unregistering unavailable transport contexts.");
			}
		}
	}

	public void TimerCallback()
	{
		UnregisterUnavailableTransportContexts();
	}
}
