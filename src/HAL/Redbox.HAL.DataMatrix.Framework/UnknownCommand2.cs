namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class UnknownCommand2 : RegisterCommand
{
	internal override string Name => "Unknown register2";

	protected internal override string Register => "66";

	protected internal override string Value => "#100";
}
