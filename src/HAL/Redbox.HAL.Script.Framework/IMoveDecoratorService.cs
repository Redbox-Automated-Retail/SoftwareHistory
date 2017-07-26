using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal interface IMoveDecoratorService
{
	ErrorCodes ShowEmptyStuck(ExecutionContext context, ILocation location, int offset);

	ErrorCodes ShowEmptyStuck(ExecutionContext context, ILocation location);

	ErrorCodes MoveOffset(ExecutionContext context, ILocation location, int offset);

	void ConfigureLowerLimitFor(string program);

	bool LowerLimitConfigured(ExecutionContext context);

	void LogLimitMessage(ApplicationLog log);

	bool LowerLimitCounterExceeded();
}
