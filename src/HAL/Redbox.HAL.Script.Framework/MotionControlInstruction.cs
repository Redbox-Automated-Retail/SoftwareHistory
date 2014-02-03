using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class MotionControlInstruction : Instruction
{
	public override string Mnemonic => "MOTIONCONTROL";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count < 1)
		{
			AddError("C002", "MOTIONCONTROL instruction expects an operand.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		string value = base.Operands[0].Value;
		if (value.Equals("RESET", StringComparison.CurrentCultureIgnoreCase))
		{
			MotionController controller = MotionController.GetController();
			ExecutionEngine.Instance.ActiveContext.PushTop(controller.ResetDeviceDriver() ? "SUCCESS" : "FAILURE");
			return;
		}
		if (value.Equals("READLIMITS", StringComparison.CurrentCultureIgnoreCase))
		{
			string[] array = MotionController.GetController().ReadLimits();
			string[] array2 = array;
			foreach (string instance in array2)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(instance);
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(array.Length);
			return;
		}
		if (value.Equals("COMSTATUS", StringComparison.CurrentCultureIgnoreCase))
		{
			MotionController controller2 = MotionController.GetController();
			bool flag = controller2.CommunicationOk();
			ExecutionEngine.Instance.ActiveContext.PushTop(flag ? "OK" : "ERROR");
			return;
		}
		if (value.Equals("INITSTATUS", StringComparison.CurrentCultureIgnoreCase))
		{
			MotionController controller3 = MotionController.GetController();
			bool flag2 = controller3.IsInitialized();
			ExecutionEngine.Instance.ActiveContext.PushTop(flag2 ? "OK" : "ERROR");
			return;
		}
		if (value.Equals("SHUTDOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			MotionController controller4 = MotionController.GetController();
			ExecutionEngine.Instance.ActiveContext.PushTop(controller4.ClosePort());
			return;
		}
		if (value.Equals("INITIALIZE", StringComparison.CurrentCultureIgnoreCase))
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("UNSUPPORTED OPERAND");
			return;
		}
		if (value.Equals("POSITIONS", StringComparison.CurrentCultureIgnoreCase))
		{
			string motionControllerPosition = ControllerService.Instance.GetMotionControllerPosition(Axis.X);
			string motionControllerPosition2 = ControllerService.Instance.GetMotionControllerPosition(Axis.Y);
			ExecutionEngine.Instance.ActiveContext.PushTop($"X = {motionControllerPosition} Y = {motionControllerPosition2}");
			return;
		}
		if (value.Equals("XPOS", StringComparison.CurrentCultureIgnoreCase))
		{
			string motionControllerPosition3 = ControllerService.Instance.GetMotionControllerPosition(Axis.X);
			ExecutionEngine.Instance.ActiveContext.PushTop(motionControllerPosition3);
			return;
		}
		if (value.Equals("YPOS", StringComparison.CurrentCultureIgnoreCase))
		{
			string motionControllerPosition4 = ControllerService.Instance.GetMotionControllerPosition(Axis.Y);
			ExecutionEngine.Instance.ActiveContext.PushTop(motionControllerPosition4);
			return;
		}
		if (value.Equals("HOMEALL", StringComparison.CurrentCultureIgnoreCase))
		{
			using (MotionController motionController = MotionController.GetController())
			{
				if (GetBoolKVPValue(new ErrorList(), "CHECKCOM", defValue: false) && !motionController.CommunicationOk())
				{
					bool flag3 = motionController.ResetDeviceDriver();
					LogHelper.Instance.Log("The motion controller is not responsive; driver reset returned {0}.", flag3 ? "SUCCESS" : "FAILURE");
				}
				ExecutionEngine.Instance.ActiveContext.PushTop(motionController.InitAxes(ExecutionEngine.Instance.ActiveContext.AppLog).Succeded);
				return;
			}
		}
		result.Errors.Add(Error.NewError("E001", "Unrecognized operand.", $"The operand {value} is not known."));
	}

	private void HandleOperandError(ExecutionResult result, string operand)
	{
		string message = $"Unrecognized or unspecified {operand} operand.";
		LogHelper.Instance.Log(message, LogEntryType.Error);
		AddError("E001", message, result.Errors);
		ExecutionEngine.Instance.ActiveContext.PushTop("FAILURE");
	}
}
