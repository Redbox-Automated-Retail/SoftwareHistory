namespace Redbox.HAL.Component.Model;

public interface IExecutionContext : IHardwareCorrectionObserver
{
	IFormattedLog ContextLog { get; }

	string ID { get; }

	string ProgramName { get; }

	void CreateResult(string code, string msg, string id);
}
