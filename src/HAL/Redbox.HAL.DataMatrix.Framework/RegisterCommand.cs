namespace Redbox.HAL.DataMatrix.Framework;

internal abstract class RegisterCommand : TextCommand
{
	internal abstract string Name { get; }

	protected internal abstract string Register { get; }

	protected internal abstract string Value { get; }

	protected override string Prefix => "C";

	protected override string Data => $"({Register}){Value}";
}
