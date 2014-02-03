namespace Redbox.HAL.Script.Framework;

internal class SyntheticBranch : Instruction
{
	public override string Mnemonic => "S_BR";

	internal override bool IsSynthetic => true;

	internal override Branch BranchType => Branch.Unconditional;
}
