namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class BypassExposureCommand : RegisterCommand
{
	internal override string Name => "Bypass exposure";

	protected internal override string Register => "a8";

	protected internal override string Value => "#100";
}
