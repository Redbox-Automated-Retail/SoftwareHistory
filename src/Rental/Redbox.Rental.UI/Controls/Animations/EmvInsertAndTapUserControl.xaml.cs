using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class EmvInsertAndTapUserControl : UserControl, IComponentConnector
{
	public enum CardReadAnimations
	{
		ChipAndContactless,
		ChipOnly,
		ContactlessOnly
	}

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

	public CardReadAnimations ShowAnimations { get; set; }

	public bool OnlyShowChipAnimation { get; set; }

	public EmvInsertAndTapUserControl()
	{
		InitializeComponent();
	}

	public void BeginAnimation()
	{
		Storyboard storyboard = new Storyboard();
		storyboard.RepeatBehavior = RepeatBehavior.Forever;
		TranslateTransform value = new TranslateTransform();
		RotateTransform value2 = new RotateTransform();
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value);
		transformGroup.Children.Add(value2);
		CardAndHand.RenderTransform = transformGroup;
		TimeSpan timeSpan = TimeSpan.FromSeconds(0.75);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(0.5);
		TimeSpan timeSpan3 = timeSpan + timeSpan2;
		TimeSpan timeSpan4 = TimeSpan.FromSeconds(2.0);
		TimeSpan timeSpan5 = timeSpan3 + timeSpan4;
		TimeSpan timeSpan6 = TimeSpan.FromSeconds(0.25);
		TimeSpan timeSpan7 = timeSpan5 + timeSpan6;
		TimeSpan timeSpan8 = TimeSpan.FromSeconds(0.25);
		TimeSpan timeSpan9 = timeSpan7 + timeSpan8;
		if (ShowAnimations == CardReadAnimations.ChipOnly)
		{
			timeSpan9 = TimeSpan.Zero;
		}
		TimeSpan timeSpan10 = timeSpan9;
		TimeSpan timeSpan11 = TimeSpan.FromSeconds(0.75);
		TimeSpan timeSpan12 = TimeSpan.FromSeconds(0.5);
		TimeSpan timeSpan13 = timeSpan11 + timeSpan12;
		TimeSpan timeSpan14 = TimeSpan.FromSeconds(2.0);
		TimeSpan timeSpan15 = timeSpan13 + timeSpan14;
		TimeSpan timeSpan16 = TimeSpan.FromSeconds(0.25);
		TimeSpan timeSpan17 = timeSpan15 + timeSpan16;
		TimeSpan timeSpan18 = TimeSpan.FromSeconds(0.25);
		TimeSpan timeSpan19 = timeSpan17 + timeSpan18;
		timeSpan11 += timeSpan9;
		timeSpan12 += timeSpan9;
		timeSpan13 += timeSpan9;
		timeSpan14 += timeSpan9;
		timeSpan15 += timeSpan9;
		timeSpan16 += timeSpan9;
		timeSpan17 += timeSpan9;
		timeSpan18 += timeSpan9;
		timeSpan19 += timeSpan9;
		TimeSpan timeSpan20 = timeSpan19;
		TranslateTransform value3 = new TranslateTransform();
		RotateTransform value4 = new RotateTransform();
		TransformGroup transformGroup2 = new TransformGroup();
		transformGroup2.Children.Add(value3);
		transformGroup2.Children.Add(value4);
		PhoneAndHand.RenderTransform = transformGroup2;
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 80.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan),
			Value = 80.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan3),
			Value = -205.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 100.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan),
			Value = 100.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan3),
			Value = 28.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		if (ShowAnimations != CardReadAnimations.ChipOnly)
		{
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(timeSpan5),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(timeSpan7),
				Value = 0.0
			});
		}
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan3),
			Value = 8.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames4);
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames5 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan10),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan11),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan13),
			Value = -48.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames6 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan10),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan11),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan13),
			Value = -110.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames7 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		if (ShowAnimations != CardReadAnimations.ContactlessOnly)
		{
			doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan((ShowAnimations == CardReadAnimations.ChipOnly) ? TimeSpan.Zero : (timeSpan10 - TimeSpan.FromMilliseconds(10.0))),
				Value = 0.0
			});
			doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(timeSpan10),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(timeSpan15),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(timeSpan17),
				Value = 0.0
			});
		}
		else
		{
			doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
				Value = 0.0
			});
		}
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames7, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames7, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames7);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames5, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames5, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames5);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames6, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames6, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames6);
		switch (ShowAnimations)
		{
		case CardReadAnimations.ChipAndContactless:
			storyboard.Duration = timeSpan20;
			break;
		case CardReadAnimations.ChipOnly:
			storyboard.Duration = timeSpan19;
			break;
		case CardReadAnimations.ContactlessOnly:
			storyboard.Duration = timeSpan9;
			break;
		}
		storyboard.Begin();
	}

	public void StopAnimation()
	{
		CardAndHand.RenderTransform = null;
	}
}
