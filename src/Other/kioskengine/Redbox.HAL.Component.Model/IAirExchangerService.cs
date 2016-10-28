namespace Redbox.HAL.Component.Model;

internal interface IAirExchangerService
{
	ExchangerFanStatus FanStatus { get; }

	bool Configured { get; }

	AirExchangerStatus CheckStatus();

	void TurnOnFan();

	void TurnOffFan();

	void LatchOn();

	void LatchOff();

	AirExchangerStatus Reset();

	int PersistentFailureCount();

	bool ShouldRetry();

	void ResetFailureCount();

	void IncrementResetFailureCount();
}
