using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.Core;

namespace Redbox.IPC.Framework;

internal class BatchCommandRunner : Redbox.IPC.Framework.ISession, IMessageSink
{
	private Redbox.IPC.Framework.ParameterDictionary m_context;

	private readonly StreamReader m_reader;

	private IDictionary<string, object> m_properties;

	public Redbox.IPC.Framework.ParameterDictionary Context
	{
		get
		{
			if (m_context == null)
			{
				m_context = new Redbox.IPC.Framework.ParameterDictionary();
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
		Redbox.IPC.Framework.BatchCommandRunner batchCommandRunner = new Redbox.IPC.Framework.BatchCommandRunner(reader);
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
				Redbox.IPC.Framework.CommandResult commandResult = Redbox.IPC.Framework.CommandService.Instance.Execute(this, text);
				if (commandResult != null)
				{
					Redbox.IPC.Framework.ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
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
