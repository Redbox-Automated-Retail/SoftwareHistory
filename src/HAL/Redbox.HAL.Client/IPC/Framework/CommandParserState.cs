namespace Redbox.HAL.IPC.Framework;

internal enum CommandParserState
{
	Start = 1,
	Command,
	Comment,
	Symbol,
	Key,
	Value,
	Whitespace,
	Error,
	StringLiteral
}
