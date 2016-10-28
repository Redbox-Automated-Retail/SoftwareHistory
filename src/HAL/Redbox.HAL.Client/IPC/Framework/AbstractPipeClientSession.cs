using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.HAL.IPC.Framework;

internal abstract class AbstractPipeClientSession : IDisposable
{
	protected readonly StringBuilder ReadBuilder = new StringBuilder();

	protected readonly string ID;

	private readonly BasePipeChannel Channel;

	private int? m_timeout;

	public int Timeout
	{
		get
		{
			return m_timeout ?? 30000;
		}
		set
		{
			m_timeout = value;
		}
	}

	public bool IsConnected => Channel.Connected;

	protected bool Disposed { get; private set; }

	public void Dispose()
	{
		DisposeInner(disposing: true);
	}

	public bool WriteCommand(string s)
	{
		return Write(s);
	}

	public bool Connect()
	{
		try
		{
			ConnectThrowOnError();
			return true;
		}
		catch (IOException)
		{
			return false;
		}
	}

	public void ConnectThrowOnError()
	{
		Channel.Connect();
		if (!Channel.Connected)
		{
			throw new TimeoutException("Failed to connect to pipe server: maybe it's not started?");
		}
		ConsumeMessages();
	}

	protected abstract List<string> ProcessRawMessage();

	protected AbstractPipeClientSession(string id, string pipeName)
	{
		Channel = new ClientPipeChannel(pipeName);
		ID = id;
	}

	protected List<string> ConsumeMessages()
	{
		StringBuildExtension.Reset(ReadBuilder);
		string value = Encoding.ASCII.GetString(Channel.ReceiveMessage());
		ReadBuilder.Append(value);
		return ProcessRawMessage();
	}

	protected bool Write(string s)
	{
		if (!Channel.Connected)
		{
			return false;
		}
		if (!s.EndsWith(Environment.NewLine))
		{
			s = $"{s}{Environment.NewLine}";
		}
		return Channel.SendMessage(Encoding.ASCII.GetBytes(s));
	}

	protected bool BufferHasMoreLines()
	{
		return ReadBuilder.ToString().IndexOf(Environment.NewLine) == -1;
	}

	protected void DisposeInner(bool disposing)
	{
		if (Disposed)
		{
			return;
		}
		try
		{
			Write("quit");
		}
		catch (IOException)
		{
		}
		finally
		{
			Channel.Dispose();
			Disposed = true;
		}
	}
}
