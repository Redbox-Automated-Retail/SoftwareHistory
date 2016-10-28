using System.Runtime.InteropServices;

namespace NetFwTypeLib;

[ComVisible(true)]
public enum NET_FW_SCOPE_
{
	NET_FW_SCOPE_ALL,
	NET_FW_SCOPE_LOCAL_SUBNET,
	NET_FW_SCOPE_CUSTOM,
	NET_FW_SCOPE_MAX
}
