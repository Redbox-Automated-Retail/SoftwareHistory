using System;

namespace Redbox.Core;

internal interface ISleepEvaluator
{
	int SleepMilliseconds(int retryCount, Exception exception);
}
