using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class GripperInstruction : Instruction
{
	protected static class Commands
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

		public const string PutDiscAway = "PUT-DISC-AWAY";

		public const string WrongPeek = "PEAK";

		public const string ReadDisk = "READ-DISC";
	}

	public override string Mnemonic => "GRIPPER";

	protected override bool IsOperandRecognized(string operand)
	{
		return ValidateOperand(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic.Count <= 2)
		{
			if (tokensAfterMnemonic[0].Type != TokenType.Symbol && !ValidateOperand(tokensAfterMnemonic[0].Value))
			{
				AddError("C002", "Expected a symbol: OPEN, CLOSE, RENT, STATUS, CLEAR, EXTEND, EXTEND-FOR-TIME, RETRACT, PUSH, PEEK and an optional TIMEOUT=m and WAIT=TRUE|FALSE key value pair.", result.Errors);
			}
			else
			{
				base.Operands.AddRange(tokensAfterMnemonic);
			}
		}
		else
		{
			AddError("C002", "Expected a symbol: OPEN, CLOSE, RENT, STATUS, CLEAR, EXTEND, RETRACT, PUSH, PEEK and an optional TIMEOUT=m and WAIT=TRUE|FALSE key value pair.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		_ = ExecutionEngine.Instance.ActiveContext.ProgramName;
		int? timeOutValue = GetTimeOutValue(result.Errors);
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		ErrorList errors = new ErrorList();
		string text = token.Value.ToUpper();
		if (ExecutionEngine.Instance.ActiveContext.GetSymbolValue(token.Value, errors) is string text2)
		{
			text = text2;
		}
		switch (text)
		{
		case "PEAK":
			ExecutionEngine.Instance.ActiveContext.PushTop("Dave, you spelled PEAK wrong - even I can spell it right.");
			break;
		case "VEND-DISC-AT-DOOR":
		{
			VendItemResult vendItemResult = ControllerService.Instance.VendItem();
			ExecutionEngine.Instance.ActiveContext.PushTop(vendItemResult.PickerStatus.ToString().ToUpper());
			break;
		}
		case "REJECT-DISC-AT-DOOR":
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.RejectDisk(ControllerConfiguration.Instance.RejectAtDoorAttempts).ToString().ToUpper());
			break;
		case "ACCEPT-DISC-AT-DOOR":
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.TakeDiskAtDoor().ToString().ToUpper());
			break;
		case "OPEN":
			TestFingerfunction(GripperFingerState.Open);
			break;
		case "CLOSE":
			TestFingerfunction(GripperFingerState.Closed);
			break;
		case "RENT":
			TestFingerfunction(GripperFingerState.Rent);
			break;
		case "STATUS":
			ExecutionEngine.Instance.ActiveContext.PushTop(PickerBoard.Instance.IsFull() ? "FULL" : "EMPTY");
			break;
		case "EXTEND":
		{
			CoreResponse response2 = Gripper.Instance.TestExtend();
			HandleTimeoutOrCommmError(response2);
			break;
		}
		case "EXTEND-FOR-TIME":
			Gripper.Instance.TimedExtend(timeOutValue);
			break;
		case "RETRACT":
		{
			CoreResponse response = Gripper.Instance.TestRetract();
			HandleTimeoutOrCommmError(response);
			break;
		}
		case "PEEK":
		{
			if (!LocationOk())
			{
				ExecutionEngine.Instance.ActiveContext.PushTop("LOCATIONOUTOFRANGE");
				break;
			}
			LocationTestResult locationTestResult = Gripper.Instance.Peek();
			if (!locationTestResult.TestOk)
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(locationTestResult.Error.ToString().ToUpper());
			}
			else
			{
				ExecutionEngine.Instance.ActiveContext.PushTop(locationTestResult.PeekResult.ToString().ToUpper());
			}
			break;
		}
		case "CLEAR":
			ExecutionEngine.Instance.ActiveContext.PushTop(ControllerService.Instance.IsGripperClear());
			break;
		case "READ-DISC":
		{
			ReadDiskResult status = ReadDisc(result);
			PushReadResults(status);
			break;
		}
		case "PUSH":
		case "PUSH-IN":
			Gripper.Instance.ClearFront();
			break;
		case "PUSH-OUT":
		{
			PushOutStatus pushOutStatus = ControllerService.Instance.GripperPushOut();
			ExecutionEngine.Instance.ActiveContext.PushTop(pushOutStatus.ToString().ToUpper());
			break;
		}
		case "CHECK-PICKER":
		{
			string vendId = null;
			KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("VEND-ID");
			if (keyValuePair2 != null)
			{
				string keyValuePairValue2 = GetKeyValuePairValue<string>(keyValuePair2, result.Errors);
				if (result.Errors.Count > 0)
				{
					break;
				}
				vendId = keyValuePairValue2;
			}
			ExecutionEngine.Instance.ActiveContext.PushTop(HelperFunctions.CheckPicker(result, vendId).ToString().ToUpper());
			break;
		}
		case "PUT-DISC-AWAY":
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("BARCODE");
			string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
			if (result.Errors.Count <= 0)
			{
				IFileDiskHandler handler = DiskfileHandlerFactory.Instance.MakeHandler(ExecutionEngine.Instance.ActiveContext.ProgramName);
				ExecutionEngine.Instance.ActiveContext.PushTop(HelperFunctions.PutDiscAway(result, keyValuePairValue, handler).ToString().ToUpper());
			}
			break;
		}
		default:
		{
			string text3 = $"Unrecognized operand {text}";
			ServiceLogHelper.Instance.WriteErrorMessage(text3);
			result.Errors.Add(Error.NewError("E001", "Unknown operand", text3));
			break;
		}
		}
	}

	private void PushReadResults(ReadDiskResult status)
	{
		if (status.ReadStatus != ReadDiscStatus.Success)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(status.ToString().ToUpper());
			return;
		}
		ExecutionEngine.Instance.ActiveContext.PushTop(status.Matrix);
		ExecutionEngine.Instance.ActiveContext.PushTop(status.ReadStatus.ToString().ToUpper());
	}

	private ReadDiskResult ReadDisc(ExecutionResult result)
	{
		CenterDiskMethod centerDiskMethod = CenterDiskMethod.None;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("SHUFFLE");
		if (keyValuePair != null)
		{
			try
			{
				centerDiskMethod = (CenterDiskMethod)Enum.Parse(typeof(CenterDiskMethod), keyValuePair.Value, ignoreCase: true);
			}
			catch
			{
				centerDiskMethod = CenterDiskMethod.None;
			}
		}
		ReadDiskResult readDiskResult = DataMatrixHelper.Instance.ReadDiscInPicker(CenterDiskMethod.None);
		if (readDiskResult.ReadStatus != ReadDiscStatus.Success)
		{
			return readDiskResult;
		}
		if (!readDiskResult.IsDiskUnknown() || centerDiskMethod == CenterDiskMethod.None)
		{
			return readDiskResult;
		}
		return DataMatrixHelper.Instance.ReadDiscInPicker(centerDiskMethod);
	}

	private void TestFingerfunction(GripperFingerState state)
	{
		CoreResponse response = Gripper.Instance.TestFinger(state);
		HandleTimeoutOrCommmError(response);
	}

	private bool ValidateOperand(string operand)
	{
		if (string.Compare(operand, "OPEN", ignoreCase: true) != 0 && string.Compare(operand, "CLOSE", ignoreCase: true) != 0 && string.Compare(operand, "STATUS", ignoreCase: true) != 0 && string.Compare(operand, "EXTEND", ignoreCase: true) != 0 && string.Compare(operand, "EXTEND-FOR-TIME", ignoreCase: true) != 0 && string.Compare(operand, "RETRACT", ignoreCase: true) != 0 && string.Compare(operand, "RENT", ignoreCase: true) != 0 && string.Compare(operand, "PUSH", ignoreCase: true) != 0 && string.Compare(operand, "PEEK", ignoreCase: true) != 0 && string.Compare(operand, "CLEAR", ignoreCase: true) != 0 && string.Compare(operand, "PUSH-OUT", ignoreCase: true) != 0 && string.Compare(operand, "PUSH-IN", ignoreCase: true) != 0 && string.Compare(operand, "ACCEPT-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "REJECT-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "VEND-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "CHECK-PICKER", ignoreCase: true) != 0 && string.Compare(operand, "READ-DISC", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "PUT-DISC-AWAY", ignoreCase: true) == 0;
		}
		return true;
	}

	private bool LocationOk()
	{
		ControllerLocation location = ControllerService.Instance.Location;
		return location.IsValid;
	}
}
