using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class AirExchangerState
{
	private const string ExchangerMnemonic = "AIRXCHGR";

	private readonly HardwareService Service;

	public ExchangerFanStatus FanStatus { get; private set; }

	public bool Configured { get; private set; }

	public ExchangerFanStatus Configure(MachineConfiguration config)
	{
		Configured = config.AirExchangerStatus != AirExchangerStatus.NotConfigured;
		FanStatus = config.AirExchangerFanStatus;
		return FanStatus;
	}

	public ExchangerFanStatus ToggleFan()
	{
		string text = string.Format("{0} {1}", "AIRXCHGR", (ExchangerFanStatus.On == FanStatus) ? "FANOFF" : "FANON");
		HardwareJob val = default(HardwareJob);
		if (((ClientCommandResult)(object)Service.ExecuteImmediate(text, ref val)).Success)
		{
			if (ExchangerFanStatus.On == FanStatus)
			{
				FanStatus = ExchangerFanStatus.Off;
			}
			else if (ExchangerFanStatus.Off == FanStatus)
			{
				FanStatus = ExchangerFanStatus.On;
			}
		}
		return FanStatus;
	}

	public bool ToggleConfiguration()
	{
		bool configured = !Configured;
		HardwareJob val = default(HardwareJob);
		if (((ClientCommandResult)(object)Service.ExecuteImmediate($"SETCFG \"EnableIceQubePolling\" \"{configured.ToString()}\" TYPE=CONTROLLER", ref val)).Success)
		{
			Configured = configured;
			return true;
		}
		return false;
	}

	public AirExchangerState(HardwareService s)
	{
		Service = s;
	}
}
