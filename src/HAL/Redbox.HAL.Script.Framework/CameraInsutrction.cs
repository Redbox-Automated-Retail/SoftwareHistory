using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

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
		Disable,
		Enable,
		ResetReturnCounter,
		ReturnErrorCount,
		SnapDecode,
		ClearStats,
		ScanForPort,
		DumpCortexStats
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
		switch (Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None))
		{
		case ExpectedOperand.ShowProperties:
			AddError("E999", $"The operand '{base.Operands[0].Value}' is deprecated.", result.Errors);
			break;
		case ExpectedOperand.Disable:
		{
			using DeviceHelper helper2 = new DeviceHelper(debug: true);
			CameraInfo cameraInfo2 = new CameraInfo(helper2);
			if (!cameraInfo2.FindActiveCamera())
			{
				context.PushTop("NO CAMERA");
			}
			else
			{
				context.PushTop(cameraInfo2.SetCameraState(DeviceState.Disable));
			}
			break;
		}
		case ExpectedOperand.Enable:
		{
			using DeviceHelper helper = new DeviceHelper(debug: true);
			CameraInfo cameraInfo = new CameraInfo(helper);
			if (!cameraInfo.FindActiveCamera())
			{
				context.PushTop("NO CAMERA");
			}
			else
			{
				context.PushTop(cameraInfo.SetCameraState(DeviceState.Enable));
			}
			break;
		}
		case ExpectedOperand.IRCamera:
			context.PushTop(BarcodeConfiguration.Instance.EnableInfared);
			break;
		case ExpectedOperand.Status:
		{
			IScannerDevice configuredDevice4 = ScannerDeviceFactory.GetConfiguredDevice();
			context.PushTop(configuredDevice4.IsConnected ? "RUNNING" : "STOPPED");
			break;
		}
		case ExpectedOperand.Start:
		{
			if (flag)
			{
				ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
			}
			IScannerDevice configuredDevice2 = ScannerDeviceFactory.GetConfiguredDevice();
			context.PushTop(configuredDevice2.Start());
			break;
		}
		case ExpectedOperand.Stop:
		{
			bool boolValue2 = false;
			FindBooleanOperand("FORCE", result, out boolValue2);
			IScannerDevice configuredDevice5 = ScannerDeviceFactory.GetConfiguredDevice();
			if (boolValue2)
			{
				configuredDevice5.TurnOff();
				context.PushTop(true);
			}
			else
			{
				context.PushTop(configuredDevice5.Stop());
			}
			break;
		}
		case ExpectedOperand.Snap:
		{
			IScannerDevice configuredDevice3 = ScannerDeviceFactory.GetConfiguredDevice();
			SnapResult snapResult = configuredDevice3.Snap();
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
		case ExpectedOperand.ClearStats:
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("SERVICE");
			string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
			if (result.Errors.Count <= 0)
			{
				ScannerServices scannerServices = Enum<ScannerServices>.ParseIgnoringCase(keyValuePairValue, ScannerServices.Emulated);
				if (ScannerServices.Emulated != scannerServices)
				{
					CameraStatistics.RemoveCameraStats(scannerServices);
				}
			}
			break;
		}
		case ExpectedOperand.DumpCortexStats:
		{
			string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service");
			CameraStatistics.DumpStats(Path.Combine(path, "CortexDecodeStats.txt"), ScannerServices.Cortex);
			break;
		}
		case ExpectedOperand.ScanForPort:
		{
			IScannerDevice configuredDevice = ScannerDeviceFactory.GetConfiguredDevice();
			context.PushTop(configuredDevice.FindInstalledPort());
			break;
		}
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
