using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class GetInstruction : Instruction
{
	private sealed class GetInstructionObserver : IGetObserver
	{
		private readonly bool TestSlot;

		public bool OnEmpty(GetResult gr, Deck deck, int slot)
		{
			if (!TestSlot)
			{
				return true;
			}
			LocationTestResult locationTestResult = Gripper.Peek();
			string message = $"GET disk at Deck = {deck.Number} Slot = {slot} shows SLOTEMPTY; PEEK returns {locationTestResult.ToString()}";
			LogHelper.Instance.Log(message, LogEntryType.Info);
			if (!locationTestResult.TestOk || locationTestResult.PeekResult == GripperPeekResult.Full)
			{
				gr.UpdateCode(GetResultCode.ItemStuck);
				deck.UpdateEmptyStuck(slot);
				return false;
			}
			return true;
		}

		public void OnStuck(GetResult gr, Deck deck, int slot)
		{
			if (TestSlot)
			{
				deck.UpdateEmptyStuck(slot);
			}
		}

		internal GetInstructionObserver(bool test)
		{
			TestSlot = test && ControllerConfiguration.Instance.TestSlotOnEmpty;
		}
	}

	public override string Mnemonic => "GET";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count > 0 && !operandTokens[0].IsKeyValuePair)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects an optional key-value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("PEEK");
		if (keyValuePair != null)
		{
			bool.TryParse(keyValuePair.Value, out var _);
		}
		bool flag = false;
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("CENTER");
		if (keyValuePair2 != null && bool.TryParse(keyValuePair2.Value, out var result3))
		{
			flag = result3;
		}
		bool test = false;
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("TESTONEMPTY");
		if (keyValuePair3 != null && bool.TryParse(keyValuePair3.Value, out var result4))
		{
			test = result4;
		}
		bool flag2 = false;
		KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("GET-RT");
		if (keyValuePair4 != null && bool.TryParse(keyValuePair4.Value, out var result5))
		{
			flag2 = result5;
		}
		if (context.IsImmediate && ControllerConfiguration.Instance.Mode == ControllerMode.Normal)
		{
			test = true;
		}
		GetInstructionObserver observer = new GetInstructionObserver(test);
		GetResult getResult = ControllerService.Instance.Get(observer, context.AppLog);
		if (getResult.Success && flag)
		{
			ControllerService.Instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
		}
		if (flag2)
		{
			context.PushTop(GetDateRepresentation(getResult.ReturnTime));
		}
		context.PushTop(getResult.Previous);
		context.PushTop(getResult.ToString().ToUpper());
	}
}
