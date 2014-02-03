namespace VisualHint.SmartPropertyGrid;

public class PropertyPostFilterOutEventArgs : PropertyEventArgs
{
	private bool _filterOut;

	public bool FilterOut
	{
		set
		{
			_filterOut = value;
		}
	}

	internal bool GetFilterOutInternal()
	{
		return _filterOut;
	}

	internal PropertyPostFilterOutEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
