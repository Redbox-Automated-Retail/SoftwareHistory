using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using UpdateClientService.API.Services.IoT.Certificate;
using UpdateClientService.API.Services.IoT.Commands;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace UpdateClientService.API.Services.IoT;

public class AwsIoTMqttProxy : IMqttProxy
{
	private const int IoTCoreBrokerPort = 8883;

	private readonly ICertificateService _certService;

	private readonly ILogger<AwsIoTMqttProxy> _logger;

	private readonly IIotCommandDispatch _commandDispatch;

	private readonly IStoreService _store;

	private readonly AppSettings _appSettings;

	private MqttClient _iotClient;

	private Action _subscribedCallback;

	private readonly ConcurrentDictionary<ushort, Action> _msgPublishedEventPairs;

	private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private int _lostConnectionCount;

	private bool _isSubscribed;

	private int _inConnect;

	private IEnumerable<string> Topics => new List<string>
	{
		$"redbox/kiosk/{_store.KioskId}/command",
		"redbox/kiosk/market/" + _store.Market + "/command",
		"redbox/kiosk/all/command"
	};

	private bool IsMqttConnected => _iotClient?.IsConnected ?? false;

	public AwsIoTMqttProxy(ILogger<AwsIoTMqttProxy> logger, ICertificateService certService, IIotCommandDispatch commandDispatch, IStoreService store, IOptions<AppSettings> appSettings)
	{
		_logger = logger;
		_certService = certService;
		_commandDispatch = commandDispatch;
		_store = store;
		_appSettings = appSettings.Value;
		_msgPublishedEventPairs = new ConcurrentDictionary<ushort, Action>();
		_logger.LogInfoWithSource("AwsIotMqttProxy instantiated", ".ctor");
	}

	public async Task<bool> PublishIoTCommandAsync(string topicName, IoTCommandModel request)
	{
		return await PublishMessageAsync(topicName, request.ToJson(), request?.LogRequest ?? true, request.QualityOfServiceLevel);
	}

	public async Task<bool> PublishAwsRuleActionAsync(string ruleName, string payload, bool logMessage = true, QualityOfServiceLevel qualityOfServiceLevel = QualityOfServiceLevel.AtMostOnce)
	{
		return await PublishMessageAsync(ruleName, payload, logMessage, qualityOfServiceLevel);
	}

