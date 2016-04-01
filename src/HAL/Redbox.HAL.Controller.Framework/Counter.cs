using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class Counter : IPersistentCounter
{
	private int m_value;

	public string Name { get; private set; }

	public int Value => m_value;

	public int Increment()
	{
		m_value++;
		return m_value;
	}

	public int Decrement()
	{
		m_value--;
		return m_value;
	}

	public void Reset()
	{
		m_value = 0;
	}

	internal Counter(string name)
		: this(name, 0)
	{
	}

	internal Counter(string name, int value)
	{
		Name = name;
		m_value = value;
	}
}
