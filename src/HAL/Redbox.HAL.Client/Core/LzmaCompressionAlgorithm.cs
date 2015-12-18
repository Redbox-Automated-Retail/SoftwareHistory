using SevenZip.Compression.LZMA;

namespace Redbox.HAL.Core;

internal sealed class LzmaCompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		return SevenZipHelper.Compress(source);
	}

	public override byte[] Decompress(byte[] source)
	{
		return SevenZipHelper.Decompress(source);
	}
}
