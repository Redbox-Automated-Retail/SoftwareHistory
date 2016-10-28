using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class SecuritySessionServices
{
	public static readonly SecuritySessionParameters DefaultContext = new SecuritySessionParameters("D");

	public static readonly SecuritySessionParameters DefaultContextWithCompression = new SecuritySessionParameters("D", SecuritySessionAttributes.EnableCompression, TimeSpan.MinValue);

	private static SecuritySessionParameters _defaultSecuritySession = DefaultContext;

	private static object _defaultSecuritySessionObject = new object();

	private static Hashtable _globalKeys = Hashtable.Synchronized(new Hashtable());

	public static SecuritySessionParameters DefaultSecuritySession
	{
		get
		{
			lock (_defaultSecuritySessionObject)
			{
				return _defaultSecuritySession;
			}
		}
		set
		{
			lock (_defaultSecuritySessionObject)
			{
				_defaultSecuritySession = value;
			}
		}
	}

	public static SecuritySessionParameters SetCurrentSecurityContext(SecuritySessionParameters securitySessionParameters)
	{
		LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("__GC__CurrentSecuritySessionParameters");
		object data = Thread.GetData(namedDataSlot);
		Thread.SetData(namedDataSlot, securitySessionParameters);
		return data as SecuritySessionParameters;
	}

	public static SecuritySessionParameters GetCurrentSecurityContext()
	{
		LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("__GC__CurrentSecuritySessionParameters");
		return Thread.GetData(namedDataSlot) as SecuritySessionParameters;
	}

	public static IKeyProvider GetGlobalKey(string name)
	{
		return _globalKeys[name] as IKeyProvider;
	}

	public static void SetGlobalKey(string name, IKeyProvider iKeyProvider)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (iKeyProvider == null)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySessionServices.SetGlobalKey", LogMessageType.KeyProviderDissociated, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, iKeyProvider.ToString(), name, null, null, "The Key Provider (\"{0}\") has been dissociated from the \"{1}\" name.", iKeyProvider.ToString(), name);
			}
			_globalKeys.Remove(name);
		}
		else
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySessionServices.SetGlobalKey", LogMessageType.KeyProviderAssociated, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, iKeyProvider.ToString(), name, null, null, "The Key Provider (\"{0}\") has been associated with the \"{1}\" name.", iKeyProvider.ToString(), name);
			}
			_globalKeys[name] = iKeyProvider;
		}
	}
}
