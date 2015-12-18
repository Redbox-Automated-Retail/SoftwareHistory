namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class InfaredLightCommand : TextCommand
{
	protected override string Prefix => "Q";

	protected override string Data => "(1f5)1";
}
