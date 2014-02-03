using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingSession
{
	Guid Id { get; }

	DateTime? EndedOn { get; }

	DateTime StartedOn { get; }

	string StoreNumber { get; }

	int ViewsShown { get; set; }

	IShoppingCart ShoppingCart { get; }

	ShoppingSessionStatus Status { get; }

	void End();

	void Save();

	void Delete();

	void AddEvent(ShoppingSessionEventType type, string description);

	void AddError(string code, string description, string details);

	void AddWarning(string code, string description, string details);

	bool IsSaved();

	void Commit(string reason);

	void Abandon(string reason);

	void LoadShoppingCart();

	void ForEachEvent(Action<IShoppingSessionEvent> action);

	void ForEachError(Action<IShoppingSessionError> action);
}
