using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

public class IPCProxy
{
	private IPCServiceHost m_listener;

	private ManualResetEvent m_waitObject = new ManualResetEvent(initialState: false);

	public static IPCProxy Instance => Singleton<IPCProxy>.Instance;

	public IPCProtocol Protocol { get; private set; }

	public void StartListener()
	{
		if (Protocol == null)
		{
			throw new Exception("Expected a parsed protocol.");
		}
		LogHelper.Instance.Log($"Starting server listener on channel type {Protocol.Channel.ToString()}");
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

	public bool ParseProtocol(string protocol)
	{
		try
		{
			Protocol = IPCProtocol.Parse(protocol);
			return true;
		}
		catch (UriFormatException e)
		{
			LogHelper.Instance.Log("ProtocolURI is malformed; please fix. HAL service is exiting.", e);
			Protocol = null;
			return false;
		}
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

	private IPCProxy()
	{
	}
}
