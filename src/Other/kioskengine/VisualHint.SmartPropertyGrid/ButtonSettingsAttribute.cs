using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class ButtonSettingsAttribute : PropertyAndChildrenAttribute
{
	private string _buttonText = "...";

	private bool _fullWidth;

	private int _hashCode;

	public string ButtonText
	{
		get
		{
			return _buttonText;
		}
		set
		{
			_buttonText = value;
		}
	}

	public bool FullWidth
	{
		get
		{
			return _fullWidth;
		}
		set
		{
			_fullWidth = value;
		}
	}

	public override object TypeId => "ButtonSettings" + base.ChildPropertyName;

	public ButtonSettingsAttribute(string buttonText, bool fullWidth)
	{
		_fullWidth = fullWidth;
		_buttonText = buttonText;
	}

	public ButtonSettingsAttribute(string childPropertyName, string buttonText, bool fullWidth)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_fullWidth = fullWidth;
		_buttonText = buttonText;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode ^= base.ChildPropertyName.GetHashCode();
			_hashCode ^= _fullWidth.GetHashCode();
			if (_buttonText != null)
			{
				_hashCode ^= _buttonText.GetHashCode();
			}
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ButtonSettingsAttribute buttonSettingsAttribute))
		{
			return false;
		}
		if (base.ChildPropertyName != buttonSettingsAttribute.ChildPropertyName)
		{
			return false;
		}
		if (!_fullWidth.Equals(buttonSettingsAttribute._fullWidth))
		{
			return false;
		}
		if (_buttonText != buttonSettingsAttribute._buttonText)
		{
			return false;
		}
		return true;
	}
}
