using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DefaultValidateDiskResult : IValidateDiskResult
{
	public ErrorCodes Error { get; internal set; }

	public bool Validated { get; internal set; }

	public IDecodeResult Result { get; internal set; }

	public DateTime ValidateTime { get; private set; }

	internal void Invalidate()
	{
		Validated = false;
		Result = null;
	}

	internal DefaultValidateDiskResult(bool valid, IDecodeResult result)
	{
		Validated = valid;
		Result = result;
		ValidateTime = DateTime.Now;
	}
}
