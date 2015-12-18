using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TextToSpeechFunctions
{
	[KernelFunction(Name = "kernel.runspeechworkflow")]
	internal static void RunSpeechWorkflow(string resourceName)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			service.RunSpeechWorkflow(resourceName);
		}
	}
}
