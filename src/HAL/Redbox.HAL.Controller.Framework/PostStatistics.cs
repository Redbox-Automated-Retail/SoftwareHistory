using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PostStatistics : IFraudPostStatistics
{
	public DateTime LastSampleDate { get; private set; }

	public int Failures { get; private set; }

	public int Successes { get; private set; }

	internal PostStatistics(DateTime last, IPersistentCounter failures, IPersistentCounter success)
	{
		LastSampleDate = last;
		Failures = failures?.Value ?? (-1);
		Successes = success?.Value ?? (-1);
	}
}
