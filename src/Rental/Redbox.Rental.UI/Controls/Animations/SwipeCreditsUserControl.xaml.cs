using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class SwipeCreditsUserControl : UserControl, ISwipeAnimationControl, IComponentConnector
{
	private bool m_isAnimated;

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

	public SwipeCreditsUserControl()
	{
		InitializeComponent();
		IsAnimated = true;
	}

	public void BeginAnimation()
	{
		Hand.IsAnimated = true;
	}

	public void StopAnimation()
	{
		Hand.RenderTransform = null;
	}
}
