using System;

namespace Redbox.HAL.Component.Model;

internal interface IValidateDiskResult
{
	ErrorCodes Error { get; }

	bool Validated { get; }

	IDecodeResult Result { get; }

	DateTime ValidateTime { get; }
}
