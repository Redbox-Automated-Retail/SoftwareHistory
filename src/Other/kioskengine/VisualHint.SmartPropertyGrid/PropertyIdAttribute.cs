using System;

namespace VisualHint.SmartPropertyGrid;

public class PropertyIdAttribute : Attribute
{
	private int _id;

	public int Id => _id;

	public PropertyIdAttribute(int id)
	{
		_id = id;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is PropertyIdAttribute propertyIdAttribute))
		{
			return false;
		}
		return _id == propertyIdAttribute._id;
	}

	public override int GetHashCode()
	{
		return _id.GetHashCode();
	}
}
