using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal sealed class ZipCompressionAlgorithm : ICompressionAlgorithm
{
	public byte[] Compress(byte[] source)
	{
		using MemoryStream memoryStream = StreamBase.NewOn(source);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		byte[] array;
		using (MemoryStream memoryStream2 = StreamBase.New())
		{
			Redbox_002EHAL_002EClient1178151_002EZipOutputStream obj = new Redbox_002EHAL_002EClient1178151_002EZipOutputStream(memoryStream2);
			obj.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
			obj.Finish();
			obj.Flush();
			array = new byte[memoryStream2.Length];
			Array.Copy(memoryStream2.GetBuffer(), 0L, array, 0L, memoryStream2.Length);
		}
		binaryReader.Close();
		return array;
	}

	public byte[] Decompress(byte[] source)
	{
		using MemoryStream memoryStream = StreamBase.New();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		using (MemoryStream memoryStream2 = StreamBase.NewOn(source))
		{
			byte[] buffer = new byte[4096];
			Redbox_002EHAL_002EClient1178151_002EZipInputStream obj = new Redbox_002EHAL_002EClient1178151_002EZipInputStream(memoryStream2);
			while (true)
			{
				int num = obj.Read(buffer, 0, 4096);
				if (num != 0)
				{
					binaryWriter.Write(buffer, 0, num);
					continue;
				}
				break;
			}
		}
		byte[] array = new byte[memoryStream.Length];
		Array.Copy(memoryStream.GetBuffer(), 0L, array, 0L, memoryStream.Length);
		binaryWriter.Close();
		return array;
	}
}
