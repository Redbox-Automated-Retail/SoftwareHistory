using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class MotionControlChannel
{
	private const int ArcusBaudRate = 115200;

	private static readonly byte[] TransEndChars = new byte[2] { 13, 0 };

	internal string Name
	{
		get
		{
			if (CommMode.Channel == Mode)
			{
				return SerialPort.Name;
			}
			return Port.PortName;
		}
	}

	private MotionController Controller { get; set; }

	private SerialPort Port { get; set; }

	private RedboxSerialPort SerialPort { get; set; }

	private bool DebugEnabled { get; set; }

	private CommMode Mode { get; set; }

	private TimeSpan WaitSpan { get; set; }

	internal bool ResetBuffers()
	{
		if (CommMode.Channel == Mode)
		{
			return SerialPort.ResetBuffers();
		}
		try
		{
			Port.DiscardInBuffer();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("MotionControlChannel.ResetBuffers: caught an exception.", e);
			return false;
		}
	}

	internal bool Open()
	{
		if (CommMode.Channel != Mode)
		{
			return OpenSerialPort();
		}
		return SerialPort.Open();
	}

	internal bool ClosePort()
	{
		if (CommMode.Channel == Mode)
		{
			return SerialPort.ClosePort();
		}
		try
		{
			Port.RtsEnable = true;
			Port.DtrEnable = false;
			if (Port.IsOpen)
			{
				Port.DiscardInBuffer();
				Port.DiscardOutBuffer();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("SafeClosePort: discarding in/out buffers caught an exception.", e);
		}
		try
		{
			Port.Close();
			if (Port.IsOpen)
			{
				LogHelper.Instance.Log("PortManager.SafeClosePort {0}: port is still open.", Port.PortName);
				return false;
			}
			LogHelper.Instance.Log("PortManager.SafeClosePort {0}: success.", Port.PortName);
			return true;
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("SafeClosePort caught an exception.", e2);
			return false;
		}
	}

	internal string SendCommand(string command)
	{
		if (Mode != CommMode.Channel)
		{
			return LegacyCommunication(command);
		}
		return Communicate(command);
	}

	internal MotionControlChannel(MotionController controller, bool debugMode)
	{
		try
		{
			Mode = SecretOptions.Instance.GetValue("ArcusCommMode", CommMode.Channel);
		}
		catch
		{
			LogHelper.Instance.Log("MotionControlChannel: unable to parse comm mode.");
			Mode = CommMode.Channel;
		}
		LogHelper.Instance.Log("MotionControlChannel: type specified is {0}", Mode.ToString());
		DebugEnabled = debugMode;
		WaitSpan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusReadWait);
		Controller = controller;
		if (CommMode.Channel == Mode)
		{
			ConfigurePort(new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.MotionControllerPortName)
			{
				WriteTerminator = RedboxSerialPort.WriteCharsCRNull,
				Test = ChannelResponse.EndsWithEOT
			});
		}
		else
		{
			Port = new SerialPort(ControllerConfiguration.Instance.MotionControllerPortName, 115200, Parity.None, 8, StopBits.One);
		}
	}

	private string Communicate(string command)
	{
		bool flag = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug) || DebugEnabled;
		StringBuilder stringBuilder = (flag ? new StringBuilder() : null);
		if (flag)
		{
			stringBuilder.AppendFormat("ArcusMotionController: Instruction={0}", command);
		}
		using ChannelResponse channelResponse = SerialPort.WriteCommand(command, ControllerConfiguration.Instance.MotionControllerTimeout);
		if (!channelResponse.CommOk)
		{
			LogHelper.Instance.Log("ArcusMotionControl.SendCommand(): channel error returned {0}", channelResponse.CommError);
			return "COMMUNICATION ERROR";
		}
		int num = ByteHelper.IndexOf(channelResponse.RawResponse, 13);
		int count = ((num == -1) ? (channelResponse.RawResponse.Length - 1) : num);
		string text = Encoding.ASCII.GetString(channelResponse.RawResponse, 0, count);
		if (flag)
		{
			stringBuilder.AppendFormat(", ChannelError = {0}, Response={1}", channelResponse.CommError, text);
			Controller.WriteToLog(stringBuilder.ToString());
		}
		return text;
	}

	private string LegacyCommunication(string command)
	{
		bool flag = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug) || DebugEnabled;
		StringBuilder stringBuilder = (flag ? new StringBuilder() : null);
		if (flag)
		{
			stringBuilder.AppendFormat("ArcusMotionController: Instruction={0}", command);
		}
		try
		{
			Port.Write(command);
			Port.Write(TransEndChars, 0, TransEndChars.Length);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("ArcusMotionController.SendCommand: Port.Write caught an unhandled exception", e);
			return "COMMUNICATION ERROR";
		}
		PerfFunctions.SpinWait(WaitSpan);
		string empty = string.Empty;
		try
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			while (true)
			{
				int num = Port.ReadChar();
				if (Port.BytesToRead == 0)
				{
					break;
				}
				stringBuilder2.Append((char)num);
			}
			empty = stringBuilder2.ToString();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("ArcusMotionController.SendCommand: ReadResponse caught an exception.", e2);
			empty = "COMMUNICATION ERROR";
		}
		if (flag)
		{
			stringBuilder.AppendFormat(", Response={0}", empty);
			Controller.WriteToLog(stringBuilder.ToString());
		}
		return empty;
	}

	private bool OpenSerialPort()
	{
		try
		{
			if (Port.IsOpen)
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		for (int i = 0; i < 3; i++)
		{
			try
			{
				Port.Open();
				Port.RtsEnable = true;
				Port.DtrEnable = true;
				Port.ReadTimeout = ControllerConfiguration.Instance.MotionControllerTimeout;
				Port.WriteTimeout = ControllerConfiguration.Instance.MotionControllerTimeout;
				Thread.Sleep(3000);
				bool isOpen = Port.IsOpen;
				LogHelper.Instance.Log("PortManager.Open port {0}: {1}.", Port.PortName, isOpen ? "success" : "EPIC failure.");
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("PortManager.Open() caught an exception.", e);
				continue;
			}
			try
			{
				if (!Port.IsOpen)
				{
					LogHelper.Instance.Log("OnSendCommands: the port isn't open; retry.", LogEntryType.Info);
					continue;
				}
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("ArcusMotionController.SendCommands: port is not open - will retry operation.", e2);
				Thread.Sleep(2000);
				continue;
			}
			try
			{
				Port.DiscardInBuffer();
				Port.DiscardOutBuffer();
				return true;
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("Unable to clear port buffers.", e3);
				ClosePort();
			}
		}
		return false;
	}

	private void ConfigurePort(RedboxSerialPortConfiguration conf)
	{
		SerialPort = new RedboxSerialPort(conf);
		SerialPort.EnableDebugging = DebugEnabled;
	}
}
