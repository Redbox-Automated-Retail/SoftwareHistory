using System;

namespace VisualHint.SmartPropertyGrid;

public class SortedPropertyAttribute : Attribute
{
	private int _sortIndex;

	public int SortedIndex => _sortIndex;

	public SortedPropertyAttribute(int sortIndex)
	{
		_sortIndex = sortIndex;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SortedPropertyAttribute sortedPropertyAttribute))
		{
			return false;
		}
		return _sortIndex == sortedPropertyAttribute._sortIndex;
	}

	public override int GetHashCode()
	{
		return _sortIndex.GetHashCode();
	}
}
