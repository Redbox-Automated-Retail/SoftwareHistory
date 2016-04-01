using System;

namespace Redbox.HAL.Component.Model;

public interface IValidateDiskResult
{
	ErrorCodes Error { get; }

	bool Validated { get; }

	IDecodeResult Result { get; }

	DateTime ValidateTime { get; }
}
