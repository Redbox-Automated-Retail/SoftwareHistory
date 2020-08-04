using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using UpdateClientService.API.Services.IoT.Certificate;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace UpdateClientService.API.Services.IoT;

public class AwsIoTMqttProxy : IMqttProxy
{
	private const int IoTCoreBrokerPort = 8883;

	private readonly ICertificateService _certService;

	private readonly ILogger<AwsIoTMqttProxy> _logger;

	private readonly IStoreService _store;

	private readonly AppSettings _appSettings;

	private MqttClient _iotClient;

	private readonly Dictionary<string, Action<object, MqttMsgPublishEventArgs>> _topicCallbackAssignedPairs;

	private readonly ConcurrentDictionary<ushort, Action> _msgSubscribedEventPairs;

	private readonly List<List<string>> _cachedTopics;

	private readonly List<TemporarySubscriptionTopics> _tempTopics;

	public int LastAvailableUSInstance { get; set; } = 4;

	public AwsIoTMqttProxy(ILogger<AwsIoTMqttProxy> logger, ICertificateService certService, IStoreService store, IOptions<AppSettings> appSettings)
	{
		_logger = logger;
		_certService = certService;
		_store = store;
		_appSettings = appSettings.Value;
		_topicCallbackAssignedPairs = new Dictionary<string, Action<object, MqttMsgPublishEventArgs>>();
		_msgSubscribedEventPairs = new ConcurrentDictionary<ushort, Action>();
		_cachedTopics = new List<List<string>>();
		_tempTopics = new List<TemporarySubscriptionTopics>();
	}

	public async Task CheckConnectionAsync()
	{
		await IfConnectionLostReConnectAsync();
		StopExpiredTemporarySubscriptions();
	}

	public async Task PublishMessageAsync(string topicName, string message)
	{
		await IfConnectionLostReConnectAsync();
		_logger.LogInfoWithSource("Publishing message to topic: " + topicName + " . message: " + message, "PublishMessageAsync");
		_iotClient.Publish(topicName, Encoding.UTF8.GetBytes(message));
	}

	public async Task SubscribeToTopicsAsync(List<string> topics, Action<object, MqttMsgPublishEventArgs> callback)
	{
		foreach (string topic in topics)
		{
			if (!_topicCallbackAssignedPairs.ContainsKey(topic))
			{
				_topicCallbackAssignedPairs.Add(topic, callback);
			}
		}
		_cachedTopics.Add(topics);
		await IfConnectionLostReConnectAsync();
		await WaitUntilTopicsSubscribed(topics);
	}

	public async Task SubscribeToTemporaryTopicsAsync(List<string> topics, TimeSpan stopSubscription, Action<object, MqttMsgPublishEventArgs> callback)
	{
		foreach (string topic in topics)
		{
			if (!_topicCallbackAssignedPairs.ContainsKey(topic))
			{
				_topicCallbackAssignedPairs.Add(topic, callback);
			}
		}
		await IfConnectionLostReConnectAsync();
		await WaitUntilTopicsSubscribed(topics);
		TemporarySubscriptionTopics temporarySubscriptionTopics;
		_tempTopics.Add(temporarySubscriptionTopics = new TemporarySubscriptionTopics
		{
			ExpireTime = DateTime.Now.Add(stopSubscription),
			Topics = topics
		});
		_logger.LogInfoWithSource($"Topics to: {topics.ToJson()}. StopSubscription time: {temporarySubscriptionTopics.ExpireTime}", "SubscribeToTemporaryTopicsAsync");
	}

	private async Task WaitUntilTopicsSubscribed(List<string> topics)
	{
		AsyncAutoResetEvent are = new AsyncAutoResetEvent(set: false);
		Action value = delegate
		{
			are.Set();
		};
		_logger.LogInfoWithSource("calling _iotClient.Subscribe for topics: " + topics.ToJson(), "WaitUntilTopicsSubscribed");
		ushort messageId = _iotClient.Subscribe(topics.ToArray(), Enumerable.Repeat((byte)1, topics.Count).ToArray());
		_msgSubscribedEventPairs.TryAdd(messageId, value);
		Stopwatch timer = Stopwatch.StartNew();
		await are.WaitAsync();
		timer.Stop();
		_logger.LogInfoWithSource($"Successfully subscribed to messageId: {messageId} . Topics: {topics.ToJson()} . It took {timer.ElapsedMilliseconds} ms to subscribe", "WaitUntilTopicsSubscribed");
		_msgSubscribedEventPairs.TryRemove(messageId, out var _);
	}

