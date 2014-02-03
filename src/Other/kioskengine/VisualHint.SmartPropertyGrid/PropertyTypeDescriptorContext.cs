using System;
using System.ComponentModel;

namespace VisualHint.SmartPropertyGrid;

public class PropertyTypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
{
	private PropertyDescriptor _descriptor;

	private object _targetInstance;

	private PropertyEnumerator _propEnum;

	private PropertyGrid _propertyGrid;

	public PropertyEnumerator OwnerPropertyEnumerator => _propEnum;

	public PropertyGrid PropertyGrid => _propertyGrid;

	public IContainer Container => null;

	public object Instance => _targetInstance;

	public PropertyDescriptor PropertyDescriptor => _descriptor;

	public PropertyTypeDescriptorContext(PropertyDescriptor descriptor, object instance, PropertyEnumerator propEnum, PropertyGrid propGrid)
	{
		_descriptor = descriptor;
		_targetInstance = instance;
		_propEnum = propEnum;
		_propertyGrid = propGrid;
	}

	public PropertyTypeDescriptorContext(PropertyDescriptor[] descriptors, object[] instances, PropertyEnumerator propEnum, PropertyGrid propGrid)
	{
		if (descriptors.Length == 1)
		{
			_descriptor = descriptors[0];
			_targetInstance = instances[0];
		}
		else
		{
			_descriptor = new MultiplePropertyDescriptor(descriptors);
			_targetInstance = instances;
		}
		_propEnum = propEnum;
		_propertyGrid = propGrid;
	}

	public void OnComponentChanged()
	{
	}

	public bool OnComponentChanging()
	{
		return true;
	}

	public object GetService(Type serviceType)
	{
		if (_propertyGrid != null && _propertyGrid is IServiceProvider)
		{
			return (_propertyGrid as IServiceProvider).GetService(serviceType);
		}
		return null;
	}
}
