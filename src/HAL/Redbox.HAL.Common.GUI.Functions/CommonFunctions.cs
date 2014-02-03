using System;
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
		string statement = (string)obj;
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(statement, out var job);
		job.WaitForCompletion();
		hardwareCommandResult = job.GetStack(out var stack);
		position = 0;
		bool result = false;
		string text = null;
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		if (stack.Count >= 1)
		{
			text = stack.Pop();
			result = int.TryParse(text, out position);
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
		if (Helper == null)
		{
			Helper = new ClientHelper();
		}
		try
		{
			HardwareService hardwareService = Helper.AutoConnect();
			if (hardwareService != null)
			{
				return hardwareService;
			}
			IPCProtocol protocol = IPCProtocol.Parse(strProtocol);
			return new HardwareService(protocol);
		}
		catch (UriFormatException)
		{
			return null;
		}
	}

	public static HardwareJob ExecuteInstruction(HardwareService service, string instruction)
	{
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(instruction, out job);
		if (!hardwareCommandResult.Success)
		{
			return null;
		}
		return job;
	}

	public static HardwareJob ExecuteInstruction(HardwareService service, string instruction, int timeout)
	{
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(instruction, timeout, out job);
		if (!hardwareCommandResult.Success)
		{
			return null;
		}
		return job;
	}

	private static string[] GetStackEntriesInner(HardwareJob job, int depth)
	{
		string[] array = new string[depth];
		for (int i = 0; i < depth; i++)
		{
			array[i] = string.Empty;
		}
		HardwareCommandResult stack = job.GetStack(out var stack2);
		if (stack.Success && stack2.Count >= depth)
		{
			for (int j = 0; j < depth; j++)
			{
				array[j] = stack2.Pop();
			}
		}
		return array;
	}
}
