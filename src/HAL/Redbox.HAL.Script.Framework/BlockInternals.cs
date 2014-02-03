using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class BlockInternals : CFAInternals
{
	private Block currentBlock;

	private List<Block> CFG { get; set; }

	private Block ProgramRoot { get; set; }

	internal override void Initialize(ExecutionResult result)
	{
		Entry(result);
	}

	internal override void FindReferencedUndefinedVariables(ExecutionResult result)
	{
		foreach (Block item in CFG)
		{
			foreach (VariableUse use in item.UseList)
			{
				if (!FindSymbolDefInBlock(item, use.Name, use.LineNumber) && !LocateDefInPredecessors(item, use, result))
				{
					result.Errors.Add(Error.NewError("C008", "Undefined symbol", $"The variable {use.Name} (referenced line {use.LineNumber}) isn't defined through one or more code paths."));
				}
			}
		}
	}

	internal override void ProcessInstruction(Instruction instruction, TokenList symbols, ExecutionResult result)
	{
		if (!instruction.IsBranch() && !instruction.IsLabel)
		{
			int count = currentBlock.Instructions.Count;
			if (count > 1)
			{
				currentBlock.Instructions[count - 1].TrueBranchTarget = instruction.LineNumber;
			}
			else
			{
				Block parent = currentBlock.Parent;
				if (parent != null)
				{
					parent.LastInstruction.TrueBranchTarget = instruction.LineNumber;
				}
			}
			currentBlock.AddInstruction(instruction, result);
			return;
		}
		if (instruction.BranchType == Instruction.Branch.Unconditional)
		{
			AddUnconditionalBranch(instruction);
		}
		Instruction instruction2 = ((instruction.BranchType == Instruction.Branch.Unconditional) ? instruction : new SyntheticBranch());
		if (instruction.BeginsContext)
		{
			currentBlock.LastInstruction.TrueBranchTarget = instruction.LineNumber;
		}
		if (instruction.BeginsContext || instruction.IsLabel)
		{
			instruction2.TrueBranchTarget = instruction.LineNumber;
		}
		currentBlock.AddInstruction(instruction2, result);
		currentBlock = currentBlock.Split(result, instruction.BeginsContext || instruction.IsLabel);
		ProcessNewBlock(currentBlock, instruction.LineNumber + 1, result);
		if (instruction.BranchType != Instruction.Branch.Unconditional)
		{
			currentBlock.AddInstruction(instruction, result);
			if (instruction.BranchType == Instruction.Branch.Conditional)
			{
				currentBlock = currentBlock.Split(result, addEdge: true);
				ProcessNewBlock(currentBlock, instruction.LineNumber + 1, result);
			}
		}
	}

	internal override void AddLabel(string labelName, Instruction instruction, ExecutionResult result)
	{
		if (base.Labels.TryGetValue(labelName, out var _))
		{
			result.Errors.Add(Error.NewError("C008", "Label Re-definition", $"The label {labelName} was already defined - please correct your source."));
		}
		else
		{
			base.Labels[labelName] = instruction;
		}
	}

	internal override void ProcessControlBlock(Instruction begin, Instruction end, ExecutionResult result)
	{
		Block block = FindBlockWithInstruction(begin, result);
		Block block2 = FindBlockWithInstruction(end, result);
		if (block == null || block2 == null)
		{
			string text = $"ProcessControlBlock: unable to locate {begin.Mnemonic} or {end.Mnemonic}";
			LogHelper.Instance.Log(text, LogEntryType.Error);
			result.Errors.Add(Error.NewError("C006", "CFA Error", text));
		}
		else
		{
			AnalyzeConditionalBranches(block, block2, begin, end, result);
			block.Analyzed = true;
		}
	}

	internal override void FixupUnconditionalBranches(ExecutionResult result)
	{
		foreach (Instruction unconditionalBranch in base.UnconditionalBranches)
		{
			Instruction value = null;
			if (!base.Labels.TryGetValue(unconditionalBranch.TargetLabel, out value))
			{
				string text = $"Missing unconditional branch: there is no branch target for label {unconditionalBranch.TargetLabel}.";
				LogHelper.Instance.Log(text, LogEntryType.Debug);
				result.Errors.Add(Error.NewError("C008", "CFA Error", text));
				break;
			}
			Block block = FindBlockWithInstruction(unconditionalBranch, result);
			Block block2 = FindBlockWithInstruction(value, result);
			if (block2 == null || block == null)
			{
				string text2 = $"CFA Error: unable to locate branch instruction and/or target.";
				LogHelper.Instance.Log(text2, LogEntryType.Debug);
				result.Errors.Add(Error.NewError("C008", "CFA Error", text2));
				break;
			}
			Block.AddEdge(block, block2);
			unconditionalBranch.TrueBranchTarget = value.LineNumber;
		}
	}

	internal override void Finalize(ExecutionResult result)
	{
		LiftSyntheticInstructions(ProgramRoot, null, result);
		MergeSuccessors(ProgramRoot, result);
		if (CFG.Count > 1)
		{
			string text = $"CFA.Finalize: Number of blocks left in CFG is {CFG.Count}; should be 1!";
			LogHelper.Instance.Log(text, LogEntryType.Error);
			result.Errors.Add(Error.NewError("C008", "CFA Error", text));
		}
		Block programRoot = ProgramRoot;
		for (int i = 0; i < programRoot.Instructions.Count; i++)
		{
			Instructions.Add(programRoot.Instructions[i]);
		}
		base.Finalized = true;
	}

	internal override void InternalsDump(string fileName)
	{
		DumpBlocks(fileName);
	}

	internal override void RemoveDeadBlocks(ExecutionResult result)
	{
		List<Block> list = new List<Block>();
		foreach (Block item in CFG)
		{
			if (item.ID != ProgramRoot.ID && item.Predecessors.Count <= 0)
			{
				if (CFA.Debug)
				{
					LogHelper.Instance.Log($"Block {item.ID} has no predecessors - removing dead code.", LogEntryType.Info);
				}
				item.Parent.Successor = item.Successor;
				if (item.Successor != null)
				{
					item.Successor.Parent = item.Parent;
				}
				for (int i = 0; i < item.Successors.Count; i++)
				{
					Block to = item.Successors[0];
					item.Successors.RemoveAt(0);
					Block.RemoveEdge(item, to);
				}
				list.Add(item);
			}
		}
		foreach (Block item2 in list)
		{
			CFG.Remove(item2);
		}
	}

	internal override void AddVariableDef(string name, int line, ExecutionResult result)
	{
		currentBlock.DefList.Add(new VariableUse
		{
			Name = name,
			LineNumber = line
		});
	}

	internal override void AddVariableUse(string name, int line, ExecutionResult result)
	{
		currentBlock.UseList.Add(new VariableUse
		{
			Name = name,
			LineNumber = line
		});
	}

	internal BlockInternals()
	{
		CFG = new List<Block>();
	}

	private void DumpBlocks(string fileName)
	{
		if (!CFA.Debug)
		{
			return;
		}
		using TextWriter textWriter = new StreamWriter("C:\\temp\\" + fileName + ".CFA");
		foreach (Block item in CFG)
		{
			textWriter.WriteLine("Block " + item.ID);
			textWriter.WriteLine("\tPredecessors:");
			foreach (Block predecessor in item.Predecessors)
			{
				textWriter.WriteLine("\t\tBlock {0}", predecessor.ID);
			}
			textWriter.WriteLine("\tSuccessors:");
			foreach (Block successor in item.Successors)
			{
				textWriter.WriteLine("\t\tBlock {0}", successor.ID);
			}
			foreach (Instruction instruction in item.Instructions)
			{
				textWriter.WriteLine(string.Concat("\tInstruction: ", instruction, "; line ", instruction.LineNumber));
				if (instruction.TrueBranchTarget != ProgramInstruction.InvalidLineNumber)
				{
					textWriter.WriteLine(string.Concat("\t\tTrue branch: ", instruction.GetByLineNumber(instruction.TrueBranchTarget), " at line ", instruction.TrueBranchTarget));
				}
				if (instruction.FalseBranchTarget != ProgramInstruction.InvalidLineNumber)
				{
					textWriter.WriteLine(string.Concat("\t\tFalse branch: ", instruction.GetByLineNumber(instruction.FalseBranchTarget), " at line ", instruction.FalseBranchTarget));
				}
			}
		}
	}

	private void ProcessNewBlock(Block block, int lineNumber, ExecutionResult result)
	{
		CFG.Add(block);
		UberInstruction uberInstruction = new UberInstruction();
		uberInstruction.LineNumber = lineNumber;
		Instruction instruction = uberInstruction;
		block.AddInstruction(instruction, result);
	}

	private void LiftSyntheticInstruction(Block block, Instruction instruction, ExecutionResult result)
	{
		if (!instruction.IsSynthetic)
		{
			string text = $"LiftSyntheticInstruction: {instruction.Mnemonic} is not a synthetic instruction (S_BR or S_UBER).";
			LogHelper.Instance.Log(text, LogEntryType.Debug);
			result.Errors.Add(Error.NewError("C006", "CFA Error", text));
			return;
		}
		if (instruction.IsBranch())
		{
			if (block.IsSynthetic)
			{
				Block parent = block.Parent;
				parent.LastInstruction.TrueBranchTarget = block.LastInstruction.TrueBranchTarget;
			}
			else
			{
				int index = block.Instructions.Count - 2;
				Instruction instruction2 = block.Instructions[index];
				if (instruction2.IsSynthetic)
				{
					string text2 = $"S_BR removal: instruction {instruction2} to update is not a real instruction.";
					LogHelper.Instance.Log(text2, LogEntryType.Error);
					result.Errors.Add(Error.NewError("C008", "CFA Error", text2));
				}
				instruction2.TrueBranchTarget = instruction.TrueBranchTarget;
			}
		}
		block.Instructions.Remove(instruction);
	}

	private void LiftSyntheticInstructions(Block begin, Block end, ExecutionResult result)
	{
		Block block = begin;
		while (block != end)
		{
			Instruction lastInstruction = block.LastInstruction;
			LiftEndBranch(block, lastInstruction, result);
			lastInstruction = block.FirstInstruction;
			LiftSyntheticInstruction(block, lastInstruction, result);
			if (block.IsEmpty)
			{
				if (block.Parent != null)
				{
					block.Parent.Successor = block.Successor;
				}
				Block successor = block.Successor;
				CFG.Remove(block);
				block = successor;
			}
			else
			{
				block = block.Successor;
			}
		}
	}

	private void LiftEndBranch(Block b, Instruction i, ExecutionResult result)
	{
		if (i.IsSynthetic && i.IsBranch())
		{
			LiftSyntheticInstruction(b, i, result);
		}
	}

	private Block FindBlockWithInstruction(Instruction instruction, ExecutionResult result)
	{
		foreach (Block item in CFG)
		{
			foreach (Instruction instruction2 in item.Instructions)
			{
				if (instruction2 == instruction)
				{
					return item;
				}
			}
		}
		string text = $"CFA Error: instruction {instruction.Mnemonic} not found in CFG!";
		LogHelper.Instance.Log(text, LogEntryType.Fatal);
		result.Errors.Add(Error.NewError("C008", "CFA Error!", text));
		return null;
	}

	private void AnalyzeConditionalBranches(Block start, Block end, Instruction beginCond, Instruction endCond, ExecutionResult result)
	{
		Block successor = start.Successor;
		Block block = start;
		Instruction instruction = beginCond;
		bool flag = successor != end;
		do
		{
			Instruction lastInstruction = successor.LastInstruction;
			if (successor.Analyzed)
			{
				if (CFA.Debug)
				{
					LogHelper.Instance.Log($"AnalyzeConditionalBranches: skipping block {successor.ID} - already analyzed.", LogEntryType.Info);
				}
				flag = successor != end;
				successor = successor.Successor;
				continue;
			}
			if (instruction.ValidateConditionalBranch(lastInstruction, result))
			{
				Block parent = successor.Parent;
				Instruction instruction2 = null;
				if (parent != null)
				{
					instruction2 = parent.LastInstruction;
					if (instruction2.IsSynthetic)
					{
						parent.LastInstruction.TrueBranchTarget = endCond.LineNumber;
					}
				}
				Block.AddEdge(block, successor);
				instruction = successor.LastInstruction;
				block = successor;
				successor.Analyzed = true;
			}
			flag = successor != end;
			successor = successor.Successor;
		}
		while (flag);
	}

	private void RemoveBlockBetween(Block lhs, Block rhs, ExecutionResult result)
	{
		Block successor = lhs.Successor;
		successor.Instructions.Clear();
		lhs.Successor = rhs;
		if (rhs != null)
		{
			rhs.Parent = lhs;
		}
		else
		{
			if (currentBlock != successor)
			{
				string text = "RemoveBlockBetween: successor block is null, and is not currentBlock";
				LogHelper.Instance.Log(text, LogEntryType.Fatal);
				result.Errors.Add(Error.NewError("C008", "CFA Error!", text));
				return;
			}
			currentBlock = lhs;
		}
		CFG.Remove(successor);
	}

	private void MergeBlock(Block parent, Block child, ExecutionResult result)
	{
		parent.Instructions.AddRange(child.Instructions);
		RemoveBlockBetween(parent, child.Successor, result);
	}

	private void MergeBlocks(Block into, Block start, Block end, ExecutionResult result)
	{
		do
		{
			Block successor = start.Successor;
			MergeBlock(into, start, result);
			start = successor;
		}
		while (start != end);
	}

	private void MergeSuccessors(Block block, ExecutionResult result)
	{
		for (Block successor = block.Successor; successor != null; successor = block.Successor)
		{
			MergeBlock(block, successor, result);
		}
	}

	private Block Entry(ExecutionResult result)
	{
		ProgramRoot = Block.MakeBlock(null);
		ProcessNewBlock(ProgramRoot, 1, result);
		currentBlock = ProgramRoot;
		return ProgramRoot;
	}

	private bool FindSymbolDefInBlock(Block block, string symbol, int lineNumber)
	{
		List<VariableUse> list = block.DefList.FindAll((VariableUse each) => each.Name.Equals(symbol) && each.LineNumber < lineNumber);
		return list.Count > 0;
	}

	private bool LocateDefInPredecessors(Block start, VariableUse variable, ExecutionResult result)
	{
		if (start.Predecessors.Count == 0)
		{
			return false;
		}
		int num = 0;
		int num2 = 0;
		foreach (Block predecessor in start.Predecessors)
		{
			if (CFA.DumpVariableLiveSearch)
			{
				LogHelper.Instance.Log($"Search for variable {variable.Name} (ref line {variable.LineNumber}) in block {predecessor.ID}", LogEntryType.Info);
			}
			if (predecessor.ID > start.ID)
			{
				if (CFA.DumpVariableLiveSearch)
				{
					LogHelper.Instance.Log($"  not searching block ID {predecessor.ID} (current block {start.ID})", LogEntryType.Info);
				}
				num2++;
				continue;
			}
			List<VariableUse> list = predecessor.DefList.FindAll((VariableUse each) => each.Name.Equals(variable.Name));
			if (list.Count > 0)
			{
				if (CFA.DumpVariableLiveSearch)
				{
					LogHelper.Instance.Log($"  found definition in block {predecessor.ID}", LogEntryType.Info);
				}
				num++;
			}
			else if (predecessor.DefList.FindAll((VariableUse each) => each.Name.Equals(variable.Name)).Count > 0)
			{
				num++;
				if (CFA.DumpVariableLiveSearch)
				{
					LogHelper.Instance.Log($"Variable {variable.Name} (ref line {variable.LineNumber}) found def in block {predecessor.ID}", LogEntryType.Info);
				}
			}
			else if (LocateDefInPredecessors(predecessor, variable, result))
			{
				num++;
			}
		}
		return num == start.Predecessors.Count - num2;
	}
}
