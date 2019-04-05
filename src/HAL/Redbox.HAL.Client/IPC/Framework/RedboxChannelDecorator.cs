using System;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal static class RedboxChannelDecorator
{
	internal static bool Write(IIPCChannel channel, string line)
	{
		if (!line.EndsWith(Environment.NewLine))
		{
			line = $"{line}{Environment.NewLine}";
		}
		byte[] bytes = Encoding.ASCII.GetBytes(line);
		bool flag = channel.Write(bytes);
		if (flag)
		{
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, bytes.Length);
		}
		return flag;
	}
}
