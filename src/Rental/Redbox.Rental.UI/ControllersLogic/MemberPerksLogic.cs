using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic;

public class MemberPerksLogic : BaseLogic
{
	private const string ImagePath = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/";

	private const string perks_hero = "perks_star.png";

	private const string perks_superhero = "perks_superstar.png";

	private const string perks_legend = "perks_legend.png";

	private static string[] PerksTierTitles = new string[4] { "Member", "Star", "Super Star", "Legend" };

	private static Snapshots Snapshots = new Snapshots
	{
		new Snapshot
		{
			LowLimit = 10.0,
			HighLimit = 19.0,
			Threshold = 10.0,
			ImagePath = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/perks_star.png",
			ImageName = Resources.perks_tier_title1?.ToUpper(),
			LabelText = "10"
		},
		new Snapshot
		{
			LowLimit = 20.0,
			HighLimit = 29.0,
			Threshold = 20.0,
			ImagePath = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/perks_superstar.png",
			ImageName = Resources.perks_tier_title2?.ToUpper(),
			LabelText = "20"
		},
		new Snapshot
		{
			LowLimit = 30.0,
			HighLimit = 69.0,
			Threshold = 50.0,
			ImagePath = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/perks_legend.png",
			ImageName = Resources.perks_tier_title3?.ToUpper(),
			LabelText = "50+"
		},
		new Snapshot
		{
			LowLimit = 0.0,
			HighLimit = 9.0,
			Threshold = double.MaxValue
		}
	};

	public PerksTiers perksTiers = new PerksTiers
	{
		new PerksTier
		{
			Title = Resources.perks_tier_title0,
			RangeLimit = 0
		},
		new PerksTier
		{
			Title = Resources.perks_tier_title1,
			RangeLimit = 9
		},
		new PerksTier
		{
			Title = Resources.perks_tier_title2,
			RangeLimit = 19
		},
		new PerksTier
		{
			Title = Resources.perks_tier_title3,
			RangeLimit = 49
		}
	};

	public Promotions promotions = new Promotions
	{
		new Promotion
		{
			PromoText = "Rent 2 movies Get $1 off your first night",
			MarketingText = Resources.perks_marketing
		},
		new Promotion
		{
			PromoText = "Rent 2 movies Get $1 off your first night",
			MarketingText = Resources.perks_marketing
		},
		new Promotion
		{
			PromoText = "Rent 2 movies Get $1 off your first night",
			MarketingText = Resources.perks_marketing
		}
	};

	public bool Instructions { get; set; }

	public Action CloseAction { get; set; }

	public MemberPerksModel Model { get; private set; }

	private static List<string> ImageNames => new List<string> { null, "perks_star.png", "perks_superstar.png", "perks_legend.png" };

	public bool IsSnapshot => Model.SnapshotButtonVisibility == Visibility.Visible;

	public bool IsPerks => Model.PerksButtonVisibility == Visibility.Visible;

	public bool IsPromo => Model.PromoButtonVisibility == Visibility.Visible;

