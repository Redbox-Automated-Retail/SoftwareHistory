namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface ISyncUnknownProcessor
{
	void Initialize();

	void ScheduleJob(int? qlm_deck, bool forceSync = false);

	void ProcessSyncLocationsComplete(string jobId);
}
