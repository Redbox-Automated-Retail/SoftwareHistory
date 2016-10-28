using System.IO;

namespace Redbox.HAL.Component.Model;

public interface IPersistentCounterService
{
	bool Exists(string name);

	IPersistentCounter Find(string name);

	IPersistentCounter Increment(string name);

	IPersistentCounter Increment(TimeoutCounters counter);

	IPersistentCounter Decrement(string name);

	bool Reset(IPersistentCounter counter);

	bool Reset(string name);

	void LogTimeouts(TextWriter log);

	void LogCounter(string counter, TextWriter writer);

	bool Insert(HardwareCorrectionStatistic statistic, IExecutionContext context, bool correctionOk);

	bool RemoveAll(HardwareCorrectionStatistic stat);

	void LogStats(HardwareCorrectionStatistic key, TextWriter log);

	void AddWeeklyResettable(IPersistentCounter counter);

	void ResetWeekly();
}
