using System;
using System.Reflection;
using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class CoreCommand
{
	public int? Timeout { get; private set; }

	public int? StatusBit { get; private set; }

	public string Command { get; private set; }

	public CommandType Type { get; private set; }

	public int CommandWait { get; private set; }

	public string ResetCommand { get; private set; }

	public object[] Parameters { get; private set; }

	public bool WaitForCompletion { get; private set; }

	public AddressSelector Address { get; private set; }

	public AddressSelector? StatusAddress { get; private set; }

	public int? WaitPauseTime { get; private set; }

	internal RedboxSerialPort Port { get; private set; }

	private static bool IsMultiprocessor { get; set; }

	public static CoreCommand MakeCommand(CommandType type, params object[] parms)
	{
		return new CoreCommand(type, null, parms);
	}

	internal CoreCommand(CommandType type, RedboxSerialPort port)
		: this(type, waitForCompletion: false, null, port, null)
	{
	}

	internal CoreCommand(CommandType type, RedboxSerialPort port, params object[] parms)
		: this(type, waitForCompletion: false, null, port, parms)
	{
	}

	internal CoreCommand(CommandType type, bool waitForCompletion, int? timeout, RedboxSerialPort port, params object[] parms)
	{
		Port = port;
		Type = type;
		Timeout = timeout;
		Parameters = parms;
		WaitForCompletion = waitForCompletion;
		FieldInfo field = typeof(CommandType).GetField(Type.ToString());
		if (field == null)
		{
			return;
		}
		CommandPropertiesAttribute properties = CommandPropertiesAttribute.GetProperties(field, ControllerConfiguration.Instance.Revision ?? "A");
		if (properties != null)
		{
			if (properties.Timeout != -1 && !Timeout.HasValue)
			{
				Timeout = properties.Timeout;
			}
			if (properties.StatusBit != -1)
			{
				StatusBit = properties.StatusBit;
			}
			Command = properties.Command;
			Address = properties.Address;
			ResetCommand = properties.ResetCommand;
			if (properties.StatusAddress != AddressSelector.None)
			{
				StatusAddress = properties.StatusAddress;
			}
			CommandWait = 8000;
			if (properties.CommandWait != -1)
			{
				CommandWait = properties.CommandWait;
			}
			if (-1 != properties.WaitPauseTime)
			{
				WaitPauseTime = properties.WaitPauseTime;
			}
			if (IsMultiprocessor && properties.MultiProcWaitTime != -1)
			{
				WaitPauseTime = properties.MultiProcWaitTime;
			}
		}
	}

	public string GetFormattedCommand()
	{
		string text = Command;
		if (text.IndexOf("{") != -1 && Parameters.Length > 0)
		{
			text = string.Format(Command, Parameters);
		}
		return text;
	}

	internal CoreResponse Execute()
	{
		CoreResponse coreResponse = SendCommand(Address, GetFormattedCommand(), CommandWait);
		if (coreResponse.CommunicationError)
		{
			return coreResponse;
		}
		if (StatusBit.HasValue && WaitForCompletion)
		{
			coreResponse.TimedOut = WaitForCommand();
			if (coreResponse.TimedOut && !string.IsNullOrEmpty(ResetCommand))
			{
				string[] array = ResetCommand.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				foreach (string command in array2)
				{
					SendCommand(Address, command, CommandWait);
				}
			}
		}
		return coreResponse;
	}

	internal bool WaitForCommand()
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, WaitPauseTime.HasValue ? WaitPauseTime.Value : 0);
		using ExecutionTimer executionTimer = new ExecutionTimer();
		do
		{
			if (WaitPauseTime.HasValue)
			{
				PerfFunctions.SpinWait(timespan);
			}
			CoreResponse coreResponse = SendCommand(StatusAddress ?? Address, "S", 8000);
			if (!coreResponse.IsBitSet(StatusBit.Value))
			{
				return false;
			}
		}
		while (!Timeout.HasValue || !(executionTimer.ElapsedMilliseconds > Timeout));
		executionTimer.Stop();
		LogHelper.Instance.Log($"CoreCommand.WaitForCommand timed out: CommandType = {Type}, threadTimer.ElapsedMilliseconds = {executionTimer.ElapsedMilliseconds}, Timeout = {Timeout}", LogEntryType.Debug);
		return true;
	}

	internal CoreResponse SendCommand(AddressSelector address, string command, int readWait)
	{
		CoreResponse coreResponse = new CoreResponse();
		Port.ResetBuffers();
		using (ChannelResponse channelResponse = Port.WriteCommand(address.ToString(), 20000))
		{
			coreResponse.CommunicationError = !channelResponse.CommOk;
			if (!channelResponse.CommOk)
			{
				LogHelper.Instance.Log("Selector on address {0} timed out; port = {1}.", address.ToString(), Port.Name);
				coreResponse.CommErrorDiagnostic = ComputeCommunicationError(address);
				return coreResponse;
			}
		}
		Port.ResetBuffers();
		using ChannelResponse channelResponse2 = Port.WriteCommand(command, readWait);
		coreResponse.CommunicationError = !channelResponse2.CommOk;
		if (channelResponse2.CommOk)
		{
			coreResponse.OpCodeResponse = Encoding.ASCII.GetString(channelResponse2.RawResponse);
			if (Port.EnableDebugging && command != "S")
			{
				LogHelper.Instance.Log("Corecommand: Command {0} Response {1}", command, coreResponse.OpCodeResponse);
			}
		}
		return coreResponse;
	}

	private string ComputeCommunicationError(AddressSelector address)
	{
		string arg = BoardName(address);
		string text = $"{arg} is not responsive.";
		LogHelper.Instance.Log(text, LogEntryType.Error);
		return text;
	}

	private string BoardName(AddressSelector address)
	{
		string text = null;
		return address switch
		{
			AddressSelector.H001 => "PCB", 
			AddressSelector.H002 => "AUX board", 
			AddressSelector.H101 => "SER board", 
			AddressSelector.H555 => "QR device", 
			_ => $"Unknown board {address.ToString()}", 
		};
	}

	static CoreCommand()
	{
		IsMultiprocessor = false;
	}
}
