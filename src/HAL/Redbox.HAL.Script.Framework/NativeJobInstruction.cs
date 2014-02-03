using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class NativeJobInstruction : Instruction
{
	public override string Mnemonic => "NATIVEJOB";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		base.Operands.AddRange(tokensAfterMnemonic);
	}

	protected override void OnExecute(ExecutionResult result)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		NativeJobAdapter nativeJobAdapter = NativeJobFactory.Instance.MakeJobFromOperand(activeContext, result, base.Operands[0].Value);
		if (nativeJobAdapter == null)
		{
			result.Errors.Add(Error.NewError("E777", "Unrecognized job.", $"The job \"{base.Operands[0].Value}\" is not recognized as a native job."));
		}
		else
		{
			nativeJobAdapter.Execute();
		}
	}
}
