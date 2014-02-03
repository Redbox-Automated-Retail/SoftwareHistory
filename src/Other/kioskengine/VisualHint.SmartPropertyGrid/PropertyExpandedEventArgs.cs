namespace VisualHint.SmartPropertyGrid;

public class PropertyExpandedEventArgs : PropertyEventArgs
{
	private bool _expanded;

	public bool Expanded => _expanded;

	public PropertyExpandedEventArgs(PropertyEnumerator propEnum, bool expanded)
		: base(propEnum)
	{
		_expanded = expanded;
	}
}