	private async Task IfConnectionLostReConnectAsync()
	{
		try
		{
			_logger.LogInfoWithSource($"isconnected: {_iotClient?.IsConnected}", "IfConnectionLostReConnectAsync");
			MqttClient iotClient = _iotClient;
			if (iotClient == null || !iotClient.IsConnected)
			{
				_logger.LogInfoWithSource("connection was lost reconnecting", "IfConnectionLostReConnectAsync");
				await NewUpMqttClientAndReconnectAsync();
				ReSubscribeToCachedTopics();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error occured.");
		}
	}

	private void ReSubscribeToCachedTopics()
	{
		foreach (List<string> cachedTopic in _cachedTopics)
		{
			_iotClient.Subscribe(cachedTopic.ToArray(), Enumerable.Repeat((byte)1, cachedTopic.Count).ToArray());
		}
	}

	private void TopicCallbackDistributor(object sender, MqttMsgPublishEventArgs e)
	{
		_logger.LogInfoWithSource("message received topic: " + e.Topic + ", message: " + Encoding.ASCII.GetString(e.Message), "TopicCallbackDistributor");
		if (!_topicCallbackAssignedPairs.ContainsKey(e.Topic))
		{
			_logger.LogWarningWithSource("Couldn't found topic: " + e.Topic + " in topic callback assigned pairs list. Don't know which method to call", "TopicCallbackDistributor");
			return;
		}
		Task.Run(delegate
		{
			_topicCallbackAssignedPairs[e.Topic](sender, e);
		});
	}

	private void StopExpiredTemporarySubscriptions()
	{
		List<TemporarySubscriptionTopics> list = _tempTopics.Where((TemporarySubscriptionTopics x) => x.ExpireTime < DateTime.Now).ToList();
		foreach (TemporarySubscriptionTopics item in list)
		{
			if (!(item.ExpireTime < DateTime.Now))
			{
				continue;
			}
			_iotClient?.Unsubscribe(item.Topics.ToArray());
			_tempTopics.Remove(item);
			foreach (string topic in item.Topics)
			{
				if (_topicCallbackAssignedPairs.ContainsKey(topic))
				{
					_topicCallbackAssignedPairs.Remove(topic);
				}
			}
		}
	}

	private async Task NewUpMqttClientAndReconnectAsync()
	{
		(X509Certificate, X509Certificate2) tuple = await _certService.GetCertificatesAsync();
		X509Certificate item = tuple.Item1;
		X509Certificate2 item2 = tuple.Item2;
		_iotClient = new MqttClient(_appSettings.IoTBrokerEndpoint, 8883, secure: true, item, item2, MqttSslProtocols.TLSv1_2);
		_logger.LogInfoWithSource($"Newed up MqttClient object: {_iotClient}", "NewUpMqttClientAndReconnectAsync");
		_logger.LogInfoWithSource($"ClientId: {_store.KioskId}", "NewUpMqttClientAndReconnectAsync");
		try
		{
			_iotClient.Connect(_store.KioskId.ToString());
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "NewUpMqttClientAndReconnectAsync");
			await _certService.DeleteLocalCertsIfNotValidAsync();
		}
		_iotClient.ConnectionClosed += _iotClient_ConnectionClosed;
		_iotClient.MqttMsgPublishReceived += TopicCallbackDistributor;
		_iotClient.MqttMsgSubscribed += _iotClient_MqttMsgSubscribed;
	}

	private void _iotClient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
	{
		Task.Run(delegate
		{
			TopicsHasSubscribed(e);
		});
	}

	private async Task TopicsHasSubscribed(MqttMsgSubscribedEventArgs e, int retry = 0)
	{
		await Task.Delay(300);
		if (!_msgSubscribedEventPairs.ContainsKey(e.MessageId))
		{
			_logger.LogInfoWithSource(string.Format("Couldn't find messageId: {0} in {1} . RetryCount: {2}", e.MessageId, "_msgSubscribedEventPairs", retry), "TopicsHasSubscribed");
			if (retry < 3)
			{
				AwsIoTMqttProxy awsIoTMqttProxy = this;
				int num = retry + 1;
				retry = num;
				awsIoTMqttProxy.TopicsHasSubscribed(e, num);
			}
		}
		else
		{
			_logger.LogInfoWithSource($"SUBACK has received messageId: {e.MessageId} retryCount: {retry}", "TopicsHasSubscribed");
			_msgSubscribedEventPairs[e.MessageId]();
		}
	}

	private async void _iotClient_ConnectionClosed(object sender, EventArgs e)
	{
		try
		{
			_logger.LogWarningWithSource("Something went wrong lost connection. Reconnecting...", "_iotClient_ConnectionClosed");
			await IfConnectionLostReConnectAsync();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "_iotClient_ConnectionClosed");
		}
	}
}
