namespace Redbox.HardwareServices.Proxy.ComponentModel;

internal interface IHardwareResult
{
	string Code { get; }

	int Deck { get; }

	int Slot { get; }

	string ItemID { get; }

	string ID { get; }

	string MetaData { get; }

	string Message { get; }

	string Timestamp { get; }

	string PreviousItem { get; }

	string ReturnTime { get; }
}
