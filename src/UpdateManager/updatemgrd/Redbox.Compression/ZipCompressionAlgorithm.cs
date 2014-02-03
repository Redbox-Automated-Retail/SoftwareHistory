using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression;

internal class ZipCompressionAlgorithm : CompressionAlgorithm
{
	private const int BufferSize = 1024000;

	public override byte[] Compress(byte[] source)
	{
		byte[] array = new byte[1024000];
		using MemoryStream input = new MemoryStream(source);
		BinaryReader binaryReader = new BinaryReader(input);
		byte[] array2;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream);
			ZipEntry zipEntry = new ZipEntry("file");
			zipEntry.DateTime = DateTime.UtcNow;
			zipEntry.Size = source.Length;
			zipEntry.CompressionMethod = CompressionMethod.Deflated;
			ZipEntry entry = zipEntry;
			zipOutputStream.PutNextEntry(entry);
			while (true)
			{
				int num = binaryReader.Read(array, 0, array.Length);
				if (num == 0)
				{
					break;
				}
				zipOutputStream.Write(array, 0, num);
			}
			zipOutputStream.Finish();
			zipOutputStream.Flush();
			array2 = new byte[memoryStream.Length];
			Array.Copy(memoryStream.GetBuffer(), 0L, array2, 0L, memoryStream.Length);
		}
		binaryReader.Close();
		return array2;
	}

	public override byte[] Decompress(byte[] source)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		using (MemoryStream baseInputStream = new MemoryStream(source))
		{
			byte[] array = new byte[1024000];
			ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
			while (true)
			{
				int num = zipInputStream.Read(array, 0, array.Length);
				if (num != 0)
				{
					binaryWriter.Write(array, 0, num);
					continue;
				}
				break;
			}
		}
		byte[] array2 = new byte[memoryStream.Length];
		Array.Copy(memoryStream.GetBuffer(), 0L, array2, 0L, memoryStream.Length);
		binaryWriter.Close();
		return array2;
	}

	public override void Compress(Stream source, Stream target)
	{
		using BinaryReader binaryReader = new BinaryReader(source);
		using ZipOutputStream zipOutputStream = new ZipOutputStream(target);
		ZipEntry zipEntry = new ZipEntry("file");
		zipEntry.DateTime = DateTime.UtcNow;
		zipEntry.Size = source.Length;
		zipEntry.CompressionMethod = CompressionMethod.Deflated;
		ZipEntry entry = zipEntry;
		zipOutputStream.PutNextEntry(entry);
		zipOutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
		zipOutputStream.Finish();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] array = new byte[1024000];
		using ZipInputStream zipInputStream = new ZipInputStream(source);
		while (true)
		{
			int num = zipInputStream.Read(array, 0, array.Length);
			if (num != 0)
			{
				target.Write(array, 0, num);
				continue;
			}
			break;
		}
	}
}
