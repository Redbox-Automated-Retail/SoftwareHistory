using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "IdleTimeoutMessageView")]
public partial class IdleTimeoutMessageViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private int m_timeout;

	private DispatcherTimer m_dispatcherTimer;

	private object m_lockObject = new object();

	private bool m_timerDisabled;

	private IdleTimeoutMessageViewModel _idleTimeoutMessageViewModel;

	private int Timeout
	{
		get
		{
			return m_timeout;
		}
		set
		{
			m_timeout = value;
			if (m_dispatcherTimer != null)
			{
				m_dispatcherTimer.Stop();
			}
			m_dispatcherTimer = new DispatcherTimer();
			m_dispatcherTimer.Interval = new TimeSpan(m_timeout * 10000);
			m_dispatcherTimer.Tick += TimerTick;
			m_dispatcherTimer.Start();
		}
	}

	public IdleTimeoutMessageViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("Yes", Commands.Yes);
		RegisterRoutedCommand("No", Commands.No);
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(YesButton);
		obj?.SetStyle(NoButton);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return _idleTimeoutMessageViewModel.OnGetSpeechControl();
	}

	private void TimerTick(object sender, EventArgs e)
	{
		StopTimer(executeTimeoutEvent: true);
	}

	private void StopTimer(bool executeTimeoutEvent)
	{
		bool flag = false;
		lock (m_lockObject)
		{
			m_dispatcherTimer?.Stop();
			if (!m_timerDisabled)
			{
				m_timerDisabled = true;
				if (executeTimeoutEvent)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			NoCommandBinding_Executed(this, null);
		}
	}

	private void YesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		_idleTimeoutMessageViewModel?.ProcessOnButtonYesClicked();
	}

	private void NoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		_idleTimeoutMessageViewModel?.ProcessOnButtonNoClicked();
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_idleTimeoutMessageViewModel = base.DataContext as IdleTimeoutMessageViewModel;
		if (_idleTimeoutMessageViewModel != null)
		{
			Timeout = _idleTimeoutMessageViewModel.Timeout;
		}
		else
		{
			StopTimer(executeTimeoutEvent: false);
		}
	}
}
