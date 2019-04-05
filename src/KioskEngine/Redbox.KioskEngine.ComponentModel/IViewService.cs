using System;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewService
{
	int ViewStackCount { get; }

	ReadOnlyCollection<IViewFrameInstance> Stack { get; }

	string LastHitActorName { get; set; }

	object LastHitActorTag { get; set; }

	event EventHandler ViewStateChanged;

	void Reset();

	void ShowFrameInDebug();

	void Show();

	string Pop();

	string Peek();

	string Peek(int index);

	void ClearCache(bool clearResourceViewsOnly);

	string PopDiscard();

	void EnterTopOfStock();

	string GetLastShownView();

	DateTime? GetViewShownTime();

	void ResetViewShownTime();

	int GetActiveHitHandlerCount();

	void Push(string viewName, object parameter, bool isCriticalErrorView = false);

	void Push(string viewName);

	void PushCriticalErrorView(string viewName, object parameter);

	IViewFrameInstance PeekViewFrame();

	IBaseViewFrame GetViewFrame(string viewName);

	bool IsInMaintenanceMode();

	bool IsSafeToChangeView();

	string GetStartOverViewName();

	void GoBack(object context, bool popOnly);

	void GoBackSafe(object context, bool popOnly);

	void UnwindTo(string viewName, bool discard, object context);

	void UnwindToSafe(string unwindToViewName, bool discard, object context);

	void CacheViewFrame(IBaseViewFrame baseViewFrame);
}
