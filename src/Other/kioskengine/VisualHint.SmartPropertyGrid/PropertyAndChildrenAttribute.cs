using System;

namespace VisualHint.SmartPropertyGrid;

public class PropertyAndChildrenAttribute : Attribute
{
	protected string mChildPropertyName = "";

	public string ChildPropertyName => mChildPropertyName;
}
