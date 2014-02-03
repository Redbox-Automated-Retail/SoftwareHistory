namespace Redbox.KioskEngine.ComponentModel;

public interface ISoundService
{
	bool TTSEnabled { get; }

	void PlaySync(byte[] buffer);

	void PlayAsync(byte[] buffer);

	void PlaySync(string fileName);

	void PlayAsync(string fileName);

	bool StopPlay();

	void Reset();

	void SayTTS(string speech);

	void SilenceTTS();
}
