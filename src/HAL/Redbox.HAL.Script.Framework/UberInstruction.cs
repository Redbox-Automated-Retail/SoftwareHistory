namespace Redbox.HAL.Script.Framework;

internal class UberInstruction : Instruction
{
	public override string Mnemonic => "S_UBER";

	internal override bool IsSynthetic => true;
}
