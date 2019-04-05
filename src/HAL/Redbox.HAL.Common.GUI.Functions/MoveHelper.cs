using System;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions.Services;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Common.GUI.Functions;

public sealed class MoveHelper
{
	private readonly HardwareService Service;

	private readonly int Timeout;

	public ErrorCodes MoveTo(int deck, int slot, string mode)
	{
		string text = $"MOVE DECK={deck} SLOT={slot} MODE={mode}";
		HardwareJob val = default(HardwareJob);
		if (!((ClientCommandResult)(object)Service.ExecuteImmediate(text, (int?)Timeout, ref val)).Success)
		{
			return ErrorCodes.ServiceChannelError;
		}
		return Enum<ErrorCodes>.ParseIgnoringCase(HardwareJobExtensions.GetTopOfStack(val), ErrorCodes.ServiceChannelError);
	}

	public ErrorCodes MoveTo(int deck, int slot)
	{
		return MoveTo(deck, slot, "NORMAL");
	}

	public IControllerPosition GetPosition()
	{
		MotionControlDataExecutor motionControlDataExecutor = new MotionControlDataExecutor(Service);
		try
		{
			((JobExecutor)motionControlDataExecutor).Run();
			return motionControlDataExecutor.Position;
		}
		finally
		{
			((IDisposable)(object)motionControlDataExecutor)?.Dispose();
		}
	}

	public ErrorCodes MoveAbs(Axis axis, int units)
	{
		return MoveAbs(axis, units, checkSensors: true);
	}

	public ErrorCodes MoveAbs(Axis axis, int units, bool checkSensors)
	{
		string text = $"MOVEABS AXIS={axis.ToString().ToUpper()} UNITS={units.ToString()} SENSOR-CHECK={checkSensors.ToString().ToUpper()}";
		HardwareJob val = default(HardwareJob);
		if (!((ClientCommandResult)(object)Service.ExecuteImmediate(text, (int?)Timeout, ref val)).Success)
		{
			return ErrorCodes.ServiceChannelError;
		}
		return Enum<ErrorCodes>.ParseIgnoringCase(HardwareJobExtensions.GetTopOfStack(val), ErrorCodes.ServiceChannelError);
	}

	public IMotionControlLimitResponse ReadLimits()
	{
		MotionControlDataExecutor motionControlDataExecutor = new MotionControlDataExecutor(Service);
		try
		{
			((JobExecutor)motionControlDataExecutor).Run();
			return motionControlDataExecutor.Limits;
		}
		finally
		{
			((IDisposable)(object)motionControlDataExecutor)?.Dispose();
		}
	}

	public MoveHelper(HardwareService service, int timeout)
	{
		Service = service;
		Timeout = timeout;
	}

	public MoveHelper(HardwareService service)
		: this(service, 64000)
	{
	}
}
