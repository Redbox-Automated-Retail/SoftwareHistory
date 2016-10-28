using System;
using System.Net.Sockets;

namespace Belikov.GenuineChannels.Utilities;

internal class SocketUtility
{
	private SocketUtility()
	{
	}

	public static void CloseSocket(object socketAsObject)
	{
		if (!(socketAsObject is Socket socket))
		{
			return;
		}
		try
		{
			socket.Shutdown(SocketShutdown.Both);
		}
		catch (Exception)
		{
		}
		try
		{
			socket.Close();
		}
		catch (Exception)
		{
		}
		try
		{
			((IDisposable)socket).Dispose();
		}
		catch (Exception)
		{
		}
	}
}
