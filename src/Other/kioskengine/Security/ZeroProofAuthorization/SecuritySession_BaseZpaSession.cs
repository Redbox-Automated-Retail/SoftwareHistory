using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[ComVisible(true)]
public abstract class SecuritySession_BaseZpaSession : SecuritySession
{
	public SymmetricAlgorithm SymmetricAlgorithm;

	private ICryptoTransform _encryptor;

	private ICryptoTransform _decryptor;

	public KeyedHashAlgorithm KeyedHashAlgorithm;

	public byte[] Salt;

	public ZpaFeatureFlags ZpaFeatureFlags;

	public SecuritySession_BaseZpaSession(string name, HostInformation remote, ZpaFeatureFlags zpaFeatureFlags)
		: base(name, remote)
	{
		ZpaFeatureFlags = zpaFeatureFlags;
	}

	public override void Encrypt(Stream input, GenuineChunkedStream output)
	{
		output.WriteByte(1);
		lock (this)
		{
			if (_encryptor != null)
			{
				GenuineUtility.CopyStreamToStream(new CryptoStream(new FinishReadingStream(input), _encryptor, CryptoStreamMode.Read), output);
			}
			else
			{
				output.WriteStream(input);
			}
			if (KeyedHashAlgorithm != null)
			{
				input.Position = 0L;
				output.WriteBuffer(KeyedHashAlgorithm.ComputeHash(input), -1);
				if (_encryptor == null)
				{
					input.Position = 0L;
				}
			}
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
		lock (this)
		{
			GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
			byte[] array = null;
			int signSize = 0;
			if (KeyedHashAlgorithm != null)
			{
				signSize = KeyedHashAlgorithm.HashSize / 8;
			}
			if (_decryptor != null)
			{
				CryptoStream cryptoStream = new CryptoStream(new FinishReadingStream(genuineChunkedStream), _decryptor, CryptoStreamMode.Write);
				array = GenuineUtility.CopyStreamToStreamExceptSign(input, cryptoStream, signSize);
				cryptoStream.FlushFinalBlock();
			}
			else
			{
				array = GenuineUtility.CopyStreamToStreamExceptSign(input, genuineChunkedStream, signSize);
			}
			if (KeyedHashAlgorithm != null)
			{
				if (!ZeroProofAuthorizationUtility.CompareBuffers(array, KeyedHashAlgorithm.ComputeHash(genuineChunkedStream), array.Length))
				{
					throw GenuineExceptions.Get_Security_WrongSignature();
				}
				genuineChunkedStream.Position = 0L;
			}
			genuineChunkedStream.ReleaseOnReadMode = true;
			return genuineChunkedStream;
		}
	}

	protected void SetupSecurityAlgorithms(string password)
	{
		lock (this)
		{
			if ((ZpaFeatureFlags & ZpaFeatureFlags.ElectronicCodebookEncryption) != ZpaFeatureFlags.None || (ZpaFeatureFlags & ZpaFeatureFlags.CipherBlockChainingEncryption) != ZpaFeatureFlags.None)
			{
				SymmetricAlgorithm = Rijndael.Create();
				SymmetricAlgorithm.Key = ZeroProofAuthorizationUtility.GeneratePasswordBasedSequence("Key" + password, Salt, 32);
				SymmetricAlgorithm.IV = ZeroProofAuthorizationUtility.GeneratePasswordBasedSequence("IV" + password, Salt, 16);
				SymmetricAlgorithm.Mode = (((ZpaFeatureFlags & ZpaFeatureFlags.ElectronicCodebookEncryption) == 0) ? CipherMode.CBC : CipherMode.ECB);
				_encryptor = SymmetricAlgorithm.CreateEncryptor();
				_decryptor = SymmetricAlgorithm.CreateDecryptor();
			}
			if ((ZpaFeatureFlags & ZpaFeatureFlags.Mac3DesCbcSigning) != ZpaFeatureFlags.None)
			{
				KeyedHashAlgorithm = KeyedHashAlgorithm.Create();
			}
			if ((ZpaFeatureFlags & ZpaFeatureFlags.HmacSha1Signing) != ZpaFeatureFlags.None)
			{
				KeyedHashAlgorithm = KeyedHashAlgorithm.Create();
			}
			if (KeyedHashAlgorithm != null)
			{
				KeyedHashAlgorithm.Key = ZeroProofAuthorizationUtility.GeneratePasswordBasedSequence("M3D" + password, Salt, 24);
			}
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Security] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Security, "SecuritySession_BaseZpaSession.SetupSecurityAlgorithms", LogMessageType.SecuritySessionKey, null, null, Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, this, base.Name, -1, 0, 0, 0, string.Format("Zero Proof Authorization Flags: {0} Encryption: {1} Data Integrity: {2}", Enum.Format(typeof(ZpaFeatureFlags), ZpaFeatureFlags, "g"), (SymmetricAlgorithm == null) ? "No" : SymmetricAlgorithm.GetType().ToString(), (KeyedHashAlgorithm == null) ? "No" : KeyedHashAlgorithm.GetType().ToString()), null, null, null, "Security Session security information is initialized.");
			}
		}
	}
}
