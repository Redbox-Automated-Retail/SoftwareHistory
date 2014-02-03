using System;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All)]
public class SortedCategoryAttribute : CategoryAttribute
{
	private int _sortedIndex;

	public int SortedIndex => _sortedIndex;

	public SortedCategoryAttribute(string category, int sortedIndex)
		: base(category)
	{
		_sortedIndex = sortedIndex;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SortedCategoryAttribute sortedCategoryAttribute))
		{
			return false;
		}
		return _sortedIndex == sortedCategoryAttribute._sortedIndex;
	}

	public override int GetHashCode()
	{
		return _sortedIndex.GetHashCode();
	}
}
