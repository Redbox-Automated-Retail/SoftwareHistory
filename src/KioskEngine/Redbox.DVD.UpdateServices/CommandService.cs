using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using Redbox.Command.Tokenizer;
using Redbox.Core;
using Redbox.Macros;
using Redbox.Tokenizer.Framework;

namespace Redbox.IPC.Framework;

internal class CommandService
{
	private readonly Dictionary<string, List<byte[]>> m_accessFilter;

	private static readonly Redbox.Macros.PropertyDictionary m_properties = new Redbox.Macros.PropertyDictionary();

	public static Redbox.IPC.Framework.CommandService Instance => Singleton<Redbox.IPC.Framework.CommandService>.Instance;

	public Redbox.Macros.PropertyDictionary Properties => m_properties;

	public void SetAccesFilter(string ruleFile)
	{
		if (!File.Exists(ruleFile))
		{
			return;
		}
		using StreamReader streamReader = new StreamReader(ruleFile);
		string text;
		while ((text = streamReader.ReadLine()) != null)
		{
			string[] array = text.Split(new string[1] { "," }, StringSplitOptions.RemoveEmptyEntries);
			string command = array[0].Trim();
			string filter = array[1].Trim();
			SetAccessFilter(command, filter);
		}
	}

	public void SetAccessFilter(string command, List<string> filters)
	{
		if (!m_accessFilter.ContainsKey(command))
		{
			m_accessFilter[command] = new List<byte[]>();
		}
		m_accessFilter[command].AddRange(filters.ConvertAll(delegate(string str)
		{
			if (str == "*")
			{
				return new byte[0];
			}
			string[] array = str.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries);
			List<byte> list = new List<byte>();
			for (int i = 0; i < array.Length && !(array[i] == "*"); i++)
			{
				list.Add(Convert.ToByte(array[i]));
			}
			return list.ToArray();
		}));
		m_accessFilter[command].Sort((byte[] lhs, byte[] rhs) => lhs.Length.CompareTo(rhs.Length));
	}

	public void SetAccessFilter(string command, string filter)
	{
		SetAccessFilter(command, new List<string> { filter });
	}

	public string GetServerBusyResult(ExecutionTimer timer, int maximumThreads)
	{
		Interlocked.Increment(ref Redbox.IPC.Framework.Statistics.Instance.NumberServerBusyResponses);
		Redbox.IPC.Framework.CommandResult commandResult = new Redbox.IPC.Framework.CommandResult();
		LogHelper.Instance.Log("Server too busy to handle request, maximum available threads: {0}.", maximumThreads);
		commandResult.Errors.Add(Error.NewError("J777", "Server too busy to service request.", "Resubmit the command request when the server isn't so busy."));
		commandResult.ExecutionTime = timer.Elapsed;
		commandResult.Success = false;
		Redbox.IPC.Framework.ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
		return commandResult.ToString();
	}

	public Redbox.IPC.Framework.CommandResult Execute(Redbox.IPC.Framework.ISession session, string input)
	{
		return Execute(session, string.Empty, input);
	}

	public Redbox.IPC.Framework.CommandResult Execute(Redbox.IPC.Framework.ISession session, string originIP, string input)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		Redbox.IPC.Framework.CommandResult commandResult = new Redbox.IPC.Framework.CommandResult();
		Redbox.IPC.Framework.CommandContext commandContext = new Redbox.IPC.Framework.CommandContext();
		commandContext.Session = session;
		commandContext.MessageSink = session;
		Redbox.IPC.Framework.CommandContext commandContext2 = commandContext;
		string text = null;
		try
		{
			if (!GetTokenizer(input, commandResult, commandContext2, out var tokenizer))
			{
				return commandResult;
			}
			SimpleToken mnemonic = tokenizer.Tokens.GetMnemonic();
			Redbox.IPC.Framework.CommandInstance command = Redbox.IPC.Framework.CommandRepository.GetCommand(mnemonic.Value);
			if (command == null)
			{
				commandResult.Errors.Add(Error.NewError("S514", $"Command {mnemonic.Value} not recognized.", "Use HELP command to learn more about available commands."));
				commandResult.ExtendedErrorMessage = $"Command '{mnemonic.Value}' not recognized. Use HELP to learn more about available commands.";
				return commandResult;
			}
			if (command.FormMethodCache.Count == 0)
			{
				commandResult.Errors.Add(Error.NewError("S322", $"Command {mnemonic.Value} does not have any valid executable forms.", "Please contact the support personnel for this command service and notify them of this error."));
				return commandResult;
			}
			if (!string.IsNullOrEmpty(originIP) && !AllowOriginForCommand(originIP, mnemonic.Value))
			{
				string text2 = $"Command {mnemonic.Value} does allow execution from {originIP}.";
				LogHelper.Instance.Log(text2);
				commandResult.Errors.Add(Error.NewError("S923", text2, "Please contact the support personnel for this command service and notify them of this error."));
				return commandResult;
			}
			SimpleTokenList symbols = tokenizer.Tokens.GetSymbols();
			if ((symbols.Count == 0 && command.HasDefault()) || command.HasOnlyDefault())
			{
				LogHelper.Instance.Log(input, LogEntryType.Info);
				command.InvokeDefault(commandResult, commandContext2, tokenizer);
				return commandResult;
			}
			if (symbols.Count == 0)
			{
				commandResult.Errors.Add(Error.NewError("S001", $"No form symbol was specified for the {mnemonic.Value.ToUpper()} comand.", $"Use the HELP command to learn how to properly invoke the {mnemonic.Value.ToUpper()} command and its forms."));
				return commandResult;
			}
			Redbox.IPC.Framework.FormMethod method = command.GetMethod(symbols[0].Value);
			if (method == null)
			{
				commandResult.Errors.Add(Error.NewError("S001", $"An unknown form was used for the {mnemonic.Value.ToUpper()} comand.", $"Use the HELP command to learn how to properly invoke the {mnemonic.Value.ToUpper()} command and its forms."));
				return commandResult;
			}
			if (method.Loggable)
			{
				LogHelper.Instance.Log(input, LogEntryType.Info);
			}
			if (!method.ValidateParameters(commandResult, tokenizer))
			{
				return commandResult;
			}
			text = $"{mnemonic.Value.ToUpper()} {symbols[0].Value.ToLower()}";
			Redbox.IPC.Framework.PerformanceCounterHelper.Instance.IncrementCommandPerSecond(text);
			method.Invoke(commandResult, commandContext2, tokenizer, command.GetInstance());
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CommandService.Execute error", e);
			commandResult.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in CommandService.Execute.", e));
		}
		finally
		{
			commandResult.ExecutionTime = executionTimer.Elapsed;
			commandResult.Success = !commandContext2.Errors.ContainsError() && !commandResult.Errors.ContainsError();
			commandResult.Messages.AddRange(commandContext2.Messages);
			commandResult.Errors.AddRange(commandContext2.Errors);
			Redbox.IPC.Framework.Statistics.Instance.TrackCommandStatistics(commandResult.ExecutionTime);
			if (text != null)
			{
				Redbox.IPC.Framework.PerformanceCounterHelper.Instance.IncrementCommandExecutionTime(text, executionTimer.ElapsedTicks);
			}
			LogHelper.Instance.Log(commandResult.ToString(IsResultLoggingEnabled()));
		}
		return commandResult;
	}

	private CommandService()
	{
		LogHelper.Instance.Log("Initializing CommandService.", LogEntryType.Info);
		foreach (string item in Redbox.IPC.Framework.CommandRepository.DiscoverInstalledCommands())
		{
			LogHelper.Instance.Log("Loaded command: " + item, LogEntryType.Info);
		}
		m_accessFilter = new Dictionary<string, List<byte[]>>();
	}

	private bool IsResultLoggingEnabled()
	{
		if (ConfigurationManager.AppSettings["IpcLogResults"] != null)
		{
			return bool.Parse(ConfigurationManager.AppSettings["IpcLogResults"]);
		}
		return false;
	}

	private bool AllowOriginForCommand(string origin, string command)
	{
		LogHelper.Instance.Log($"Checking access rules for {origin}.", LogEntryType.Debug);
		if (!m_accessFilter.ContainsKey(command))
		{
			return true;
		}
		string[] array = origin.Split('.');
		byte[] array2 = new byte[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = Convert.ToByte(array[i]);
		}
		List<byte[]> list = m_accessFilter[command];
		foreach (byte[] item in list)
		{
			bool flag = true;
			for (int j = 0; j < item.Length; j++)
			{
				if (item[j] != array2[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private static bool GetTokenizer(string input, Redbox.IPC.Framework.CommandResult result, Redbox.IPC.Framework.CommandContext context, out CommandTokenizer tokenizer)
	{
		input = m_properties.ExpandProperties(input, Redbox.Macros.Location.UnknownLocation);
		tokenizer = new CommandTokenizer(0, input);
		tokenizer.Tokenize();
		if (tokenizer.Tokens.HasOnlyComments())
		{
			return false;
		}
		if (tokenizer.Errors.ContainsError() || tokenizer.Tokens.GetMnemonic() == null)
		{
			foreach (Redbox.Tokenizer.Framework.Error error in tokenizer.Errors)
			{
				result.Errors.Add(error.IsWarning ? Error.NewWarning(error.Code, error.Description, error.Details) : Error.NewError(error.Code, error.Description, error.Details));
			}
			result.ExtendedErrorMessage = $"Parsing of input '{input}' failed. Correct the command syntax and try again.";
			return false;
		}
		foreach (SimpleToken symbol in tokenizer.Tokens.GetSymbols())
		{
			context.Parameters[symbol.Value] = symbol.Value;
		}
		SimpleTokenList keyValuePairs = tokenizer.Tokens.GetKeyValuePairs();
		foreach (SimpleToken item in keyValuePairs)
		{
			if (item.ConvertValue() is KeyValuePair keyValuePair)
			{
				context.Parameters[$"{keyValuePair.Key}:"] = keyValuePair.Value;
			}
		}
		return true;
	}
}
