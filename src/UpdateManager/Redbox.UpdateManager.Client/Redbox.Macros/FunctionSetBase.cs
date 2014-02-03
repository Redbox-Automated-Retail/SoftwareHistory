namespace Redbox.Macros;

internal abstract class FunctionSetBase
{
	protected PropertyDictionary Properties { get; set; }

	protected FunctionSetBase(PropertyDictionary properties)
	{
		Properties = properties;
	}
}
