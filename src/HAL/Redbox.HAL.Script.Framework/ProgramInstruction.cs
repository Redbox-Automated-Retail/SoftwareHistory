using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ProgramInstruction : Instruction
{
	public override string EndContextMnemonic => "ENDPROG";

	public override bool BeginsContext => true;

	public override string Mnemonic => "PROG";

	internal string Version { get; set; }

	internal static int InvalidLineNumber => -42;

	internal static int EndLineNumber => -2;

	internal bool IsCooperative { get; set; }

	public static ProgramInstruction CreateEmpty(string programName)
	{
		ProgramInstruction programInstruction = new ProgramInstruction();
		programInstruction.Operands.Add(new Token(TokenType.StringLiteral, programName, isKeyValuePair: false));
		return programInstruction;
	}
}
