using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class VendDiscAnimationUserControl : UserControl, IVendDiscAnimationControl, IComponentConnector
{
	private bool _isAnimated;

	public VendViewType VendType { get; set; }

	public string TitleText
	{
		get
		{
			return TitleTextBlock.Text;
		}
		set
		{
			TitleTextBlock.Text = value;
		}
	}

	public string MessageText
	{
		get
		{
			return MessageTextBlock.Text;
		}
		set
		{
			MessageTextBlock.Text = value;
		}
	}

	public bool IsSignedIn { get; set; }

	public bool IsLoyaltySignedIn { get; set; }

	public bool IsPickupDisc
	{
		get
		{
			return VendDiscPickup.Visibility == Visibility.Visible;
		}
		set
		{
			if (value)
			{
				VendDiscPickup.Visibility = Visibility.Visible;
				VendDiscServe.Visibility = Visibility.Collapsed;
				TurnOffArrows();
			}
			else
			{
				VendDiscPickup.Visibility = Visibility.Collapsed;
				VendDiscServe.Visibility = Visibility.Visible;
				MessageGridBlock1.Visibility = ((!IsLoyaltySignedIn) ? Visibility.Collapsed : Visibility.Visible);
				MessageGridBlock2.Visibility = ((!IsSignedIn) ? Visibility.Collapsed : Visibility.Visible);
				TurnOnArrows();
			}
		}
	}

	public string[] VendDiscMessages
	{
		get
		{
			return new string[4] { MessageTextBlock1.Text, MessageTextBlock2.Text, MessageTextBlock3.Text, MessageTextBlock.Text };
		}
		set
		{
			TitleTextBlock.Text = ((value.Length != 0) ? value[0] : string.Empty);
			MessageTextBlock.Text = ((value.Length > 1) ? value[1] : string.Empty);
			MessageTextBlock1.Text = ((value.Length > 2) ? value[2] : string.Empty);
			MessageTextBlock2.Text = ((value.Length > 3) ? value[3] : string.Empty);
			MessageTextBlock3.Text = ((value.Length > 4) ? value[4] : string.Empty);
		}
	}

	public bool IsAnimated
	{
		get
		{
			return _isAnimated;
		}
		set
		{
			if (_isAnimated != value)
			{
				_isAnimated = value;
				if (value)
				{
					MessageCheck3.Visibility = Visibility.Visible;
					BeginAnimation();
				}
				else
				{
					StopAnimation();
				}
			}
		}
	}

	public VendDiscAnimationUserControl()
	{
		InitializeComponent();
		TurnOffArrows();
	}

	public void TurnOffArrows()
	{
		MessageCheck1.Visibility = Visibility.Hidden;
		MessageCheck2.Visibility = Visibility.Hidden;
		MessageCheck3.Visibility = Visibility.Hidden;
	}

	public void TurnOnArrows()
	{
		MessageCheck1.Visibility = Visibility.Visible;
		Timer timer = new Timer(1000.0);
		timer.Elapsed += delegate
		{
			Application.Current.Dispatcher.Invoke(() => MessageCheck2.Visibility = Visibility.Visible);
		};
		timer.Start();
	}

	public void BeginAnimation()
	{
		RedChevron.IsAnimated = true;
		RedboxDisc.IsAnimated = true;
	}

	public void StopAnimation()
	{
		RedChevron.RenderTransform = null;
		RedboxDisc.RenderTransform = null;
	}
}
