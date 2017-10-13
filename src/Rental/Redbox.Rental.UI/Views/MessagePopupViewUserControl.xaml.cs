using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public partial class MessagePopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private int m_timeout;

	private DispatcherTimer m_dispatcherTimer;

	private object m_lockObject = new object();

	private bool m_timerDisabled;

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
			if (m_timeout > 0)
			{
				m_dispatcherTimer.Start();
			}
		}
	}

	private MessagePopupViewModel Model => base.DataContext as MessagePopupViewModel;

	public MessagePopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("Button1", MessagePopupViewCommands.Button1);
		RegisterRoutedCommand("Button2", MessagePopupViewCommands.Button2);
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(Button1);
		obj?.SetStyle(Button2);
	}

	public void StopTimer()
	{
		StopTimer(executeTimeoutEvent: false);
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
		if (flag && Model?.OnTimeOut != null)
		{
			Model.OnTimeOut();
		}
	}

	private void Button1CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (Model?.OnButton1Clicked != null)
		{
			Model.OnButton1Clicked();
		}
	}

	private void Button2CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (Model?.OnButton2Clicked != null)
		{
			Model.OnButton2Clicked();
		}
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (Model != null)
		{
			Timeout = Model.TimeOut;
			if (Model.ContentUserControl != null)
			{
				UserControlContainer.Children.Clear();
				UserControlContainer.Children.Add(Model.ContentUserControl);
			}
		}
	}
}
