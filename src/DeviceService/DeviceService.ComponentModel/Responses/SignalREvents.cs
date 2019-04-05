namespace DeviceService.ComponentModel.Responses;

public class SignalREvents
{
	public const string Event = "Event";

	public const string BaseEvent = "BaseEvent";

	public const string SimpleEvent = "SimpleEvent";

	public const string IsConnectedResponseEvent = "IsConnectedResponseEvent";

	public const string GetUnitHealthResponseEvent = "GetUnitHealthResponseEvent";

	public const string UnencryptedCardReadResponseEvent = "UnencryptedCardReadResponseEvent";

	public const string EncryptedCardReadResponseEvent = "EncryptedCardReadResponseEvent";

	public const string EMVCardReadResponseEvent = "EMVCardReadResponseEvent";

	public const string ReadConfiguration = "ReadConfiguration";

	public const string WriteConfiguration = "WriteConfiguration";

	public const string CardReaderConnectedEvent = "CardReaderConnectedEvent";

	public const string CardReaderDisconnectedEvent = "CardReaderDisconnectedEvent";

	public const string TimeoutResponseEvent = "TimeoutResponseEvent";

	public const string CancelCommandResponseEvent = "CancelCommandResponseEvent";

	public const string RebootCardReaderResponseEvent = "RebootCardReaderResponseEvent";

	public const string ReportAuthorizationResultResponseEvent = "ReportAuthorizationResultResponseEvent";

	public const string ValidateVersionReponseEvent = "ValidateVersionReponseEvent";
}
