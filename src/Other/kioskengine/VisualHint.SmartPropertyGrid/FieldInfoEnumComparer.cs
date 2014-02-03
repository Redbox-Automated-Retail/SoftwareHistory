using System;
using System.Collections.Generic;
using System.Reflection;

namespace VisualHint.SmartPropertyGrid;

internal class FieldInfoEnumComparer : IComparer<object>
{
	private Type _enumType;

	public FieldInfoEnumComparer(Type enumType)
	{
		_enumType = enumType;
	}

	public int Compare(object o1, object o2)
	{
		FieldInfo fieldInfo = o1 as FieldInfo;
		FieldInfo fieldInfo2 = o2 as FieldInfo;
		int hashCode = Enum.Parse(_enumType, fieldInfo.Name).GetHashCode();
		int hashCode2 = Enum.Parse(_enumType, fieldInfo2.Name).GetHashCode();
		if (hashCode < hashCode2)
		{
			return -1;
		}
		if (hashCode > hashCode2)
		{
			return 1;
		}
		return 0;
	}
}
