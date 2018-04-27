using System.Windows;
using Redbox.Rental.UI.ControllersLogic;

namespace Redbox.Rental.UI.Models;

public class MemberPerksModel : BaseModel<MemberPerksModel>
{
	public static readonly DependencyProperty SnapshotButtonVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty SnapshotTitleProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotTitle", TYPES.STRING);

	public static readonly DependencyProperty SnapshotPointsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotPoints", TYPES.STRING);

	public static readonly DependencyProperty SnapshotExpireProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotExpire", TYPES.STRING);

	public static readonly DependencyProperty SnapshotRentalsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotRentals", TYPES.OBJECT);

	public static readonly DependencyProperty SnapshotNextLevelProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotNextLevel", TYPES.STRING);

	public static readonly DependencyProperty PerksImageVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksImageVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PerksButtonVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PerksTitleProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksTitle", TYPES.STRING);

	public static readonly DependencyProperty PerksImagePathProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksImagePath", TYPES.STRING);

	public static readonly DependencyProperty PerksRentalsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksRentals", TYPES.STRING);

	public static readonly DependencyProperty PerksBulletsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksBullets", TYPES.STRARRAY);

	public static readonly DependencyProperty PromoButtonVisibilityProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromoButtonVisibility", TYPES.VISIBILITY, Visibility.Visible);

	public static readonly DependencyProperty PromoTitleProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromoTitle", TYPES.STRING);

	public static readonly DependencyProperty PromotionsProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("Promotions", TYPES.OBJECT);

	public static readonly DependencyProperty SnapshotButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotButtonText", TYPES.STRING);

	public static readonly DependencyProperty PerksButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksButtonText", TYPES.STRING);

	public static readonly DependencyProperty PromosButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromosButtonText", TYPES.STRING);

	public static readonly DependencyProperty PerksMarketingProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksMarketing", TYPES.STRING);

	public static readonly DependencyProperty SnapshotSelectedProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("SnapshotSelected", TYPES.BOOL, false);

	public static readonly DependencyProperty PerksSelectedProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PerksSelected", TYPES.BOOL, true);

	public static readonly DependencyProperty PromosSelectedProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("PromosSelected", TYPES.BOOL, false);

	public static readonly DependencyProperty MemberTierProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("MemberTier", typeof(TierType), TierType.Member);

	public static readonly DependencyProperty CancelButtonTextProperty = BaseModel<MemberPerksModel>.CreateDependencyProperty("CancelButtonText", TYPES.STRING);

	public MemberPerksLogic Logic { get; set; }

	public DynamicRoutedCommand SelectedButtonCommand { get; set; }

	public DynamicRoutedCommand SnapshotButtonCommand { get; set; }

	public DynamicRoutedCommand PerksButtonCommand { get; set; }

	public DynamicRoutedCommand PromoButtonCommand { get; set; }

	public DynamicRoutedCommand CancelButtonCommand { get; set; }

	public Visibility SnapshotButtonVisibility
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Visibility)GetValue(SnapshotButtonVisibilityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotButtonVisibilityProperty, value);
			});
		}
	}

	public string SnapshotTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SnapshotTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotTitleProperty, value);
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

	public object SnapshotRentals
	{
		get
		{
			return base.Dispatcher.Invoke(() => GetValue(SnapshotRentalsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotRentalsProperty, value);
			});
		}
	}

	public string SnapshotNextLevel
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SnapshotNextLevelProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotNextLevelProperty, value);
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

	public string PerksTitle
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(PerksTitleProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksTitleProperty, value);
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

	public string[] PerksBullets
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string[])GetValue(PerksBulletsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PerksBulletsProperty, value);
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

	public object Promotions
	{
		get
		{
			return base.Dispatcher.Invoke(() => GetValue(PromotionsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(PromotionsProperty, value);
			});
		}
	}

	public string SnapshotButtonText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(SnapshotButtonTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotButtonTextProperty, value);
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

	public bool SnapshotSelected
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(SnapshotSelectedProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(SnapshotSelectedProperty, value);
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
}
