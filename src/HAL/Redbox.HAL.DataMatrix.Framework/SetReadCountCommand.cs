namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class SetReadCountCommand : RegisterCommand
{
	private readonly string m_value;

	internal override string Name => "Read count";

	protected internal override string Register => "34";

	protected internal override string Value => m_value;

	internal SetReadCountCommand(int k)
	{
		if (k >= 0 && k <= 9)
		{
			m_value = k.ToString();
		}
		else
		{
			m_value = "6";
		}
	}
}
