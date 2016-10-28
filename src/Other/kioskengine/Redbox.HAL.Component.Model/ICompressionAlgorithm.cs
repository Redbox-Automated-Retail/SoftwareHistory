namespace Redbox.HAL.Component.Model;

internal interface ICompressionAlgorithm
{
	byte[] Compress(byte[] source);

	byte[] Decompress(byte[] source);
}
