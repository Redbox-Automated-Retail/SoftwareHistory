using System;
using System.Collections.Generic;
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

	private static readonly PropertyDictionary m_properties = new PropertyDictionary();

	public static CommandService Instance => Singleton<CommandService>.Instance;

	public PropertyDictionary Properties => m_properties;

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
		Interlocked.Increment(ref Statistics.Instance.NumberServerBusyResponses);
		CommandResult commandResult = new CommandResult();
		LogHelper.Instance.Log("Server too busy to handle request, maximum available threads: {0}.", maximumThreads);
		commandResult.Errors.Add(Error.NewError("J777", "Server too busy to service request.", "Resubmit the command request when the server isn't so busy."));
		commandResult.ExecutionTime = timer.Elapsed;
		commandResult.Success = false;
		ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
		return commandResult.ToString();
	}

	public CommandResult Execute(ISession session, string input)
	{
		return Execute(session, string.Empty, input);
	}

	public CommandResult Execute(ISession session, string originIP, string input)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		CommandResult commandResult = new CommandResult();
		CommandContext commandContext = new CommandContext();
		commandContext.Session = session;
		commandContext.MessageSink = session;
		CommandContext commandContext2 = commandContext;
		try
		{
			if (!GetTokenizer(input, commandResult, commandContext2, out var tokenizer))
			{
				return commandResult;
			}
			SimpleToken mnemonic = ((SimpleTokenizer<CommandParserState>)tokenizer).Tokens.GetMnemonic();
			CommandInstance command = CommandRepository.GetCommand(((TokenBase<SimpleTokenType>)(object)mnemonic).Value);
			if (command == null)
			{
				commandResult.Errors.Add(Error.NewError("S514", $"Command {((TokenBase<SimpleTokenType>)(object)mnemonic).Value} not recognized.", "Use HELP command to learn more about available commands."));
				commandResult.ExtendedErrorMessage = $"Command '{((TokenBase<SimpleTokenType>)(object)mnemonic).Value}' not recognized. Use HELP to learn more about available commands.";
				return commandResult;
			}
			if (command.FormMethodCache.Count == 0)
			{
				commandResult.Errors.Add(Error.NewError("S322", $"Command {((TokenBase<SimpleTokenType>)(object)mnemonic).Value} does not have any valid executable forms.", "Please contact the support personnel for this command service and notify them of this error."));
				return commandResult;
			}
			if (!string.IsNullOrEmpty(originIP) && !AllowOriginForCommand(originIP, ((TokenBase<SimpleTokenType>)(object)mnemonic).Value))
			{
				string text = $"Command {((TokenBase<SimpleTokenType>)(object)mnemonic).Value} does allow execution from {originIP}.";
				LogHelper.Instance.Log(text);
				commandResult.Errors.Add(Error.NewError("S923", text, "Please contact the support personnel for this command service and notify them of this error."));
				return commandResult;
			}
			SimpleTokenList symbols = ((SimpleTokenizer<CommandParserState>)tokenizer).Tokens.GetSymbols();
			if ((((List<SimpleToken>)(object)symbols).Count == 0 && command.HasDefault()) || command.HasOnlyDefault())
			{
				LogHelper.Instance.Log(input, LogEntryType.Info);
				command.InvokeDefault(commandResult, commandContext2, tokenizer);
				return commandResult;
			}
			if (((List<SimpleToken>)(object)symbols).Count == 0)
			{
				commandResult.Errors.Add(Error.NewError("S001", $"No form symbol was specified for the {((TokenBase<SimpleTokenType>)(object)mnemonic).Value.ToUpper()} comand.", $"Use the HELP command to learn how to properly invoke the {((TokenBase<SimpleTokenType>)(object)mnemonic).Value.ToUpper()} command and its forms."));
				return commandResult;
			}
			FormMethod method = command.GetMethod(((TokenBase<SimpleTokenType>)(object)((List<SimpleToken>)(object)symbols)[0]).Value);
			if (method == null)
			{
				commandResult.Errors.Add(Error.NewError("S001", $"An unknown form was used for the {((TokenBase<SimpleTokenType>)(object)mnemonic).Value.ToUpper()} comand.", $"Use the HELP command to learn how to properly invoke the {((TokenBase<SimpleTokenType>)(object)mnemonic).Value.ToUpper()} command and its forms."));
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
			PerformanceCounterHelper.Instance.IncrementCommandPerSecond(((TokenBase<SimpleTokenType>)(object)mnemonic).Value.ToUpper(), ((TokenBase<SimpleTokenType>)(object)((List<SimpleToken>)(object)symbols)[0]).Value.ToLower());
			method.Invoke(commandResult, commandContext2, tokenizer, command.GetInstance());
		}
		catch (Exception e)
		{
			commandResult.Errors.Add(Error.NewError("S999", "An unhandled exception was raised in CommandService.Execute.", e));
		}
		finally
		{
			commandResult.ExecutionTime = executionTimer.Elapsed;
			commandResult.Success = !commandContext2.Errors.ContainsError() && !commandResult.Errors.ContainsError();
			commandResult.Messages.AddRange(commandContext2.Messages);
			commandResult.Errors.AddRange(commandContext2.Errors);
			Statistics.Instance.TrackCommandStatistics(commandResult.ExecutionTime);
			LogHelper.Instance.Log(commandResult.ToString(includeReturnResult: false));
		}
		return commandResult;
	}

	private CommandService()
	{
		LogHelper.Instance.Log("Initializing CommandService.", LogEntryType.Info);
		foreach (string item in CommandRepository.DiscoverInstalledCommands())
		{
			LogHelper.Instance.Log("Loaded command: " + item, LogEntryType.Info);
		}
		m_accessFilter = new Dictionary<string, List<byte[]>>();
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

	private static bool GetTokenizer(string input, CommandResult result, CommandContext context, out CommandTokenizer tokenizer)
	{
		input = m_properties.ExpandProperties(input, Location.UnknownLocation);
		tokenizer = new CommandTokenizer(0, input);
		((TokenizerBase<CommandParserState>)(object)tokenizer).Tokenize();
		if (((SimpleTokenizer<CommandParserState>)tokenizer).Tokens.HasOnlyComments())
		{
			return false;
		}
		if (((TokenizerBase<CommandParserState>)(object)tokenizer).Errors.ContainsError() || ((SimpleTokenizer<CommandParserState>)tokenizer).Tokens.GetMnemonic() == null)
		{
			foreach (Error item in (List<Error>)(object)((TokenizerBase<CommandParserState>)(object)tokenizer).Errors)
			{
				result.Errors.Add(item.IsWarning ? Error.NewWarning(item.Code, item.Description, item.Details) : Error.NewError(item.Code, item.Description, item.Details));
			}
			result.ExtendedErrorMessage = $"Parsing of input '{input}' failed. Correct the command syntax and try again.";
			return false;
		}
		foreach (SimpleToken item2 in (List<SimpleToken>)(object)((SimpleTokenizer<CommandParserState>)tokenizer).Tokens.GetSymbols())
		{
			context.Parameters[((TokenBase<SimpleTokenType>)(object)item2).Value] = ((TokenBase<SimpleTokenType>)(object)item2).Value;
		}
		SimpleTokenList keyValuePairs = ((SimpleTokenizer<CommandParserState>)tokenizer).Tokens.GetKeyValuePairs();
		foreach (SimpleToken item3 in (List<SimpleToken>)(object)keyValuePairs)
		{
			object obj = item3.ConvertValue();
			KeyValuePair val = (KeyValuePair)((obj is KeyValuePair) ? obj : null);
			if (val != null)
			{
				context.Parameters[$"{val.Key}:"] = val.Value;
			}
		}
		return true;
	}
}
