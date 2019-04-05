using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Browse;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "CollectionMessageView")]
public partial class CollectionMessageViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private int m_timeout;

	private DispatcherTimer m_dispatcherTimer;

	private object m_lockObject = new object();

	private bool m_timerDisabled;

	private int TimeOut
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

	private CollectionMessageModel CollectionMessageModel => base.DataContext as CollectionMessageModel;

	public CollectionMessageViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(SeeAllCollectionsButton);
		obj?.SetStyle(CancelButton);
		obj?.SetStyle(DisplayCollection1);
		obj?.SetStyle(DisplayCollection2);
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
		if (flag)
		{
			CollectionMessageModel.ProcessTimeOut();
		}
	}

	private void CancelDialogCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		CollectionMessageModel?.PrcoessCancelDialog();
	}

	private void MoreCollectionsCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		CollectionMessageModel?.ProcessSeeAllCollections();
	}

	private void BrowseItemSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if ((e?.OriginalSource as FrameworkElement)?.DataContext is IBrowseItemModel browseItemModel)
		{
			CollectionMessageModel.ProcessCollectionSelected(browseItemModel, e.Parameter);
		}
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (CollectionMessageModel != null)
		{
			TimeOut = CollectionMessageModel.TimeOut;
		}
	}
}
