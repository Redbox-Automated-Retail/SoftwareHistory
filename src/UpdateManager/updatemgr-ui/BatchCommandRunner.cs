using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal class BatchCommandRunner : ISession, IMessageSink
{
	private ParameterDictionary m_context;

	private readonly StreamReader m_reader;

	private IDictionary<string, object> m_properties;

	public ParameterDictionary Context
	{
		get
		{
			if (m_context == null)
			{
				m_context = new ParameterDictionary();
			}
			return m_context;
		}
	}

	public IDictionary<string, object> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, object>();
			}
			return m_properties;
		}
	}

	public event EventHandler Disconnect;

	public BatchCommandRunner(StreamReader reader)
	{
		m_reader = reader;
	}

	public bool Send(string message)
	{
		return true;
	}

	public bool IsConnected()
	{
		return true;
	}

	public static void ExecuteStartupFiles()
	{
		string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.txt");
		string[] array = files;
		foreach (string fileName in array)
		{
			ExecuteFile(fileName);
		}
	}

	public static void ExecuteFile(string fileName)
	{
		using StreamReader reader = new StreamReader(fileName);
		BatchCommandRunner batchCommandRunner = new BatchCommandRunner(reader);
		batchCommandRunner.Start();
	}

	public void Start()
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			stringBuilder.Length = 0;
			if (!Read(stringBuilder))
			{
				break;
			}
			string text = stringBuilder.ToString();
			if (text.Length > 0)
			{
				LogHelper.Instance.Log(text, LogEntryType.Info);
				if (text.IndexOf("quit", 0, StringComparison.CurrentCultureIgnoreCase) != -1)
				{
					break;
				}
				CommandResult commandResult = CommandService.Instance.Execute(this, text);
				if (commandResult != null)
				{
					ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
					LogHelper.Instance.Log(commandResult.ToString(), LogEntryType.Info);
				}
			}
		}
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
