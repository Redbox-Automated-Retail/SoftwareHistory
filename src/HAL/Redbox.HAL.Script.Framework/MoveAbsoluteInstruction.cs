using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MoveAbsoluteInstruction : Instruction
{
	private static class KeyNames
	{
		public const string Axis = "AXIS";

		public const string Units = "UNITS";

		public const string SecondaryUnits = "SECONDARY-UNITS";

		public const string SensorCheck = "SENSOR-CHECK";
	}

	public override string Mnemonic => "MOVEABS";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0 || operandTokens.Count > 3)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected key value pair AXIS=a, UNITS=n, SECONDARY-UNITS=n. The values of AXIS should be X or Y. The value for UNITS and SECONDARY-UNITS may be numeric literals or symbols.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
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
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair3, result.Errors, context);
		int? keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors, context);
		if (result.Errors.Count <= 0 && keyValuePairValue.HasValue)
		{
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
			MotionResult motionResult = ControllerService.Instance.MoveAbsolute(target, checkSensors);
			context.PushTop(motionResult.ToString().ToUpper());
		}
	}
}
