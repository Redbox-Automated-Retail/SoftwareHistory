namespace Redbox.Command.Tokenizer;

internal enum CommandParserState
{
	Start = 1,
	Command,
	Comment,
	Symbol,
	Key,
	Value,
	Whitespace,
	Error
}
