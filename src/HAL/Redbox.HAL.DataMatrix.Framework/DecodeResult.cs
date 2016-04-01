using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class DecodeResult : IDecodeResult
{
	private int m_count;

	public string Matrix { get; private set; }

	public int Count => m_count;

	internal void Increment()
	{
		m_count++;
	}

	internal DecodeResult(string matrix)
	{
		Matrix = matrix;
	}
}
