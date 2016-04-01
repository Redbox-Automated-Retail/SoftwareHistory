using System.IO;

namespace Redbox.HAL.Component.Model;

internal interface IPersistentCounterService
{
	bool Exists(string name);

	IPersistentCounter Find(string name);

	IPersistentCounter Find(TimeoutCounters counter);

	IPersistentCounter Increment(string name);

	IPersistentCounter Increment(TimeoutCounters counter);

	IPersistentCounter Decrement(string name);

	bool ResetCounter(IPersistentCounter counter);

	IPersistentCounter Reset(string name);

	IPersistentCounter Reset(TimeoutCounters ctr);

	void ResetTimeoutCounters();

	void LogTimeouts(TextWriter log);

	void LogTimeout(TimeoutCounters counter, TextWriter writer);

	void LogCounter(string counter, TextWriter writer);

	bool Insert(HardwareCorrectionStatistic statistic, IExecutionContext context, bool correctionSuccessful);

	int RemoveAll(HardwareCorrectionStatistic stat);

	void LogStats(HardwareCorrectionStatistic key, TextWriter log);
}
