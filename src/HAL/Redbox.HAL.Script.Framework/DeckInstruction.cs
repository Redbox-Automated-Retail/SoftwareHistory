using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class DeckInstruction : Instruction
{
	private enum DeckOperands
	{
		None,
		MaxSlots
	}

	public override string Mnemonic => "DECK";

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		base.OnParse(tokens, result);
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		if (tokensAfterMnemonic.Count < 1)
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", "The INVENTORY instruction expects a deck, slot and SET operands."));
		}
		else if (!ValidateOperand(tokensAfterMnemonic[0].Value))
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {tokensAfterMnemonic[0].Value} for DECK isn't recognized."));
		}
		else
		{
			base.Operands.AddRange(tokensAfterMnemonic);
		}
	}

	protected override void OnExecute(ExecutionResult result)
	{
		DeckOperands deckOperands = Enum<DeckOperands>.ParseIgnoringCase(base.Operands[0].Value, DeckOperands.None);
		int? nullableNumericOperand = GetNullableNumericOperand(result, "DECK");
		if (deckOperands == DeckOperands.MaxSlots)
		{
			Deck deck = DecksManager.Instance.GetDeck(nullableNumericOperand.Value);
			ExecutionEngine.Instance.ActiveContext.PushTop(deck.NumberOfSlots);
		}
	}

	private bool ValidateOperand(string operand)
	{
		try
		{
			Enum.Parse(typeof(DeckOperands), operand, ignoreCase: true);
			return true;
		}
		catch
		{
			return false;
		}
	}
}
