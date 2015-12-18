using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class GripperInstruction : Instruction
{
	private static class Commands
	{
		public const string Open = "OPEN";

		public const string Close = "CLOSE";

		public const string Status = "STATUS";

		public const string Extend = "EXTEND";

		public const string ExtendForTime = "EXTEND-FOR-TIME";

		public const string Retract = "RETRACT";

		public const string Rent = "RENT";

		public const string Push = "PUSH";

		public const string Peek = "PEEK";

		public const string Clear = "CLEAR";

		public const string PushOut = "PUSH-OUT";

		public const string DiskPushedBack = "DISK-PUSHED-BACK";

		public const string PushIn = "PUSH-IN";

		public const string AcceptDiscAtDoor = "ACCEPT-DISC-AT-DOOR";

		public const string RejectDiscAtDoor = "REJECT-DISC-AT-DOOR";

		public const string VendDiscAtDoor = "VEND-DISC-AT-DOOR";

		public const string CheckPicker = "CHECK-PICKER";

		public const string WrongPeek = "PEAK";

		public const string ReadDisk = "READ-DISC";
	}

	public override string Mnemonic => "GRIPPER";

	protected override bool IsOperandRecognized(string operand)
	{
		return ValidateOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count <= 2 && operandTokens[0].Type != TokenType.Symbol && !ValidateOperand(operandTokens[0].Value))
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a symbol: OPEN, CLOSE, RENT, STATUS, CLEAR, EXTEND, EXTEND-FOR-TIME, RETRACT, PUSH, PEEK and an optional TIMEOUT=m and WAIT=TRUE|FALSE key value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		_ = context.ProgramName;
		int? timeOutValue = GetTimeOutValue(result.Errors, context);
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		ErrorList errors = new ErrorList();
		string text = token.Value.ToUpper();
		if (context.GetSymbolValue(token.Value, errors) is string text2)
		{
			text = text2;
		}
		switch (text)
		{
		case "PEAK":
			context.PushTop("Dave, you spelled PEAK wrong - even I can spell it right.");
			break;
		case "VEND-DISC-AT-DOOR":
		{
			VendItemResult vendItemResult = ControllerService.Instance.VendItem();
			context.PushTop(vendItemResult.PickerStatus.ToString().ToUpper());
			break;
		}
		case "REJECT-DISC-AT-DOOR":
			context.PushTop(ControllerService.Instance.RejectDisk(ControllerConfiguration.Instance.RejectAtDoorAttempts).ToString().ToUpper());
			break;
		case "ACCEPT-DISC-AT-DOOR":
			context.PushTop(ControllerService.Instance.TakeDiskAtDoor().ToString().ToUpper());
			break;
		case "OPEN":
		{
			CoreResponse response4 = Gripper.TestFinger(GripperFingerState.Open);
			HandleTimeoutOrCommmError(response4, context);
			break;
		}
		case "CLOSE":
		{
			CoreResponse response3 = Gripper.TestFinger(GripperFingerState.Closed);
			HandleTimeoutOrCommmError(response3, context);
			break;
		}
		case "RENT":
		{
			CoreResponse response2 = Gripper.TestFinger(GripperFingerState.Rent);
			HandleTimeoutOrCommmError(response2, context);
			break;
		}
		case "STATUS":
			context.PushTop(PickerBoard.IsFull() ? "FULL" : "EMPTY");
			break;
		case "EXTEND":
		{
			CoreResponse response = Gripper.TestExtend();
			HandleTimeoutOrCommmError(response, context);
			break;
		}
		case "EXTEND-FOR-TIME":
			Gripper.TimedExtend(timeOutValue);
			break;
		case "RETRACT":
		{
			CoreResponse response5 = Gripper.TestRetract();
			HandleTimeoutOrCommmError(response5, context);
			break;
		}
		case "PEEK":
		{
			if (!ControllerService.Instance.Location.IsValid)
			{
				context.PushTop("LOCATIONOUTOFRANGE");
				break;
			}
			LocationTestResult locationTestResult = Gripper.Peek();
			if (!locationTestResult.TestOk)
			{
				context.PushTop(locationTestResult.Error.ToString().ToUpper());
			}
			else
			{
				context.PushTop(locationTestResult.PeekResult.ToString().ToUpper());
			}
			break;
		}
		case "CLEAR":
			context.PushTop(Gripper.IsClear());
			break;
		case "READ-DISC":
		{
			CenterDiskMethod centerDiskMethod = CenterDiskMethod.None;
			KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("SHUFFLE");
			if (keyValuePair2 != null)
			{
				centerDiskMethod = Enum<CenterDiskMethod>.ParseIgnoringCase(keyValuePair2.Value, CenterDiskMethod.None);
			}
			using Scanner scanner = new Scanner();
			ScanResult scanResult = scanner.Read();
			if (!scanResult.SnapOk || scanResult.ReadCode || centerDiskMethod == CenterDiskMethod.None)
			{
				PushReadResults(scanner, scanResult, context);
				break;
			}
			ControllerService.Instance.CenterItem(centerDiskMethod);
			PushReadResults(scanner, scanner.Read(), context);
			break;
		}
		case "PUSH":
		case "PUSH-IN":
			Gripper.ClearFront();
			break;
		case "PUSH-OUT":
		{
			PushOutStatus pushOutStatus = PickerBoard.PushOut();
			context.PushTop(pushOutStatus.ToString().ToUpper());
			break;
		}
		case "CHECK-PICKER":
		{
			string vendId = null;
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("VEND-ID");
			if (keyValuePair != null)
			{
				string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
				if (result.Errors.Count > 0)
				{
					break;
				}
				vendId = keyValuePairValue;
			}
			context.PushTop(HelperFunctions.CheckPicker(result, context, vendId).ToString().ToUpper());
			break;
		}
		default:
		{
			string text3 = $"Unrecognized operand {text}";
			context.ServiceLog(text3, LogEntryType.Error);
			result.Errors.Add(Error.NewError("E001", "Unknown operand", text3));
			break;
		}
		}
	}

	private void PushReadResults(Scanner scanner, ScanResult status, ExecutionContext context)
	{
		if (status.SnapOk)
		{
			context.PushTop(status.ScannedMatrix);
		}
		context.PushTop(scanner.Diagnostic(status));
	}

	private bool ValidateOperand(string operand)
	{
		if (string.Compare(operand, "OPEN", ignoreCase: true) != 0 && string.Compare(operand, "CLOSE", ignoreCase: true) != 0 && string.Compare(operand, "STATUS", ignoreCase: true) != 0 && string.Compare(operand, "EXTEND", ignoreCase: true) != 0 && string.Compare(operand, "EXTEND-FOR-TIME", ignoreCase: true) != 0 && string.Compare(operand, "RETRACT", ignoreCase: true) != 0 && string.Compare(operand, "RENT", ignoreCase: true) != 0 && string.Compare(operand, "PUSH", ignoreCase: true) != 0 && string.Compare(operand, "PEEK", ignoreCase: true) != 0 && string.Compare(operand, "CLEAR", ignoreCase: true) != 0 && string.Compare(operand, "PUSH-OUT", ignoreCase: true) != 0 && string.Compare(operand, "PUSH-IN", ignoreCase: true) != 0 && string.Compare(operand, "ACCEPT-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "REJECT-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "VEND-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "CHECK-PICKER", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "READ-DISC", ignoreCase: true) == 0;
		}
		return true;
	}
}
