using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class RuntimeInstruction : Instruction
{
	public override string Mnemonic => "RUNTIME";

	public override bool IsCooperativeMultitask => false;

	protected override bool IsOperandRecognized(string operand)
	{
		return "YIELD-ONLY".Equals(operand);
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count == 0)
		{
			AddError("C002", "The RUNTIME instruction expects at least one operand.", result.Errors);
		}
		else if (tokensAfterMnemonic[0].Type != TokenType.Symbol)
		{
			AddError("C002", "Expected YIELD-ONLY, IS-STARTUP-INIT, or YIELD-FOR-PORTS as an operand.", result.Errors);
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		if (base.Operands.Count != 1)
		{
			AddError("E999", "The RUNTIME instruction expects one token", result.Errors);
			return;
		}
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		string value = base.Operands[0].Value;
		if (value.Equals("YIELD-ONLY", StringComparison.CurrentCultureIgnoreCase))
		{
			ExecutionEngine.Instance.PerformContextSwitch(runtimeOnly: true);
		}
		else if (value.Equals("IS-STARTUP-INIT"))
		{
			string iD = activeContext.ID;
			activeContext.PushTop(iD.Equals(ExecutionEngine.Instance.InitJob.ID, StringComparison.CurrentCultureIgnoreCase));
		}
		else if (value.Equals("YIELD-FOR-PORTS"))
		{
			int num = ControllerConfiguration.Instance.COMPortInitSleepTime;
			if (num < 60000)
			{
				num = 60000;
			}
			PerfFunctions.Wait(num);
		}
	}
}
