using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public class ResultHelper
{
	public static ResultHelper Instance => Singleton<ResultHelper>.Instance;

	public List<string> EnumerateResults(ExecutionContext context)
	{
		return context.CachedResults;
	}

	internal string CreateCameraCaptureErrorResult()
	{
		return CreateResult(StackEnd.Bottom, "CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.", null, null, null, null, null);
	}

	internal string CreateItemStuckResult(string id)
	{
		return CreateResult(StackEnd.Bottom, "ItemStuckInGripper", "There is an item stuck in the gripper", null, null, id, null, null);
	}

	internal string CreateMoveErrorResult()
	{
		return CreateResult(StackEnd.Bottom, "MachineError", "There was an error executing the MOVE instruction.", null, null, null, null, null);
	}

	internal string CreateMoveErrorResult(int deck, int slot)
	{
		return CreateResult(StackEnd.Bottom, "MachineError", "There was an error executing the MOVE instruction.", deck, slot, null, null, null);
	}

	internal string CreateInfoResult(StackEnd end, string code, string message)
	{
		return CreateResult(end, code, message, null, null, null, null, null);
	}

	internal string CreateInfoResult(string code, string message)
	{
		return CreateResult(StackEnd.Bottom, code, message, null, null, null, null, null);
	}

	internal string CreateInfoResult(string code, string message, string id)
	{
		return CreateResult(StackEnd.Bottom, code, message, null, null, id, null, null);
	}

	internal string CreateResult(string code, string message, int? deck, int? slot, string item, DateTime? returnTime, string previousID)
	{
		return CreateResult(StackEnd.Bottom, code, message, deck, slot, item, returnTime, previousID);
	}

	internal string CreateResult(StackEnd end, string code, string message, int? deck, int? slot, string item, DateTime? returnTime, string previousID)
	{
		string text = "~";
		if (deck.HasValue && slot.HasValue)
		{
			text = $"{deck},{slot}";
		}
		string text2 = "~";
		if (returnTime.HasValue)
		{
			text2 = returnTime.ToString();
		}
		Guid guid = Guid.NewGuid();
		DateTime now = DateTime.Now;
		string text3 = item ?? "~";
		string text4 = previousID ?? "~";
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (activeContext.IsImmediate)
		{
			return string.Empty;
		}
		if (message.Contains("'"))
		{
			message = message.Replace('\'', ' ');
		}
		string text5 = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "RESULT", guid, now, code, text, text3, message, text2, text4);
		activeContext.CachedResults.Add(text5);
		return text5;
	}

	private ResultHelper()
	{
	}
}
