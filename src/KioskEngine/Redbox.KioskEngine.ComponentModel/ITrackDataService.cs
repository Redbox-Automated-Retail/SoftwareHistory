using System;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel;

public interface ITrackDataService
{
	event EventHandler Notify;

	int GetKeyId();

	void ClearData();

	void ClearNotify();

	string GetNumber();

	string GetTrack1();

	string GetTrack2();

	bool HasValidData();

	string GetLastFour();

	string GetLastName();

	CardType? GetCardType();

	string GetFirstName();

	string GetExpiryYear();

	string GetExpiryMonth();

	void StopTrackParsing();

	void StartTrackParsing();

	ErrorList GetTrackErrors();

	string GenerateCardHashId();
}
