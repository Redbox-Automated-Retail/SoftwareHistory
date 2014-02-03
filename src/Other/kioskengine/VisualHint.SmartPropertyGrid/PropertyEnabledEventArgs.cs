namespace VisualHint.SmartPropertyGrid;

public class PropertyEnabledEventArgs : PropertyEventArgs
{
	private bool _directlyModified;

	public bool DirectlyModified => _directlyModified;

	public PropertyEnabledEventArgs(PropertyEnumerator propEnum, bool direct)
		: base(propEnum)
	{
		_directlyModified = direct;
	}
}
