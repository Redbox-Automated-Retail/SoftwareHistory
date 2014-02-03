namespace VisualHint.SmartPropertyGrid;

public class DisplayedValuesNeededEventArgs : PropertyEventArgs
{
	private object[] _displayedValues = new string[0];

	public object[] DisplayedValues
	{
		get
		{
			return _displayedValues;
		}
		set
		{
			_displayedValues = (object[])value.Clone();
		}
	}

	public object[] DisplayedValuePairs
	{
		set
		{
			_displayedValues = new KeyValuePair[value.Length / 2];
			int num = 1;
			object key = null;
			foreach (object obj in value)
			{
				if (num % 2 != 0)
				{
					key = obj;
				}
				else
				{
					_displayedValues[num / 2 - 1] = new KeyValuePair(key, obj as string);
				}
				num++;
			}
		}
	}

	public DisplayedValuesNeededEventArgs(PropertyEnumerator propEnum)
		: base(propEnum)
	{
	}
}
