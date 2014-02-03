using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingSessionService : IDisposable
{
	void Reset();

	void Clear();

	string StartNewSession(string storeNumber, string sessionId);

	void AbandonAll(string reason);

	void ExporToXml(string fileName);

	ErrorList Initialize(string path);

	void SetCurrentSession(string id);

	IShoppingSession GetCurrentSession();

	IShoppingSession GetSession(string id);

	void ForEach(Action<IShoppingSession> action);
}
