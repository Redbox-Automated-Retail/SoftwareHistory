using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class MoveDecoratorService : IMoveDecoratorService, IConfigurationObserver
{
	private bool m_configured;

	private int m_lowerLimitCounter;

	private readonly int DefaultOffset = 5;

	private readonly object Lock = new object();

	private readonly List<string> ConfiguredPrograms = new List<string>();

	public void NotifyConfigurationLoaded()
	{
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		m_configured = ControllerConfiguration.Instance.LowerLimitAsError;
	}

	public ErrorCodes ShowEmptyStuck(ExecutionContext context, ILocation location)
	{
		return ShowEmptyStuck(context, location, DefaultOffset);
	}

	public ErrorCodes ShowEmptyStuck(ExecutionContext context, ILocation location, int offset)
	{
		ErrorCodes errorCodes = MoveOffset(context, location, offset);
		if (errorCodes != ErrorCodes.Success)
		{
			if (!context.IsImmediate)
			{
				context.CreateInfoResult("MoveOffsetError", "There was an error moving the offset.");
			}
		}
		else
		{
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			service.SetFinger(GripperFingerState.Rent);
			service.TimedExtend();
			service.SetSensors(on: true);
			if (!context.IsImmediate)
			{
				context.CreateInfoResult("MoveOffsetOk", "The damaged disk is located 5 slots left, starting at the disk next to the extended gripper fingers.");
			}
		}
		return errorCodes;
	}

	public ErrorCodes MoveOffset(ExecutionContext context, ILocation location, int offset)
	{
		IDeck deck = ServiceLocator.Instance.GetService<IDecksService>().GetFrom(location);
		int num = location.Slot - offset;
		if (num <= 0)
		{
			num = deck.NumberOfSlots - Math.Abs(num);
		}
		ErrorCodes result = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(location.Deck, num, MoveMode.None, context.AppLog);
		string msg = $"MOVE-OFFSET Deck = {deck.Number} Slot = {num} returned {result.ToString().ToUpper()}";
		context.AppLog.Write(msg);
		return result;
	}

	public void ConfigureLowerLimitFor(string program)
	{
		lock (Lock)
		{
			if (!ConfiguredPrograms.Contains(program))
			{
				ConfiguredPrograms.Add(program);
			}
		}
	}

	public bool LowerLimitConfigured(ExecutionContext context)
	{
		lock (Lock)
		{
			return m_configured && ConfiguredPrograms.Find((string each) => each.Equals(context.ProgramName, StringComparison.CurrentCultureIgnoreCase)) != null;
		}
	}

	public void LogLimitMessage(ApplicationLog log)
	{
		if (m_configured)
		{
			log.WriteFormatted("Lower limit counter threshold exceeded (counter = {0}).", m_lowerLimitCounter);
		}
	}

	public bool LowerLimitCounterExceeded()
	{
		if (!m_configured)
		{
			return false;
		}
		return m_lowerLimitCounter >= 2;
	}

	internal MoveDecoratorService()
	{
		ServiceLocator.Instance.GetService<IConfigurationService>().FindConfiguration(Configurations.Controller).AddObserver(this);
	}
}
