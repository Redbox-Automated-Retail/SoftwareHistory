using System;

namespace VisualHint.SmartPropertyGrid;

internal class PropertyValueSimulatedCreateInstanceException : Exception
{
	public PropertyValueSimulatedCreateInstanceException(Exception innerException)
		: base("", innerException)
	{
	}
}
