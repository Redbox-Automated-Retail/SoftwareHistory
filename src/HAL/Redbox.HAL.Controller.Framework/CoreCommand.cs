using System;
using System.Reflection;
using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class CoreCommand
{
	internal readonly int OperationTimeout;

	internal readonly RedboxSerialPort Port;

	internal readonly int? StatusBit;

	internal readonly int CommandWait;

	internal readonly string ResetCommand;

	internal readonly AddressSelector Address;

	internal readonly int WaitPauseTime;

	internal string Command { get; private set; }

	internal static CoreCommand Create(CommandType type, int? timeout, RedboxSerialPort port)
	{
		return new CoreCommand(type, timeout, port);
	}

	internal static CoreCommand Create(CommandType type, RedboxSerialPort port)
	{
		return new CoreCommand(type, null, port);
	}

	internal static CoreCommand Create(CommandType type, RedboxSerialPort port, params object[] args)
	{
		CoreCommand coreCommand = new CoreCommand(type, null, port);
		coreCommand.Command = string.Format(coreCommand.Command, args);
		return coreCommand;
	}

	internal CoreResponse Execute()
	{
		if (ControllerConfiguration.Instance.EnableCommandTrace)
		{
			LogHelper.Instance.Log("[CoreCommand] Executing command {0}", Command);
		}
		CoreResponse coreResponse = SendCommand(Command);
		if (coreResponse.CommunicationError || !StatusBit.HasValue)
		{
			return coreResponse;
		}
		coreResponse.TimedOut = WaitForCommand();
		if (coreResponse.TimedOut && !string.IsNullOrEmpty(ResetCommand))
		{
			string[] array = ResetCommand.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string command in array2)
			{
				SendCommand(command);
			}
		}
		return coreResponse;
	}

	internal bool WaitForCommand()
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, WaitPauseTime);
		if (ControllerConfiguration.Instance.EnableCommandTrace)
		{
			LogHelper.Instance.Log("[WaitForCommand] Start.");
		}
		using ExecutionTimer executionTimer = new ExecutionTimer();
		do
		{
			PerfFunctions.SpinWait(timespan);
			CoreResponse coreResponse = SendCommand("S");
			if (coreResponse.CommunicationError)
			{
				return true;
			}
			if (!coreResponse.IsBitSet(StatusBit.Value))
			{
				return false;
			}
		}
		while (executionTimer.ElapsedMilliseconds <= OperationTimeout);
		return true;
	}

	internal CoreResponse SendCommand(string command)
	{
		CoreResponse coreResponse = new CoreResponse(Address);
		Port.ResetBuffers();
		using (IChannelResponse channelResponse = Port.SendRecv(Address.ToString(), 5000))
		{
			coreResponse.CommunicationError = !channelResponse.CommOk;
			if (!channelResponse.CommOk)
			{
				LogHelper.Instance.Log("[SendCommand] Selector on address {0} timed out; port = {1}.", Address.ToString(), Port.Name);
				coreResponse.CommErrorDiagnostic = ComputeCommunicationError(Address);
				return coreResponse;
			}
		}
		Port.ResetBuffers();
		using IChannelResponse channelResponse2 = Port.SendRecv(command, CommandWait);
		coreResponse.CommunicationError = !channelResponse2.CommOk;
		if (channelResponse2.CommOk)
		{
			coreResponse.OpCodeResponse = Encoding.ASCII.GetString(channelResponse2.RawResponse);
			if (ControllerConfiguration.Instance.EnableCommandTrace)
			{
				LogHelper.Instance.Log("[SendCommand] Command {0} Response {1}", command, coreResponse.OpCodeResponse);
			}
		}
		return coreResponse;
	}

	private string ComputeCommunicationError(AddressSelector address)
	{
		string text = $"{BoardName(address)} is not responsive.";
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

	private CoreCommand(CommandType type, int? timeout, RedboxSerialPort port)
	{
		Port = port;
		FieldInfo field = typeof(CommandType).GetField(type.ToString());
		if (field == null)
		{
			LogHelper.Instance.Log("[CoreCommand] Could not locate type field on {0}", type.ToString());
			return;
		}
		CommandPropertiesAttribute properties = CommandPropertiesAttribute.GetProperties(field);
		if (properties == null)
		{
			LogHelper.Instance.Log("[CoreCommand] Could not locate PropertiesAttribute on {0}", type.ToString());
			return;
		}
		Command = properties.Command;
		Address = properties.Address;
		ResetCommand = properties.ResetCommand;
		WaitPauseTime = ((-1 != properties.WaitPauseTime) ? properties.WaitPauseTime : 0);
		OperationTimeout = timeout ?? 0;
		StatusBit = null;
		if (-1 != properties.StatusBit)
		{
			StatusBit = properties.StatusBit;
			if (OperationTimeout == 0 || WaitPauseTime == 0)
			{
				LogHelper.Instance.Log("[CoreCommand] On CommandType {0} there is a status bit but Timeout = {1} and WaitPause = {2}", type.ToString(), OperationTimeout, WaitPauseTime);
			}
		}
		CommandWait = ((-1 != properties.CommandWait) ? properties.CommandWait : 8000);
	}
}
