using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.Security;

internal class SecuritySession_SelfEstablishingSymmetric : SecuritySession
{
	public KeyProvider_SelfEstablishingSymmetric KeyProvider_SelfEstablishingSymmetric;

	private RSACryptoServiceProvider _rsaCryptoServiceProviderDecryptor;

	private byte[] RijndaelKey;

	private string _localInstanceGuid = Guid.NewGuid().ToString("N");

	private string _remoteInstanceGuid;

	private ICryptoTransform _encryptor;

	private ICryptoTransform _decryptor;

	public SecuritySession_SelfEstablishingSymmetric(string name, HostInformation remote)
		: base(name, remote)
	{
		try
		{
			_rsaCryptoServiceProviderDecryptor = new RSACryptoServiceProvider();
		}
		catch
		{
			_rsaCryptoServiceProviderDecryptor = new RSACryptoServiceProvider(new CspParameters
			{
				Flags = CspProviderFlags.UseMachineKeyStore
			});
		}
	}

	public override GenuineChunkedStream EstablishSession(Stream input, bool connectionLevel)
	{
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
		lock (this)
		{
			BinaryWriter binaryWriter = new BinaryWriter(genuineChunkedStream);
			binaryWriter.Write(value: false);
			if (input == null || input == Stream.Null)
			{
				RSAParameters rSAParameters = _rsaCryptoServiceProviderDecryptor.ExportParameters(includePrivateParameters: false);
				binaryFormatter.Serialize(genuineChunkedStream, rSAParameters);
				binaryFormatter.Serialize(genuineChunkedStream, _localInstanceGuid);
				return genuineChunkedStream;
			}
			Rijndael rijndael = null;
			object obj = binaryFormatter.Deserialize(input);
			if (obj is RSAParameters)
			{
				_remoteInstanceGuid = (string)binaryFormatter.Deserialize(input);
				if (string.Compare(_remoteInstanceGuid, _localInstanceGuid, ignoreCase: false) > 0)
				{
					return EstablishSession(Stream.Null, connectionLevel);
				}
				if (RijndaelKey == null)
				{
					rijndael = Rijndael.Create();
					RijndaelKey = rijndael.Key;
				}
				RSAParameters parameters = (RSAParameters)obj;
				RSACryptoServiceProvider rSACryptoServiceProvider;
				try
				{
					rSACryptoServiceProvider = new RSACryptoServiceProvider();
				}
				catch
				{
					CspParameters cspParameters = new CspParameters();
					cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
					rSACryptoServiceProvider = new RSACryptoServiceProvider(cspParameters);
				}
				rSACryptoServiceProvider.ImportParameters(parameters);
				byte[] graph = RSAUtility.Encrypt(rSACryptoServiceProvider, RijndaelKey);
				binaryFormatter.Serialize(genuineChunkedStream, graph);
				return genuineChunkedStream;
			}
			if (obj is byte[])
			{
				byte[] rijndaelKey = RSAUtility.Decrypt(_rsaCryptoServiceProviderDecryptor, (byte[])obj);
				RijndaelKey = rijndaelKey;
				binaryFormatter.Serialize(genuineChunkedStream, "OK");
				SessionEstablished();
				return genuineChunkedStream;
			}
			if (obj is string)
			{
				SessionEstablished();
				return null;
			}
		}
		throw GenuineExceptions.Get_Receive_IncorrectData();
	}

	public override void Encrypt(Stream input, GenuineChunkedStream output)
	{
		output.WriteByte(1);
		lock (_encryptor)
		{
			GenuineUtility.CopyStreamToStream(new CryptoStream(new FinishReadingStream(input), _encryptor, CryptoStreamMode.Read), output);
		}
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
		if (RijndaelKey == null)
		{
			throw GenuineExceptions.Get_Security_ContextWasNotEstablished(base.Name);
		}
		lock (RijndaelKey)
		{
			if (_decryptor == null)
			{
				Rijndael rijndael = Rijndael.Create();
				rijndael.Key = RijndaelKey;
				rijndael.Mode = CipherMode.ECB;
				_encryptor = rijndael.CreateEncryptor();
				_decryptor = rijndael.CreateDecryptor();
			}
		}
		lock (_decryptor)
		{
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
			GenuineUtility.CopyStreamToStream(new CryptoStream(new FinishReadingStream(input), _decryptor, CryptoStreamMode.Read), genuineChunkedStream);
			GenuineUtility.CopyStreamToStream(input, Stream.Null);
			input.Close();
			return genuineChunkedStream;
		}
	}

	protected override void SessionEstablished()
	{
		lock (this)
		{
			Rijndael rijndael = Rijndael.Create();
			rijndael.Key = RijndaelKey;
			rijndael.Mode = CipherMode.ECB;
			if (_encryptor == null)
			{
				_encryptor = rijndael.CreateEncryptor();
				_decryptor = rijndael.CreateDecryptor();
			}
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_SelfEstablishingSymmetric.SessionEstablished", LogMessageType.SecuritySessionKey, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, base.Name, -1, 0, 0, 0, "Encryption with " + rijndael.GetType().ToString(), null, null, null, "Security Session security information is initialized.");
			}
		}
		base.SessionEstablished();
	}
}
