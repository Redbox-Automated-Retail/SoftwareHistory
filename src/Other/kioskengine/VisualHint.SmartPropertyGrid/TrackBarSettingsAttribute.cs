using System;

namespace VisualHint.SmartPropertyGrid;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
public class TrackBarSettingsAttribute : PropertyAndChildrenAttribute
{
	private object _resolution;

	private object _smallChange;

	private object _largeChange;

	private int _hashCode;

	public object Resolution => _resolution;

	public object SmallChange => _smallChange;

	public object LargeChange => _largeChange;

	public override object TypeId => "TrackBarSettings" + base.ChildPropertyName;

	public TrackBarSettingsAttribute(object resolution, object smallChange, object largeChange)
	{
		_resolution = resolution;
		_smallChange = smallChange;
		_largeChange = largeChange;
	}

	public TrackBarSettingsAttribute(string childPropertyName, object resolution, object smallChange, object largeChange)
	{
		if (childPropertyName == null)
		{
			throw new ArgumentException("The name of the child property can't be null");
		}
		mChildPropertyName = childPropertyName;
		_resolution = resolution;
		_smallChange = smallChange;
		_largeChange = largeChange;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode ^= base.ChildPropertyName.GetHashCode();
			_hashCode ^= _resolution.GetHashCode();
			_hashCode ^= _smallChange.GetHashCode();
			_hashCode ^= _largeChange.GetHashCode();
		}
		return _hashCode;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TrackBarSettingsAttribute trackBarSettingsAttribute))
		{
			return false;
		}
		if (base.ChildPropertyName != trackBarSettingsAttribute.ChildPropertyName)
		{
			return false;
		}
		if (!_resolution.Equals(trackBarSettingsAttribute._resolution))
		{
			return false;
		}
		if (!_smallChange.Equals(trackBarSettingsAttribute._smallChange))
		{
			return false;
		}
		if (!_largeChange.Equals(trackBarSettingsAttribute._largeChange))
		{
			return false;
		}
		return true;
	}
}
