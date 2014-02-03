namespace Redbox.HAL.Core;

internal enum FunctionTokenizerState
{
	Start,
	Function,
	Parameter,
	Whitespace,
	Error
}
