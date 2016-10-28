using System.Runtime.InteropServices;

namespace NetFwTypeLib;

[ComVisible(true)]
public enum NET_FW_IP_VERSION_
{
	NET_FW_IP_VERSION_V4,
	NET_FW_IP_VERSION_V6,
	NET_FW_IP_VERSION_ANY,
	NET_FW_IP_VERSION_MAX
}
