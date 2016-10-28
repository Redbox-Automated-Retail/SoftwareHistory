using System;
using System.Net;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Authorization;

[ComVisible(true)]
public class ConnectionIsBeingAcceptedGlobalEventArgs : EventArgs
{
	public bool Cancel = false;

	public IPEndPoint IPEndPoint;

	public string Url;

	public string LoginName;

	public byte[] HashedPassword;

	public object ProvidedCredentials;
}
