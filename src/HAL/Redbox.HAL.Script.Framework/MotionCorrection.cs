using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct MotionCorrection : IDisposable
{
	private readonly int Deck;

	private readonly int Slot;

	private readonly MoveMode Mode;

	private readonly IFormattedLog Log;

	public void Dispose()
	{
	}

	internal ErrorCodes Move(bool correct)
	{
		return Move(ServiceLocator.Instance.GetService<IMotionControlService>(), correct);
	}

	internal ErrorCodes Move(MoveCorrectionBitmask bit)
	{
		return Move(ServiceLocator.Instance.GetService<IMotionControlService>(), CanCorrect(bit));
	}

	internal ErrorCodes Move(IMotionControlService service, bool correct)
	{
		ErrorCodes errorCodes = service.MoveTo(Deck, Slot, Mode, Log);
		if (ErrorCodes.ArcusNotResponsive != errorCodes)
		{
			return errorCodes;
		}
		if (correct && service.Reset(Log, quick: true))
		{
			errorCodes = service.MoveTo(Deck, Slot, Mode, Log);
		}
		return errorCodes;
	}

	internal ErrorCodes Move(IMotionControlService service, MoveCorrectionBitmask bit)
	{
		return Move(service, CanCorrect(bit));
	}

	internal MotionCorrection(int deck, int slot, MoveMode mode, IFormattedLog log)
	{
		this = default(MotionCorrection);
		Deck = deck;
		Slot = slot;
		Log = log;
		Mode = mode;
	}

	private bool CanCorrect(MoveCorrectionBitmask bit)
	{
		MoveCorrectionBitmask moveCorrectionFlags = (MoveCorrectionBitmask)ControllerConfiguration.Instance.MoveCorrectionFlags;
		return (moveCorrectionFlags & bit) == bit;
	}
}
