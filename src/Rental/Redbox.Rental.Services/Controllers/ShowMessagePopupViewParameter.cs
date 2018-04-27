using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Services.Controllers;

public class ShowMessagePopupViewParameter : IViewAnalyticsName
{
	public string TitleText { get; }

	public string TitleStyle { get; }

	public string MessageText { get; }

	public string MessageStyle { get; }

	public string Button1Text { get; }

	public string Button1TextStyle { get; }

	public string Button1Style { get; }

	public string Button2Text { get; }

	public string Button2TextStyle { get; }

	public string Button2Style { get; }

	public int Timeout { get; }

	public Action Button1Action { get; }

	public Action Button2Action { get; }

	public Action TimeOutAction { get; }

	public string ActorTag { get; }

	public bool HideBackground { get; }

	public UserControl ContentUserControl { get; }

	public string BorderBrushColor { get; }

	public Thickness MainGridMargin { get; }

	public List<string> AnalyticsContextList { get; }

	public string ViewAnalyticsName { get; }

	public ShowMessagePopupViewParameter(string titleText, string messageText, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, string analyticsContext = null)
		: this(titleText, null, messageText, null, button1Text, button2Text, timeout, button1Action, button2Action, timeOutAction, actorTag, analyticsContext)
	{
	}

	public ShowMessagePopupViewParameter(string titleText, string messageText, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, List<string> analyticsContextList = null)
		: this(titleText, null, messageText, null, button1Text, button2Text, timeout, button1Action, button2Action, timeOutAction, actorTag, analyticsContextList)
	{
	}

	public ShowMessagePopupViewParameter(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, string analyticsContext = null)
		: this(titleText, titleStyle, messageText, messageStyle, button1Text, null, null, button2Text, null, null, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground: false, null, null, default(Thickness), analyticsContext)
	{
	}

	public ShowMessagePopupViewParameter(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button2Text, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, List<string> analyticsContextList = null)
		: this(titleText, titleStyle, messageText, messageStyle, button1Text, null, null, button2Text, null, null, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground: false, null, null, default(Thickness), analyticsContextList)
	{
	}

	public ShowMessagePopupViewParameter(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, bool hideBackground, UserControl contentUserControl, string borderBrushColor, Thickness mainGrifMargin, string analyticsContext = null, string viewName = null)
		: this(titleText, titleStyle, messageText, messageStyle, button1Text, button1TextStyle, button1Style, button2Text, button2TextStyle, button2Style, timeout, button1Action, button2Action, timeOutAction, actorTag, hideBackground, contentUserControl, borderBrushColor, mainGrifMargin, (analyticsContext == null) ? null : new List<string> { analyticsContext }, viewName)
	{
	}

	public ShowMessagePopupViewParameter(string titleText, string titleStyle, string messageText, string messageStyle, string button1Text, string button1TextStyle, string button1Style, string button2Text, string button2TextStyle, string button2Style, int timeout, Action button1Action, Action button2Action, Action timeOutAction, string actorTag, bool hideBackground, UserControl contentUserControl, string borderBrushColor, Thickness mainGrifMargin, List<string> analyticsContextList = null, string viewName = null)
	{
		TitleText = titleText;
		TitleStyle = titleStyle;
		MessageText = messageText;
		MessageStyle = messageStyle;
		Button1Text = button1Text;
		Button1TextStyle = button1TextStyle;
		Button1Style = button1Style;
		Button2Text = button2Text;
		Button2TextStyle = button2TextStyle;
		Button2Style = button2Style;
		Timeout = timeout;
		Button1Action = button1Action;
		Button2Action = button2Action;
		TimeOutAction = timeOutAction;
		ActorTag = actorTag;
		HideBackground = hideBackground;
		ContentUserControl = contentUserControl;
		BorderBrushColor = borderBrushColor;
		MainGridMargin = mainGrifMargin;
		ViewAnalyticsName = viewName;
		AnalyticsContextList = analyticsContextList;
	}
}
