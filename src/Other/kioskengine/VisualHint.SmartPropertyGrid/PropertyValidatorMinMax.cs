using System;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

public class PropertyValidatorMinMax : PropertyValidatorBase
{
	private IComparable _min;

	private IComparable _max;

	public IComparable Min
	{
		get
		{
			return _min;
		}
		set
		{
			_min = value;
		}
	}

	public IComparable Max
	{
		get
		{
			return _max;
		}
		set
		{
			_max = value;
		}
	}

	public PropertyValidatorMinMax(PropertyEnumerator propEnum, IComparable min, IComparable max)
		: base(propEnum)
	{
		_min = min;
		_max = max;
	}

	public PropertyValidatorMinMax(IComparable min, IComparable max)
	{
		_min = min;
		_max = max;
	}

	public override bool Check(object value, bool modify)
	{
		if (value == null)
		{
			return true;
		}
		if (base.Enumerator == null)
		{
			throw new NullReferenceException("The constructor of the validator should have been passed an enumerator on the corresponding property.");
		}
		TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
		PropertyTypeDescriptorContext typeDescriptorContext = base.Enumerator.Property.Value.GetTypeDescriptorContext(base.Enumerator);
		IComparable comparable = ((!converter.CanConvertFrom(typeDescriptorContext, value.GetType())) ? ((IComparable)value) : ((IComparable)converter.ConvertFrom(typeDescriptorContext, base.Enumerator.Property.Value.CultureInfo, value)));
		bool flag = true;
		if (comparable.CompareTo(Min) == -1)
		{
			if (modify)
			{
				base.Enumerator.Property.Value.SetValue(Min);
			}
			flag = false;
		}
		else if (comparable.CompareTo(Max) == 1)
		{
			if (modify)
			{
				base.Enumerator.Property.Value.SetValue(Max);
			}
			flag = false;
		}
		if (!flag)
		{
			PropertyValidatorBase.Message = string.Concat(comparable, " is not contained between ", Min, " and ", Max, ".");
		}
		else
		{
			PropertyValidatorBase.Message = "";
		}
		return flag;
	}

	public override int GetHashCode()
	{
		return _min.GetHashCode() ^ _max.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyValidatorMinMax propertyValidatorMinMax))
		{
			return false;
		}
		if (_min.Equals(propertyValidatorMinMax._min))
		{
			return _max.Equals(propertyValidatorMinMax._max);
		}
		return false;
	}
}
