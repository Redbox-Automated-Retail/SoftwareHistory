using System.Windows.Forms;

namespace Redbox.KioskEngine.ComponentModel;

public interface IEngineApplication
{
	bool HasCorruptedDatabase { get; }

	bool AppStarting { get; set; }

	void ThreadSafeHostUpdate(MethodInvoker invoker);

	bool CanRestart();

	bool CanShutDown();

	void FlagForSafeShutdown();

	void ShutDown();

	void CorruptDbDetected(string dbName);
}
