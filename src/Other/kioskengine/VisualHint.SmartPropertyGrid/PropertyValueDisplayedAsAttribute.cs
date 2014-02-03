using System;
using System.Collections;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class PropertyValueDisplayedAsAttribute : PropertyAndChildrenAttribute
{
	private object[] _displayedValues;

	private int _hashCode;

	private bool _triggerDisplayedValuesNeededEventAtDropDown;

	public bool TriggerDisplayedValuesNeededEventAtDropDown => _triggerDisplayedValuesNeededEventAtDropDown;

	public virtual object[] DisplayedValues
	{
		get
		{
			if (base.ChildPropertyName.Length != 0)
			{
				return null;
			}
			return _displayedValues;
		}
	}

	public override object TypeId => "PropertyValueDisplayedAs" + base.ChildPropertyName;

	public PropertyValueDisplayedAsAttribute(bool usePairs, object[] displayedValues)
	{
		if (!usePairs)
		{
			_displayedValues = (object[])displayedValues.Clone();
			return;
		}
		_displayedValues = new KeyValuePair[displayedValues.Length / 2];
		int num = 1;
		object key = null;
		foreach (object obj in displayedValues)
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

	public PropertyValueDisplayedAsAttribute(bool triggerDisplayedValuesNeededEventAtDropDown)
	{
		_triggerDisplayedValuesNeededEventAtDropDown = triggerDisplayedValuesNeededEventAtDropDown;
	}

	public PropertyValueDisplayedAsAttribute(string[] displayedValues)
	{
		_displayedValues = new string[displayedValues.Length];
		displayedValues.CopyTo(_displayedValues, 0);
	}

	public PropertyValueDisplayedAsAttribute(ICollection displayedValues)
	{
		_displayedValues = new string[displayedValues.Count];
		displayedValues.CopyTo(_displayedValues, 0);
	}

	public PropertyValueDisplayedAsAttribute(string displayedValue)
	{
		_displayedValues = new string[1];
		_displayedValues[0] = displayedValue;
	}

	public PropertyValueDisplayedAsAttribute(string childPropertyName, object[] displayedValues)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		_displayedValues = (object[])displayedValues.Clone();
		mChildPropertyName = childPropertyName;
	}

	public PropertyValueDisplayedAsAttribute(string childPropertyName, bool usePairs, object[] displayedValues)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		if (!usePairs)
		{
			_displayedValues = (object[])displayedValues.Clone();
			return;
		}
		_displayedValues = new KeyValuePair[displayedValues.Length / 2];
		int num = 1;
		object key = null;
		foreach (object obj in displayedValues)
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

	public PropertyValueDisplayedAsAttribute(string childPropertyName, string[] displayedValues)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		_displayedValues = new string[displayedValues.Length];
		displayedValues.CopyTo(_displayedValues, 0);
		mChildPropertyName = childPropertyName;
	}

	public PropertyValueDisplayedAsAttribute(string childPropertyName, ICollection displayedValues)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		_displayedValues = new string[displayedValues.Count];
		displayedValues.CopyTo(_displayedValues, 0);
		mChildPropertyName = childPropertyName;
	}

	public virtual object[] GetDisplayedValues(string propertyName)
	{
		if (propertyName.CompareTo(base.ChildPropertyName) == 0)
		{
			return _displayedValues;
		}
		return null;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			if (_displayedValues != null)
			{
				_hashCode = Utilities.GetHashCode(_displayedValues);
			}
			_hashCode ^= base.ChildPropertyName.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyValueDisplayedAsAttribute propertyValueDisplayedAsAttribute))
		{
			return false;
		}
		if (base.ChildPropertyName.CompareTo(propertyValueDisplayedAsAttribute.ChildPropertyName) != 0)
		{
			return false;
		}
		if (_displayedValues != null)
		{
			if (_displayedValues.Length != propertyValueDisplayedAsAttribute._displayedValues.Length)
			{
				return false;
			}
			for (int i = 0; i < _displayedValues.Length; i++)
			{
				if (_displayedValues[i] != propertyValueDisplayedAsAttribute._displayedValues[i])
				{
					return false;
				}
			}
		}
		return true;
	}
}
