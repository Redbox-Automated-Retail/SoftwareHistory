using System;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Rental.UI.Controls;

namespace Redbox.Rental.UI.Views;

public partial class IdleTimeoutMessageViewUserControl : TextToSpeechUserControl, IComponentConnector
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

	public string ButtonNoText
	{
		get
		{
			return ButtonNo_Text.Text;
		}
		set
		{
			ButtonNo_Text.Text = value;
		}
	}

	public string ButtonYesTest
	{
		get
		{
			return ButtonYes_Text.Text;
		}
		set
		{
			ButtonYes_Text.Text = value;
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

	public event Action OnButtonYesClicked;

	public event Action OnButtonNoClicked;

	public IdleTimeoutMessageViewUserControl()
	{
		InitializeComponent();
		RegisterRoutedCommand("Yes", Commands.Yes);
		RegisterRoutedCommand("No", Commands.No);
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
			NoCommandBinding_Executed(this, null);
		}
	}

	private void YesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (OnButtonYesClicked != null)
		{
			OnButtonYesClicked();
		}
	}

	private void NoCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (OnButtonNoClicked != null)
		{
			OnButtonNoClicked();
		}
	}
}
