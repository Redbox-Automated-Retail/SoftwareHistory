namespace Redbox.HAL.DataStorage;

public sealed class Counter
{
	private int m_value;

	public string Name { get; set; }

	public int Value
	{
		get
		{
			return m_value;
		}
		internal set
		{
			m_value = value;
		}
	}

	internal void Increment()
	{
		m_value++;
	}

	internal void Decrement()
	{
		m_value--;
	}
}
