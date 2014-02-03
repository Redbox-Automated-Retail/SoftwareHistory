using System;
using System.Collections;
using System.ComponentModel;

namespace Alsing.Design;

public class FilteredTypeConverter : TypeConverter
{
	protected virtual void FilterProperties(IDictionary Properties, object value)
	{
	}

	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		PropertyDescriptorCollection propertyDescriptorCollection = (propertyDescriptorCollection = TypeDescriptor.GetProperties(value, attributes, noCustomTypeDesc: false));
		Hashtable hashtable = new Hashtable();
		foreach (PropertyDescriptor item in propertyDescriptorCollection)
		{
			hashtable[item.Name] = item;
		}
		FilterProperties(hashtable, value);
		PropertyDescriptor[] array = new PropertyDescriptor[hashtable.Values.Count];
		hashtable.Values.CopyTo(array, 0);
		return new PropertyDescriptorCollection(array);
	}

	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
