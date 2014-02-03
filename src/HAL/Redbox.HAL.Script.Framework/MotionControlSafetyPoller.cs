using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class MotionControlSafetyPoller : ScriptPoller
{
	private bool? m_pollerEnabled = null;

	internal static MotionControlSafetyPoller Instance => Singleton<MotionControlSafetyPoller>.Instance;

	protected override string ThreadName => "Motion Control Poller";

	protected override ProgramPriority Priority => ProgramPriority.Low;

	protected override string JobLabel => "None";

	protected override string JobName => string.Empty;

	protected override bool IsConfigured
	{
		get
		{
			if (DoorSensors.Instance.SensorsEnabled)
			{
				return EnableSafetyPoller;
			}
			return false;
		}
	}

	private int PollInterval { get; set; }

	private bool EnableSafetyPoller
	{
		get
		{
			if (m_pollerEnabled.HasValue)
			{
				return m_pollerEnabled.Value;
			}
			m_pollerEnabled = RegistryOptions.Instance.GetSecretValue<bool?>("SOFTWARE\\Redbox\\HAL", "EnableSafetyPoller") ?? false;
			return m_pollerEnabled.Value;
		}
	}

	protected override void ExecuteInner()
	{
		LogHelper.Instance.Log("The safety sensors aren't implemented.", LogEntryType.Info);
	}

	private MotionControlSafetyPoller()
	{
		PollInterval = RegistryOptions.Instance.GetSecretValue<int?>("SOFTWARE\\Redbox\\HAL", "MotionControlPollerFrequency") ?? 500;
	}
}
