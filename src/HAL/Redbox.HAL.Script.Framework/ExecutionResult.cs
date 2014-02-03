using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public class ExecutionResult
{
	private ErrorList m_errors;

	public ErrorList Errors
	{
		get
		{
			if (m_errors == null)
			{
				m_errors = new ErrorList();
			}
			return m_errors;
		}
	}

	public TimeSpan? ExecutionTime { get; set; }

	public bool RestartAtCurrentIP { get; set; }

	public Instruction SwitchToContext { get; set; }

	public void Reset()
	{
		Errors.Clear();
		ExecutionTime = null;
		SwitchToContext = null;
		RestartAtCurrentIP = false;
	}

	public void UpdateExecutionTime(TimeSpan timeSpan)
	{
		if (!ExecutionTime.HasValue)
		{
			ExecutionTime = timeSpan;
		}
		else
		{
			ExecutionTime += timeSpan;
		}
	}
}
