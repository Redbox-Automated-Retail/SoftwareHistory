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
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck deck = service.GetFrom(location);
		int num = location.Slot - offset;
		if (num <= 0)
		{
			num = deck.NumberOfSlots - Math.Abs(num);
		}
		ErrorCodes errorCodes = CorrectiveMove(context, location.Deck, num, MoveMode.None);
		string msg = $"MOVE-OFFSET Deck = {deck.Number} Slot = {num} returned {errorCodes.ToString().ToUpper()}";
		context.AppLog.Write(msg);
		return errorCodes;
	}

	public bool ResetMotionControllerChecked(ExecutionContext context)
	{
		if (!ControllerConfiguration.Instance.RestartControllerDuringUserJobs)
		{
			return false;
		}
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		bool flag = service.Reset(context.AppLog, quick: true);
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service2.Insert(HardwareCorrectionStatistic.Arcus, context, flag);
		return flag;
	}

	public ErrorCodes CorrectiveMoveVend(ExecutionContext context, MoveMode mode)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorCodes errorCodes = service.MoveVend(mode, context.AppLog);
		if (ErrorCodes.ArcusNotResponsive == errorCodes && ResetMotionControllerChecked(context))
		{
			errorCodes = service.MoveVend(mode, context.AppLog);
		}
		return errorCodes;
	}

	public ErrorCodes CorrectiveMove(ExecutionContext context, ILocation location, MoveMode mode)
	{
		return CorrectiveMove(context, location.Deck, location.Slot, mode);
	}

	public ErrorCodes CorrectiveMove(ExecutionContext context, int deck, int slot, MoveMode mode)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorCodes errorCodes = service.MoveTo(deck, slot, mode, context.AppLog);
		if (ErrorCodes.ArcusNotResponsive == errorCodes && ResetMotionControllerChecked(context))
		{
			errorCodes = service.MoveTo(deck, slot, mode, context.AppLog);
		}
		else if (ErrorCodes.LowerLimitError == errorCodes && LowerLimitConfigured(context))
		{
			m_lowerLimitCounter++;
			ErrorCodes errorCodes2 = service.HomeAxis(Axis.Y, context.AppLog);
			context.AppLog.WriteFormatted("Move returned {0}; HOME Y returned {1}; return TIMEOUT", errorCodes.ToString(), errorCodes2.ToString());
			errorCodes = ErrorCodes.Timeout;
		}
		return errorCodes;
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
		if (!m_configured)
		{
			return false;
		}
		lock (Lock)
		{
			return ConfiguredPrograms.Find((string each) => each.Equals(context.ProgramName, StringComparison.CurrentCultureIgnoreCase)) != null;
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
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		IAttributeXmlConfiguration attributeXmlConfiguration = service.FindConfiguration(Configurations.Controller);
		attributeXmlConfiguration.AddObserver(this);
	}
}
