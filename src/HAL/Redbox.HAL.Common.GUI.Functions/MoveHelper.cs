using System.Collections.Generic;
using Redbox.HAL.Client;

namespace Redbox.HAL.Common.GUI.Functions;

public class MoveHelper
{
	private readonly HardwareService Service;

	public MoveHelper(HardwareService service)
	{
		Service = service;
	}

	public MoveHelperResult MoveTo(int deck, int slot, string mode)
	{
		return MoveTo(Service, deck, slot, mode);
	}

	public MoveHelperResult MoveTo(int deck, int slot, string mode, bool readLimits)
	{
		MoveHelperResult moveHelperResult = MoveTo(Service, deck, slot, mode);
		if (!readLimits)
		{
			return moveHelperResult;
		}
		moveHelperResult.Limits = ReadLimits();
		return moveHelperResult;
	}

	public string[] ReadLimits()
	{
		List<string> list = new List<string>();
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, "MOTIONCONTROL READLIMITS");
		if (hardwareJob == null)
		{
			return list.ToArray();
		}
		if (hardwareJob.GetStack(out var stack).Errors.Count > 0)
		{
			return list.ToArray();
		}
		string s = stack.Peek();
		try
		{
			int num = int.Parse(s);
			stack.Pop();
			for (int i = 0; i < num; i++)
			{
				list.Add(stack.Pop());
			}
		}
		catch
		{
			list.Clear();
		}
		return list.ToArray();
	}

	private MoveHelperResult MoveTo(HardwareService service, int deck, int slot, string mode)
	{
		if (string.IsNullOrEmpty(mode))
		{
			mode = "NORMAL";
		}
		string statement = $"MOVE DECK={deck} SLOT={slot} MODE={mode}";
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(statement, 64000, out var job);
		MoveHelperResult moveHelperResult = new MoveHelperResult();
		if (!hardwareCommandResult.Success)
		{
			moveHelperResult.MoveResultMessage = "SERVICE DOWN";
			return moveHelperResult;
		}
		moveHelperResult.MoveResultMessage = CommonFunctions.GetTopOfStack(job);
		return moveHelperResult;
	}
}
