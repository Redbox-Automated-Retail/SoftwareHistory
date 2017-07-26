using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class FraudSensorInstruction : Instruction
{
	private enum SensorOperands
	{
		None,
		Reset,
		StartScan,
		StopScan,
		Initialize,
		Shutdown,
		LastPostResult
	}

	public override string Mnemonic => "FRAUDSENSOR";

	protected override ExpectedOperands Expected => ExpectedOperands.AtLeastOne;

	protected override bool IsOperandRecognized(string operand)
	{
		return Enum<SensorOperands>.ParseIgnoringCase(operand, SensorOperands.None) != SensorOperands.None;
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0 || !IsOperandRecognized(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected at least one operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		if (!service.IsConfigured)
		{
			context.PushTop(ErrorCodes.DeviceNotConfigured.ToString().ToUpper());
			return;
		}
		IFormattedLog nilLog = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().NilLog;
		switch (Enum<SensorOperands>.ParseIgnoringCase(base.Operands[0].Value, SensorOperands.None))
		{
		case SensorOperands.Reset:
			context.PushTop(service.FraudSensor.Reset(nilLog).ToString().ToUpper());
			break;
		case SensorOperands.StartScan:
			context.PushTop(service.FraudSensor.StartScan().ToString().ToUpper());
			break;
		case SensorOperands.StopScan:
			context.PushTop(service.FraudSensor.StopScan().ToString().ToUpper());
			break;
		case SensorOperands.Initialize:
			context.PushTop(service.FraudSensor.Initialize(nilLog).ToString().ToUpper());
			break;
		case SensorOperands.Shutdown:
			context.PushTop(service.FraudSensor.Shutdown().ToString().ToUpper());
			break;
		case SensorOperands.LastPostResult:
			context.PushTop(service.FraudSensor.LastPostResult.ToString().ToUpper());
			break;
		}
	}
}
