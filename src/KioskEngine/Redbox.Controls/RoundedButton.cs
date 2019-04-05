using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Redbox.Controls.Themes;
using Redbox.Core;

namespace Redbox.Controls;

public class RoundedButton : Button, IThemedControl
{
	public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UsedDefaultCornerRadiusProperty = DependencyProperty.Register("UsedDefaultCornerRadius", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsClickAnimatedProperty = DependencyProperty.Register("IsClickAnimated", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsClickBeforeAnimationProperty = DependencyProperty.Register("IsClickBeforeAnimation", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0, OnCornerRadiusPropertyChanged)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty CornerRadiusInnerProperty = DependencyProperty.Register("CornerRadiusInner", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DropShadowDepthProperty = DependencyProperty.Register("DropShadowDepth", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(5.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty GlassHighlightVisibilityProperty = DependencyProperty.Register("GlassHighlightVisibility", typeof(Visibility), typeof(RoundedButton), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty GlassHighlightWidthProperty = DependencyProperty.Register("GlassHighlightWidth", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0));

	public static readonly DependencyProperty GlassHighlightHeightProperty = DependencyProperty.Register("GlassHighlightHeight", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0));

	public static readonly DependencyProperty DropShadowOpacityProperty = DependencyProperty.Register("DropShadowOpacity", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(100.0));

	public static readonly DependencyProperty GlassHighlightCornerRadiusProperty = DependencyProperty.Register("GlassHighlightCornerRadius", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0));

	public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty BorderGradientColorProperty = DependencyProperty.Register("BorderGradientColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty BorderGradientStartPointProperty = DependencyProperty.Register("BorderGradientStartPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 0.0)));

	public static readonly DependencyProperty BorderGradientEndPointProperty = DependencyProperty.Register("BorderGradientEndPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 1.0)));

	public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty BackgroundGradientColorProperty = DependencyProperty.Register("BackgroundGradientColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Gray));

	public static readonly DependencyProperty BackgroundGradientStartPointProperty = DependencyProperty.Register("BackgroundGradientStartPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 0.0)));

	public static readonly DependencyProperty BackgroundGradientEndPointProperty = DependencyProperty.Register("BackgroundGradientEndPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 1.0)));

	public bool State
	{
		get
		{
			return (bool)GetValue(StateProperty);
		}
		set
		{
			SetValue(StateProperty, value);
		}
	}

	public double CornerRadiusInner
	{
		get
		{
			return (double)GetValue(CornerRadiusInnerProperty);
		}
		set
		{
			SetValue(CornerRadiusInnerProperty, value);
		}
	}

	public double CornerRadius
	{
		get
		{
			return (double)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	public double GlassHighlightWidth
	{
		get
		{
			return (double)GetValue(GlassHighlightWidthProperty);
		}
		set
		{
			SetValue(GlassHighlightWidthProperty, value);
		}
	}

	public double GlassHighlightHeight
	{
		get
		{
			return (double)GetValue(GlassHighlightHeightProperty);
		}
		set
		{
			SetValue(GlassHighlightHeightProperty, value);
		}
	}

	public double GlassHighlightCornerRadius
	{
		get
		{
			return (double)GetValue(GlassHighlightCornerRadiusProperty);
		}
		set
		{
			SetValue(GlassHighlightCornerRadiusProperty, value);
		}
	}

	public double DropShadowOpacity
	{
		get
		{
			return (double)GetValue(DropShadowOpacityProperty);
		}
		set
		{
			SetValue(DropShadowOpacityProperty, value);
		}
	}

	public double DropShadowDepth
	{
		get
		{
			return (double)GetValue(DropShadowDepthProperty);
		}
		set
		{
			SetValue(DropShadowDepthProperty, value);
		}
	}

	public Color BorderColor
	{
		get
		{
			return (Color)GetValue(BorderColorProperty);
		}
		set
		{
			SetValue(BorderColorProperty, value);
		}
	}

	public Color BorderGradientColor
	{
		get
		{
			return (Color)GetValue(BorderGradientColorProperty);
		}
		set
		{
			SetValue(BorderGradientColorProperty, value);
		}
	}

	public Point BorderGradientStartPoint
	{
		get
		{
			return (Point)GetValue(BorderGradientStartPointProperty);
		}
		set
		{
			SetValue(BorderGradientStartPointProperty, value);
		}
	}

	public Point BorderGradientEndPoint
	{
		get
		{
			return (Point)GetValue(BorderGradientEndPointProperty);
		}
		set
		{
			SetValue(BorderGradientEndPointProperty, value);
		}
	}

	public Color BackgroundColor
	{
		get
		{
			return (Color)GetValue(BackgroundColorProperty);
		}
		set
		{
			SetValue(BackgroundColorProperty, value);
		}
	}

	public Color BackgroundGradientColor
	{
		get
		{
			return (Color)GetValue(BackgroundGradientColorProperty);
		}
		set
		{
			SetValue(BackgroundGradientColorProperty, value);
		}
	}

	public Point BackgroundGradientStartPoint
	{
		get
		{
			return (Point)GetValue(BackgroundGradientStartPointProperty);
		}
		set
		{
			SetValue(BackgroundGradientStartPointProperty, value);
		}
	}

	public Point BackgroundGradientEndPoint
	{
		get
		{
			return (Point)GetValue(BackgroundGradientEndPointProperty);
		}
		set
		{
			SetValue(BackgroundGradientEndPointProperty, value);
		}
	}

	public Visibility GlassHighlightVisibility
	{
		get
		{
			return (Visibility)GetValue(GlassHighlightVisibilityProperty);
		}
		set
		{
			SetValue(GlassHighlightVisibilityProperty, value);
		}
	}

	public bool IsAnimated
	{
		get
		{
			return (bool)GetValue(IsClickAnimatedProperty);
		}
		set
		{
			SetValue(IsClickAnimatedProperty, value);
		}
	}

	public bool UsedDefaultCornerRadius
	{
		get
		{
			return (bool)GetValue(UsedDefaultCornerRadiusProperty);
		}
		set
		{
			SetValue(UsedDefaultCornerRadiusProperty, value);
		}
	}

	public bool IsClickBeforeAnimation
	{
		get
		{
			return (bool)GetValue(IsClickBeforeAnimationProperty);
		}
		set
		{
			SetValue(IsClickBeforeAnimationProperty, value);
		}
	}

	public event EventHandler OnAnimationComplete;

	public RoundedButton()
	{
		base.Focusable = false;
	}

	protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
		if (e.ClickCount > 1)
		{
			e.Handled = true;
		}
		base.OnPreviewMouseDown(e);
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		if (UsedDefaultCornerRadius)
		{
			CornerRadius = base.ActualHeight / 2.0;
		}
		AdjustGlassHighlight(this);
		AdjustCornRadiusInner(this);
	}

	private static void AdjustGlassHighlight(RoundedButton roundedButton)
	{
		roundedButton.GlassHighlightWidth = Math.Max(roundedButton.ActualWidth - roundedButton.CornerRadius, 0.0);
		roundedButton.GlassHighlightHeight = roundedButton.CornerRadius * 0.66;
		roundedButton.GlassHighlightCornerRadius = roundedButton.CornerRadius * 0.66;
	}

	protected override void OnClick()
	{
		if (IsAnimated)
		{
			_ = base.RenderTransformOrigin;
			if (base.RenderTransformOrigin.X == 0.0 && base.RenderTransformOrigin.Y == 0.0)
			{
				base.RenderTransformOrigin = new Point(0.5, 0.5);
			}
			DoubleAnimation doubleAnimation = new DoubleAnimation(0.95, 1.0, new Duration(TimeSpan.FromSeconds(0.2)));
			DoubleAnimation animation = new DoubleAnimation(0.95, 1.0, new Duration(TimeSpan.FromSeconds(0.2)));
			ScaleTransform scaleTransform = (ScaleTransform)(base.RenderTransform = new ScaleTransform());
			doubleAnimation.Completed += Animation_Completed;
			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
		}
		if (IsClickBeforeAnimation || !IsAnimated)
		{
			State = !State;
			LogButtonClick();
			base.OnClick();
		}
	}

	private void LogButtonClick()
	{
		string text = null;
		DependencyObject parent = VisualTreeHelper.GetParent(this);
		while (parent != null && !(parent is Window) && !(parent is UserControl))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		if (parent != null && parent is UserControl)
		{
			UserControl userControl = parent as UserControl;
			text = userControl?.Name;
			if (string.IsNullOrEmpty(text))
			{
				text = userControl?.GetType()?.ToString();
			}
		}
		else if (parent != null && parent is Window)
		{
			Window window = parent as Window;
			text = window?.Name;
			if (string.IsNullOrEmpty(text))
			{
				text = window?.GetType()?.ToString();
			}
		}
		string arg = "";
		if (!string.IsNullOrEmpty(base.Name))
		{
			arg = $"Button Name: {base.Name}";
		}
		else
		{
			string text2 = base.Tag?.ToString();
			if (!string.IsNullOrEmpty(text2))
			{
				arg = $"Tag: {text2.Substring(0, Math.Min(text2.Length, 50))}";
			}
		}
		LogHelper.Instance.Log($"RoundedButton click: Parent: {text}, {arg}");
	}

	private void Animation_Completed(object sender, EventArgs e)
	{
		if (!IsClickBeforeAnimation)
		{
			base.OnClick();
		}
		if (OnAnimationComplete != null)
		{
			OnAnimationComplete(this, null);
		}
	}

	private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RoundedButton obj = d as RoundedButton;
		AdjustGlassHighlight(obj);
		AdjustCornRadiusInner(obj);
		obj.InvalidateProperty(FrameworkElement.WidthProperty);
	}

	private static void AdjustCornRadiusInner(RoundedButton roundedButton)
	{
		double cornerRadius = roundedButton.CornerRadius;
		if (--cornerRadius <= 0.0)
		{
			cornerRadius = 0.0;
		}
		roundedButton.CornerRadiusInner = cornerRadius;
	}
}
