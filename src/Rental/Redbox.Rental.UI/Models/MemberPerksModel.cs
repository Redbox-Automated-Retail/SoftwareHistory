using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public class MemberPerksModel : BaseModel<MemberPerksModel>, IPromoListModel
{
	public static readonly DependencyProperty SnapshotPointsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotPoints", TYPES.STRING);

	public static readonly DependencyProperty SnapshotPointsLabelProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotPointsLabel", TYPES.STRING);

	public static readonly DependencyProperty SnapshotExpireProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotExpire", TYPES.STRING);

	public static readonly DependencyProperty EarnedRewardsMessageProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("EarnedRewardsMessage", TYPES.STRING);

	public static readonly DependencyProperty PerksLevelMessageProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksLevelMessage", TYPES.STRING);

	public static readonly DependencyProperty PerksImageVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksImageVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PerksTitleVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksTitleVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PerksButtonVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PerksLevelProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksLevel", TYPES.STRING);

	public static readonly DependencyProperty PerksImagePathProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksImagePath", TYPES.STRING);

	public static readonly DependencyProperty PerksRentalsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksRentals", TYPES.STRING);

	public static readonly DependencyProperty PromoButtonVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromoButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PromoTitleProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromoTitle", TYPES.STRING);

	public static readonly DependencyProperty PerksButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksButtonText", TYPES.STRING);

	public static readonly DependencyProperty PromosButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromosButtonText", TYPES.STRING);

	public static readonly DependencyProperty PerksMarketingProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksMarketing", TYPES.STRING);

	public static readonly DependencyProperty PerksSelectedProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksSelected", TYPES.BOOL, true);

	public static readonly DependencyProperty PromosSelectedProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromosSelected", TYPES.BOOL, false);

	public static readonly DependencyProperty MemberTierProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("MemberTier", typeof(TierType), TierType.Member);

	public static readonly DependencyProperty CancelButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("CancelButtonText", TYPES.STRING);

	public static readonly DependencyProperty InfoButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("InfoButtonText", TYPES.STRING);

	public static readonly DependencyProperty PromoListMessageProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromoListMessage", TYPES.STRING);

	public static readonly DependencyProperty ShowPromoListMessageProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("ShowPromoListMessage", TYPES.BOOL, false);

	public static readonly DependencyProperty CurrentPagePromosProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("CurrentPagePromos", typeof(ObservableCollection<IPerksOfferListItem>), new ObservableCollection<IPerksOfferListItem>());

	public static readonly DependencyProperty CurrentPageNumberProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("CurrentPageNumber", TYPES.INT, 1);

	public static readonly DependencyProperty IsAdaModeProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("IsAdaMode", TYPES.BOOL, false);

	public MemberPerksLogic Logic { get; set; }

	public DynamicRoutedCommand SelectedButtonCommand { get; set; }

	public DynamicRoutedCommand SnapshotButtonCommand { get; set; }

	public DynamicRoutedCommand PerksButtonCommand { get; set; }

	public DynamicRoutedCommand PromoButtonCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public DynamicRoutedCommand InfoButtonCommand { get; set; }

	public DynamicRoutedCommand NextPagePressedCommand { get; set; }

	public DynamicRoutedCommand PreviousPagePressedCommand { get; set; }

	public DynamicRoutedCommand PageNumberPressedCommand { get; set; }

	public DynamicRoutedCommand AddOrRemovePromoCommand { get; set; }

	public DynamicRoutedCommand DetailsCommand { get; set; }

	public bool IsAdaMode
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAdaModeProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAdaModeProperty, value);
			});
		}
	}

	public string SnapshotPoints
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SnapshotPointsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotPointsProperty, value);
			});
		}
	}

	public string SnapshotPointsLabel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SnapshotPointsLabelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotPointsLabelProperty, value);
			});
		}
	}

	public string SnapshotExpire
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SnapshotExpireProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotExpireProperty, value);
			});
		}
	}

	public string EarnedRewardsMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(EarnedRewardsMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(EarnedRewardsMessageProperty, value);
			});
		}
	}

	public string PerksLevelMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksLevelMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksLevelMessageProperty, value);
			});
		}
	}

	public Visibility PerksImageVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PerksImageVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksImageVisibilityProperty, value);
			});
		}
	}

	public Visibility PerksTitleVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PerksTitleVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksTitleVisibilityProperty, value);
			});
		}
	}

	public Visibility PerksButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PerksButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksButtonVisibilityProperty, value);
			});
		}
	}

	public string PerksLevel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksLevelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksLevelProperty, value);
			});
		}
	}

	public string PerksImagePath
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksImagePathProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksImagePathProperty, value);
			});
		}
	}

	public string PerksRentals
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksRentalsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksRentalsProperty, value);
			});
		}
	}

	public Visibility PromoButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(PromoButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoButtonVisibilityProperty, value);
			});
		}
	}

	public string PromoTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PromoTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoTitleProperty, value);
			});
		}
	}

	public string PerksButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksButtonTextProperty, value);
			});
		}
	}

	public string PromosButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PromosButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromosButtonTextProperty, value);
			});
		}
	}

	public string PerksMarketing
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksMarketingProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksMarketingProperty, value);
			});
		}
	}

	public bool PerksSelected
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(PerksSelectedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksSelectedProperty, value);
			});
		}
	}

	public bool PromosSelected
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(PromosSelectedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromosSelectedProperty, value);
			});
		}
	}

	public TierType MemberTier
	{
		get
		{
			return base.Dispatcher.Invoke(() => (TierType)GetValue(MemberTierProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MemberTierProperty, value);
			});
		}
	}

	public string CancelButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(CancelButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CancelButtonTextProperty, value);
			});
		}
	}

	public string InfoButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(InfoButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(InfoButtonTextProperty, value);
			});
		}
	}

	public string PromoListMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PromoListMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromoListMessageProperty, value);
			});
		}
	}

	public bool ShowPromoListMessage
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(ShowPromoListMessageProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ShowPromoListMessageProperty, value);
			});
		}
	}

	public ObservableCollection<IPerksOfferListItem> CurrentPagePromos
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<IPerksOfferListItem>)GetValue(CurrentPagePromosProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPagePromosProperty, value);
			});
		}
	}

	public int CurrentPageNumber
	{
		get
		{
			return base.Dispatcher.Invoke(() => (int)GetValue(CurrentPageNumberProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(CurrentPageNumberProperty, value);
			});
		}
	}

	public List<IPerksOfferListItem> StoredPromoCodes { get; set; }

	public bool IsPromoCodeListVisible { get; set; }

	public bool IsEmptyPromoListMessageVisible { get; set; }

	public string EmptyPromoListMessage { get; set; }

	public int NumberOfPages { get; set; }

	public int PromosPerPage { get; set; }

	public PromoListLogic PromoLogic => Logic.PromoLogic;
}
