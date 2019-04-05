using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class QueueMessageProcessorService : IQueueMessageProcessorService
{
	private List<string> _supportedMessageTypes = new List<string>();

	public string Name { get; set; }

	public bool IsDefault { get; set; }

	public List<string> SupportedMessageTypes => _supportedMessageTypes;

	public ProcessMessageDelegate ProcessMessage { get; set; }

	public MessageProcessedDelegate MessageProcessed { get; set; }

	public BeforeMessageProcessingDelegate BeforeMessageProcessing { get; set; }
}
