using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ViewService : IViewService
{
	private string m_lastShownView;

	private DateTime? m_viewShownOn;

	private readonly IDictionary<string, IBaseViewFrame> m_viewFrameCache = new Dictionary<string, IBaseViewFrame>();

	public static ViewService Instance => Singleton<ViewService>.Instance;

	public int ViewStackCount => InnerStack.Count;

	public ReadOnlyCollection<IViewFrameInstance> Stack
	{
		get
		{
			List<IViewFrameInstance> list = new List<IViewFrameInstance>();
			foreach (IViewFrameInstance item in InnerStack)
			{
				list.Add(item);
			}
			return list.AsReadOnly();
		}
	}

	public string LastHitActorName { get; set; }

	public object LastHitActorTag { get; set; }

	internal ViewStack InnerStack { get; private set; }

	public event EventHandler ViewStateChanged;

	public void Reset()
	{
		InnerStack.Clear();
		ClearCache(clearResourceViewsOnly: true);
		RaiseViewStateChanged();
	}

	public void ShowFrameInDebug()
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		LogShowViewFrameInstance(viewFrameInstance);
		viewFrameInstance.ViewFrame.UpdateScene(viewFrameInstance);
		if (viewFrameInstance.ViewFrame.Errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm
			{
				Errors = viewFrameInstance?.ViewFrame?.Errors,
				Text = $"Errors for view resource: {viewFrameInstance?.ViewFrame?.ViewName}"
			};
			errorForm.Show();
			errorForm.BringToFront();
		}
	}

	private void LogShowViewFrameInstance(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance?.ViewFrame is IViewFrame viewFrame)
		{
			LogHelper.Instance.Log("Showing view '{0}' from TOS: clear = {1}, viewWindow = {2}, width = {3}, height = {4}, scene = {5}, onEnter = {6}, onLeave = {7}.", viewFrame.ViewName, viewFrame.Clear, viewFrame.ViewWindow, viewFrame.Width, viewFrame.Height, viewFrame.SceneName, viewFrame.OnEnterResourceName, viewFrame.OnLeaveResourceName);
		}
		else if (viewFrameInstance?.ViewFrame is IWPFViewFrame iWPFViewFrame)
		{
			LogHelper.Instance.Log("Showing view '{0}' from TOS", iWPFViewFrame.ViewName);
		}
	}

	public void Show()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IShoppingSession shoppingSession = null;
		IShoppingSessionService service2 = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service2 != null)
		{
			shoppingSession = service2.GetCurrentSession();
			if (shoppingSession != null)
			{
				shoppingSession.ViewsShown++;
			}
		}
		if (shoppingSession != null && m_viewShownOn.HasValue)
		{
			ISessionAnalyticsService service3 = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			if (service3 != null)
			{
				service3.AddAttribute("view", "viewEnd", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ms"));
				DateTime now = DateTime.Now;
				DateTime? viewShownOn = m_viewShownOn;
				service3.AddAttribute("view", "viewDuration", (now - viewShownOn).ToString());
			}
			m_lastShownView = null;
		}
		IRenderingService service4 = ServiceLocator.Instance.GetService<IRenderingService>();
		if (InnerStack.Peek() == null)
		{
			LogHelper.Instance.Log("View stack is empty.");
			service.Filter["view_name"] = null;
			service4.ActiveScene.SuspendDrawing();
			service4.ActiveScene.Clear();
			service4.ActiveScene.ResumeDrawing();
			shoppingSession?.AddEvent(ShoppingSessionEventType.ShowView, "View stack empty.");
			return;
		}
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		service.Filter["view_name"] = viewFrameInstance?.ViewFrame?.ViewName;
		LogShowViewFrameInstance(viewFrameInstance);
		viewFrameInstance.ViewFrame.UpdateScene(viewFrameInstance);
		if (shoppingSession != null)
		{
			ISessionAnalyticsService service5 = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			if (service5 != null)
			{
				IViewFrame viewFrame = viewFrameInstance?.ViewFrame as IViewFrame;
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("viewName", viewFrameInstance.ViewFrame.ViewName);
				dictionary.Add("viewSequence", shoppingSession.ViewsShown.ToString());
				dictionary.Add("viewVersion", viewFrame?.ViewVersion ?? "not defined");
				dictionary.Add("activeFlag", viewFrameInstance?.ViewFrame?.ActiveFlag ?? "not defined");
				dictionary.Add("viewCulture", service.Filter["culture"] ?? string.Empty);
				dictionary.Add("viewStart", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ms"));
				service5.AddNode("views", "view");
				service5.AddAttributes("view", dictionary);
				service5.AddNode("view", "viewEvents");
			}
		}
		if (!viewFrameInstance.ViewFrame.RaiseOnEnter(viewFrameInstance))
		{
			LogHelper.Instance.Log("No on enter script specified for view named: {0}", viewFrameInstance.ViewFrame.ViewName);
		}
		m_viewShownOn = DateTime.Now;
		m_lastShownView = viewFrameInstance.ViewFrame.ViewName;
		TTSRunSpeech(viewFrameInstance);
		ShowErrors(viewFrameInstance);
	}

	private void TTSRunSpeech(IViewFrameInstance viewFrameInstance)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			service.RunSpeechWorkflow("tts_" + viewFrameInstance.ViewFrame.ViewName);
		}
	}

	private void ShowErrors(IViewFrameInstance viewFrameInstance)
	{
		if (viewFrameInstance.ViewFrame.Errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm
			{
				Errors = viewFrameInstance.ViewFrame.Errors,
				Text = $"Errors for view resource: {viewFrameInstance.ViewFrame.ViewName}"
			};
			errorForm.Show();
			errorForm.BringToFront();
			RaiseViewStateChanged();
		}
	}

	public string Pop()
	{
		if (InnerStack.Peek() == null)
		{
			LogHelper.Instance.Log("View stack is empty; nothing to pop.");
			return null;
		}
		IViewFrameInstance viewFrameInstance = InnerStack.Pop();
		if (viewFrameInstance.ViewFrame is IViewFrame viewFrame)
		{
			LogHelper.Instance.Log("Pop view '{0}' from TOS; executing onLeave = {1}", viewFrameInstance.ViewFrame.ViewName, viewFrame.OnLeaveResourceName);
		}
		viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
		if (InnerStack.Peek()?.ViewFrame is IViewFrame { Clear: false })
		{
			string text = Pop();
			LogHelper.Instance.Log("Redraw view '{0}' down the stack for transparent TOS view.", text);
			Show();
			Push(text);
			Show();
		}
		RaiseViewStateChanged();
		return viewFrameInstance.ViewFrame.ViewName;
	}

	public string Peek()
	{
		return InnerStack.PeekViewName();
	}

	public string Peek(int index)
	{
		string result = null;
		int num = 0;
		foreach (IViewFrameInstance item in InnerStack)
		{
			if (index == num)
			{
				result = item?.ViewFrame?.ViewName;
				break;
			}
			num++;
		}
		return result;
	}

	public void ClearCache(bool clearResourceViewsOnly)
	{
		LogHelper.Instance.Log("Clearing ViewFrame cache. ResourceViewsOnly: {0}", clearResourceViewsOnly);
		if (!clearResourceViewsOnly)
		{
			m_viewFrameCache.Clear();
		}
		else
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, IBaseViewFrame> item in m_viewFrameCache)
			{
				if (item.Value is IViewFrame)
				{
					list.Add(item.Key);
				}
			}
			foreach (string item2 in list)
			{
				if (m_viewFrameCache.ContainsKey(item2))
				{
					m_viewFrameCache.Remove(item2);
				}
			}
		}
		RaiseViewStateChanged();
	}

	public string PopDiscard()
	{
		IViewFrameInstance viewFrameInstance = InnerStack.Pop();
		if (viewFrameInstance?.ViewFrame != null)
		{
			RaiseViewStateChanged();
			viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
			return viewFrameInstance.ViewFrame.ViewName;
		}
		return null;
	}

	public void EnterTopOfStock()
	{
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		if (viewFrameInstance != null)
		{
			if (viewFrameInstance.ViewFrame is IViewFrame viewFrame)
			{
				viewFrame.Scene.MakeDirty(null);
			}
			viewFrameInstance.ViewFrame.RaiseOnEnter(viewFrameInstance);
			RaiseViewStateChanged();
		}
	}

	public string GetLastShownView()
	{
		return m_lastShownView;
	}

	public DateTime? GetViewShownTime()
	{
		return m_viewShownOn;
	}

	public void ResetViewShownTime()
	{
		m_viewShownOn = DateTime.Now;
	}

	public int GetActiveHitHandlerCount()
	{
		int result = 0;
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service != null)
		{
			result = service.ActiveScene.ActiveHitHandlers;
		}
		return result;
	}

	public void Push(string viewName)
	{
		Push(viewName, null);
	}

	public void Push(string viewName, object parameter)
	{
		IViewFrameInstance viewFrameInstance = InnerStack.Peek();
		if (viewFrameInstance?.ViewFrame != null)
		{
			viewFrameInstance.ViewFrame.RaiseOnLeave(viewFrameInstance);
		}
		Guid guid = Guid.NewGuid();
		InnerStack.Push(guid, viewName, parameter);
		LogHelper.Instance.Log("Push view '{0}' {1} to TOS.", viewName, guid);
		RaiseViewStateChanged();
	}

	public IViewFrameInstance PeekViewFrame()
	{
		return InnerStack.Peek();
	}

	public void CacheViewFrame(IBaseViewFrame baseViewFrame)
	{
		if (baseViewFrame != null && !string.IsNullOrEmpty(baseViewFrame.ViewName))
		{
			m_viewFrameCache[baseViewFrame.ViewName] = baseViewFrame;
		}
	}

	public IBaseViewFrame GetViewFrame(string viewName)
	{
		IBaseViewFrame baseViewFrame = null;
		if (m_viewFrameCache.ContainsKey(viewName))
		{
			baseViewFrame = m_viewFrameCache[viewName];
		}
		else
		{
			LogHelper.Instance.Log("Cache miss, building cached ViewFrame for view named: {0}", viewName);
			ViewFrame viewFrame = new ViewFrame
			{
				ViewName = viewName
			};
			viewFrame.Parse();
			baseViewFrame = viewFrame;
			if (baseViewFrame != null)
			{
				m_viewFrameCache[viewName] = baseViewFrame;
			}
		}
		return baseViewFrame;
	}

	public bool IsInMaintenanceMode()
	{
		return Peek()?.Contains("maintenance_view") ?? false;
	}

	public bool IsSafeToChangeView()
	{
		return !IsInMaintenanceMode();
	}

	public string GetStartOverViewName()
	{
		return "start_view";
	}

	public void GoBack(object context, bool popOnly)
	{
		LogHelper.Instance.Log("> view_goback; context: {0}, {1}", context, popOnly);
		if (Peek() == GetStartOverViewName())
		{
			Show();
			return;
		}
		Pop();
		if (!popOnly)
		{
			Show();
		}
	}

	public void GoBackSafe(object context, bool popOnly)
	{
		if (IsSafeToChangeView())
		{
			GoBack(context, popOnly);
			return;
		}
		LogHelper.Instance.Log("view_goback_safe({0}) denied.", context);
	}

	public void UnwindToSafe(string unwindToViewName, bool discard, object context)
	{
		if (IsSafeToChangeView())
		{
			UnwindTo(unwindToViewName, discard, context);
		}
		else
		{
			LogHelper.Instance.Log(string.Concat("UnwindToSafe( ", context, " ) denied."));
		}
	}

	public void UnwindTo(string unwindToViewName, bool discard, object context)
	{
		string text = Peek();
		if (!(text != unwindToViewName) || text == null)
		{
			return;
		}
		while (text != null && text != unwindToViewName)
		{
			if (discard)
			{
				LogHelper.Instance.Log("> PopViewDiscard {0}; context: {1}", text, context);
				PopDiscard();
			}
			else
			{
				LogHelper.Instance.Log("> PopView {0}; context: {1}", text, context);
				Pop();
			}
			text = Peek();
		}
		Show();
	}

	private ViewService()
	{
		InnerStack = new ViewStack();
	}

	private void RaiseViewStateChanged()
	{
		if (ViewStateChanged != null)
		{
			ViewStateChanged(this, EventArgs.Empty);
		}
	}
}
