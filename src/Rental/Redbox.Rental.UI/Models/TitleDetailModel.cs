using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public class TitleDetailModel : DependencyObject
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty BrowseProductControlModelProperty = DependencyProperty.Register("BrowseProductControlModel", typeof(BrowseControlModel), typeof(TitleDetailModel), new FrameworkPropertyMetadata(null, OnBrowseProductControlModelPropertyChanged));

	public static readonly DependencyProperty BackTextProperty = DependencyProperty.Register("BackText", typeof(string), typeof(TitleDetailModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty BackgroundImageVisibilityProperty = DependencyProperty.Register("BackgroundImageVisibility", typeof(Visibility), typeof(TitleDetailModel), new FrameworkPropertyMetadata(Visibility.Collapsed));

	public BrowseControlModel BrowseProductControlModel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (BrowseControlModel)GetValue(BrowseProductControlModelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BrowseProductControlModelProperty, value);
			});
		}
	}

	public string BackText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(BackTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackTextProperty, value);
			});
		}
	}

	public Visibility BackgroundImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(BackgroundImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(BackgroundImageVisibilityProperty, value);
			});
		}
	}

	public event Action<IBrowseItemModel, object> AddPrimary;

	public event Action<IBrowseItemModel, object> AddBluRay;

	public event Action<IBrowseItemModel, object> BuyButtonHit;

	public event Action<IBrowseItemModel> CollectionButtonHit;

	public event Action<IBrowseItemModel> MoreInfoHit;

	public event Action GoBackHit;

	public event Action<TitleDetailModel> OnTitleDetailModelChanged;

	internal ISpeechControl ProcessGetSpeechControl()
	{
		return OnGetSpeechControl?.Invoke();
	}

	public void ProcessAddPrimaryItem(IBrowseItemModel model, object o)
	{
		AddPrimary?.Invoke(model, o);
	}

	public void ProcessAddBluRay(IBrowseItemModel model, object o)
	{
		AddBluRay?.Invoke(model, o);
	}

	public void ProcessBuyButtonHit(IBrowseItemModel model, object o)
	{
		BuyButtonHit?.Invoke(model, o);
	}

	public void ProcessCollectionButtonHit(IBrowseItemModel model)
	{
		CollectionButtonHit?.Invoke(model);
	}

	public void ProcessMoreInfoHit(IBrowseItemModel model)
	{
		MoreInfoHit?.Invoke(model);
	}

	public void ProcessGoBackHit()
	{
		GoBackHit?.Invoke();
	}

	private static void OnBrowseProductControlModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TitleDetailModel { OnTitleDetailModelChanged: not null } titleDetailModel)
		{
			titleDetailModel.OnTitleDetailModelChanged(titleDetailModel);
		}
	}
}
