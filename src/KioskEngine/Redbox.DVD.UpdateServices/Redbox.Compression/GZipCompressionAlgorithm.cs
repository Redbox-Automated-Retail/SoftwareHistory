using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.Compression;

internal class GZipCompressionAlgorithm : Redbox.Compression.CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream(source))
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				ICSharpCode.SharpZipLib.GZip.GZipOutputStream gZipOutputStream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(memoryStream2);
				gZipOutputStream.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
				gZipOutputStream.Finish();
				gZipOutputStream.Flush();
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
			using (MemoryStream baseInputStream = new MemoryStream(source))
			{
				byte[] buffer = new byte[4096];
				ICSharpCode.SharpZipLib.GZip.GZipInputStream gZipInputStream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(baseInputStream, 4096);
				while (true)
				{
					bool flag = true;
					int num = gZipInputStream.Read(buffer, 0, 4096);
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
		ICSharpCode.SharpZipLib.GZip.GZipOutputStream gZipOutputStream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(target);
		gZipOutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
		gZipOutputStream.Finish();
		gZipOutputStream.Flush();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[4096];
		ICSharpCode.SharpZipLib.GZip.GZipInputStream gZipInputStream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(source, 4096);
		while (true)
		{
			bool flag = true;
			int num = gZipInputStream.Read(buffer, 0, 4096);
			if (num == 0)
			{
				break;
			}
			target.Write(buffer, 0, num);
		}
	}
}
