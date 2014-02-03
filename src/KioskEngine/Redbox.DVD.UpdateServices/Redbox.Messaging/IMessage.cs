using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IMessage
{
	byte[] MessageData { get; set; }

	string Data { get; set; }

	byte[] DataAndPropertiesBytes { get; set; }

	string DataAndPropertiesString { get; set; }

	string MessageId { get; set; }

	string CorrelationId { get; set; }

	Redbox.Messaging.MessageType MessageType { get; set; }

	Dictionary<string, string> Properties { get; set; }

	void AddAdditionalProperties();
}
