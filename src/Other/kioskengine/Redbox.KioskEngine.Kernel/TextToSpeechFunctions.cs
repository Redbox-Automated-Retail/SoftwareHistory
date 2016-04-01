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

	[KernelFunction(Name = "kernel.ttsenabled")]
	internal static bool TTSIsEnabled()
	{
		return ServiceLocator.Instance.GetService<ITextToSpeechService>()?.TTSEnabled ?? false;
	}

	[KernelFunction(Name = "kernel.ttsgettimeout")]
	internal static int TTSGetTimeout(string timeoutType, int defaultTimeout)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service == null || !service.TTSEnabled)
		{
			return defaultTimeout;
		}
		return timeoutType switch
		{
			"short" => 30000, 
			"medium" => 60000, 
			"long" => 180000, 
			"help" => 480000, 
			_ => 30000, 
		};
	}

	[KernelFunction(Name = "kernel.ttsinreturnflow")]
	internal static void TTSInReturnFlow(bool inReturnFlow)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null)
		{
			service.InReturnFlow = inReturnFlow;
		}
	}
}
