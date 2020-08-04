using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace UpdateClientService.API.Services.IoT;

public interface IMqttProxy
{
	int LastAvailableUSInstance { get; set; }

	Task PublishMessageAsync(string topicName, string message);

	Task SubscribeToTopicsAsync(List<string> topics, Action<object, MqttMsgPublishEventArgs> callback);

	Task SubscribeToTemporaryTopicsAsync(List<string> topics, TimeSpan stopSubscription, Action<object, MqttMsgPublishEventArgs> callback);

	Task CheckConnectionAsync();
}
