namespace Redbox.KioskEngine.ComponentModel;

public interface ITextToSpeechService
{
	bool TTSEnabled { get; }

	void Reset();

	void RunSpeechWorkflow(string viewName);

	void TTSRepeatSequence();
}
