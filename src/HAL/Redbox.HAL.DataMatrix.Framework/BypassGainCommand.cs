namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class BypassGainCommand : RegisterCommand
{
	internal override string Name => "Bypass gain";

	protected internal override string Register => "a9";

	protected internal override string Value => "#20";
}
