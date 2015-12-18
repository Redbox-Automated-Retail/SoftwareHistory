using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public sealed class ExecutionResult
{
	public readonly ErrorList Errors = new ErrorList();

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

	internal void AddMissingOperandError(string details)
	{
		AddError(ExecutionErrors.MissingOperand, details);
	}

	internal void AddInvalidOperandError(string details)
	{
		AddError(ExecutionErrors.InvalidOperand, details);
	}

	internal void AddError(ExecutionErrors error, string details)
	{
		Error item = Error.NewError(ExecutionErrorsTable.ErrorCodeTable[error], ExecutionErrorsTable.ErrorTable[error], details);
		Errors.Add(item);
	}
}
