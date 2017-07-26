using System;
using System.Collections.Generic;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class CarouselModel : DependencyObject
{
	public static readonly DependencyProperty CarouselProductsProperty = DependencyProperty.Register("CarouselProducts", typeof(List<DisplayProductModel>), typeof(CarouselModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty TitleRollupOverlayModelProperty = DependencyProperty.Register("TitleRollupOverlayModel", typeof(TitleRollupOverlayModel), typeof(CarouselModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(CarouselModel), new FrameworkPropertyMetadata(true, OnIsAnimationOnPropertyChanged));

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

	public event DisplayProductModelEvent OnCarouselRotation;

	public event DisplayProductModelEvent OnDisplayProductModelSelected;

	public event DisplayProductModelEvent OnAddDisplayProductModel;

	public event DisplayProductModelEvent OnTitleRollupOverlayCancel;

	public event DisplayProductModelEvent OnTitleRollupOverlayTimeout;

	public event DisplayProductModelEvent OnTitleRollupOverlayAddDVD;

	public event DisplayProductModelEvent OnTitleRollupOverlayAddBluray;

	public event VisibilityChanged OnTitleRollupOverlayVisibilityChange;

	public event Action<CarouselModel> OnIsAnimationOnChanged;

	public CarouselModel()
	{
		TitleRollupOverlayModel = new TitleRollupOverlayModel();
		TitleRollupOverlayModel.OnTitleRollupOverlayCancel += TitleRollupOverlayModel_OnTitleRollupOverlayCancel;
		TitleRollupOverlayModel.OnTitleRollupOverlayTimeout += TitleRollupOverlayModel_OnTitleRollupOverlayTimeout;
		TitleRollupOverlayModel.OnTitleRollupOverlayAddDVD += TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD;
		TitleRollupOverlayModel.OnTitleRollupOverlayAddBluray += TitleRollupOverlayModel_OnTitleRollupOverlayAddBluray;
		TitleRollupOverlayModel.OnTitleRollupOverlayVisibilityChange += TitleRollupOverlayModel_OnTitleRollupOverlayVisibilityChange;
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayVisibilityChange(Visibility newVisibility)
	{
		ProcessTitleRollupVisibilityChange(newVisibility);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayAddBluray(DisplayProductModel displayProductModel)
	{
		ProcessTitleRollupOverlayAddBluray(displayProductModel);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayAddDVD(DisplayProductModel displayProductModel)
	{
		ProcessTitleRollupOverlayAddDVD(displayProductModel);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayTimeout(DisplayProductModel displayProductModel)
	{
		ProcessTitleRollupOverlayTimeout(displayProductModel);
	}

	private void TitleRollupOverlayModel_OnTitleRollupOverlayCancel(DisplayProductModel displayProductModel)
	{
		ProcessTitleRollupOverlayCancel(displayProductModel);
	}

	public void ProcessTitleRollupVisibilityChange(Visibility newVisibility)
	{
		if (OnTitleRollupOverlayVisibilityChange != null)
		{
			OnTitleRollupOverlayVisibilityChange(newVisibility);
		}
	}

	public void ProcessTitleRollupOverlayAddBluray(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayAddBluray != null)
		{
			OnTitleRollupOverlayAddBluray(displayProductModel);
		}
	}

	public void ProcessTitleRollupOverlayAddDVD(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayAddDVD != null)
		{
			OnTitleRollupOverlayAddDVD(displayProductModel);
		}
	}

	public void ProcessTitleRollupOverlayTimeout(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayTimeout != null)
		{
			OnTitleRollupOverlayTimeout(displayProductModel);
		}
	}

	public void ProcessTitleRollupOverlayCancel(DisplayProductModel displayProductModel)
	{
		if (OnTitleRollupOverlayCancel != null)
		{
			OnTitleRollupOverlayCancel(displayProductModel);
		}
	}

	public void ProcessAddDisplayProductModel(DisplayProductModel displayProductModel)
	{
		if (OnAddDisplayProductModel != null)
		{
			OnAddDisplayProductModel(displayProductModel);
		}
	}

	public void ProcessCarouselRotation(DisplayProductModel displayProductModel)
	{
		if (OnCarouselRotation != null)
		{
			OnCarouselRotation(displayProductModel);
		}
	}

	public void ProcessDisplayProductModelSelected(DisplayProductModel displayProductModel)
	{
		if (OnDisplayProductModelSelected != null)
		{
			OnDisplayProductModelSelected(displayProductModel);
		}
	}

	private static void OnIsAnimationOnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is CarouselModel { OnIsAnimationOnChanged: not null } carouselModel)
		{
			carouselModel.OnIsAnimationOnChanged(carouselModel);
		}
	}
}
