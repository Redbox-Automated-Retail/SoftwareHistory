using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Views;

public partial class MarketingScreenSaverUserControl : UserControl, IComponentConnector
{
	public static readonly RoutedEvent AnimationStartedEvent = EventManager.RegisterRoutedEvent("AnimationStarted", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(MarketingScreenSaverUserControl));

	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(MarketingScreenSaverUserControl), new PropertyMetadata(false, IsAnimationOnChanged));

	public bool IsAnimationOn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAnimationOnProperty, value);
			});
		}
	}

	public event RoutedEventHandler AnimationStarted
	{
		add
		{
			AddHandler(AnimationStartedEvent, value);
		}
		remove
		{
			RemoveHandler(AnimationStartedEvent, value);
		}
	}

	public MarketingScreenSaverUserControl()
	{
		InitializeComponent();
	}

	private static void IsAnimationOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is MarketingScreenSaverUserControl marketingScreenSaverUserControl)
		{
			if ((bool)e.NewValue && !(bool)e.OldValue)
			{
				(marketingScreenSaverUserControl.FindResource("GradientCycle") as Storyboard)?.Begin();
				(marketingScreenSaverUserControl.FindResource("ButtonPulseAnimation") as Storyboard)?.Begin();
				(marketingScreenSaverUserControl.FindResource("ButtonSizeAnimation") as Storyboard)?.Begin();
			}
			else if ((bool)e.OldValue && !(bool)e.NewValue)
			{
				(marketingScreenSaverUserControl.FindResource("GradientCycle") as Storyboard)?.Stop();
				(marketingScreenSaverUserControl.FindResource("ButtonPulseAnimation") as Storyboard)?.Stop();
				(marketingScreenSaverUserControl.FindResource("ButtonSizeAnimation") as Storyboard)?.Stop();
			}
		}
	}

	public void RaiseAnimationStarted()
	{
		RoutedEventArgs e = new RoutedEventArgs(AnimationStartedEvent);
		RaiseEvent(e);
	}
}
