namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class UnknownCommand1 : RegisterCommand
{
	internal override string Name => "Uknown register1";

	protected internal override string Register => "d1";

	protected internal override string Value => "2";
}
