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

	int TouchScreenResetsToAlert { get; }

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

	bool ViewActive { get; }

	bool ViewReporting { get; }

	int ViewHealingType { get; }

	int ViewInActivity { get; }

	int ViewInActivityABEMode { get; }

	bool CCReaderActive { get; }

	bool CCReaderSendNoDataEvents { get; }

	int CCReaderNoDataAttemptsToAlert { get; }

	bool CCReaderSendBadDataEvents { get; }

	int CCReaderBadDataAttemptsToAlert { get; }

	bool EMVReaderActive { get; }

	bool EMVReaderTrackMultipleFallbacks { get; }

	int EMVReaderMultipleFallbacksThreshold { get; }

	bool EMVReaderDisconnectAlertActive { get; }

	int EMVReaderDisconnectAlertVelocity { get; }

	int EMVReaderDisconnectSampleSizeHours { get; }

	int PingInterval { get; }

	bool PingServiceEnabled { get; }

	bool KioskServicesPingEnabled { get; }
}
