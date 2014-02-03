using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.Security.SSPI;

internal class SecuritySession_SspiClient : SecuritySession
{
	public KeyProvider_SspiClient KeyProvider_SspiClient;

	public SspiClientSecurityContext SspiSecurityContext;

	public static TimeSpan MaxSpanToPerformAuthentication = TimeSpan.FromMinutes(3.0);

	public SecuritySession_SspiClient(string name, HostInformation remote, KeyProvider_SspiClient keyProvider_SspiClient)
		: base(name, remote)
	{
		KeyProvider_SspiClient = keyProvider_SspiClient;
	}

	public override GenuineChunkedStream EstablishSession(Stream input, bool connectionLevel)
	{
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
					if (KeyProvider_SspiClient.DelegatedContext != null)
					{
						try
						{
							SspiApi.ImpersonateSecurityContext(KeyProvider_SspiClient.DelegatedContext.SspiSecurityContext._phContext);
							SspiSecurityContext = new SspiClientSecurityContext(KeyProvider_SspiClient);
							binaryWriter.Write((byte)0);
							SspiSecurityContext.BuildUpSecurityContext(null, genuineChunkedStream);
							return genuineChunkedStream;
						}
						finally
						{
							SspiApi.RevertSecurityContext(KeyProvider_SspiClient.DelegatedContext.SspiSecurityContext._phContext);
						}
					}
					SspiSecurityContext = new SspiClientSecurityContext(KeyProvider_SspiClient);
					binaryWriter.Write((byte)0);
					SspiSecurityContext.BuildUpSecurityContext(null, genuineChunkedStream);
					return genuineChunkedStream;
				}
				switch ((SspiPacketStatusFlags)input.ReadByte())
				{
				case SspiPacketStatusFlags.ContinueAuthentication:
				{
					GenuineChunkedStream genuineChunkedStream2 = new GenuineChunkedStream(releaseOnReadMode: false);
					SspiSecurityContext.BuildUpSecurityContext(input, genuineChunkedStream2);
					if (genuineChunkedStream2.Length == 0)
					{
						genuineChunkedStream.WriteByte(3);
						SessionEstablished();
					}
					else
					{
						genuineChunkedStream.WriteByte(1);
						genuineChunkedStream.WriteStream(genuineChunkedStream2);
					}
					return genuineChunkedStream;
				}
				case SspiPacketStatusFlags.ExceptionThrown:
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
				case SspiPacketStatusFlags.ForceInitialization:
					return EstablishSession(Stream.Null, connectionLevel);
				case SspiPacketStatusFlags.InitializeFromScratch:
					throw GenuineExceptions.Get_Processing_LogicError($"The remote host must have the Security Session of the type SecuritySession_SspiServer registered with the name {base.Name}. SecuritySession_SspiServer never sends SspiPacketMark.InitializeFromScratch packet marker.");
				case SspiPacketStatusFlags.SessionEstablished:
					SessionEstablished();
					break;
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
			binaryWriter.Write((byte)4);
			binaryFormatter.Serialize(genuineChunkedStream, ex2);
			return genuineChunkedStream;
		}
		return null;
	}

	public override void Encrypt(Stream input, GenuineChunkedStream output)
	{
		output.WriteByte(1);
		SspiSecurityContext.EncryptMessage(input, output, KeyProvider_SspiClient.RequiredFeatures);
	}

	public override Stream Decrypt(Stream input)
	{
		if (input.ReadByte() == 0)
		{
			Stream stream = EstablishSession(input, connectionLevel: false);
			if (stream != null)
			{
				GenuineThreadPool.QueueUserWorkItem(base.SendMessage, stream, longDuration: false);
			}
			return null;
		}
		return SspiSecurityContext.DecryptMessage(input, KeyProvider_SspiClient.RequiredFeatures);
	}
}
