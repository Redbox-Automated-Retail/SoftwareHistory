using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CortexSettingsValidator
{
	private readonly CortexService Service;

	private readonly char[] Separator = new char[1] { ':' };

	private static readonly string[] Registers = new string[45]
	{
		"26:0", "d1:2", "a8:#100", "a9:#20", "66:#100", "1b7:0", "256:2", "aa:#7fffffff", "34:5", "b4:1",
		"0A:0", "0B:0", "0C:0", "16:1", "18:0", "19:1", "1A:0", "29:0", "2A:0", "2B:0",
		"47:0", "49:0", "4A:0", "4B:0", "4C:0", "4F:0", "50:0", "6A:0", "6B:0", "6C:0",
		"6D:0", "6E:0", "6F:0", "85:0", "CD:0", "CE:0", "CF:0", "F7:0", "F8:0", "127:0",
		"12D:0", "217:0", "24A:0", "250:0", "21D:1"
	};

	internal CortexSettingsValidator(CortexService service)
	{
		Service = service;
	}

	internal bool Validate(IMessageSink sink)
	{
		string[] registers = Registers;
		for (int i = 0; i < registers.Length; i++)
		{
			string[] array = registers[i].Split(Separator);
			GetRegisterValue getRegisterValue = new GetRegisterValue(array[0], array[1]);
			getRegisterValue.Send(Service.ConfiguredPort());
			if (!getRegisterValue.IsCorrectlySet)
			{
				sink.Send($"The register {array[0]} received an unexpected response.");
			}
			else
			{
				sink.Send($"Register {array[0]} matched expected value");
			}
			Thread.Sleep(200);
		}
		return true;
	}
}
