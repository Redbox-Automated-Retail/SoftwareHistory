using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class SecuritySession_Basic : SecuritySession
{
	public SecuritySession_Basic(string name)
		: base(name, null)
	{
		base.IsEstablishedEvent.Set();
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
		{
			binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_Basic.SecuritySession_Basic", LogMessageType.SecuritySessionKey, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, base.Name, -1, 0, 0, 0, "No security features supported.", null, null, null, "This Security Session does not require establishing.");
		}
	}

	public override GenuineChunkedStream EstablishSession(Stream input, bool connectionLevel)
	{
		throw new NotImplementedException();
	}

	public override void Encrypt(Stream input, GenuineChunkedStream output)
	{
		output.WriteStream(input);
	}

	public override Stream Decrypt(Stream input)
	{
		return input;
	}
}
