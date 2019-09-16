using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class CarouselTestModel : DependencyObject
{
	public static readonly DependencyProperty CarouselProductsProperty = DependencyProperty.Register("CarouselProducts", typeof(List<DisplayProductModel>), typeof(CarouselTestModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TitleRollupOverlayModelProperty = DependencyProperty.Register("TitleRollupOverlayModel", typeof(TitleRollupOverlayModel), typeof(CarouselTestModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(CarouselTestModel), new FrameworkPropertyMetadata(true, OnIsAnimationOnPropertyChanged));

	public static readonly DependencyProperty IsStartViewAdABTestProperty = DependencyProperty.Register("IsStartViewAdABTest", typeof(bool), typeof(CarouselTestModel), new FrameworkPropertyMetadata(false));

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

	public TitleRollupOverlayModel TitleRollupOverlayModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TitleRollupOverlayModel)GetValue(TitleRollupOverlayModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(TitleRollupOverlayModelProperty, value);
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

	public event BrowseItemModelEvent OnCarouselRotation;

	public event BrowseItemModelEvent OnDisplayProductModelSelected;

	public event BrowseItemModelEvent OnAddDisplayProductModel;

	public event Action<CarouselTestModel> OnIsAnimationOnChanged;

	public void ProcessAddDisplayProductModel(DisplayProductModel displayProductModel, object parameter)
	{
		if (OnAddDisplayProductModel != null)
		{
			OnAddDisplayProductModel(displayProductModel, parameter);
		}
	}

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
