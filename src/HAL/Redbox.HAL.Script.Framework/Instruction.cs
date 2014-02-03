using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class Instruction : ICloneable<Instruction>
{
	public enum Branch
	{
		None,
		Unconditional,
		Conditional
	}

	protected const string SuccessMessage = "SUCCESS";

	protected const string FailureMessage = "FAILURE";

	protected const string TimeoutMessage = "TIMEOUT";

	private const string ErrorMessageFormat = "The {0} instruction failed (line {1} of script {2}): {3}";

	private TokenList m_operands;

	private InstructionList m_instructions;

	private int m_trueBranch = ProgramInstruction.EndLineNumber;

	private int m_falseBranch = ProgramInstruction.InvalidLineNumber;

	public Instruction FirstInstruction => Instructions[0];

	public virtual string EndContextMnemonic => null;

	public virtual bool BeginsContext => false;

	public virtual bool IsCooperativeMultitask => false;

	public Instruction ParentContext { get; set; }

	public virtual string LabelName { get; set; }

	public virtual bool EndsContext => false;

	public virtual string Mnemonic => string.Empty;

	public virtual bool ExitsContext => false;

	public TokenList Operands
	{
		get
		{
			if (m_operands == null)
			{
				m_operands = new TokenList();
			}
			return m_operands;
		}
		private set
		{
			m_operands = value;
		}
	}

	public int LineNumber { get; set; }

	public int X { get; internal set; }

	public int Y { get; internal set; }

	internal virtual Branch BranchType => Branch.None;

	internal virtual bool IsSynthetic => false;

	internal InstructionList Instructions
	{
		get
		{
			if (m_instructions == null)
			{
				m_instructions = new InstructionList(this);
			}
			return m_instructions;
		}
		set
		{
			m_instructions = value;
		}
	}

	internal VariableAction VariableAction { get; set; }

	internal bool IsLabel { get; set; }

	internal virtual bool CreatesVariableDef => false;

	internal virtual int TrueBranchTarget
	{
		get
		{
			return m_trueBranch;
		}
		set
		{
			m_trueBranch = value;
		}
	}

	internal virtual int FalseBranchTarget
	{
		get
		{
			return m_falseBranch;
		}
		set
		{
			m_falseBranch = value;
		}
	}

	internal virtual string CounterSymbol { get; set; }

	internal virtual string TargetLabel => string.Empty;

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Instruction instruction))
		{
			return false;
		}
		if (instruction.Mnemonic.ToUpper().Equals(Mnemonic))
		{
			return instruction.LineNumber == LineNumber;
		}
		return false;
	}

	public static bool MnemonicImpliesConstSymbol(string mnemonic)
	{
		return mnemonic.ToUpper().Equals("CONST");
	}

	public static Instruction Parse(TokenList tokens, int lineNumber, ExecutionResult result)
	{
		if (tokens.Count == 0)
		{
			return null;
		}
		Type[] types = typeof(Instruction).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (type.IsSubclassOf(typeof(Instruction)) && Activator.CreateInstance(type) is Instruction instruction && string.Compare(instruction.Mnemonic, tokens.GetMnemonic().Value, ignoreCase: true) == 0)
			{
				instruction.LineNumber = lineNumber;
				Token label = tokens.GetLabel();
				if (label != null)
				{
					instruction.LabelName = label.Value;
				}
				instruction.OnParse(tokens, result);
				return instruction;
			}
		}
		return null;
	}

	public Instruction GetByLineNumber(int lineNumber)
	{
		return GetByLineNumber(this, lineNumber);
	}

	public void Execute(ExecutionResult result)
	{
		try
		{
			OnExecute(result);
		}
		catch (Exception ex)
		{
			string text = string.Format("Program '{0}', Line {1}: An unhandled exception was raised by the execution engine.", (ExecutionEngine.Instance.ActiveContext != null) ? ExecutionEngine.Instance.ActiveContext.Registers.ActiveFrame.ProgramName : "Idle", LineNumber);
			result.Errors.Add(Error.NewError("E001", text, ex.ToString()));
			LogHelper.Instance.Log(text, ex);
			CleanUpResources();
		}
	}

	public ProgramInstruction GetProgram()
	{
		Instruction instruction = ParentContext ?? this;
		while (instruction != null && !(instruction is ProgramInstruction))
		{
			instruction = instruction.ParentContext;
		}
		return instruction as ProgramInstruction;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (LabelName != null)
		{
			stringBuilder.Append(LabelName + ":");
		}
		stringBuilder.AppendFormat(" {0,-16}", Mnemonic);
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (Token operand in Operands)
		{
			if (stringBuilder2.Length > 0)
			{
				stringBuilder2.Append(" ");
			}
			if (operand.Type == TokenType.StringLiteral)
			{
				stringBuilder2.AppendFormat("\"{0}\"", operand.Value.ToUpper());
			}
			else
			{
				stringBuilder2.Append(operand.Value.ToUpper());
			}
		}
		if (stringBuilder2.Length > 0)
		{
			stringBuilder.Append(stringBuilder2.ToString());
		}
		return stringBuilder.ToString();
	}

	public Instruction Clone(params object[] parms)
	{
		Instruction instruction = (Instruction)Activator.CreateInstance(GetType());
		instruction.LabelName = LabelName;
		instruction.LineNumber = LineNumber;
		instruction.Operands = Operands.Clone();
		instruction.Instructions = Instructions.Clone(instruction);
		instruction.TrueBranchTarget = TrueBranchTarget;
		instruction.FalseBranchTarget = FalseBranchTarget;
		instruction.CounterSymbol = CounterSymbol;
		return instruction;
	}

	public bool ExecuteInstruction(ExecutionResult result)
	{
		try
		{
			ExecutionEngine.Instance.ActiveContext.Registers.ActiveFrame.PC = TrueBranchTarget;
			CoreExecute(result);
			return true;
		}
		catch (Exception ex)
		{
			result.Errors.Add(Error.NewError("E001", $"Line {LineNumber}: An unhandled exception was raised by the execution engine.", ex.ToString()));
			LogHelper.Instance.Log("An instruction threw an unhandled exception.", ex);
			CleanUpResources();
		}
		return false;
	}

	internal bool OperandRecognized(string operand)
	{
		if (operand.Equals("TRUE") || operand.Equals("FALSE"))
		{
			return true;
		}
		return IsOperandRecognized(operand);
	}

	protected virtual void OnParse(TokenList tokens, ExecutionResult result)
	{
	}

	protected virtual void OnExecute(ExecutionResult result)
	{
	}

	protected virtual bool IsOperandRecognized(string op)
	{
		return false;
	}

	protected virtual bool CoreExecute(ExecutionResult result)
	{
		OnExecute(result);
		return true;
	}

	protected void LogIncorrectBranch(string targetMnemonic, ExecutionResult result)
	{
	}

	protected void LogInstructionError(string form, string message)
	{
		LogHelper.Instance.Log($"The {form} instruction failed (line {LineNumber} of script {ExecutionEngine.Instance.ActiveContext.ProgramName}): {message}", LogEntryType.Error);
	}

	protected bool CheckAssignment(TokenType type, ErrorList errors)
	{
		if (type == TokenType.ConstSymbol)
		{
			AddError("C002", "Illegal attempt to " + Mnemonic + " value to CONST symbol.", errors);
			return false;
		}
		return true;
	}

	protected bool GetLocation(ExecutionResult result, out Location location)
	{
		int deck = -1;
		int slot = -1;
		location = null;
		if (!GetLocationInner(result, out deck, out slot))
		{
			return false;
		}
		location = new Location(deck, slot);
		return true;
	}

	protected bool GetLocation(ExecutionResult result, out int deck, out int slot)
	{
		return GetLocationInner(result, out deck, out slot);
	}

	protected string GetID(ExecutionResult result)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("ID");
		if (keyValuePair == null)
		{
			result.Errors.Add(Error.NewError("E099", "Missing operand.", "The ID operand is missing."));
			return null;
		}
		string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors);
		if (result.Errors.Count != 0)
		{
			return string.Empty;
		}
		return keyValuePairValue;
	}

	protected bool GetBoolKVPValue(ErrorList list, string op, bool defValue)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair(op);
		if (keyValuePair == null)
		{
			return defValue;
		}
		bool keyValuePairValue = GetKeyValuePairValue<bool>(keyValuePair, list);
		if (list.Count == 0)
		{
			return keyValuePairValue;
		}
		return defValue;
	}

	protected void HandleTimeoutOrCommmError(CoreResponse response)
	{
		if (response.CommunicationError)
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(response.CommErrorDiagnostic);
		}
		else
		{
			ExecutionEngine.Instance.ActiveContext.PushTop(response.TimedOut ? "TIMEOUT" : "SUCCESS");
		}
	}

	protected int? GetDeckOperand(ErrorList errors)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("DECK");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, errors);
		if (errors.Count > 0)
		{
			return null;
		}
		return keyValuePairValue;
	}

	internal object[] ConvertSymbolsToParameterArray(ErrorList errors)
	{
		List<object> list = new List<object>();
		for (int i = 1; i < Operands.Count; i++)
		{
			Token token = Operands[i];
			if (!token.IsKeyValuePair)
			{
				object obj = token.ConvertValue();
				if ((token.Type == TokenType.Symbol || token.Type == TokenType.ConstSymbol) && token.Value.Equals(obj))
				{
					obj = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(token.Value, errors);
				}
				list.Add(obj);
			}
		}
		return list.ToArray();
	}

	internal bool GetKeyValuePairValue<T>(string key, out T val, ErrorList errors)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair(key);
		if (keyValuePair == null)
		{
			val = default(T);
			return false;
		}
		val = GetKeyValuePairValue<T>(keyValuePair, errors);
		return true;
	}

	internal T GetKeyValuePairValue<T>(KeyValuePair keyValuePair, ErrorList errors)
	{
		if (keyValuePair == null)
		{
			return default(T);
		}
		object value = null;
		if (keyValuePair.Type == TokenType.NumericLiteral)
		{
			value = int.Parse(keyValuePair.Value);
		}
		else if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
		{
			object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue(keyValuePair.Value, errors);
			if (symbolValue != null)
			{
				value = symbolValue;
			}
		}
		else
		{
			value = keyValuePair.Value;
		}
		T result = default(T);
		try
		{
			result = (T)ConversionHelper.ChangeType(value, typeof(T));
			return result;
		}
		catch (Exception ex)
		{
			AddError("E002", "Type conversion error: " + ex, errors);
		}
		return result;
	}

	internal static string ConvertTimeoutBoolToMessage(bool timedOut)
	{
		if (!timedOut)
		{
			return "SUCCESS";
		}
		return "TIMEOUT";
	}

	internal int? GetNullableNumericOperand(ExecutionResult result, string name)
	{
		KeyValuePair keyValuePair = Operands.GetKeyValuePair(name);
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors);
		if (result.Errors.Count > 0)
		{
			return null;
		}
		return keyValuePairValue;
	}

	internal string GetDateRepresentation(DateTime? dateTime)
	{
		if (dateTime.HasValue)
		{
			return dateTime.Value.ToString();
		}
		return "NONE";
	}

	internal int? GetTimeOutValue(ErrorList errors)
	{
		int? result = null;
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("TIMEOUT");
		if (keyValuePair == null)
		{
			return null;
		}
		if (keyValuePair.Type == TokenType.Symbol || keyValuePair.Type == TokenType.ConstSymbol)
		{
			return (int?)ExecutionEngine.Instance.ActiveContext.GetSymbolValue(keyValuePair.Value, errors);
		}
		if (keyValuePair.Type == TokenType.NumericLiteral && int.TryParse(keyValuePair.Value, out var result2))
		{
			result = result2;
		}
		return result;
	}

	internal bool GetWaitFlag()
	{
		bool result = false;
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("WAIT");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			result = result2;
		}
		return result;
	}

	internal void AddError(string code, string message, ErrorList errors)
	{
		errors.Add(Error.NewError(code, message, $"Line {LineNumber}: {this}"));
	}

	internal void AddWarning(string code, string message, ErrorList errors)
	{
		errors.Add(Error.NewWarning(code, message, $"Line {LineNumber}: {this}"));
	}

	internal Instruction GetByLineNumber(Instruction instruction, int lineNumber)
	{
		if (instruction.LineNumber == lineNumber)
		{
			return instruction;
		}
		foreach (Instruction instruction2 in instruction.Instructions)
		{
			Instruction byLineNumber = GetByLineNumber(instruction2, lineNumber);
			if (byLineNumber != null)
			{
				return byLineNumber;
			}
		}
		return null;
	}

	internal static void CleanUpResources()
	{
		PickerBoard.Instance.SetSensors(on: false);
	}

	internal bool IsSymbolic(Token tok)
	{
		if (tok.Type != TokenType.ConstSymbol)
		{
			return tok.Type == TokenType.Symbol;
		}
		return true;
	}

	internal bool IsBranch()
	{
		if (BranchType != Branch.Unconditional)
		{
			return BranchType == Branch.Conditional;
		}
		return true;
	}

	internal bool IsConditional()
	{
		return BranchType == Branch.Conditional;
	}

	internal virtual bool ValidateConditionalBranch(Instruction branchTarget, ExecutionResult result)
	{
		return true;
	}

	private bool GetLocationInner(ExecutionResult result, out int deck, out int slot)
	{
		deck = -1;
		slot = -1;
		int? deckOperand = GetDeckOperand(result.Errors);
		if (!deckOperand.HasValue)
		{
			return false;
		}
		deck = deckOperand.Value;
		KeyValuePair keyValuePair = Operands.GetKeyValuePair("SLOT");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors);
		if (result.Errors.Count > 0)
		{
			return false;
		}
		if (keyValuePairValue.HasValue)
		{
			slot = keyValuePairValue.Value;
		}
		return true;
	}
}
