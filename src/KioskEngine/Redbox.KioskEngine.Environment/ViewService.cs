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

	private readonly IDictionary<string, ViewFrame> m_viewFrameCache = new Dictionary<string, ViewFrame>();

	public static ViewService Instance => Singleton<ViewService>.Instance;

	public int ViewStackCount => InnerStack.Count;

	public ReadOnlyCollection<IViewFrame> Stack
	{
		get
		{
			List<IViewFrame> list = new List<IViewFrame>();
			foreach (IViewFrame item in InnerStack)
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
		ClearCache();
		RaiseViewStateChanged();
	}

	public void ShowFrameInDebug()
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		IViewFrame viewFrame = InnerStack.Peek();
		LogHelper.Instance.Log("Showing view '{0}' from TOS in DEBUG window: clear = {1}, viewWindow = {2}, width = {3}, height = {4}, scene = {5}, onEnter = {6}, onLeave = {7}.", viewFrame.ViewName, viewFrame.Clear, viewFrame.ViewWindow, viewFrame.Width, viewFrame.Height, viewFrame.SceneName, viewFrame.OnEnterResourceName, viewFrame.OnLeaveResourceName);
		viewFrame.UpdateScene();
		if (viewFrame.Errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm
			{
				Errors = viewFrame.Errors,
				Text = $"Errors for view resource: {viewFrame.ViewName}"
			};
			errorForm.Show();
			errorForm.BringToFront();
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
		IViewFrame viewFrame = InnerStack.Peek();
		service.Filter["view_name"] = viewFrame.ViewName;
		LogHelper.Instance.Log("Showing view '{0}' from TOS: clear = {1}, viewWindow = {2}, width = {3}, height = {4}, scene = {5}, onEnter = {6}, onLeave = {7}.", viewFrame.ViewName, viewFrame.Clear, viewFrame.ViewWindow, viewFrame.Width, viewFrame.Height, viewFrame.SceneName, viewFrame.OnEnterResourceName, viewFrame.OnLeaveResourceName);
		viewFrame.UpdateScene();
		if (shoppingSession != null)
		{
			ISessionAnalyticsService service5 = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
			if (service5 != null)
			{
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("viewName", viewFrame.ViewName);
				dictionary.Add("viewSequence", shoppingSession.ViewsShown.ToString());
				dictionary.Add("viewVersion", viewFrame.ViewVersion ?? "not defined");
				dictionary.Add("activeFlag", viewFrame.ActiveFlag ?? "not defined");
				dictionary.Add("viewCulture", service.Filter["culture"] ?? string.Empty);
				dictionary.Add("viewStart", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:ms"));
				service5.AddNode("views", "view");
				service5.AddAttributes("view", dictionary);
				service5.AddNode("view", "viewEvents");
			}
		}
		viewFrame.RaiseOnEnter();
		m_viewShownOn = DateTime.Now;
		m_lastShownView = viewFrame.ViewName;
		ITextToSpeechService service6 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service6 != null && service6.TTSEnabled)
		{
			service6.RunSpeechWorkflow("tts_" + viewFrame.ViewName);
		}
		if (viewFrame.Errors.ContainsError())
		{
			ErrorForm errorForm = new ErrorForm
			{
				Errors = viewFrame.Errors,
				Text = $"Errors for view resource: {viewFrame.ViewName}"
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
		IViewFrame viewFrame = InnerStack.Pop();
		LogHelper.Instance.Log("Pop view '{0}' from TOS; executing onLeave = {1}", viewFrame.ViewName, viewFrame.OnLeaveResourceName);
		viewFrame.RaiseOnLeave();
		IViewFrame viewFrame2 = InnerStack.Peek();
		if (viewFrame2 != null && !viewFrame2.Clear)
		{
			string text = Pop();
			LogHelper.Instance.Log("Redraw view '{0}' down the stack for transparent TOS view.", text);
			Show();
			Push(text);
			Show();
		}
		RaiseViewStateChanged();
		return viewFrame.ViewName;
	}

	public string Peek()
	{
		return InnerStack.PeekViewName();
	}

	public string Peek(int index)
	{
		string result = null;
		int num = 0;
		foreach (IViewFrame item in InnerStack)
		{
			if (index == num)
			{
				result = item?.ViewName;
				break;
			}
			num++;
		}
		return result;
	}

	public void ClearCache()
	{
		LogHelper.Instance.Log("Clearing ViewFrame cache.");
		m_viewFrameCache.Clear();
		RaiseViewStateChanged();
	}

	public string PopDiscard()
	{
		IViewFrame viewFrame = InnerStack.Pop();
		if (viewFrame != null)
		{
			RaiseViewStateChanged();
			viewFrame.RaiseOnLeave();
			return viewFrame.ViewName;
		}
		return null;
	}

	public void EnterTopOfStock()
	{
		IViewFrame viewFrame = InnerStack.Peek();
		if (viewFrame != null)
		{
			viewFrame.Scene.MakeDirty(null);
			viewFrame.RaiseOnEnter();
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
		InnerStack.Peek()?.RaiseOnLeave();
		InnerStack.Push(viewName);
		LogHelper.Instance.Log("Push view '{0}' to TOS.", viewName);
		RaiseViewStateChanged();
	}

	public IViewFrame PeekViewFrame()
	{
		return InnerStack.Peek();
	}

	public IViewFrame GetViewFrame(string viewName)
	{
		if (m_viewFrameCache.ContainsKey(viewName))
		{
			return m_viewFrameCache[viewName];
		}
		LogHelper.Instance.Log("Cache miss, building cached ViewFrame for view named: {0}", viewName);
		ViewFrame viewFrame = new ViewFrame
		{
			ViewName = viewName
		};
		viewFrame.Parse();
		m_viewFrameCache[viewName] = viewFrame;
		return viewFrame;
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
