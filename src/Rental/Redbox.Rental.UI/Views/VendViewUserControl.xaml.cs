using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Controls.Animations;

namespace Redbox.Rental.UI.Views;

public partial class VendViewUserControl : BaseUserControl, IWPFActor, IComponentConnector
{
	public bool IsPickup { get; set; }

	public string[] TextMessages
	{
		set
		{
			if (VendDiscControl != null)
			{
				VendDiscControl.VendDiscMessages = value;
			}
		}
	}

	public bool IsSignedIn
	{
		set
		{
			if (VendDiscControl != null)
			{
				VendDiscControl.IsSignedIn = value;
			}
		}
	}

	public bool IsLoyaltySignedIn
	{
		set
		{
			if (VendDiscControl != null)
			{
				VendDiscControl.IsLoyaltySignedIn = value;
			}
		}
	}

	private IVendDiscAnimationControl VendAnimationControl => VendDiscControl;

	public VendViewType VendType
	{
		get
		{
			return VendDiscControl.VendType;
		}
		set
		{
			VendDiscControl.VendType = value;
			VendDiscControl.IsPickupDisc = IsPickup;
			switch (value)
			{
			case VendViewType.RentalsOnTheirWay:
				VendDiscControl.IsAnimated = true;
				break;
			case VendViewType.RecomendationOnPickupVend:
				VendDiscControl.IsAnimated = true;
				break;
			case VendViewType.None:
			case VendViewType.PleaseWait:
				break;
			}
		}
	}

	public BitmapImage BannerImage
	{
		set
		{
			AdImageElem.Source = value;
			VendBannerGrid.Visibility = ((AdImageElem.Source == null) ? Visibility.Collapsed : Visibility.Visible);
		}
	}

	public VendViewUserControl()
	{
		InitializeComponent();
		InitVisibilityHasChanged();
	}

	protected override void VisibleHasChanged()
	{
	}
}
