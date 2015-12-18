using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client;

public class ClientHelper
{
	public delegate void InitComplete();

	private ManualResetEvent m_resetEvent = new ManualResetEvent(initialState: false);

	public HardwareService Service { get; private set; }

	private IClientOutputSink Sink { get; set; }

	public bool BootstrapInitRunning()
	{
		if (Service == null)
		{
			Sink.WriteMessage("The service is not configured.");
			return false;
		}
		HardwareCommandResult initStatus = Service.GetInitStatus(out var status);
		if (!initStatus.Success)
		{
			Sink.WriteMessage("Unable to determine init status.");
			return false;
		}
		if ("COMPLETED" == status || "ERRORED" == status)
		{
			Sink.WriteMessage("Boot init ended with status: {0}", status);
			return false;
		}
		return true;
	}

	public bool WaitforInit(int pause)
	{
		return WaitForInitInner(pause);
	}

	public bool WaitforInit()
	{
		return WaitForInitInner(5000);
	}

	public bool WaitforInit(InitComplete onDone)
	{
		bool result = WaitForInitInner(5000);
		onDone?.Invoke();
		return result;
	}

	public HardwareService Connect()
	{
		return ConnectInner(Constants.HALIPCStrings.TcpServer);
	}

	public HardwareService Connect(string ipc)
	{
		if (string.IsNullOrEmpty(ipc))
		{
			throw new UriFormatException("The URI string is null or empty - please re-configure.");
		}
		return ConnectInner(ipc);
	}

	public HardwareService AutoConnect()
	{
		if (Service != null)
		{
			return Service;
		}
		Service = ConnectInner(Constants.HALIPCStrings.TcpServer);
		if (Service != null)
		{
			LogHelper.Instance.Log("Autoconnect: found server running on {0}", Constants.HALIPCStrings.TcpServer);
			return Service;
		}
		string text = FindLocalIP();
		if (string.IsNullOrEmpty(text))
		{
			return Service = null;
		}
		string text2 = $"rcp://{text}:7001";
		Service = ConnectInner(text2);
		if (Service != null)
		{
			LogHelper.Instance.Log("Autoconnect: found server running on {0}", text2);
		}
		return Service;
	}

	public bool TestCommunication()
	{
		if (Service == null)
		{
			ConnectInner(Constants.HALIPCStrings.TcpServer);
		}
		return TestServiceConnection(Service);
	}

	public bool WaitForJob(HardwareJob job, out HardwareJobStatus endStatus)
	{
		endStatus = HardwareJobStatus.Completed;
		try
		{
			HardwareJobStatus _s0 = HardwareJobStatus.Completed;
			bool waitForJob = true;
			job.StatusChanged += delegate(HardwareJob j, HardwareJobStatus status)
			{
				_s0 = status;
				waitForJob = status != HardwareJobStatus.Stopped && status != HardwareJobStatus.Errored && status != HardwareJobStatus.Garbage && status != HardwareJobStatus.Completed && status != HardwareJobStatus.Suspended;
			};
			job.Connect();
			job.Pend();
			while (waitForJob)
			{
				m_resetEvent.WaitOne(250, exitContext: false);
			}
			endStatus = _s0;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
		finally
		{
			job.Disconnect();
		}
	}

	public bool ExecuteImmediate(string inst)
	{
		if (Service == null)
		{
			return false;
		}
		HardwareJob job;
		return Service.ExecuteImmediate(inst, out job).Success;
	}

	public T ExecuteImmediateAndGetResult<T>(HardwareService service, string inst)
	{
		HardwareCommandResult hardwareCommandResult = service.ExecuteImmediate(inst, out var job);
		if (hardwareCommandResult == null || !hardwareCommandResult.Success)
		{
			return default(T);
		}
		string[] stackEntriesInner = GetStackEntriesInner(job, 1);
		return (T)ConversionHelper.ChangeType(stackEntriesInner[0], typeof(T));
	}

	public ClientHelper(IClientOutputSink sink)
	{
		Sink = ((sink == null) ? NullSink.Instance : sink);
	}

	public ClientHelper()
	{
		Sink = NullSink.Instance;
	}

	public ClientHelper(HardwareService service)
	{
		Sink = NullSink.Instance;
		Service = service;
	}

	private bool WaitForInitInner(int pause)
	{
		Sink.WriteMessage("Bootstrap init is running...");
		while (true)
		{
			Thread.Sleep(pause);
			if (!BootstrapInitRunning())
			{
				break;
			}
			Sink.WriteMessage("{0}: still waiting for bootstrap init.", DateTime.Now.ToLongTimeString());
		}
		Sink.WriteMessage("Init done");
		return true;
	}

	private HardwareService ConnectInner(string ipcUrl)
	{
		IPCProtocol iPCProtocol = null;
		try
		{
			iPCProtocol = IPCProtocol.Parse(ipcUrl);
			HardwareService hardwareService = new HardwareService(iPCProtocol);
			return TestServiceConnection(hardwareService) ? hardwareService : null;
		}
		catch (UriFormatException)
		{
			return Service = null;
		}
	}

	private bool TestServiceConnection(HardwareService service)
	{
		if (service == null)
		{
			return false;
		}
		HardwareCommandResult hardwareCommandResult = service.ExecuteServiceCommand("SERVICE test-comm", 5000);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		return hardwareCommandResult.CommandMessages[0] == "ACK";
	}

	private string FindLocalIP()
	{
		IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress[] addressList = hostEntry.AddressList;
		foreach (IPAddress iPAddress in addressList)
		{
			if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				return iPAddress.ToString();
			}
		}
		return null;
	}

	private string[] GetStackEntriesInner(HardwareJob job, int depth)
	{
		string[] array = new string[depth];
		for (int i = 0; i < depth; i++)
		{
			array[i] = string.Empty;
		}
		HardwareCommandResult stack = job.GetStack(out var stack2);
		if (stack.Success && stack2.Count >= depth)
		{
			for (int j = 0; j < depth; j++)
			{
				array[j] = stack2.Pop();
			}
		}
		return array;
	}
}
