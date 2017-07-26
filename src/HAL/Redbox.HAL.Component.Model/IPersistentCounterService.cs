using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IPersistentCounterService
{
	bool Exists(string name);

	IPersistentCounter Find(string name);

	IPersistentCounter Find(TimeoutCounters counter);

	IPersistentCounter Increment(string name);

	IPersistentCounter Increment(TimeoutCounters counter);

	IPersistentCounter Decrement(string name);

	IPersistentCounter Decrement(TimeoutCounters counter);

	bool Reset(IPersistentCounter counter);

	bool Reset(string name);

	bool Insert(HardwareCorrectionStatistic statistic, IExecutionContext context, bool correctionOk);

	bool RemoveAll(HardwareCorrectionStatistic stat);

	List<IHardwareCorrectionStatistic> GetStats(HardwareCorrectionStatistic key);

	void AddWeeklyResettable(IPersistentCounter counter);

	void ResetWeekly();
}
