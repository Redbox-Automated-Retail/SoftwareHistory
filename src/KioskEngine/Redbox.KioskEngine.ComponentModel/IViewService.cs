using System;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewService
{
	ReadOnlyCollection<IViewFrame> Stack { get; }

	string LastHitActorName { get; set; }

	object LastHitActorTag { get; set; }

	event EventHandler ViewStateChanged;

	void Reset();

	void ShowFrameInDebug();

	void Show();

	string Pop();

	string Peek();

	void ClearCache();

	string PopDiscard();

	void EnterTopOfStock();

	string GetLastShownView();

	DateTime? GetViewShownTime();

	void ResetViewShownTime();

	int GetActiveHitHandlerCount();

	void Push(string viewName);

	IViewFrame PeekViewFrame();

	IViewFrame GetViewFrame(string viewName);

	bool IsInMaintenanceMode();

	bool IsSafeToChangeView();

	string GetStartOverViewName();

	void GoBack(object context, bool popOnly);

	void GoBackSafe(object context, bool popOnly);
}
