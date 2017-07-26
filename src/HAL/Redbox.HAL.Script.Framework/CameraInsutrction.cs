using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class CameraInsutrction : Instruction
{
	protected static class ResultCodes
	{
		public const string TimedOut = "CAPTURE TIMED OUT";

		public const string CaptureError = "CAPTURE ERROR";

		public const string Success = "SUCCESS";
	}

	private enum ExpectedOperand
	{
		None,
		Center,
		Start,
		Stop,
		Snap,
		Status,
		ShowProperties,
		IsWorking,
		IRCamera,
		Reset,
		ResetReturnCounter,
		ReturnErrorCount,
		SnapDecode,
		ScanForPort
	}

	public override string Mnemonic => "CAMERA";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.Symbol || !IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError(string.Format("Unrecognized operand {0}; Expected a symbol: START, STOP, SNAP, STATUS, and an optional CENTER={TRUE|FALSE} operand.", operandTokens[0].Value));
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		bool flag = false;
		if (FindBooleanOperand(ExpectedOperand.Center.ToString().ToUpper(), result, out var boolValue))
		{
			flag = boolValue;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IScannerDevice configuredDevice = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
		switch (Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None))
		{
		case ExpectedOperand.IRCamera:
			context.PushTop(false);
			break;
		case ExpectedOperand.ShowProperties:
			AddError("E999", $"The operand '{base.Operands[0].Value}' is deprecated.", result.Errors);
			break;
		case ExpectedOperand.Status:
			context.PushTop(configuredDevice.IsConnected ? "RUNNING" : "STOPPED");
			break;
		case ExpectedOperand.Start:
			if (flag)
			{
				service.Center(CenterDiskMethod.VendDoorAndBack);
			}
			context.PushTop(configuredDevice.Start());
			break;
		case ExpectedOperand.Stop:
			context.PushTop(configuredDevice.Stop());
			break;
		case ExpectedOperand.Snap:
		{
			ISnapResult snapResult = configuredDevice.Snap();
			if (!snapResult.SnapOk)
			{
				context.PushTop("CAPTURE ERROR");
				string msg = "CAMERA CAPTURE returned a capture error.";
				context.AppLog.Write(msg, LogEntryType.Error);
			}
			else
			{
				context.PushTop(snapResult.Path);
				context.PushTop("SUCCESS");
			}
			break;
		}
		case ExpectedOperand.IsWorking:
			AddError("E666", "IS-WORKING is a deprecated operand", result.Errors);
			break;
		case ExpectedOperand.ResetReturnCounter:
			ReturnJob.ResetCameraErrorCounter();
			break;
		case ExpectedOperand.ReturnErrorCount:
			context.PushTop(ReturnJob.CameraErrorCount);
			break;
		case ExpectedOperand.SnapDecode:
		{
			using Scanner scanner = new Scanner();
			ScanResult scanResult = scanner.Read();
			context.PushTop(scanResult.ScannedMatrix);
			break;
		}
		case ExpectedOperand.ScanForPort:
			context.PushTop(configuredDevice.FindInstalledPort());
			break;
		case ExpectedOperand.Reset:
			break;
		}
	}

	private bool FindBooleanOperand(string operand, ExecutionResult result, out bool boolValue)
	{
		boolValue = false;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair(operand);
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			boolValue = result2;
			return true;
		}
		return false;
	}

	private bool IsValidOperand(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
