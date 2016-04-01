using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class SessionErrorState
{
	private int m_lowerLimitCounter;

	internal static SessionErrorState Instance => Singleton<SessionErrorState>.Instance;

	internal bool LowerLimitConfigured(ExecutionContext context)
	{
		context.AppLog.WriteFormatted("LowerLimitConfigured check on program {0}; counter = {1}", context.ProgramName, m_lowerLimitCounter);
		if (ControllerConfiguration.Instance.LowerLimitAsError)
		{
			return context.ProgramName == "vend";
		}
		return false;
	}

	internal void IncrementLowerLimit()
	{
		m_lowerLimitCounter++;
	}

	internal void LogLimitMessage(ApplicationLog log)
	{
		log.WriteFormatted("Lower limit counter threshold exceeded (counter = {0}).", m_lowerLimitCounter);
	}

	internal bool LowerLimitCounterExceeded()
	{
		return m_lowerLimitCounter >= 2;
	}

	private SessionErrorState()
	{
	}
}
