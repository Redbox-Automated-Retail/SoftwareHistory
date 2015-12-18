using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

public sealed class IPCProxy : IDisposable
{
	private IPCServiceHost m_listener;

	private bool Disposed;

	private readonly IPCProtocol Protocol;

	private readonly ManualResetEvent m_waitObject = new ManualResetEvent(initialState: false);

	public bool ProtocolValid => Protocol != null;

	public static IPCProxy Parse(string protocol)
	{
		IPCProtocol iPCProtocol = null;
		try
		{
			iPCProtocol = IPCProtocol.Parse(protocol);
		}
		catch (UriFormatException e)
		{
			LogHelper.Instance.Log("ProtocolURI is malformed.", e);
			iPCProtocol = null;
		}
		return new IPCProxy(iPCProtocol);
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			StopListener();
		}
	}

	public void StartListener()
	{
		if (Protocol == null)
		{
			throw new Exception("Expected a parsed protocol.");
		}
		if (Protocol.Channel == ChannelType.NamedPipe)
		{
			RunPipeListener();
		}
		else
		{
			ExecuteIPCListener();
		}
	}

	public void StopListener()
	{
		if (m_listener != null)
		{
			m_listener.Stop();
		}
		m_waitObject.Set();
	}

	public bool ExecuteStartupFile(string serviceStartup)
	{
		if (!File.Exists(serviceStartup))
		{
			return true;
		}
		using StreamReader reader = new StreamReader(serviceStartup);
		BatchCommandRunner batchCommandRunner = new BatchCommandRunner(reader);
		batchCommandRunner.Start();
		return true;
	}

	private void ExecuteIPCListener()
	{
		m_listener = IPCServiceHost.Create(Protocol, GetProductName(typeof(ProgramCommand).Assembly), GetVersion(typeof(ProgramCommand).Assembly), GetCopyright(typeof(ProgramCommand).Assembly), Environment.ProcessorCount, Environment.ProcessorCount << 3);
		m_listener.Start();
		m_waitObject.WaitOne();
	}

	private string GetProductName(Assembly assembly)
	{
		return ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute)))?.Product;
	}

	private string GetCopyright(Assembly assembly)
	{
		return ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)))?.Copyright;
	}

	private string GetVersion(Assembly assembly)
	{
		return assembly.GetName().Version.ToString();
	}

	private void RunPipeListener()
	{
		m_listener = new NamedPipeServiceHost();
		m_listener.Start();
	}

	private IPCProxy(IPCProtocol protocol)
	{
		Protocol = protocol;
	}
}
