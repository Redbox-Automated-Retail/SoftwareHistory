using System;

namespace Redbox.Rental.Model.KioskHealth;

public interface IKioskHealthConfiguration
{
	bool KioskHealthActive { get; }

	int KioskEventsUnloadInterval { get; }

	bool TouchScreenActive { get; }

	bool TouchScreenReporting { get; }

	bool TouchScreenHealing { get; }

	TimeSpan TouchScreenStartCheck { get; }

	TimeSpan TouchScreenEndCheck { get; }

	int TouchScreenInActivity { get; }

	bool ArcusActive { get; }

	bool ArcusReporting { get; }

	bool RouterActive { get; }

	bool RouterReporting { get; }

	bool RouterHealing { get; }

	int RouterResetRetryMinutes { get; }

	int RouterResetAfterXMessageFailures { get; }

	int RouterResetMaxAttempts { get; }

	bool HardwareCorrectionStatsActive { get; }

	bool HardwareCorrectionStatsReporting { get; }
}
