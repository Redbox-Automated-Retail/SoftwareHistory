namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class SetReadFailureCommand : RegisterCommand
{
	internal override string Name => "Read failure";

	protected internal override string Register => "55";

	protected internal override string Value => "1";
}
