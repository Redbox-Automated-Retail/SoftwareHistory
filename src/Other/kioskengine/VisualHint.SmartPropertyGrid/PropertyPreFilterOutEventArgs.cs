using System;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

public class PropertyPreFilterOutEventArgs : EventArgs
{
	public enum FilterModes
	{
		FilterDefault,
		FilterOut,
		DontFilter
	}

	private FilterModes _filterOut;

	private PropertyDescriptor _propertyDescriptor;

	private object _targetInstance;

	public FilterModes FilterOut
	{
		set
		{
			_filterOut = value;
		}
	}

	public PropertyDescriptor PropertyDescriptor => _propertyDescriptor;

	public object TargetInstance => _targetInstance;

	internal FilterModes GetFilterOutInternal()
	{
		return _filterOut;
	}

	public PropertyPreFilterOutEventArgs(PropertyDescriptor propertyDescriptor, object targetInstance)
	{
		_propertyDescriptor = propertyDescriptor;
		_targetInstance = targetInstance;
	}
}
