using System;

namespace VisualHint.SmartPropertyGrid;

public class PropertyHeightMultiplierAttribute : Attribute
{
	private int _heightMultiplier;

	private int _hashCode;

	public int HeightMultiplier => _heightMultiplier;

	public PropertyHeightMultiplierAttribute(int heightMultiplier)
	{
		_heightMultiplier = heightMultiplier;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyHeightMultiplierAttribute propertyHeightMultiplierAttribute))
		{
			return false;
		}
		return propertyHeightMultiplierAttribute.HeightMultiplier == _heightMultiplier;
	}

	public override int GetHashCode()
	{
		if (_hashCode == 0)
		{
			_hashCode = _heightMultiplier.GetHashCode();
		}
		return _heightMultiplier;
	}
}
