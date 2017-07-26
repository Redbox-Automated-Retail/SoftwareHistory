using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class InsertDiscAnimationUserControl : UserControl, IComponentConnector
{
	private bool m_isAnimated;

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

	public InsertDiscAnimationUserControl()
	{
		InitializeComponent();
		RedboxDisc.AnimationScrollDirection = RedboxDiscUserControl.ScrollDirection.Left;
		if (MessageTextBlock.Text.StartsWith("Culture specific title"))
		{
			MessageTextBlock.Text = "Please insert your disc.";
		}
		IsAnimated = true;
	}

	public void BeginAnimation()
	{
		RedChevron.IsAnimated = true;
		RedboxDisc.IsAnimated = true;
	}

	public void StopAnimation()
	{
		RedChevron.IsAnimated = false;
		RedboxDisc.IsAnimated = false;
	}
}
