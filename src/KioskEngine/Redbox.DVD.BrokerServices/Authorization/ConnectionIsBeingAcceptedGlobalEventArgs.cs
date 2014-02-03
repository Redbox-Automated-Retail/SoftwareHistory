using System;
using System.Net;

namespace Belikov.GenuineChannels.Authorization;

internal class ConnectionIsBeingAcceptedGlobalEventArgs : EventArgs
{
	public bool Cancel = false;

	public IPEndPoint IPEndPoint;

	public string Url;

	public string LoginName;

	public byte[] HashedPassword;

	public object ProvidedCredentials;
}