	protected void PopulatePerks(PerksMemberInfo memberInfo)
	{
		double num = memberInfo.YearRentals;
		Snapshots.Rentals = num;
		for (int i = 0; i < 4; i++)
		{
			Snapshots[i].Rentals = num;
		}
		Model.SnapshotTitle = Resources.perks_snapshot_title;
		Model.SnapshotPoints = $"{memberInfo.Points:n0} {Resources.perks_snapshot_points}";
		Model.SnapshotExpire = ((memberInfo.ExpirePoints != 0) ? $"{memberInfo.ExpirePoints:n0} {Resources.perks_snapshot_expire} {memberInfo.ExpireDate:d}" : "");
		int memberTier = (int)Model.MemberTier;
		int num2 = SetSnapshotThresholds(num);
		bool flag = num2 < memberTier;
		num2 = (flag ? memberTier : num2);
		string snapshotNextLevel = GetSimpleNextLevel(num2, num);
		string title = perksTiers[num2].Title;
		bool flag2 = false;
		switch (PerksTierTitles[num2].ToLower())
		{
		default:
			flag2 = true;
			Model.MemberTier = TierType.Member;
			memberInfo.Bullets = new string[3]
			{
				Resources.perks_level0_message1,
				Resources.perks_level0_message2,
				Resources.perks_level0_message3
			};
			break;
		case "star":
			Model.MemberTier = TierType.Star;
			memberInfo.Bullets = new string[3]
			{
				Resources.perks_level1_message1,
				Resources.perks_level1_message2,
				Resources.perks_level1_message3
			};
			if (flag)
			{
				Snapshots[0].Threshold = -1.0;
				if (num < 10.0)
				{
					snapshotNextLevel = string.Format(Resources.perks_snapshot_maintain, 10.0 - num);
				}
			}
			break;
		case "super star":
			Model.MemberTier = TierType.Superstar;
			memberInfo.Bullets = new string[4]
			{
				Resources.perks_level2_message1,
				Resources.perks_level2_message2,
				Resources.perks_level2_message3,
				Resources.perks_level2_message4
			};
			if (flag)
			{
				Snapshots[1].Threshold = -1.0;
				if (num < 20.0)
				{
					snapshotNextLevel = string.Format(Resources.perks_snapshot_maintain, 20.0 - num);
				}
			}
			break;
		case "legend":
			Model.MemberTier = TierType.Legend;
			memberInfo.Bullets = new string[4]
			{
				Resources.perks_level3_message1,
				Resources.perks_level3_message2,
				Resources.perks_level3_message3,
				Resources.perks_level3_message4
			};
			if (flag)
			{
				Snapshots[2].Threshold = -1.0;
				if (num < 50.0)
				{
					snapshotNextLevel = string.Format(Resources.perks_snapshot_maintain, 50.0 - num);
				}
			}
			break;
		}
		Model.SnapshotRentals = Snapshots;
		Model.SnapshotNextLevel = snapshotNextLevel;
		Model.PerksTitle = title;
		Model.PerksImagePath = (flag2 ? "" : ("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/" + ImageNames[num2]));
		Model.PerksRentals = Model.SnapshotNextLevel;
		Model.PerksBullets = memberInfo.Bullets;
		Model.PerksMarketing = Resources.perks_marketing;
		Model.PerksImageVisibility = (flag2 ? Visibility.Collapsed : Visibility.Visible);
		Model.PromoTitle = Resources.perks_promo_title;
		Model.Promotions = promotions;
	}

	private string GetSimpleNextLevel(int index, double rentals)
	{
		double num;
		do
		{
			num = index switch
			{
				1 => 20.0, 
				2 => 50.0, 
				3 => -1.0, 
				_ => 10.0, 
			};
		}
		while (num >= 0.0 && num - rentals < 0.0);
		if (!(num >= 0.0))
		{
			return Resources.perks_snapshot_on_top;
		}
		return string.Format(Resources.perks_snapshot_next_level, num - rentals);
	}

	private int SetSnapshotThresholds(double rentals)
	{
		if (rentals < 10.0)
		{
			Snapshots[0].Threshold = double.MaxValue;
			Snapshots[1].Threshold = double.MaxValue;
			Snapshots[2].Threshold = double.MaxValue;
			return 0;
		}
		if (rentals < 20.0)
		{
			Snapshots[0].Threshold = 10.0;
			Snapshots[1].Threshold = double.MaxValue;
			Snapshots[2].Threshold = double.MaxValue;
			return 1;
		}
		if (rentals < 50.0)
		{
			Snapshots[0].Threshold = 10.0;
			Snapshots[1].Threshold = 20.0;
			Snapshots[2].Threshold = double.MaxValue;
			return 2;
		}
		Snapshots[0].Threshold = 10.0;
		Snapshots[1].Threshold = 20.0;
		Snapshots[2].Threshold = 50.0;
		return 3;
	}

	public void SnapshotButtonCommand_Execute()
	{
		Model.SnapshotSelected = true;
		MemberPerksModel model = Model;
		bool perksSelected = (Model.PromosSelected = false);
		model.PerksSelected = perksSelected;
		HandleWPFHit();
	}

	public void PerksButtonCommand_Execute()
	{
		Model.PerksSelected = true;
		MemberPerksModel model = Model;
		bool snapshotSelected = (Model.PromosSelected = false);
		model.SnapshotSelected = snapshotSelected;
		HandleWPFHit();
	}

	public void PromoButtonCommand_Execute()
	{
		Model.PromosSelected = true;
		MemberPerksModel model = Model;
		bool snapshotSelected = (Model.PerksSelected = false);
		model.SnapshotSelected = snapshotSelected;
		HandleWPFHit();
	}

	public void CancelButtonCommand_Execute()
	{
		CloseAction?.Invoke();
		HandleWPFHit();
	}

	public void SelectedButtonCommand_Execute(string buttonValue)
	{
	}

	public MemberPerksLogic(MemberPerksModel perksModel, PerksMemberInfo memberInfo)
	{
		Instructions = true;
		CloseAction = memberInfo.CancelAction;
		Model = perksModel;
		Model.CancelButtonCommand = RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		PopulatePerks(memberInfo);
	}
}
