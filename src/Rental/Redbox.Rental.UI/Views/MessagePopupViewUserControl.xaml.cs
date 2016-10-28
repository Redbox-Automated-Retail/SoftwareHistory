using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

public partial class MessagePopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private int m_timeout;

	private DispatcherTimer m_dispatcherTimer;

	private object m_lockObject = new object();

	private bool m_timerDisabled;

	public string Message
	{
		get
		{
			return MessageText.Text;
		}
		set
		{
			MessageText.Text = value;
		}
	}

	public string Title
	{
		get
		{
			return TitleText.Text;
		}
		set
		{
			TitleText.Text = value;
		}
	}

	public string Button1Text
	{
		get
		{
			return Button1_Text.Text;
		}
		set
		{
			Button1_Text.Text = value;
			Button1.Visibility = (string.IsNullOrEmpty(Button1_Text.Text) ? Visibility.Hidden : Visibility.Visible);
		}
	}

	public string Button2Text
	{
		get
		{
			return Button2_Text.Text;
		}
		set
		{
			Button2_Text.Text = value;
			Button2.Visibility = (string.IsNullOrEmpty(Button2_Text.Text) ? Visibility.Hidden : Visibility.Visible);
		}
	}

	public int Timeout
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

	public event Action OnButton1Clicked;

	public event Action OnButton2Clicked;

	public event Action OnTimeOut;

	public MessagePopupViewUserControl()
	{
		InitializeComponent();
		RegisterRoutedCommand("Button1", MessagePopupViewCommands.Button1);
		RegisterRoutedCommand("Button2", MessagePopupViewCommands.Button2);
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
			m_dispatcherTimer.Stop();
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
			OnTimeOut();
		}
	}

	private void Button1CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (OnButton1Clicked != null)
		{
			OnButton1Clicked();
		}
	}

	private void Button2CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (OnButton2Clicked != null)
		{
			OnButton2Clicked();
		}
	}
}
