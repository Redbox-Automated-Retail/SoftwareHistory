using System;

namespace VisualHint.SmartPropertyGrid;

public class ValueValidationEventArgs : PropertyEventArgs
{
	private PropertyEnumerator _invalidPropEnum;

	private PropertyValue.ValueValidationResult _valueValidationResult;

	private object _valueToValidate;

	private Exception _exception;

	public PropertyEnumerator InvalidPropEnum => _invalidPropEnum;

	public PropertyValue.ValueValidationResult ValueValidationResult => _valueValidationResult;

	public object ValueToValidate
	{
		get
		{
			return _valueToValidate;
		}
		set
		{
			_valueToValidate = value;
		}
	}

	public Exception Exception => _exception;

	public string Message
	{
		get
		{
			if (_valueValidationResult == PropertyValue.ValueValidationResult.Validated || _valueValidationResult == PropertyValue.ValueValidationResult.PreviousValueRestored)
			{
				return "";
			}
			if (_exception != null)
			{
				if (_exception.InnerException != null)
				{
					return _exception.InnerException.Message;
				}
				return _exception.Message;
			}
			return PropertyValidatorBase.Message;
		}
	}

	public ValueValidationEventArgs(PropertyEnumerator propEnum, PropertyEnumerator invalidPropEnum, object value, PropertyValue.ValueValidationResult result)
		: base(propEnum)
	{
		_valueValidationResult = result;
		_valueToValidate = value;
		_invalidPropEnum = invalidPropEnum;
	}

	public ValueValidationEventArgs(PropertyEnumerator propEnum, PropertyEnumerator invalidPropEnum, object value, PropertyValue.ValueValidationResult result, Exception e)
		: this(propEnum, invalidPropEnum, value, result)
	{
		_exception = e;
	}
}
