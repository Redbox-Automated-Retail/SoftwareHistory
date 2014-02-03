namespace Redbox.Macros;

internal abstract class FunctionSetBase
{
	protected Redbox.Macros.PropertyDictionary Properties { get; set; }

	protected FunctionSetBase(Redbox.Macros.PropertyDictionary properties)
	{
		Properties = properties;
	}
}
