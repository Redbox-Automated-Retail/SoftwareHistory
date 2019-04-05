using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class DisplayProductCarouselUserControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty ShowVacantItemBackgroundProperty = DependencyProperty.Register("ShowVacantItemBackground", typeof(bool), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ShaderOpacityProperty = DependencyProperty.Register("ShaderOpacity", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.25)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register("ShadowDepth", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register("ShadowOpacity", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OpacityMaskStartXProperty = DependencyProperty.Register("OpacityMaskStartX", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(1.0, OpacityMaskStartXChanged)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OpacityMaskStartPointProperty = DependencyProperty.Register("OpacityMaskStartPoint", typeof(Point), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(new Point(1.0, 0.0))
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OpacityMaskEndPointProperty = DependencyProperty.Register("OpacityMaskEndPoint", typeof(Point), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(new Point(1.0, 0.0))
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OpacityMaskGradientOpaqueStopProperty = DependencyProperty.Register("OpacityMaskGradientOpaqueStop", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty OpacityMaskGradientTransparentStopProperty = DependencyProperty.Register("OpacityMaskGradientTransparentStop", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	private DisplayProductModel _displayProductModel;

	public bool ShowVacantItemBackground
	{
		get
		{
			return (bool)GetValue(ShowVacantItemBackgroundProperty);
		}
		set
		{
			SetValue(ShowVacantItemBackgroundProperty, value);
		}
	}

	public double ShaderOpacity
	{
		get
		{
			return (double)GetValue(ShaderOpacityProperty);
		}
		set
		{
			SetValue(ShaderOpacityProperty, value);
		}
	}

	public double ShadowDepth
	{
		get
		{
			return (double)GetValue(ShadowDepthProperty);
		}
		set
		{
			SetValue(ShadowDepthProperty, value);
		}
	}

	public double ShadowOpacity
	{
		get
		{
			return (double)GetValue(ShadowOpacityProperty);
		}
		set
		{
			SetValue(ShadowOpacityProperty, value);
		}
	}

	public double OpacityMaskStartX
	{
		get
		{
			return (double)GetValue(OpacityMaskStartXProperty);
		}
		set
		{
			SetValue(OpacityMaskStartXProperty, value);
		}
	}

	public Point OpacityMaskStartPoint
	{
		get
		{
			return (Point)GetValue(OpacityMaskStartPointProperty);
		}
		set
		{
			SetValue(OpacityMaskStartPointProperty, value);
		}
	}

	public Point OpacityMaskEndPoint
	{
		get
		{
			return (Point)GetValue(OpacityMaskEndPointProperty);
		}
		set
		{
			SetValue(OpacityMaskEndPointProperty, value);
		}
	}

	public double OpacityMaskGradientOpaqueStop
	{
		get
		{
			return (double)GetValue(OpacityMaskGradientOpaqueStopProperty);
		}
		set
		{
			SetValue(OpacityMaskGradientOpaqueStopProperty, value);
		}
	}

	public double OpacityMaskGradientTransparentStop
	{
		get
		{
			return (double)GetValue(OpacityMaskGradientTransparentStopProperty);
		}
		set
		{
			SetValue(OpacityMaskGradientTransparentStopProperty, value);
		}
	}

	public DisplayProductCarouselUserControl()
	{
		InitializeComponent();
		ConfigureForLeftMove();
	}

	public void ConfigureForLeftMove()
	{
		OpacityMaskStartX = 1.0;
		OpacityMaskGradientOpaqueStop = 0.0;
		OpacityMaskGradientTransparentStop = 0.5;
		OpacityMaskEndPoint = new Point(1.01, 0.0);
	}

	public void ConfigureForRightMove()
	{
		OpacityMaskStartX = 0.0;
		OpacityMaskGradientOpaqueStop = 0.0;
		OpacityMaskGradientTransparentStop = 0.5;
		OpacityMaskEndPoint = new Point(-0.01, 0.0);
	}

	private static void OpacityMaskStartXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is DisplayProductCarouselUserControl displayProductCarouselUserControl)
		{
			displayProductCarouselUserControl.OpacityMaskStartPoint = new Point((double)e.NewValue, 0.0);
		}
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_displayProductModel = base.DataContext as DisplayProductModel;
		MainGrid.Visibility = ((_displayProductModel == null && !ShowVacantItemBackground) ? Visibility.Collapsed : Visibility.Visible);
		if (_displayProductModel == null && ShowVacantItemBackground)
		{
			MainGrid.Background = new SolidColorBrush(Colors.White);
		}
		else if (_displayProductModel != null && _displayProductModel.ADAMiniCartAddButtonVisibility == Visibility.Visible)
		{
			MainGrid.Background = new SolidColorBrush(Colors.Transparent);
		}
		UserControl_SizeChanged(null, null);
	}

	private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}
}
