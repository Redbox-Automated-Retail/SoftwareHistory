using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class RedChevronUserControl : UserControl, IComponentConnector
{
	private bool m_isAnimated;

	public bool IsAnimated
	{
		get
		{
			return m_isAnimated;
		}
		set
		{
			if (m_isAnimated != value)
			{
				m_isAnimated = value;
				if (value)
				{
					BeginAnimation();
				}
				else
				{
					StopAnimation();
				}
			}
		}
	}

	public RedChevronUserControl()
	{
		InitializeComponent();
	}

	public void BeginAnimation()
	{
		DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, 150.0, new Duration(TimeSpan.FromSeconds(3.0)));
		doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		TranslateTransform translateTransform = new TranslateTransform();
		ChevronContainer.RenderTransform = translateTransform;
		translateTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
	}

	public void StopAnimation()
	{
		ChevronContainer.RenderTransform = null;
	}
}
