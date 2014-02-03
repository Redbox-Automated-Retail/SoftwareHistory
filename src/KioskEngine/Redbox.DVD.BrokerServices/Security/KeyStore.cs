using System;
using System.Collections;
using System.Threading;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.Security;

internal class KeyStore : MarshalByRefObject, IKeyStore
{
	private Hashtable _keys = Hashtable.Synchronized(new Hashtable());

	public KeyStore()
	{
		_keys[SecuritySessionServices.DefaultContext.Name] = new KeyProvider_Basic();
	}

	public IKeyProvider GetKey(string name)
	{
		IKeyProvider keyProvider = _keys[name] as IKeyProvider;
		if (keyProvider == null)
		{
			keyProvider = SecuritySessionServices.GetGlobalKey(name);
		}
		return keyProvider;
	}

	public void SetKey(string name, IKeyProvider iKeyProvider)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (iKeyProvider == null)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "KeyStore.SetKey", LogMessageType.KeyProviderDissociated, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, name, -1, 0, 0, 0, iKeyProvider.ToString(), name, null, null, "The Key Provider (\"{0}\") has been dissociated from the \"{1}\" name.", iKeyProvider.ToString(), name);
			}
			_keys.Remove(name);
		}
		else
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySessionServices.SetGlobalKey", LogMessageType.KeyProviderAssociated, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, name, -1, 0, 0, 0, iKeyProvider.ToString(), name, null, null, "The Key Provider (\"{0}\") has been associated with the \"{1}\" name.", iKeyProvider.ToString(), name);
			}
			_keys[name] = iKeyProvider;
		}
	}
}