	private async Task<bool> PublishMessageAsync(string topicName, string message, bool logMessage = true, QualityOfServiceLevel qualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce)
	{
		bool result = false;
		if (!IsMqttConnected)
		{
			_logger.LogInfoWithSource("Not connected. Cannot publish to topic: " + topicName, "PublishMessageAsync");
			return false;
		}
		string text = (logMessage ? ("Message : " + message) : $"Message length: {message?.Length ?? 0}");
		_logger.LogInfoWithSource("Publishing message to topic: " + topicName + " ." + text, "PublishMessageAsync");
		try
		{
			string text2 = null;
			ushort messageId;
			if (qualityOfServiceLevel == QualityOfServiceLevel.AtMostOnce)
			{
				messageId = _iotClient.Publish(topicName, Encoding.UTF8.GetBytes(message), qualityOfServiceLevel.ToM2MqttQOS(), retain: false);
				result = true;
			}
			else
			{
				AsyncAutoResetEvent are = new AsyncAutoResetEvent(set: false);
				Action value = delegate
				{
					are.Set();
					result = true;
				};
				messageId = _iotClient.Publish(topicName, Encoding.UTF8.GetBytes(message), qualityOfServiceLevel.ToM2MqttQOS(), retain: false);
				_msgPublishedEventPairs.TryAdd(messageId, value);
				CancellationToken token = new CancellationTokenSource(5000).Token;
				Stopwatch timer = Stopwatch.StartNew();
				try
				{
					await are.WaitAsync(token);
				}
				catch (TaskCanceledException)
				{
					_logger.LogInformation($"Message {messageId} Timed Out");
				}
				timer.Stop();
				long elapsedMilliseconds = timer.ElapsedMilliseconds;
				_msgPublishedEventPairs.TryRemove(messageId, out var _);
				text2 = $",  Duration: {elapsedMilliseconds}";
			}
			string text3 = (result ? "Successfully" : "Failed to");
			_logger.LogInfoWithSource($"{text3} published messageId: {messageId}, Topic: {topicName}, QoS Level {qualityOfServiceLevel}{text2}", "PublishMessageAsync");
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "Exception while publishing message to topic " + topicName);
			result = false;
		}
		return result;
	}

	private async Task<bool> SubscribeTopics(IEnumerable<string> topics)
	{
		_logger.LogInfoWithSource("Attempting to subscribe to topics", "SubscribeTopics");
		_isSubscribed = false;
		if ((topics != null && topics.Count() == 0) || !IsMqttConnected)
		{
			return _isSubscribed;
		}
		AsyncManualResetEvent are = new AsyncManualResetEvent(set: false);
		_subscribedCallback = delegate
		{
			are.Set();
		};
		_logger.LogInfoWithSource("calling _iotClient.Subscribe for topics: " + topics.ToJson(), "SubscribeTopics");
		ushort messageId = _iotClient.Subscribe(topics.ToArray(), Enumerable.Repeat((byte)1, topics.Count()).ToArray());
		Stopwatch timer = Stopwatch.StartNew();
		_isSubscribed = !(await are.WaitAsyncAndDisposeOnFinish(TimeSpan.FromSeconds(60.0)));
		timer.Stop();
		if (_isSubscribed)
		{
			_logger.LogInfoWithSource($"Successfully subscribed to messageId: {messageId}, Topics: {topics.ToJson()}, Duration: {timer.ElapsedMilliseconds}", "SubscribeTopics");
		}
		else
		{
			_logger.LogErrorWithSource($"Couldn't get SUBACK for {timer.ElapsedMilliseconds} milli seconds. For topics: {topics.ToJson()}", "SubscribeTopics");
		}
		_subscribedCallback = null;
		return _isSubscribed;
	}

	public async Task<bool> CheckConnectionAsync()
	{
		if (Interlocked.CompareExchange(ref _inConnect, 1, 0) == 1)
		{
			_logger.LogError("Already in CheckConnectionAsync, exiting");
			return false;
		}
		try
		{
			if (IsMqttConnected)
			{
				return true;
			}
			return await Connect();
		}
		finally
		{
			_inConnect = 0;
		}
	}

	private async Task<bool> Connect()
	{
		_logger.LogInfoWithSource($"Attempting to connect KioskId: {_store.KioskId} to mqtt", "Connect");
		try
		{
			Clear();
			await ConnectToMqtt();
			bool isMqttConnected = IsMqttConnected;
			bool flag = isMqttConnected;
			bool flag2 = flag;
			if (flag2)
			{
				flag2 = await SubscribeTopics(Topics);
			}
			isMqttConnected = flag2;
			bool flag3 = isMqttConnected;
			bool flag4 = flag3;
			if (flag4)
			{
				flag4 = await RegisterUCSVersion();
			}
			return flag4;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured while attempting to connect MqttClient.");
			return false;
		}
	}

	public async Task Disconnect()
	{
		try
		{
			_iotClient?.Disconnect();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured while attempting to disconnect from MqttClient, this can happen if the connection lost already.");
		}
		Clear();
	}

	private void Clear()
	{
		_iotClient = null;
		_subscribedCallback = null;
		_isSubscribed = false;
	}

	private async Task ConnectToMqtt()
	{
		_logger.LogInfoWithSource("Attempting to connect to mqtt", "ConnectToMqtt");
		IotCert iotCert = await _certService.GetCertificateAsync();
		if (iotCert == null)
		{
			_logger.LogErrorWithSource("No cert, aborting connection attempt", "ConnectToMqtt");
			return;
		}
		try
		{
			_iotClient = new MqttClient(_appSettings?.IoTBrokerEndpoint, 8883, secure: true, iotCert.RootCa, iotCert.DeviceCertPfx, MqttSslProtocols.TLSv1_2);
			_iotClient.ConnectionClosed += _iotClient_ConnectionClosed;
			_iotClient.MqttMsgPublishReceived += TopicCallbackDistributor;
			_iotClient.MqttMsgSubscribed += _iotClient_MqttMsgSubscribed;
			_iotClient.MqttMsgUnsubscribed += _iotClient_MqttMsgUnsubscribed;
			_iotClient.MqttMsgPublished += _iotClient_MqttMsgPublished;
			_iotClient.Connect(_store.KioskId.ToString());
			_logger.LogInfoWithSource($"MqttClient created and connected with KioskId: {_store.KioskId}", "ConnectToMqtt");
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occurred in ConnectToMqtt");
			await _certService.Validate();
		}
	}

	private async Task<bool> RegisterUCSVersion()
	{
		_logger.LogInfoWithSource("Attempting to register ucs", "RegisterUCSVersion");
		UCSRegisterMessage message = new UCSRegisterMessage
		{
			KioskId = _store.KioskId,
			Version = "2.0",
			AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
		};
		bool flag = await PublishMessageAsync("$aws/rules/kioskucsregister", message.ToJson());
		string text = (flag ? "succeeded" : "failed");
		_logger.LogInfoWithSource("Register UCS version " + message.Version + " " + text, "RegisterUCSVersion");
		return flag;
	}

	private void TopicCallbackDistributor(object sender, MqttMsgPublishEventArgs e)
	{
		string text = Encoding.ASCII.GetString(e?.Message);
		_logger.LogInfoWithSource($"message received topic: {e?.Topic}, message length: {text?.Length ?? 0}", "TopicCallbackDistributor");
		Task.Run(() => _commandDispatch.Execute(e?.Message, e?.Topic));
	}

	private void _iotClient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
	{
		Task.Run(delegate
		{
			_logger.LogInfoWithSource($"SUBACK has received messageId: {e.MessageId}", "_iotClient_MqttMsgSubscribed");
			_subscribedCallback?.Invoke();
		});
	}

	private void _iotClient_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
	{
		_logger.LogInfoWithSource($"Topics have unsubscribed - messageId: {e.MessageId}", "_iotClient_MqttMsgUnsubscribed");
	}

	private void _iotClient_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
	{
		Task.Run(async delegate
		{
			await MessageHasPublished(e);
		});
	}

	private async Task MessageHasPublished(MqttMsgPublishedEventArgs e, int retry = 3)
	{
		int attempt = 0;
		while (attempt++ < retry && !_msgPublishedEventPairs.ContainsKey(e.MessageId))
		{
			_logger.LogWarningWithSource(string.Format("Couldn't find messageId: {0} in {1}, Attempt: {2}", e.MessageId, "_msgPublishedEventPairs", attempt), "MessageHasPublished");
			await Task.Delay(300);
		}
		if (!e.IsPublished)
		{
			_logger.LogErrorWithSource(string.Format("Couldn't publish messageId: {0} in {1}, Attempt: {2}", e.MessageId, "_msgPublishedEventPairs", attempt), "MessageHasPublished");
			return;
		}
		_logger.LogInfoWithSource($"PUBACK has received messageId: {e.MessageId} Attempt: {attempt}", "MessageHasPublished");
		_msgPublishedEventPairs[e.MessageId]();
	}

	private async void _iotClient_ConnectionClosed(object sender, EventArgs e)
	{
		_logger.LogInfoWithSource("Received Connection Closed Event", "_iotClient_ConnectionClosed");
	}
}
