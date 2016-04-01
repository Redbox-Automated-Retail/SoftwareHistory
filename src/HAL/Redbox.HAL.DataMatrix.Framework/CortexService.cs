using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CortexService : ScannerAdapter, IScannerDevice, IDisposable, IConfigurationObserver
{
	private RedboxSerialPort Port;

	private string ConfiguredPortName;

	private bool RebootOnStart;

	private CortexStartupAction StartupAction;

	private bool SnapOnDecodeFailure;

	private bool LogDetailedScan;

	private int WakeupPause;

	private readonly bool HardCycleOnReboot;

	private readonly PortModes Mode;

	private readonly RedboxSerialPortConfiguration Configuration;

	public bool RequiresExternalLighting => false;

	public bool IsConnected { get; private set; }

	public bool InError => false;

	public string Name => Service.ToString();

	public bool Restart()
	{
		if (HardCycleOnReboot)
		{
			Stop();
			PerfFunctions.Wait(500);
			return Start();
		}
		return RestartDecoder();
	}

	public bool TestConfiguration(IMessageSink sink)
	{
		CortexSettingsValidator cortexSettingsValidator = new CortexSettingsValidator(this);
		return cortexSettingsValidator.Validate(sink);
	}

	public bool ResetDeviceDriver()
	{
		return false;
	}

	public IScanResult Scan()
	{
		if (!IsConnected)
		{
			LogHelper.Instance.Log("[Cortex Service] Scan is called without initialization.");
			ScanResult scanResult = new ScanResult();
			scanResult.ResetOnException();
			return scanResult;
		}
		return Scan(SnapOnDecodeFailure);
	}

	public ISnapResult Snap()
	{
		return Snap(GenerateSnapFileName());
	}

	public ISnapResult Snap(string fileName)
	{
		if (!IsConnected)
		{
			LogHelper.Instance.Log("[Cortex Service] Snap: camera is not connected.");
			return new SnapResult(fileName);
		}
		return SnapInner(fileName);
	}

	public bool Start()
	{
		if (IsConnected)
		{
			return true;
		}
		if (ConfiguredPortName == "NONE")
		{
			LogHelper.Instance.Log("[Cortex Service] There is no configured port.");
			return false;
		}
		Port = PortManager.Instance.Scan(ConfiguredPortName, Configuration, TestPort, Mode);
		if (Port == null)
		{
			LogHelper.Instance.Log("[Cortex Service] Unable to open communcation port.");
			return false;
		}
		if (RebootOnStart)
		{
			RestartDecoder();
		}
		InfaredLightCommand infaredLightCommand = new InfaredLightCommand();
		infaredLightCommand.Send(Port);
		switch (StartupAction)
		{
		case CortexStartupAction.Decode:
		{
			LogHelper.Instance.Log("[Cortex Service] Scan on startup.");
			IScanResult scanResult = Scan(snapOnFailure: false);
			IsConnected = !scanResult.DeviceError;
			break;
		}
		case CortexStartupAction.Snap:
		{
			LogHelper.Instance.Log("[Cortex Service] Snap on startup.");
			ISnapResult snapResult = SnapInner(GenerateSnapFileName());
			IsConnected = snapResult.SnapOk;
			break;
		}
		default:
			IsConnected = true;
			break;
		}
		return IsConnected;
	}

	public bool Stop()
	{
		if (!IsConnected)
		{
			return true;
		}
		if (Port != null)
		{
			Port.Dispose();
			Port = null;
		}
		IsConnected = false;
		return true;
	}

	public string FindInstalledPort()
	{
		RedboxSerialPort redboxSerialPort = PortManager.Instance.Scan(Configuration, TestPort, Mode);
		if (redboxSerialPort == null)
		{
			return "NONE";
		}
		string name = redboxSerialPort.Name;
		LogHelper.Instance.Log("[Cortex Service] FindInstalledPort: found port {0}", name);
		redboxSerialPort.Dispose();
		return name;
	}

	public void NotifyConfigurationLoaded()
	{
		NotifyConfigurationChangeEnd();
	}

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
		Configuration.OpenWait = BarcodeConfiguration.Instance.CortexPortOpenWait;
		Configuration.WritePause = BarcodeConfiguration.Instance.WritePause;
		ConfiguredPortName = BarcodeConfiguration.Instance.SnapDecodePort;
		RebootOnStart = BarcodeConfiguration.Instance.RebootOnStart;
		StartupAction = BarcodeConfiguration.Instance.CortexStartupAction;
		SnapOnDecodeFailure = BarcodeConfiguration.Instance.CortexSnapOnDecodeFailure;
		LogDetailedScan = BarcodeConfiguration.Instance.LogDetailedScan;
		WakeupPause = BarcodeConfiguration.Instance.ScannerWakeupPause;
	}

	protected override void DisposeInner()
	{
		Stop();
	}

	internal CortexService()
		: base(ScannerServices.Cortex)
	{
		IPersistentMapService service = ServiceLocator.Instance.GetService<IPersistentMapService>();
		IPersistentMap map = service.GetMap();
		IRuntimeService service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
		Configuration = new RedboxSerialPortConfiguration();
		int value = map.GetValue("CortexReadBuffer", service2.PageSize << 1);
		Configuration.ReceiveBufferSize = value;
		LogHelper.Instance.Log("[CortexService] Receive buffer size = {0}", value);
		Configuration.WriteTimeout = 2000;
		Configuration.WriteTerminator = null;
		Mode = map.GetValue("CortexPortMode", PortModes.Callback);
		HardCycleOnReboot = map.GetValue("CortexHardCycle", def: true);
	}

	internal RedboxSerialPort ConfiguredPort()
	{
		return Port;
	}

	private IScanResult Scan(bool snapOnFailure)
	{
		ScanResult scanResult = new ScanResult();
		DecodeCommand decodeCommand = new DecodeCommand();
		decodeCommand.Send(Port);
		if (decodeCommand.PortError)
		{
			LogHelper.Instance.Log("[Cortex Service] Scan: There was a communication error with the device.");
			scanResult.ResetOnException();
			scanResult.DeviceError = true;
			return scanResult;
		}
		scanResult.Add(decodeCommand.FoundCodes);
		InsertStat(scanResult, clearImage: false, decodeCommand.ExecutionTime.TotalSeconds);
		if (scanResult.DecodeResults.Count == 0 && snapOnFailure)
		{
			ISnapResult snapResult = Snap();
			if (!snapResult.SnapOk)
			{
				LogHelper.Instance.Log("[CortexService] Failed to take image = {0}", snapResult.Path);
			}
		}
		scanResult.ExecutionTime = decodeCommand.ExecutionTime;
		return scanResult;
	}

	private bool RestartDecoder()
	{
		RebootCommand rebootCommand = new RebootCommand();
		rebootCommand.Send(Port);
		PerfFunctions.Wait(WakeupPause);
		return true;
	}

	private ISnapResult SnapInner(string fileName)
	{
		TakePictureCommand takePictureCommand = new TakePictureCommand();
		takePictureCommand.Send(Port);
		if (takePictureCommand.PortError)
		{
			LogHelper.Instance.Log("[CortexService]: There was a communication error.");
			return new SnapResult(fileName);
		}
		ImagePacket packet = null;
		CortexPacket[] parsedPackets = takePictureCommand.ParsedPackets;
		foreach (CortexPacket cortexPacket in parsedPackets)
		{
			if (cortexPacket.RawPacketType == 'm')
			{
				packet = new ImagePacket(cortexPacket);
			}
		}
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			GetFileCommand getFileCommand = new GetFileCommand(packet);
			getFileCommand.Send(Port);
			string fmt = "[Cortex Service] Snap time: {0}ms";
			if (getFileCommand.ImageBytes.Length != getFileCommand.Descriptor.ImageSize)
			{
				string msg = $"[Cortex Service] Unable to construct image. Bytes = {getFileCommand.ImageBytes.Length} (expected {getFileCommand.Descriptor.ImageSize})";
				LogHelper.Instance.Log(msg);
				executionTimer.Stop();
				LogHelper.Instance.Log(fmt, executionTimer.ElapsedMilliseconds);
				return new SnapResult(fileName);
			}
			try
			{
				using (Image image = Image.FromStream(new MemoryStream(getFileCommand.ImageBytes)))
				{
					image.Save(fileName, ImageFormat.Jpeg);
				}
				executionTimer.Stop();
				if (LogDetailedScan)
				{
					LogHelper.Instance.Log(fmt, executionTimer.ElapsedMilliseconds);
				}
			}
			catch
			{
				executionTimer.Stop();
				string msg2 = $"[Cortex Service] Exception: unable to construct image. Bytes = {getFileCommand.ImageBytes.Length} (expected {getFileCommand.Descriptor.ImageSize})";
				LogHelper.Instance.Log(msg2);
				LogHelper.Instance.Log(fmt, executionTimer.ElapsedMilliseconds);
			}
		}
		DeleteFileCommand deleteFileCommand = new DeleteFileCommand(packet);
		deleteFileCommand.Send(Port);
		return new SnapResult(fileName);
	}

	private bool TestPort(RedboxSerialPort port)
	{
		GetRegisterValue getRegisterValue = new GetRegisterValue(new SetReadFailureCommand());
		getRegisterValue.Send(port);
		bool isCorrectlySet = getRegisterValue.IsCorrectlySet;
		LogHelper.Instance.Log("[Cortex Service] Test port returned {0} on port {1}", isCorrectlySet, port.Name);
		return isCorrectlySet;
	}
}
