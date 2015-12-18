namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class VersionCommand : TextCommand
{
	protected override string Prefix => "I";

	protected override string Data => null;
}
