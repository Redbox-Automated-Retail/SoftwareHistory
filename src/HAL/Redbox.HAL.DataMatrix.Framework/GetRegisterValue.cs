using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class GetRegisterValue : TextCommand
{
	private readonly string Register;

	private readonly string ExpectedValue;

	internal bool IsCorrectlySet
	{
		get
		{
			CortexPacket[] parsedPackets = base.ParsedPackets;
			if (parsedPackets.Length == 0 || parsedPackets[0].RawPacketType != 'd')
			{
				LogHelper.Instance.Log("The response packet for GetRegister is malformed (found {0} packet).", (parsedPackets.Length == 0) ? "nil" : parsedPackets[0].RawPacketType.ToString());
				return false;
			}
			return true;
		}
	}

	protected override string Prefix => "G";

	protected override string Data => $"({Register})";

	internal GetRegisterValue(RegisterCommand command)
	{
		Register = command.Register;
		ExpectedValue = command.Value;
	}

	internal GetRegisterValue(string register, string expected)
	{
		Register = register;
		ExpectedValue = expected;
	}
}
