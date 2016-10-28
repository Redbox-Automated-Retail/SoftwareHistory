using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.TransportContext;

[ComVisible(true)]
public interface ITransportContextProvider
{
	ITransportContext ITransportContext { get; }
}
