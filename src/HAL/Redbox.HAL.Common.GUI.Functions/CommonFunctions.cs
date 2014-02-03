using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public static class CommonFunctions
{
	private const int SellThruSlotWidth = 466;

	private static bool? m_developerMode = null;

	public static ClientHelper Helper { get; private set; }

	public static bool DeveloperMode
	{
		get
		{
			if (m_developerMode.HasValue)
			{
				return m_developerMode.Value;
			}
			m_developerMode = RegistryOptions.Instance.GetSecretValue<bool?>("SOFTWARE\\Redbox\\HAL Management Console", "DeveloperMode") ?? false;
			return m_developerMode.Value;
		}
	}

	public static bool ComputeQuadrants(decimal? startOffset, int? numberOfQuadrants, int? sellThruSlots, int? sellThruOffset, int? slotsPerQuadrant, decimal? slotWidth, XmlNode deckNode)
	{
		decimal? num = startOffset;
		for (int i = 0; i < numberOfQuadrants; i++)
		{
			XmlNode node = deckNode.ChildNodes[i];
			node.SetAttributeValue("Offset", (int)num.Value);
			decimal? num2 = 0m;
			num2 = ((!sellThruSlots.HasValue || !sellThruOffset.HasValue) ? ((decimal?)slotsPerQuadrant.Value * slotWidth + slotWidth) : ((decimal?)(slotsPerQuadrant - 1).Value * slotWidth + (decimal?)0.5m + (decimal?)466m));
			num += num2;
		}
		return true;
	}

	public static bool GetMotionControlPosition(Axis axis, HardwareService service, out int position)
	{
		object obj;
		switch (axis)
		{
		case Axis.None:
			position = 0;
			return false;
		default:
			obj = "MOTIONCONTROL YPOS";
			break;
		case Axis.X:
			obj = "MOTIONCONTROL XPOS";
			break;
		}
		string text = (string)obj;
		HardwareJob val2 = default(HardwareJob);
		HardwareCommandResult val = service.ExecuteImmediate(text, ref val2);
		val2.WaitForCompletion();
		Stack<string> stack = default(Stack<string>);
		val = val2.GetStack(ref stack);
		position = 0;
		bool result = false;
		string text2 = null;
		if (!((ClientCommandResult)(object)val).Success)
		{
			return false;
		}
		if (stack.Count >= 1)
		{
			text2 = stack.Pop();
			result = int.TryParse(text2, out position);
		}
		return result;
	}

	public static string[] GetStackEntries(HardwareJob job, int depth)
	{
		return GetStackEntriesInner(job, depth);
	}

	public static string GetTopOfStack(HardwareJob job)
	{
		return GetStackEntriesInner(job, 1)[0];
	}

	public static HardwareService ConfigureCommunication(string strProtocol)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		if (Helper == null)
		{
			Helper = new ClientHelper();
		}
		try
		{
			HardwareService val = Helper.AutoConnect();
			if (val != null)
			{
				return val;
			}
			IPCProtocol iPCProtocol = IPCProtocol.Parse(strProtocol);
			return new HardwareService(iPCProtocol);
		}
		catch (UriFormatException)
		{
			return null;
		}
	}

	public static HardwareJob ExecuteInstruction(HardwareService service, string instruction)
	{
		HardwareJob result = null;
		HardwareCommandResult val = service.ExecuteImmediate(instruction, ref result);
		if (!((ClientCommandResult)(object)val).Success)
		{
			return null;
		}
		return result;
	}

	public static HardwareJob ExecuteInstruction(HardwareService service, string instruction, int timeout)
	{
		HardwareJob result = null;
		HardwareCommandResult val = service.ExecuteImmediate(instruction, (int?)timeout, ref result);
		if (!((ClientCommandResult)(object)val).Success)
		{
			return null;
		}
		return result;
	}

	private static string[] GetStackEntriesInner(HardwareJob job, int depth)
	{
		string[] array = new string[depth];
		for (int i = 0; i < depth; i++)
		{
			array[i] = string.Empty;
		}
		Stack<string> stack2 = default(Stack<string>);
		HardwareCommandResult stack = job.GetStack(ref stack2);
		if (((ClientCommandResult)(object)stack).Success && stack2.Count >= depth)
		{
			for (int j = 0; j < depth; j++)
			{
				array[j] = stack2.Pop();
			}
		}
		return array;
	}
}
