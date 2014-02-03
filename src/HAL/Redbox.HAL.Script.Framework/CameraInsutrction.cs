using System;
using System.IO;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class CameraInsutrction : Instruction
{
	protected static class Commands
	{
		public const string Center = "CENTER";

		public const string Start = "START";

		public const string Stop = "STOP";

		public const string Snap = "SNAP";

		public const string Status = "STATUS";

		public const string ShowProperties = "SHOWPROPERTIES";

		public const string IsWorking = "ISWORKING";

		public const string IRCamera = "IRCAMERA";

		public const string ConfigureIR = "CONFIGUREIR";

		public const string Reset = "RESET";

		public static bool IsValidCommand(string cmd)
		{
			switch (cmd.ToUpper())
			{
			case "CENTER":
			case "SNAP":
			case "STOP":
			case "START":
			case "STATUS":
			case "SHOWPROPERTIES":
			case "ISWORKING":
			case "IRCAMERA":
			case "CONFIGUREIR":
			case "RESET":
				return true;
			default:
				return false;
			}
		}
	}

	protected static class ResultCodes
	{
		public const string TimedOut = "CAPTURE TIMED OUT";

		public const string CaptureError = "CAPTURE ERROR";

		public const string Success = "SUCCESS";
	}

	public override string Mnemonic => "CAMERA";

	protected override bool IsOperandRecognized(string operand)
	{
		return Commands.IsValidCommand(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol || !Commands.IsValidCommand(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", string.Format("Unrecognized operand {0}; Expected a symbol: START, STOP, SNAP, STATUS, and an optional CENTER={TRUE|FALSE} operand.", tokensAfterMnemonic[0].Value), result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		bool flag = false;
		if (FindBooleanOperand("CENTER".ToString().ToUpper(), result, out var boolValue))
		{
			flag = boolValue;
		}
		foreach (Token operand in base.Operands)
		{
			if (operand.Value.Equals("SHOWPROPERTIES"))
			{
				AddError("E999", $"The operand '{operand.Value}' is deprecated.", result.Errors);
				break;
			}
			if (operand.Value.Equals("RESET"))
			{
				continue;
			}
			if (operand.Value.Equals("IRCAMERA"))
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(DataMatrixHelper.Instance.IRCameraConfigured);
				break;
			}
			if (operand.Value.Equals("CONFIGUREIR"))
			{
				if (!GetKeyValuePairValue<string>("STATE", out var val, result.Errors))
				{
					val = "on";
				}
				DataMatrixHelper.Instance.IRCameraConfigured = (val.Equals("on", StringComparison.CurrentCultureIgnoreCase) ? true : false);
				ExecutionEngine.Instance.ActiveContext.PushTop(DataMatrixHelper.Instance.IRCameraConfigured);
				break;
			}
			if (operand.Value.Equals("STATUS"))
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(CameraService.Instance.IsRunning ? "RUNNING" : "STOPPED");
				break;
			}
			if (operand.Value.Equals("START"))
			{
				if (flag)
				{
					ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
				}
				bool flag2 = CameraService.Instance.Start();
				ExecutionEngine.Instance.ActiveContext.PushTop(flag2);
				break;
			}
			if (operand.Value.Equals("STOP"))
			{
				bool boolValue2 = false;
				FindBooleanOperand("FORCE", result, out boolValue2);
				if (boolValue2)
				{
					CameraService.Instance.TurnOffCamera();
				}
				else
				{
					ExecutionEngine.Instance.ActiveContext.PushTop(CameraService.Instance.Stop());
				}
				break;
			}
			if (operand.Value.Equals("SNAP"))
			{
				if (CameraConfiguration.Instance.Mode == CameraMode.Emulated)
				{
					if (CameraEmulationState.Instance.ExpectedStartCaptureResult == null)
					{
						ExecutionEngine.Instance.ActiveContext.PushTop("CAPTURE ERROR");
						break;
					}
					ExecutionEngine.Instance.ActiveContext.PushTop(CameraEmulationState.Instance.ExpectedStartCaptureResult);
					ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
				}
				else
				{
					DataMatrixHelper.Instance.CameraSnap(out var path);
					PushResultToStack(path);
				}
				break;
			}
			if (!operand.Value.Equals("ISWORKING", StringComparison.CurrentCultureIgnoreCase))
			{
				continue;
			}
			using (new CameraHelper(10000))
			{
				try
				{
					PickerBoard.Instance.SetSensors(on: true);
					if (DataMatrixHelper.Instance.CameraSnap(out var path2) == CameraResult.CaptureError)
					{
						ExecutionEngine.Instance.ActiveContext.PushTop("CAPTURE ERROR");
						break;
					}
					if (ImageHelper.Instance.ImageIsBlack(path2))
					{
						ExecutionEngine.Instance.ActiveContext.PushTop("BLACK-IMAGE");
						break;
					}
					ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
				}
				finally
				{
					PickerBoard.Instance.SetSensors(on: false);
				}
			}
		}
	}

	private void PushResultToStack(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			ExecutionEngine.Instance.ActiveContext.PushTop("CAPTURE ERROR");
			string msg = "CAMERA CAPTURE returned a capture error.";
			ExecutionEngine.Instance.ActiveContext.AppLog.Write(msg, LogEntryType.Error);
		}
		else
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(path);
			ExecutionEngine.Instance.ActiveContext.PushTop("SUCCESS");
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

	private bool ShouldWait(ExecutionResult result, out bool waitValue)
	{
		waitValue = false;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("WAIT");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			waitValue = result2;
			return true;
		}
		return true;
	}
}
