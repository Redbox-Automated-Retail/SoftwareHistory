using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal interface IMoveDecoratorService
{
	ErrorCodes ShowEmptyStuck(ExecutionContext context, ILocation location, int offset);

	ErrorCodes ShowEmptyStuck(ExecutionContext context, ILocation location);

	ErrorCodes MoveOffset(ExecutionContext context, ILocation location, int offset);

	bool ResetMotionControllerChecked(ExecutionContext context);

	ErrorCodes CorrectiveMoveVend(ExecutionContext context, MoveMode mode);

	ErrorCodes CorrectiveMove(ExecutionContext context, ILocation location, MoveMode mode);

	ErrorCodes CorrectiveMove(ExecutionContext context, int deck, int slot, MoveMode mode);

	void ConfigureLowerLimitFor(string program);

	bool LowerLimitConfigured(ExecutionContext context);

	void LogLimitMessage(ApplicationLog log);

	bool LowerLimitCounterExceeded();
}
