using System;

namespace Skybound.ComponentModel;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class PropertyListValueAttribute : Attribute
{
	public static readonly PropertyListValueAttribute Value = new PropertyListValueAttribute();
}
