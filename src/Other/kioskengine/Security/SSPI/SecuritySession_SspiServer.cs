using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Threading;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public class SecuritySession_SspiServer : SecuritySession
{
	public class _Debugging_SspiServer_SecurityAlgorithm
	{
		public string AuthenticationType;

		public bool IsAnonymous;

		public bool IsAuthenticated;

		public bool IsGuest;

		public bool IsSystem;

		public string Name;
	}

	public KeyProvider_SspiServer KeyProvider_SspiServer;

	public SspiServerSecurityContext SspiSecurityContext;

	public WindowsIdentity WindowsIdentity => SspiApi.QuerySecurityContextToken(SspiSecurityContext._phContext);

	public SecuritySession_SspiServer(string name, HostInformation remote, KeyProvider_SspiServer keyProvider_SspiServer)
		: base(name, remote)
	{
		KeyProvider_SspiServer = keyProvider_SspiServer;
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
					binaryWriter.Write((byte)2);
					return genuineChunkedStream;
				}
				switch ((SspiPacketStatusFlags)input.ReadByte())
				{
				case SspiPacketStatusFlags.InitializeFromScratch:
					SspiSecurityContext = new SspiServerSecurityContext(KeyProvider_SspiServer);
					goto case SspiPacketStatusFlags.ContinueAuthentication;
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
					Exception ex = ReadException(input);
					if (Remote != null)
					{
						Remote.ITransportContext.IGenuineEventProvider.Fire(new GenuineEventArgs(GenuineEventType.SecuritySessionFailed, ex, Remote, this));
					}
					DispatchException(ex);
					flag = true;
					throw ex;
				}
				case SspiPacketStatusFlags.ForceInitialization:
					throw GenuineExceptions.Get_Processing_LogicError($"The remote host must have the Security Session of the type SecuritySession_SspiClient registered with the name {base.Name}. SecuritySession_SspiClient never sends SspiPacketMark.ForceInitialization packet mark.");
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

	private Exception ReadException(Stream stream)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(stream) as Exception;
	}

	protected override void SessionEstablished()
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			WindowsIdentity windowsIdentity = WindowsIdentity;
			binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_SspiServer.SessionEstablished", LogMessageType.SecuritySessionKey, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, base.Name, -1, 0, 0, 0, string.Format("SSPI Package: {0} Features: {1} Win Auth Type: {2} IsAnonymous: {3} IsGuest: {4} IsSystem: {5} Name: {6}", KeyProvider_SspiServer.PackageName, Enum.Format(typeof(SspiFeatureFlags), KeyProvider_SspiServer.RequiredFeatures, "g"), windowsIdentity.AuthenticationType, windowsIdentity.IsAnonymous, windowsIdentity.IsGuest, windowsIdentity.IsSystem, windowsIdentity.Name), null, null, null, "Security Session security information is initialized.");
		}
		_isEstablished.Set();
		SendAssociatedMessages();
	}

	public override void Encrypt(Stream input, GenuineChunkedStream output)
	{
		output.WriteByte(1);
		SspiSecurityContext.EncryptMessage(input, output, KeyProvider_SspiServer.RequiredFeatures);
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
		return SspiSecurityContext.DecryptMessage(input, KeyProvider_SspiServer.RequiredFeatures);
	}

	public override void Invoke(Message message, bool connectionLevel)
	{
		bool flag = (KeyProvider_SspiServer.RequiredFeatures & SspiFeatureFlags.Impersonation) != SspiFeatureFlags.None || (KeyProvider_SspiServer.RequiredFeatures & SspiFeatureFlags.Delegation) != 0;
		try
		{
			if (flag)
			{
				SspiApi.ImpersonateSecurityContext(SspiSecurityContext._phContext);
			}
			base.Invoke(message, connectionLevel);
		}
		finally
		{
			if (flag)
			{
				SspiApi.RevertSecurityContext(SspiSecurityContext._phContext);
			}
		}
	}
}
