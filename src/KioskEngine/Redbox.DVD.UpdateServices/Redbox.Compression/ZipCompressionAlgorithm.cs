using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression;

internal class ZipCompressionAlgorithm : Redbox.Compression.CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		byte[] array;
		using (MemoryStream memoryStream = new MemoryStream(source))
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			using (MemoryStream memoryStream2 = new MemoryStream())
			{
				ICSharpCode.SharpZipLib.Zip.ZipOutputStream zipOutputStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(memoryStream2);
				ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("file");
				zipEntry.DateTime = DateTime.UtcNow;
				zipEntry.Size = source.Length;
				zipEntry.CompressionMethod = ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated;
				ICSharpCode.SharpZipLib.Zip.ZipEntry entry = zipEntry;
				zipOutputStream.PutNextEntry(entry);
				zipOutputStream.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
				zipOutputStream.Finish();
				zipOutputStream.Flush();
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
				ICSharpCode.SharpZipLib.Zip.ZipInputStream zipInputStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(baseInputStream);
				while (true)
				{
					bool flag = true;
					int num = zipInputStream.Read(buffer, 0, 4096);
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
		ICSharpCode.SharpZipLib.Zip.ZipOutputStream zipOutputStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(target);
		ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry("file");
		zipEntry.DateTime = DateTime.UtcNow;
		zipEntry.Size = source.Length;
		zipEntry.CompressionMethod = ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated;
		ICSharpCode.SharpZipLib.Zip.ZipEntry entry = zipEntry;
		zipOutputStream.PutNextEntry(entry);
		zipOutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
		zipOutputStream.Finish();
		zipOutputStream.Flush();
		binaryReader.Close();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[4096];
		ICSharpCode.SharpZipLib.Zip.ZipInputStream zipInputStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(source);
		while (true)
		{
			bool flag = true;
			int num = zipInputStream.Read(buffer, 0, 4096);
			if (num == 0)
			{
				break;
			}
			target.Write(buffer, 0, num);
		}
	}
}
