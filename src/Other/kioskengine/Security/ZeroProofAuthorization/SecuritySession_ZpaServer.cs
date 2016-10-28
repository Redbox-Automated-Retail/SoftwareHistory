using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[ComVisible(true)]
public class SecuritySession_ZpaServer : SecuritySession_BaseZpaSession
{
	public KeyProvider_ZpaServer KeyProvider_ZpaServer;

	private object _login;

	public object Login => _login;

	public SecuritySession_ZpaServer(string name, HostInformation remote, KeyProvider_ZpaServer keyProvider_ZpaServer)
		: base(name, remote, keyProvider_ZpaServer.ZpaFeatureFlags)
	{
		KeyProvider_ZpaServer = keyProvider_ZpaServer;
		Random random = new Random();
		Salt = ZeroProofAuthorizationUtility.GenerateArbitrarySequence(128 + random.Next(128));
	}

	public override GenuineChunkedStream EstablishSession(Stream input, bool connectionLevel)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		bool flag = false;
		if (base.IsEstablished)
		{
			return null;
		}
		GenuineChunkedStream genuineChunkedStream = null;
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		if (connectionLevel)
		{
			input?.ReadByte();
			genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
		}
		else
		{
			genuineChunkedStream = CreateOutputStream();
		}
		BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
		binaryWriter.Write((byte)0);
		try
		{
			lock (this)
			{
				ZpaPacketStatusFlag zpaPacketStatusFlag = ((input != Stream.Null) ? ((ZpaPacketStatusFlag)input.ReadByte()) : ZpaPacketStatusFlag.ForceInitialization);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_ZpaServer.EstablishSession", LogMessageType.SecuritySessionEstablishing, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, base.Name, -1, 0, 0, 0, Enum.Format(typeof(ZpaPacketStatusFlag), zpaPacketStatusFlag, "g"), null, null, null, "ZPA Server Session is being established. Status: {0}.", Enum.Format(typeof(ZpaPacketStatusFlag), zpaPacketStatusFlag, "g"));
				}
				switch (zpaPacketStatusFlag)
				{
				case ZpaPacketStatusFlag.ExceptionThrown:
				{
					Exception ex = GenuineUtility.ReadException(input);
					Remote.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.SecuritySessionFailed, ex, Remote, this));
					DispatchException(ex);
					flag = true;
					throw ex;
				}
				case ZpaPacketStatusFlag.ForceInitialization:
					binaryWriter.Write((byte)20);
					binaryFormatter.Serialize(genuineChunkedStream, Salt);
					return genuineChunkedStream;
				case ZpaPacketStatusFlag.HashedPassword:
				{
					_login = binaryFormatter.Deserialize(input);
					string password = KeyProvider_ZpaServer.IAuthorizationManager.GetPassword(_login);
					byte[] first = (byte[])binaryFormatter.Deserialize(input);
					byte[] array = ZeroProofAuthorizationUtility.CalculateDefaultKeyedHash(password, Salt);
					if (!ZeroProofAuthorizationUtility.CompareBuffers(first, array, array.Length))
					{
						throw GenuineExceptions.Get_Security_PasswordKnowledgeIsNotProved();
					}
					SetupSecurityAlgorithms(password);
					SessionEstablished();
					binaryWriter.Write((byte)30);
					return genuineChunkedStream;
				}
				case ZpaPacketStatusFlag.Salt:
				case ZpaPacketStatusFlag.SessionEstablished:
					throw GenuineExceptions.Get_Processing_LogicError($"The remote host must have the Security Session of the type SecuritySession_ZpaServer registered with the name {base.Name}. SecuritySession_ZpaServer never sends {Enum.GetName(typeof(ZpaPacketStatusFlag), zpaPacketStatusFlag)} packet marker.");
				}
			}
		}
		catch (Exception ex2)
		{
			if (Remote != null)
			{
				Remote.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.SecuritySessionFailed, ex2, Remote, this));
			}
			DispatchException(ex2);
			if (flag)
			{
				throw;
			}
			binaryWriter.Write((byte)35);
			binaryFormatter.Serialize(genuineChunkedStream, ex2);
			return genuineChunkedStream;
		}
		return null;
	}
}
