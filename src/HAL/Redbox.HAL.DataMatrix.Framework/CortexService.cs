using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CortexService : ScannerAdapter, IScannerDevice, IDisposable
{
	private RedboxSerialPort Port;

	private readonly bool HardCycleOnReboot;

	private readonly PortModes Mode;

	private readonly RedboxSerialPortConfiguration Configuration;

	public bool RequiresExternalLighting => false;

	public bool IsConnected { get; private set; }

	public bool InError => false;

	internal static CortexService Instance => Singleton<CortexService>.Instance;

	public bool Restart()
	{
		if (HardCycleOnReboot)
		{
			TurnOff();
			PerfFunctions.Wait(500);
			return Start();
		}
		return RestartDecoder();
	}

	public ScanResult Scan()
	{
		if (!IsConnected)
		{
			LogHelper.Instance.Log("[Cortex Service] Scan is called without initialization.");
			ScanResult scanResult = new ScanResult();
			scanResult.ResetOnException();
			return scanResult;
		}
		return ScanInner(snapOnFailure: true);
	}

	public SnapResult Snap()
	{
		return Snap(GenerateSnapFileName());
	}

	public SnapResult Snap(string fileName)
	{
		if (!IsConnected)
		{
			LogHelper.Instance.Log("[Cortex Service] Snap: camera is not connected.");
			return new SnapResult();
		}
		return SnapInner(fileName);
	}

	public bool Start()
	{
		if (IsConnected)
		{
			return true;
		}
		string snapDecodePort = BarcodeConfiguration.Instance.SnapDecodePort;
		if (snapDecodePort == "NONE")
		{
			LogHelper.Instance.Log("[Cortex Service] There is no configured port.");
			return false;
		}
		Configuration.WritePause = BarcodeConfiguration.Instance.WritePause;
		Port = PortManager.Instance.Scan(snapDecodePort, Configuration, TestPort, Mode);
		if (Port == null)
		{
			LogHelper.Instance.Log("[Cortex Service] Unable to open communcation port.");
			return false;
		}
		if (BarcodeConfiguration.Instance.RebootOnStart)
		{
			RestartDecoder();
		}
		InfaredLightCommand infaredLightCommand = new InfaredLightCommand();
		infaredLightCommand.Send(Port);
		switch (BarcodeConfiguration.Instance.CortexStartupAction)
		{
		case CortexStartupAction.Decode:
		{
			LogHelper.Instance.Log("[Cortex Service] Scan on startup.");
			ScanResult scanResult = ScanInner(snapOnFailure: false);
			IsConnected = scanResult.SnapResult.SnapOk;
			break;
		}
		case CortexStartupAction.Snap:
		{
			LogHelper.Instance.Log("[Cortex Service] Snap on startup.");
			SnapResult snapResult = SnapInner(GenerateSnapFileName());
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
		TurnOff();
		return true;
	}

	public void TurnOff()
	{
		if (IsConnected)
		{
			if (Port != null)
			{
				Port.Dispose();
				Port = null;
			}
			IsConnected = false;
		}
	}

	public string FindInstalledPort()
	{
		Configuration.WritePause = BarcodeConfiguration.Instance.WritePause;
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

	protected override void DisposeInner()
	{
		TurnOff();
	}

	private ScanResult ScanInner(bool snapOnFailure)
	{
		ScanResult scanResult = new ScanResult();
		DecodeCommand decodeCommand = new DecodeCommand(CortexDecodeVersion.Version2);
		decodeCommand.Send(Port);
		if (decodeCommand.PortError)
		{
			LogHelper.Instance.Log("[Cortex Service] Scan: There was a communication error with the device.");
			scanResult.ResetOnException();
			scanResult.NewSnapResult(new SnapResult());
			return scanResult;
		}
		scanResult.SnapResult.SnapOk = true;
		scanResult.AddCodes(decodeCommand.FoundCodes);
		InsertStat(scanResult, clearImage: false, decodeCommand.ExecutionTime.TotalSeconds);
		if (!scanResult.ReadCode && snapOnFailure)
		{
			scanResult.NewSnapResult(SnapInner(GenerateSnapFileName()));
			LogHelper.Instance.Log("[Cortex Service] Decode failed matrix = {0} secure codes = {1}.", scanResult.ScannedMatrix, scanResult.SecureCount);
			if (BarcodeConfiguration.Instance.UseInliteOnFail && scanResult.SnapOk)
			{
				LogHelper.Instance.Log("[Cortex Service] Snap using Inlite.");
				scanResult.ResetOnException();
				BarcodeReader.Instance.Scan(scanResult.SnapResult.Path, scanResult);
				InsertStat(scanResult, clearImage: true, scanResult.ExecutionTime.TotalSeconds);
			}
		}
		scanResult.ExecutionTime = decodeCommand.ExecutionTime;
		return scanResult;
	}

	private bool RestartDecoder()
	{
		RebootCommand rebootCommand = new RebootCommand();
		rebootCommand.Send(Port);
		PerfFunctions.Wait(BarcodeConfiguration.Instance.ScannerWakeupPause);
		return true;
	}

	private SnapResult SnapInner(string fileName)
	{
		TakePictureCommand takePictureCommand = new TakePictureCommand();
		takePictureCommand.Send(Port);
		if (takePictureCommand.PortError)
		{
			LogHelper.Instance.Log("[CortexService]: There was a communication error.");
			return new SnapResult();
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
				return new SnapResult();
			}
			try
			{
				using (Image image = Image.FromStream(new MemoryStream(getFileCommand.ImageBytes)))
				{
					image.Save(fileName, ImageFormat.Jpeg);
				}
				executionTimer.Stop();
				if (BarcodeConfiguration.Instance.LogDetailedScan)
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
		return TestSnap(fileName);
	}

	private bool TestPort(RedboxSerialPort port)
	{
		GetRegisterValue getRegisterValue = new GetRegisterValue(new SetReadFailureCommand());
		getRegisterValue.Send(port);
		bool isCorrectlySet = getRegisterValue.IsCorrectlySet;
		LogHelper.Instance.Log("[Cortex Service] Test port returned {0} on port {1}", isCorrectlySet, port.Name);
		return isCorrectlySet;
	}

	private CortexService()
		: base(ScannerServices.Cortex)
	{
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		Configuration = new RedboxSerialPortConfiguration();
		int value = dataStore.GetValue("CortexReadBuffer", RuntimeService.Instance.PageSize << 1);
		Configuration.ReceiveBufferSize = value;
		LogHelper.Instance.Log("[CortexService] Receive buffer size = {0}", value);
		Configuration.WritePause = BarcodeConfiguration.Instance.WritePause;
		Configuration.WriteTimeout = 2000;
		Configuration.WriteTerminator = null;
		Configuration.OpenWait = BarcodeConfiguration.Instance.CortexPortOpenWait;
		Mode = dataStore.GetValue("CortexPortMode", PortModes.Callback);
		HardCycleOnReboot = dataStore.GetValue("CortexHardCycle", def: true);
	}
}
