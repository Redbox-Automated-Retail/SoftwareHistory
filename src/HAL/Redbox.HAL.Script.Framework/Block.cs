using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class Block
{
	private List<Instruction> m_instructionList;

	private List<Variable> m_blockVariables = new List<Variable>();

	private static int idCount;

	internal bool IsSynthetic
	{
		get
		{
			if (Instructions.Count == 2 && FirstInstruction.IsSynthetic && LastInstruction.IsSynthetic)
			{
				return true;
			}
			return false;
		}
	}

	internal bool IsEmpty => Instructions.Count == 0;

	internal Instruction FirstInstruction => m_instructionList[0];

	internal Instruction LastInstruction => m_instructionList[m_instructionList.Count - 1];

	internal bool Analyzed { get; set; }

	internal int ID { get; set; }

	internal Block Parent { get; set; }

	internal Block Successor { get; set; }

	internal List<Block> Predecessors { get; private set; }

	internal List<Block> Successors { get; private set; }

	internal List<Instruction> Instructions
	{
		get
		{
			return m_instructionList;
		}
		set
		{
			m_instructionList = value;
		}
	}

	internal List<VariableUse> DefList { get; private set; }

	internal List<VariableUse> UseList { get; private set; }

	internal static Block MakeBlock(Block parent)
	{
		int id = Interlocked.Increment(ref idCount);
		return new Block(id, parent);
	}

	internal bool AddSuccessor(Block block)
	{
		Block block2 = Successors.Find((Block each) => each.ID == block.ID);
		if (block2 != null)
		{
			return false;
		}
		Successors.Add(block);
		return true;
	}

	internal bool AddPredecessor(Block block)
	{
		Block block2 = Predecessors.Find((Block each) => each.ID == block.ID);
		if (block2 != null)
		{
			return false;
		}
		Predecessors.Add(block);
		return true;
	}

	internal static bool AddEdge(Block from, Block to)
	{
		from.AddSuccessor(to);
		to.AddPredecessor(from);
		return true;
	}

	internal static bool RemoveEdge(Block from, Block to)
	{
		from.Successors.Remove(to);
		to.Predecessors.Remove(from);
		return true;
	}

	internal Block Split(ExecutionResult result, bool addEdge)
	{
		if (!LastInstruction.IsBranch())
		{
			string text = "Invalid attempt to split a block: last instruction not a branch!";
			result.Errors.Add(Error.NewError("C006", "Invalid attempt to split block", text));
			LogHelper.Instance.Log(text, LogEntryType.Error);
			return null;
		}
		Block block = (Successor = MakeBlock(this));
		if (addEdge)
		{
			AddEdge(this, block);
		}
		return block;
	}

	internal Instruction PreviousInstruction(Instruction inst)
	{
		Instruction result = null;
		foreach (Instruction instruction in m_instructionList)
		{
			if (object.ReferenceEquals(instruction, inst))
			{
				return result;
			}
			result = instruction;
		}
		return null;
	}

	internal Instruction NextInstruction(Instruction inst)
	{
		int num = 0;
		foreach (Instruction instruction in m_instructionList)
		{
			if (instruction == inst)
			{
				return (num > m_instructionList.Count) ? null : m_instructionList[num + 1];
			}
			num++;
		}
		return null;
	}

	internal void AddInstruction(Instruction instruction, ExecutionResult result)
	{
		m_instructionList.Add(instruction);
	}

	internal void RemoveInstruction(Instruction instruction, ExecutionResult result)
	{
		m_instructionList.Remove(instruction);
	}

	private Block(int id, Block parent)
	{
		ID = id;
		Parent = parent;
		m_instructionList = new List<Instruction>();
		Successor = null;
		Predecessors = new List<Block>();
		Successors = new List<Block>();
		DefList = new List<VariableUse>();
		UseList = new List<VariableUse>();
	}
}
