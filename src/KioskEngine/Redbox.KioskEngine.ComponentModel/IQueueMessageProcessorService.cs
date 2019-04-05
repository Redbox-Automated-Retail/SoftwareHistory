using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IQueueMessageProcessorService
{
	string Name { get; set; }

	bool IsDefault { get; set; }

	List<string> SupportedMessageTypes { get; }

	ProcessMessageDelegate ProcessMessage { get; set; }

	MessageProcessedDelegate MessageProcessed { get; set; }

	BeforeMessageProcessingDelegate BeforeMessageProcessing { get; set; }
}
