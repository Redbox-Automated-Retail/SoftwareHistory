using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.Compression;

internal class BZip2CompressionAlgorithm : Redbox.Compression.CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream(source))
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream bZip2OutputStream = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(memoryStream2);
				bZip2OutputStream.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
				bZip2OutputStream.Flush();
				array = new byte[memoryStream2.Length];
				Array.Copy(memoryStream2.GetBuffer(), 0L, array, 0L, memoryStream2.Length);
			}
			binaryReader.Close();
		}
		return array;
	}

	public override byte[] Decompress(byte[] source)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			using (MemoryStream stream = new MemoryStream(source))
			{
				byte[] buffer = new byte[4096];
				ICSharpCode.SharpZipLib.BZip2.BZip2InputStream bZip2InputStream = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(stream);
				while (true)
				{
					bool flag = true;
					int num = bZip2InputStream.Read(buffer, 0, 4096);
					if (num == 0)
					{
						break;
					}
					binaryWriter.Write(buffer, 0, num);
				}
			}
			array = new byte[memoryStream.Length];
			Array.Copy(memoryStream.GetBuffer(), 0L, array, 0L, memoryStream.Length);
			binaryWriter.Close();
		}
		return array;
	}

	public override void Compress(Stream source, Stream target)
	{
		BinaryReader binaryReader = new BinaryReader(source);
		ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream bZip2OutputStream = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(target);
		bZip2OutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
		bZip2OutputStream.Flush();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[4096];
		ICSharpCode.SharpZipLib.BZip2.BZip2InputStream bZip2InputStream = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(source);
		while (true)
		{
			bool flag = true;
			int num = bZip2InputStream.Read(buffer, 0, 4096);
			if (num == 0)
			{
				break;
			}
			target.Write(buffer, 0, num);
		}
	}
}
