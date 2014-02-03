namespace Redbox.Macros;

internal class FunctionArgument
{
	private readonly int _index;

	private readonly string _name;

	private readonly object _value;

	private readonly Redbox.Macros.ExpressionTokenizer.Position _beforeArgument;

	private readonly Redbox.Macros.ExpressionTokenizer.Position _afterArgument;

	public int Index => _index;

	public string Name => _name;

	public object Value => _value;

	public Redbox.Macros.ExpressionTokenizer.Position BeforeArgument => _beforeArgument;

	public Redbox.Macros.ExpressionTokenizer.Position AfterArgument => _afterArgument;

	public FunctionArgument(string name, int index, object value, Redbox.Macros.ExpressionTokenizer.Position beforeArgument, Redbox.Macros.ExpressionTokenizer.Position afterArgument)
	{
		_name = name;
		_index = index;
		_value = value;
		_beforeArgument = beforeArgument;
		_afterArgument = afterArgument;
	}
}
