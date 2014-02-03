using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class StatisticsInstruction : Instruction
{
	private enum Operations
	{
		Log,
		Clear,
		LogUnknown
	}

	private static string m_statsDirectory;

	public override string Mnemonic => "STATS";

	private static string StatisticsDirectory
	{
		get
		{
			if (!string.IsNullOrEmpty(m_statsDirectory))
			{
				return m_statsDirectory;
			}
			m_statsDirectory = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Statistics");
			return m_statsDirectory;
		}
	}

	protected override void OnParse(TokenList tokens, ExecutionResult result)
	{
		TokenList tokensAfterMnemonic = tokens.GetTokensAfterMnemonic();
		base.Operands.AddRange(tokensAfterMnemonic);
	}

	protected override void OnExecute(ExecutionResult result)
	{
		Operations operations = (Operations)Enum.Parse(typeof(Operations), base.Operands[0].Value, ignoreCase: true);
		if (!Directory.Exists(StatisticsDirectory))
		{
			try
			{
				Directory.CreateDirectory(StatisticsDirectory);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unable to create statistics directory.", e);
				return;
			}
		}
		if (operations == Operations.Log)
		{
			string path = Path.Combine(StatisticsDirectory, "HAL-stats.log");
			if (File.Exists(path))
			{
				try
				{
					FileHelper.CreateBackup(path, BackupAction.Move);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("Unable to backup statistics file.", e2);
					return;
				}
			}
			using StreamWriter streamWriter = new StreamWriter(path);
			ControllerConfiguration.Instance.LogProblemLocations(streamWriter, 0);
			LogCounterValue(streamWriter, "CameraCaptureFailures", "Camera failure");
			LogCounterValue(streamWriter, "MCCorrectedCommErrors", "Corrected motion control communication failure");
			LogCounterValue(streamWriter, "MCCommFailures", "Motion control communication failure");
			return;
		}
		if (Operations.Clear == operations || Operations.LogUnknown != operations)
		{
			return;
		}
		string path2 = Path.Combine(StatisticsDirectory, "EmptyStuck.txt");
		if (File.Exists(path2))
		{
			try
			{
				FileHelper.CreateBackup(path2, BackupAction.Move);
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("Unable to backup summary file.", e3);
				return;
			}
		}
		using StreamWriter streamWriter2 = new StreamWriter(path2);
		streamWriter2.WriteLine("Dump of empty/stuck causing UNKNOWNs.");
		ControllerConfiguration.Instance.LogProblemLocations(streamWriter2, ControllerConfiguration.Instance.MarkLocationUnknownThreshold);
	}

	private void LogCounterValue(TextWriter writer, string counterName, string friendlyName)
	{
		Counter counter = CounterManager.Instance.Find(counterName);
		int num = 0;
		if (counter != null)
		{
			num = counter.Value;
		}
		writer.WriteLine("{0} counter has a value of {1}", friendlyName, num);
	}
}
