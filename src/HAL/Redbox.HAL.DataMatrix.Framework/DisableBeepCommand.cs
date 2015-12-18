namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class DisableBeepCommand : RegisterCommand
{
	internal override string Name => "Disable beep";

	protected internal override string Register => "26";

	protected internal override string Value => "0";
}
