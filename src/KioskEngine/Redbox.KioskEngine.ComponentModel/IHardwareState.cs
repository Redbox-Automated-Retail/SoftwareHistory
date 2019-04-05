using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IHardwareState
{
	int CurrentCameraGeneration { get; }

	int BarcodeDecoder { get; }

	int MaxDeck { get; }

	int MaxSlot { get; }

	string ExchangerStatus { get; }

	string AirExchangerFanStatus { get; }

	IEnumerable<Tuple<int, int>> ExcludedLocations { get; }

	bool IsInitialized { get; }

	void Initialize();

	void Refresh();

	void EnqueueProperties();
}
