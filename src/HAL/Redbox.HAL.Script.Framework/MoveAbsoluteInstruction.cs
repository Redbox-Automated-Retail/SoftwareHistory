using System;
using System.Text;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class MoveAbsoluteInstruction : Instruction
{
	protected static class KeyNames
	{
		public const string Axis = "AXIS";

		public const string Units = "UNITS";

		public const string SecondaryUnits = "SECONDARY-UNITS";

		public const string SensorCheck = "SENSOR-CHECK";
	}

	public override string Mnemonic => "MOVEABS";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count > 0 && tokensAfterMnemonic.Count <= 3)
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
		else
		{
			AddError("C002", "Expected key value pair AXIS=a, UNITS=n, SECONDARY-UNITS=n. The values of AXIS should be X or Y. The value for UNITS and SECONDARY-UNITS may be numeric literals or symbols.", result.Errors);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		bool checkSensors = true;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("SENSOR-CHECK");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			checkSensors = result2;
		}
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("AXIS");
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("UNITS");
		KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("SECONDARY-UNITS");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair3, result.Errors);
		int? keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors);
		if (result.Errors.Count > 0 || !keyValuePairValue.HasValue)
		{
			return;
		}
		Axis axis = Axis.X;
		if (string.Compare(keyValuePair2.Value, Axis.X.ToString(), ignoreCase: true) == 0)
		{
			axis = Axis.X;
		}
		else if (string.Compare(keyValuePair2.Value, Axis.Y.ToString(), ignoreCase: true) == 0)
		{
			axis = Axis.Y;
		}
		else if (string.Compare(keyValuePair2.Value, Axis.XY.ToString(), ignoreCase: true) == 0)
		{
			axis = Axis.XY;
		}
		MoveTarget target = new MoveTarget
		{
			Axis = axis
		};
		if (axis == Axis.X)
		{
			target.XCoordinate = keyValuePairValue;
		}
		else if (Axis.XY == axis)
		{
			target.XCoordinate = keyValuePairValue;
			target.YCoordinate = keyValuePairValue2;
		}
		else if (Axis.Y == axis)
		{
			target.YCoordinate = keyValuePairValue;
		}
		if (ExecutionEngine.Instance.ActiveContext.IsImmediate && !ControllerService.Instance.MoveToAbsoluteLocationIsValid(axis, keyValuePairValue, keyValuePairValue2))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Move to");
			if (keyValuePairValue.HasValue)
			{
				stringBuilder.Append($" X units {keyValuePairValue.Value}");
			}
			if (keyValuePairValue2.HasValue)
			{
				stringBuilder.Append($" Y units {keyValuePairValue2.Value}");
			}
			stringBuilder.Append(" failed because it's a secure location.");
			LogHelper.Instance.Log(stringBuilder.ToString(), LogEntryType.Error);
			ExecutionEngine.Instance.ActiveContext.PushTop(MotionResult.InvalidMoveToSecureLocation.ToString().ToUpper());
		}
		else
		{
			MotionResult motionResult = ControllerService.Instance.MoveAbsolute(target, checkSensors);
			ExecutionEngine.Instance.ActiveContext.PushTop(motionResult.ToString().ToUpper());
		}
	}
}
