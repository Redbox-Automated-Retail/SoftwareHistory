using System;
using System.IO;
using SevenZip;
using SevenZip.Compression.LZMA;

namespace Redbox.Compression;

internal class LzmaCompressionAlgorithm : Redbox.Compression.CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using MemoryStream source2 = new MemoryStream(source);
		Compress(source2, memoryStream);
		return memoryStream.ToArray();
	}

	public override byte[] Decompress(byte[] source)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using MemoryStream source2 = new MemoryStream(source);
		Decompress(source2, memoryStream);
		return memoryStream.ToArray();
	}

	public override void Compress(Stream source, Stream target)
	{
		SevenZip.CoderPropID[] propIDs = new SevenZip.CoderPropID[8]
		{
			SevenZip.CoderPropID.DictionarySize,
			SevenZip.CoderPropID.PosStateBits,
			SevenZip.CoderPropID.LitContextBits,
			SevenZip.CoderPropID.LitPosBits,
			SevenZip.CoderPropID.Algorithm,
			SevenZip.CoderPropID.NumFastBytes,
			SevenZip.CoderPropID.MatchFinder,
			SevenZip.CoderPropID.EndMarker
		};
		object[] properties = new object[8] { 2097152, 2, 4, 0, 2, 273, "bt4", true };
		SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
		encoder.SetCoderProperties(propIDs, properties);
		encoder.WriteCoderProperties(target);
		long length = source.Length;
		for (int i = 0; i < 8; i++)
		{
			target.WriteByte((byte)(length >> 8 * i));
		}
		encoder.Code(source, target, -1L, -1L, null);
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] array = new byte[5];
		if (source.Read(array, 0, 5) != 5)
		{
			throw new Exception("input .lzma is too short");
		}
		SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
		decoder.SetDecoderProperties(array);
		long num = 0L;
		for (int i = 0; i < 8; i++)
		{
			int num2 = source.ReadByte();
			if (num2 < 0)
			{
				throw new Exception("Can't Read 1");
			}
			num |= (long)((ulong)(byte)num2 << 8 * i);
		}
		long inSize = source.Length - source.Position;
		decoder.Code(source, target, inSize, num, null);
	}
}
