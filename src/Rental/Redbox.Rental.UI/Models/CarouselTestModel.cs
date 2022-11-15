using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class CarouselTestModel : DependencyObject
{
	public static readonly DependencyProperty CarouselProductsProperty = DependencyProperty.Register("CarouselProducts", typeof(List<DisplayProductModel>), typeof(CarouselTestModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(CarouselTestModel), new FrameworkPropertyMetadata(true, OnIsAnimationOnPropertyChanged));

	public static readonly DependencyProperty IsStartViewAdABTestProperty = DependencyProperty.Register("IsStartViewAdABTest", typeof(bool), typeof(CarouselTestModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsScreenSaverStartViewProperty = DependencyProperty.Register("IsScreenSaverStartView", typeof(bool), typeof(CarouselTestModel), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsDarkModeProperty = DependencyProperty.Register("IsDarkMode", typeof(bool), typeof(CarouselTestModel), new FrameworkPropertyMetadata(false));

	public List<DisplayProductModel> CarouselDisplayProductModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (List<DisplayProductModel>)GetValue(CarouselProductsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CarouselProductsProperty, value);
			});
		}
	}

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

	public bool IsStartViewAdABTest
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsStartViewAdABTestProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsStartViewAdABTestProperty, value);
			});
		}
	}

	public bool IsScreenSaverStartView
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsScreenSaverStartViewProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsScreenSaverStartViewProperty, value);
			});
		}
	}

	public bool IsDarkMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsDarkModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsDarkModeProperty, value);
			});
		}
	}

	public event BrowseItemModelEvent OnCarouselRotation;

	public event BrowseItemModelEvent OnDisplayProductModelSelected;

	public event Action<CarouselTestModel> OnIsAnimationOnChanged;

	public void ProcessCarouselRotation(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnCarouselRotation != null)
		{
			OnCarouselRotation(displayProductModel, parameter);
		}
	}

	public void ProcessDisplayProductModelSelected(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnDisplayProductModelSelected != null)
		{
			OnDisplayProductModelSelected(displayProductModel, parameter);
		}
	}

	private static void OnIsAnimationOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is CarouselTestModel { OnIsAnimationOnChanged: not null } carouselTestModel)
		{
			carouselTestModel.OnIsAnimationOnChanged(carouselTestModel);
		}
	}
}
