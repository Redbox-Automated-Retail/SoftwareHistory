using System;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

[Flags]
internal enum Features : short
{
	None = 0,
	MultiDiscVend = 1
}
