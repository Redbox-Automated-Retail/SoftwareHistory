using System.Collections;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

public class PropertyComparerNoSort : IComparer
{
	private PropertyDescriptorCollection _pdCollection;

	internal PropertyDescriptorCollection OriginalCollection
	{
		set
		{
			_pdCollection = value;
		}
	}

	public int Compare(object x, object y)
	{
		PropertyDescriptor propertyDescriptor = x as PropertyDescriptor;
		PropertyDescriptor propertyDescriptor2 = y as PropertyDescriptor;
		if (propertyDescriptor.Equals(propertyDescriptor2))
		{
			return 0;
		}
		if (_pdCollection == null)
		{
			return -1;
		}
		if (_pdCollection.IndexOf(propertyDescriptor) >= _pdCollection.IndexOf(propertyDescriptor2))
		{
			return 1;
		}
		return -1;
	}
}
