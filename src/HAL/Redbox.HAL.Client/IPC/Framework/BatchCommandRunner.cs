using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal class BatchCommandRunner : ISession, IMessageSink
{
	private readonly ErrorList m_errorList = new ErrorList();

	private readonly IDictionary<string, string> m_context = new Dictionary<string, string>();

	private readonly StreamReader m_reader;

	public IDictionary<string, string> Context => m_context;

	public bool LogDetailedMessages { get; set; }

	internal ErrorList Errors => m_errorList;

	public event EventHandler Disconnect
	{
		add
		{
		}
		remove
		{
		}
	}

	public BatchCommandRunner(StreamReader reader)
	{
		m_reader = reader;
		LogDetailedMessages = false;
	}

	public bool Send(string message)
	{
		return true;
	}

	public static ErrorList ExecuteStartupFiles()
	{
		string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt");
		ErrorList errorList = new ErrorList();
		string[] array = files;
		foreach (string fileName in array)
		{
			ExecuteFile(fileName, errorList);
		}
		return errorList;
	}

	public static void ExecuteFile(string fileName, ErrorList errorList)
	{
		using StreamReader reader = new StreamReader(fileName);
		BatchCommandRunner batchCommandRunner = new BatchCommandRunner(reader);
		batchCommandRunner.Start();
		if (batchCommandRunner.Errors.Count > 0)
		{
			errorList.AddRange(batchCommandRunner.Errors);
			batchCommandRunner.Errors.Clear();
		}
	}

	public ErrorList ExecuteFile()
	{
		Start();
		return Errors;
	}

	public void Start()
	{
		StringBuilder stringBuilder = new StringBuilder();
		CommandResult commandResult;
		while (true)
		{
			stringBuilder.Length = 0;
			if (!Read(stringBuilder))
			{
				return;
			}
			string text = stringBuilder.ToString();
			if (text.Length <= 0)
			{
				continue;
			}
			if (LogDetailedMessages)
			{
				LogHelper.Instance.Log(text);
			}
			if (text.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
			{
				return;
			}
			commandResult = CommandService.Instance.Execute(this, text);
			if (commandResult != null)
			{
				ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
				if (LogDetailedMessages)
				{
					LogEntryType type = ((commandResult.Errors.Count > 0) ? LogEntryType.Error : LogEntryType.Info);
					LogHelper.Instance.Log(commandResult.ToString(), type);
				}
				if (commandResult.Errors.ContainsError())
				{
					break;
				}
			}
		}
		Errors.AddRange(commandResult.Errors);
	}

	private bool Read(StringBuilder builder)
	{
		string text = m_reader.ReadLine();
		if (text == null)
		{
			return false;
		}
		builder.Append(text);
		return true;
	}
}
