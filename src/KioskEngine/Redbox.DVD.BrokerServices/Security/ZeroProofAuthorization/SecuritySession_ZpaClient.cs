using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

internal class SecuritySession_ZpaClient : SecuritySession_BaseZpaSession
{
	private KeyProvider_ZpaClient _keyProvider_ZpaClient;

	public KeyProvider_ZpaClient KeyProvider_ZpaClient => _keyProvider_ZpaClient;

	public SecuritySession_ZpaClient(string name, HostInformation remote, KeyProvider_ZpaClient keyProvider_ZpaClient)
		: base(name, remote, keyProvider_ZpaClient.ZpaFeatureFlags)
	{
		_keyProvider_ZpaClient = keyProvider_ZpaClient;
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
				if (input == Stream.Null)
				{
					binaryWriter.Write((byte)15);
					binaryFormatter.Serialize(genuineChunkedStream, KeyProvider_ZpaClient.Login);
					return genuineChunkedStream;
				}
				ZpaPacketStatusFlag zpaPacketStatusFlag = (ZpaPacketStatusFlag)input.ReadByte();
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
				{
					binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_ZpaClient.EstablishSession", LogMessageType.SecuritySessionEstablishing, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, base.Name, -1, 0, 0, 0, Enum.Format(typeof(ZpaPacketStatusFlag), zpaPacketStatusFlag, "g"), null, null, null, "ZPA Client Session is being established. Status: {0}.", Enum.Format(typeof(ZpaPacketStatusFlag), zpaPacketStatusFlag, "g"));
				}
				switch (zpaPacketStatusFlag)
				{
				case ZpaPacketStatusFlag.ExceptionThrown:
				{
					Exception ex = GenuineUtility.ReadException(input);
					if (Remote != null)
					{
						Remote.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.SecuritySessionFailed, ex, Remote, this));
					}
					DispatchException(ex);
					flag = true;
					throw ex;
				}
				case ZpaPacketStatusFlag.ForceInitialization:
				case ZpaPacketStatusFlag.HashedPassword:
					throw GenuineExceptions.Get_Processing_LogicError($"The remote host must have the Security Session of the type SecuritySession_ZpaClient registered with the name {base.Name}. SecuritySession_ZpaClient never sends {Enum.GetName(typeof(ZpaPacketStatusFlag), zpaPacketStatusFlag)} packet marker.");
				case ZpaPacketStatusFlag.Salt:
					Salt = (byte[])binaryFormatter.Deserialize(input);
					SetupSecurityAlgorithms(KeyProvider_ZpaClient.Password);
					binaryWriter.Write((byte)25);
					binaryFormatter.Serialize(genuineChunkedStream, KeyProvider_ZpaClient.Login);
					binaryFormatter.Serialize(genuineChunkedStream, ZeroProofAuthorizationUtility.CalculateDefaultKeyedHash(KeyProvider_ZpaClient.Password, Salt));
					return genuineChunkedStream;
				case ZpaPacketStatusFlag.SessionEstablished:
					SessionEstablished();
					return null;
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

	protected override void SessionEstablished()
	{
		SetupSecurityAlgorithms(KeyProvider_ZpaClient.Password);
		base.SessionEstablished();
	}
}
