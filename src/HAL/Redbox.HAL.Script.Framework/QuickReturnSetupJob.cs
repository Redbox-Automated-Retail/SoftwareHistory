using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "quick-return-setup", Operand = "QUICK-RETURN-SETUP")]
internal sealed class QuickReturnSetupJob : NativeJobAdapter
{
	private class DeviceState
	{
		internal readonly string Name;

		internal readonly string GraphFile;

		internal readonly string LedState;

		internal DeviceState(string _name, string _file, string _ledState)
		{
			Name = _name;
			GraphFile = _file;
			LedState = _ledState;
		}
	}

	private static readonly DeviceState[] States = new DeviceState[8]
	{
		new DeviceState("Start", "start.grf", "led=Background,mode=On;led=GreenButton,mode=Blink;led=GreenArrow,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"),
		new DeviceState("Busy", "busy.grf", "led=Background,mode=On;led=GreenButton,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=On;led=RedArrow,mode=Off"),
		new DeviceState("Success", "success.grf", "led=Background,mode=On;led=GreenButton,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"),
		new DeviceState("Insert-Error", "insert-error.grf", "led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=Blink;led=RedArrow,mode=Off"),
		new DeviceState("Return-Error", "return-error.grf", "led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenArrow,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"),
		new DeviceState("Return", "return.grf", "led=Background,mode=On;led=GreenButton,mode=Off;led=GreenArrow,mode=Blink;led=RedButton,mode=Off;led=RedArrow,mode=Off"),
		new DeviceState("Error", "Error.grf", "led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenButton,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off"),
		new DeviceState("Wait", "Wait.grf", "led=Background,mode=On;led=GreenArrow,mode=Off;led=GreenButton,mode=Off;led=RedButton,mode=Off;led=RedArrow,mode=Off")
	};

	protected override void ExecuteInner()
	{
		if (!QuickReturnDevice.Instance.Reset())
		{
			LogHelper.Instance.Log("RESET of the quick return device returned an error.");
			AddError("Unable to reset the quick return device.");
			return;
		}
		DeviceState[] states = States;
		foreach (DeviceState deviceState in states)
		{
			if (!QuickReturnDevice.Instance.CreateState(deviceState.Name, deviceState.GraphFile, deviceState.LedState))
			{
				LogHelper.Instance.Log("[QuickReturn Setup] Unable to set state {0}", deviceState.Name);
				AddError("Unable to setup the quick return device.");
				break;
			}
		}
	}

	internal QuickReturnSetupJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
