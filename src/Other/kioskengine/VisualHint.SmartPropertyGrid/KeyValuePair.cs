using System.Diagnostics;

namespace VisualHint.SmartPropertyGrid;

internal class KeyValuePair
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object _key;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private object _value;

	public object Key => _key;

	public object Value => _value;

	public KeyValuePair(object key, object value)
	{
		_value = value;
		_key = key;
	}
}
