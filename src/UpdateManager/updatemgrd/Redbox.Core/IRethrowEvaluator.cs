using System;

namespace Redbox.Core;

internal interface IRethrowEvaluator
{
	bool Rethrow(int retryCount, Exception exception);
}
