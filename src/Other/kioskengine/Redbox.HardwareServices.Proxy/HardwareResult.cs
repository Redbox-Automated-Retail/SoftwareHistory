using Redbox.HardwareServices.Proxy.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

internal class HardwareResult : IHardwareResult
{
	public int Deck { get; internal set; }

	public int Slot { get; internal set; }

	public string ID { get; internal set; }

	public string Code { get; internal set; }

	public string ItemID { get; internal set; }

	public string Message { get; internal set; }

	public string MetaData { get; internal set; }

	public string Timestamp { get; internal set; }

	public string PreviousItem { get; internal set; }

	public string ReturnTime { get; internal set; }
}
