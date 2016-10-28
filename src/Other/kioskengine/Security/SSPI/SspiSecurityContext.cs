using System;
using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public abstract class SspiSecurityContext : IDisposable
{
	public SspiApi.SecHandle _credHandle = new SspiApi.SecHandle();

	public long _ptsExpiry = 0L;

	public SspiApi.SecHandle _phContext = new SspiApi.SecHandle();

	private bool _disposed = false;

	private SspiApi.SecPkgContext_Sizes _secPkgContext_Sizes = default(SspiApi.SecPkgContext_Sizes);

	private bool _secPkgContext_SizesInitialized = false;

	private object _secPkgContext_SizesLock = new object();

	public SspiSecurityContext()
	{
	}

	~SspiSecurityContext()
	{
		Dispose();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			SspiApi.DeleteSecurityContext(_phContext);
			SspiApi.FreeCredentialsHandle(_credHandle);
		}
	}

	public void EncryptMessage(Stream messageStream, GenuineChunkedStream outputStream, SspiFeatureFlags sspiFeatureFlags)
	{
		lock (_secPkgContext_SizesLock)
		{
			if (!_secPkgContext_SizesInitialized)
			{
				SspiApi.QueryContextSizes(_phContext, ref _secPkgContext_Sizes);
				_secPkgContext_SizesInitialized = true;
			}
		}
		byte[] chunk = null;
		int position = 0;
		BinaryWriter binaryWriter = new BinaryWriter(outputStream);
		if ((sspiFeatureFlags & SspiFeatureFlags.Encryption) != SspiFeatureFlags.None)
		{
			SspiApi.EncryptMessage(_phContext, messageStream, binaryWriter, ref _secPkgContext_Sizes);
		}
		else if ((sspiFeatureFlags & SspiFeatureFlags.Signing) != SspiFeatureFlags.None)
		{
			outputStream.WriteInt32AndRememberItsLocation(0, out chunk, out position);
			long length = outputStream.Length;
			byte[] array = new byte[(int)messageStream.Length];
			GenuineUtility.ReadDataFromStream(messageStream, array, 0, array.Length);
			SspiApi.MakeSignature(_phContext, array, binaryWriter, ref _secPkgContext_Sizes);
			MessageCoder.WriteInt32(chunk, position, (int)(outputStream.Length - length));
			binaryWriter.Write(array.Length);
			binaryWriter.Write(array, 0, array.Length);
		}
		else
		{
			GenuineUtility.CopyStreamToStream(messageStream, outputStream);
		}
	}

	public Stream DecryptMessage(Stream sourceStream, SspiFeatureFlags sspiFeatureFlags)
	{
		lock (_secPkgContext_SizesLock)
		{
			if (!_secPkgContext_SizesInitialized)
			{
				SspiApi.QueryContextSizes(_phContext, ref _secPkgContext_Sizes);
				_secPkgContext_SizesInitialized = true;
			}
		}
		BinaryReader binaryReader = new BinaryReader(sourceStream);
		if ((sspiFeatureFlags & SspiFeatureFlags.Encryption) != SspiFeatureFlags.None)
		{
			return SspiApi.DecryptMessage(_phContext, binaryReader);
		}
		if ((sspiFeatureFlags & SspiFeatureFlags.Signing) != SspiFeatureFlags.None)
		{
			int num = binaryReader.ReadInt32();
			byte[] array = new byte[num];
			GenuineUtility.ReadDataFromStream(sourceStream, array, 0, num);
			int num2 = binaryReader.ReadInt32();
			byte[] array2 = new byte[num2];
			GenuineUtility.ReadDataFromStream(sourceStream, array2, 0, num2);
			SspiApi.VerifySignature(_phContext, array2, array);
			return new MemoryStream(array2, 0, array2.Length, writable: false, publiclyVisible: true);
		}
		return sourceStream;
	}
}
