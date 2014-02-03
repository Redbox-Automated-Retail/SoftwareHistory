using System.Collections.Generic;
using Microsoft.Win32;

namespace Redbox.HAL.Script.Framework;

public sealed class Registers
{
	private Stack<CallFrame> m_callStack;

	public CallFrame ActiveFrame
	{
		get
		{
			if (CallStack.Count != 0)
			{
				return CallStack.Peek();
			}
			return null;
		}
	}

	internal Stack<CallFrame> CallStack
	{
		get
		{
			if (m_callStack == null)
			{
				m_callStack = new Stack<CallFrame>();
			}
			return m_callStack;
		}
	}

	public Instruction PopCallFrame()
	{
		if (CallStack.Count != 0)
		{
			return CallStack.Pop().ParentContext;
		}
		return null;
	}

	public void PushCallFrame(string programName, Instruction parentContext)
	{
		CallStack.Push(new CallFrame
		{
			ProgramName = programName,
			ParentContext = parentContext,
			X = parentContext.X,
			Y = parentContext.Y
		});
	}

	public bool JumpToLabel(string labelName)
	{
		while (ActiveFrame != null)
		{
			int num = ActiveFrame.ParentContext.Instructions.FindInstructionIndexAtLabel(labelName);
			if (num != -1)
			{
				ActiveFrame.PC = num - 1;
				return true;
			}
			PopCallFrame();
		}
		return false;
	}

	public void Reset()
	{
		CallStack.Clear();
	}

	internal void Load(RegistryKey parentKey, IDictionary<string, ProgramInstruction> programCache)
	{
		RegistryKey registryKey = parentKey.CreateSubKey("Registers");
		if (registryKey == null)
		{
			return;
		}
		RegistryKey registryKey2 = registryKey.CreateSubKey("Call Stack");
		if (registryKey2 != null)
		{
			string[] subKeyNames = registryKey2.GetSubKeyNames();
			foreach (string name in subKeyNames)
			{
				RegistryKey parentKey2 = registryKey2.OpenSubKey(name);
				CallStack.Push(CallFrame.Load(parentKey2, programCache));
			}
			registryKey2.Close();
		}
		registryKey.Close();
	}

	internal void Save(RegistryKey parentKey)
	{
		RegistryKey registryKey = parentKey.CreateSubKey("Registers");
		if (registryKey == null)
		{
			return;
		}
		List<string> list = new List<string>(registryKey.GetSubKeyNames());
		if (list.Contains("Call Stack"))
		{
			registryKey.DeleteSubKeyTree("Call Stack");
		}
		RegistryKey registryKey2 = registryKey.CreateSubKey("Call Stack");
		if (registryKey2 != null)
		{
			int num = CallStack.Count;
			foreach (CallFrame item in CallStack)
			{
				RegistryKey registryKey3 = registryKey2.CreateSubKey($"{num:0000}");
				if (registryKey3 != null)
				{
					item.Save(registryKey3);
					num--;
					continue;
				}
				break;
			}
			registryKey2.Close();
		}
		registryKey.Close();
	}
}
