using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class SecuritySession_KnownSymmetric : SecuritySession
{
	private ICryptoTransform _encryptor;

	private ICryptoTransform _decryptor;

	public SymmetricAlgorithm SymmetricAlgorithm;

	public SecuritySession_KnownSymmetric(SymmetricAlgorithm symmetricAlgorithm, string name)
		: base(name, null)
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_KnownSymmetric.SecuritySession_KnownSymmetric", LogMessageType.SecuritySessionKey, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, name, -1, 0, 0, 0, "Encryption using " + symmetricAlgorithm.GetType().ToString(), null, null, null, "Security Session security information is initialized.");
		}
		SymmetricAlgorithm = symmetricAlgorithm;
		_encryptor = SymmetricAlgorithm.CreateEncryptor();
		_decryptor = SymmetricAlgorithm.CreateDecryptor();
		base.IsEstablishedEvent.Set();
	}

	public override GenuineChunkedStream EstablishSession(Stream input, bool connectionLevel)
	{
		throw new NotImplementedException();
	}

	public override void Encrypt(Stream input, GenuineChunkedStream output)
	{
		lock (_encryptor)
		{
			GenuineUtility.CopyStreamToStream(new CryptoStream(new FinishReadingStream(input), _encryptor, CryptoStreamMode.Read), output);
		}
	}

	public override Stream Decrypt(Stream input)
	{
		lock (_decryptor)
		{
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
			GenuineUtility.CopyStreamToStream(new CryptoStream(new FinishReadingStream(input), _decryptor, CryptoStreamMode.Read), genuineChunkedStream);
			return genuineChunkedStream;
		}
	}
}
