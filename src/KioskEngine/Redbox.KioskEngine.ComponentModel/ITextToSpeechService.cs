namespace Redbox.KioskEngine.ComponentModel;

public interface ITextToSpeechService
{
	bool TTSEnabled { get; }

	bool AudioDeviceConnected { get; set; }

	bool InReturnFlow { get; set; }

	void Reset();

	void RunSpeechWorkflow(string viewName);

	void TTSRepeatSequence();

	int GetTimeout(string timeoutType, int defaultTimeout);
}
