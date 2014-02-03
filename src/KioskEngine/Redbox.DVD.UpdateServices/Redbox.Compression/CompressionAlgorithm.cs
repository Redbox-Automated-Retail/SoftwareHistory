using System.IO;

namespace Redbox.Compression;

internal abstract class CompressionAlgorithm
{
	public static Redbox.Compression.CompressionAlgorithm GetAlgorithm(CompressionType type)
	{
		return type switch
		{
			CompressionType.None => new Redbox.Compression.NullCompressionAlgorithm(), 
			CompressionType.GZip => new Redbox.Compression.GZipCompressionAlgorithm(), 
			CompressionType.LZMA => new Redbox.Compression.LzmaCompressionAlgorithm(), 
			CompressionType.BZip2 => new Redbox.Compression.BZip2CompressionAlgorithm(), 
			CompressionType.Zip => new Redbox.Compression.ZipCompressionAlgorithm(), 
			_ => new Redbox.Compression.NullCompressionAlgorithm(), 
		};
	}

	public abstract byte[] Compress(byte[] source);

	public abstract byte[] Decompress(byte[] source);

	public abstract void Compress(Stream source, Stream target);

	public abstract void Decompress(Stream source, Stream target);
}
