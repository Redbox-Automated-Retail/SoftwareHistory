using System.Collections;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

public class PropertyComparerDefaultSort : SPGComparer, IComparer
{
	public PropertyComparerDefaultSort(PropertyGrid grid)
		: base(grid)
	{
	}

	public int Compare(object x, object y)
	{
		PropertyDescriptor propertyDescriptor = x as PropertyDescriptor;
		PropertyDescriptor propertyDescriptor2 = y as PropertyDescriptor;
		if (propertyDescriptor.Equals(propertyDescriptor2))
		{
			return 0;
		}
		if (this != null && base.Grid.CompareUnderCategories)
		{
			string text = propertyDescriptor.Category;
			if (text == null)
			{
				text = PropertyGrid.MiscCategoryName;
			}
			string text2 = propertyDescriptor2.Category;
			if (text2 == null)
			{
				text2 = PropertyGrid.MiscCategoryName;
			}
			if (text.CompareTo(text2) < 0)
			{
				return -1;
			}
			if (text.CompareTo(text2) > 0)
			{
				return 1;
			}
		}
		SortedPropertyAttribute sortedPropertyAttribute = (SortedPropertyAttribute)propertyDescriptor.Attributes[typeof(SortedPropertyAttribute)];
		SortedPropertyAttribute sortedPropertyAttribute2 = (SortedPropertyAttribute)propertyDescriptor2.Attributes[typeof(SortedPropertyAttribute)];
		if (sortedPropertyAttribute != null && sortedPropertyAttribute2 == null)
		{
			return -1;
		}
		if (sortedPropertyAttribute2 != null && sortedPropertyAttribute == null)
		{
			return 1;
		}
		if (sortedPropertyAttribute != null && sortedPropertyAttribute2 != null)
		{
			int num = sortedPropertyAttribute.SortedIndex.CompareTo(sortedPropertyAttribute2.SortedIndex);
			if (num != 0)
			{
				return num;
			}
			return -1;
		}
		int num2 = propertyDescriptor.DisplayName.CompareTo(propertyDescriptor2.DisplayName);
		if (num2 <= 0)
		{
			return -1;
		}
		return 1;
	}
}
