using System.Speech.Synthesis;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class SoundService : ISoundService
{
	private SpeechSynthesizer m_reader;

	public static SoundService Instance => Singleton<SoundService>.Instance;

	public bool TTSEnabled => true;

	public void Reset()
	{
		LogHelper.Instance.Log("Reset sound service.");
		StopPlay();
	}

	public void PlaySync(byte[] buffer)
	{
		SimpleSound.Play(buffer, SoundFlags.SND_MEMORY);
	}

	public void PlaySync(string fileName)
	{
		SimpleSound.Play(fileName, SoundFlags.SND_FILENAME);
	}

	public void PlayAsync(byte[] buffer)
	{
		SimpleSound.Play(buffer, SoundFlags.SND_ASYNC | SoundFlags.SND_MEMORY);
	}

	public void PlayAsync(string fileName)
	{
		SimpleSound.Play(fileName, SoundFlags.SND_ASYNC | SoundFlags.SND_FILENAME);
	}

	public bool StopPlay()
	{
		return SimpleSound.StopPlay();
	}

	public void SayTTS(string speech)
	{
		if (m_reader != null)
		{
			m_reader.Dispose();
		}
		if (!string.IsNullOrEmpty(speech) && HeadphoneConnected())
		{
			m_reader = new SpeechSynthesizer();
			m_reader.SpeakAsync(speech);
			m_reader.SpeakCompleted += ReaderSpeakCompleted;
		}
	}

	public void SilenceTTS()
	{
		if (m_reader != null)
		{
			m_reader.Dispose();
		}
	}

	private SoundService()
	{
	}

	private void ReaderSpeakCompleted(object sender, SpeakCompletedEventArgs e)
	{
	}

	private bool HeadphoneConnected()
	{
		return true;
	}
}
