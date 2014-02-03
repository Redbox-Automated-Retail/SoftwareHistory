using System;

namespace Skybound.ComponentModel;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class StandardValueAttribute : Attribute
{
	public string Name;

	public StandardValueAttribute()
	{
	}

	public StandardValueAttribute(string name)
	{
		Name = name;
	}
}
