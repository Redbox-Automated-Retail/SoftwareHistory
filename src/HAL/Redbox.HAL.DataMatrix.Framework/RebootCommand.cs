namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class RebootCommand : TextCommand
{
	protected override string Prefix => "Z";

	protected override string Data => "0";

	internal RebootCommand()
	{
	}
}
