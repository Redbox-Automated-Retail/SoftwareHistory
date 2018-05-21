using System;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Views;

public class Constants
{
	public class ControlNames
	{
		public const string StartView = "StartView";

		public const string BrowseView = "BrowseView";

		public const string TitleDetailsView = "TitleDetailsView";

		public const string ShoppingCartView = "ShoppingCartView";

		public const string ShoppingCartUpdateADAView = "ShoppingCartUpdateADAView";

		public const string ConfirmEmailView = "ConfirmEmailView";

		public const string ZipCodeView = "ZipCodeView";

		public const string TitleDetailsDevView = "TitleDetailsDevView";

		public const string KeyboardEmailComingSoonView = "KeyboardEmailComingSoonView";

		public const string KeyboardEmailReceiptView = "KeyboardEmailReceiptView";

		public const string KeyboardEmailChangeView = "KeyboardEmailChangeView";

		public const string KeyboardPromoView = "KeyboardPromoView";

		public const string KeyboardSignInView = "KeyboardSignInView";

		public const string MemberPointsView = "MemberPointsView";

		public const string MemberPerksView = "MemberPerksView";

		public const string GeneralMessageView = "GeneralMessageView";

		public const string PromptSignInView = "PromptSignInView";

		public const string PleaseWaitView = "PleaseWaitView";

		public const string PhoneAndPinView = "PhoneAndPinView";

		public const string RecommendationOnPickupView = "RecommendationOnPickupView";

		public const string SignInView = "SignInView";

		public const string MultiNightPricingPopupView = "MultiNightPricingPopupView";

		public const string JoinPerksPromoView = "JoinPerksPromoView";

		public const string JoinedPerksView = "JoinedPerksView";

		public const string JoinTextClubPromoView = "JoinTextClubPromoView";

		public const string EmailReceiptView = "EmailReceiptView";

		public const string ControlPanelLoginView = "ControlPanelLoginView";

		public const string FormatSelectionPopupView = "FormatSelectionPopupView";

		public const string RentalFormatSelectionPopupView = "RentalFormatSelectionPopupView";

		public const string UpsellView = "UpsellView";

		public const string MessagePopupView = "MessagePopupViewUserControl";

		public const string HelpView = "HelpViewUserControl";
	}

	public class ViewNames
	{
		public const string BrowseView = "browse_view";

		public const string ShoppingCartView = "shopping_cart_view";

		public const string ShoppingCartUpdateADAView = "shopping_cart_update_ada_view";

		public const string KeyboardEmailComingSoonView = "keyboard_email_coming_soon_view";

		public const string KeyboardEmailReceiptView = "keyboard_email_receipt_view";

		public const string KeyboardEmailChangeView = "keyboard_email_change_view";

		public const string KeyboardPromoView = "keyboard_promo_view";

		public const string KeyboardSignInView = "keyboard_sign_in_view";

		public const string MaintenanceView = "maintenance_view";

		public const string MemberPerksView = "member_perks_view";

		public const string MemberPointsView = "member_points_view";

		public const string GeneralMessageView = "general_message_view";

		public const string PromptSignInView = "prompt_sign_in_view";

		public const string PleaseWaitView = "please_wait_view";

		public const string PhoneAndPinView = "phone_and_pin_view";

		public const string RecommendationOnPickupView = "recommendation_on_pickup_view";

		public const string StartView = "start_view";

		public const string SignInView = "sign_in_view";

		public const string TitleDetailsDevModeView = "title_details_dev_view";

		public const string MultiNightPricingPopupView = "multi_night_pricing_popup_view";

		public const string UpsellView = "upsell_view";

		public const string JoinPerksPromoView = "join_perks_promo_view";

		public const string JoinedPerksView = "joined_perks_view";

		public const string JoinTextClubPromoView = "joined_text_club_promo_view";

		public const string EmailReceiptView = "email_receipt_view";

		public const string ControlPanelLoginView = "control_panel_login_view";

		public const string FormatFilterPopupView = "format_filter_popup_view";

		public const string PurchasePopupView = "purchase_popup_view";

		public const string RentalPopupView = "rental_popup_view";

		public const string MessagePopupView = "message_popup_view";

		public const string _4kAlertPopupView = "4K_warning_popup_view";

		public const string HelpView = "help_document_view";
	}

	public class ThemeNames
	{
		public const string RedboxClassic = "Redbox Classic";

		public const string Glass = "Glass";

		public const string Prototype = "Prototype";
	}

	public class Images
	{
		private static BitmapImage _collectionsCarousel;

		private static BitmapImage _movieMissingBoxArt;

		private static BitmapImage _gameMissingBoxArt;

		public static BitmapImage CollectionsCarousel
		{
			get
			{
				if (_collectionsCarousel == null)
				{
					_collectionsCarousel = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "Collections-Carousel.jpg")));
				}
				return _collectionsCarousel;
			}
		}

		public static BitmapImage MovieMissingBoxArt
		{
			get
			{
				if (_movieMissingBoxArt == null)
				{
					_movieMissingBoxArt = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "movie-missing-image.png")));
				}
				return _movieMissingBoxArt;
			}
		}

		public static BitmapImage GameMissingBoxArt
		{
			get
			{
				if (_gameMissingBoxArt == null)
				{
					_gameMissingBoxArt = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "game-missing-image.png")));
				}
				return _gameMissingBoxArt;
			}
		}
	}

	public enum SubPlatforms
	{
		WiiMotionPlusCompatible = 1,
		WiiMotionPlusRequired,
		WiiFitCompatible,
		WiiFitRequired,
		MoveCompatible,
		MoveRequired,
		KinectCompatible,
		KinectRequired,
		NunchuckRequired,
		WiiMote,
		Ps4MoveCompatible,
		Ps4MoveRequired,
		Ps4CameraRequired,
		KinectXboxOneCompatible,
		KinectXboxOneRequired
	}

	public class SubPlatformImages
	{
		private static BitmapImage _kinectCompatible;

		private static BitmapImage _kinectRequired;

		private static BitmapImage _kinectXboxOneCompatible;

		private static BitmapImage _kinectXboxOneRequired;

		private static BitmapImage _ps4CameraRequired;

		private static BitmapImage _ps4MoveCompatible;

		private static BitmapImage _ps4MoveRequired;

		private static BitmapImage _moveCompatible;

		private static BitmapImage _moveRequired;

		private static BitmapImage _wiiFitCompatible;

		private static BitmapImage _wiiFitRequired;

		private static BitmapImage _wiiMote;

		private static BitmapImage _wiiMotionPlusCompatible;

		private static BitmapImage _wiiMotionPlusRequired;

		private static BitmapImage _nunchuckRequired;

		public static BitmapImage KinectCompatible
		{
			get
			{
				if (_kinectCompatible == null)
				{
					_kinectCompatible = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "kinect-compatible.png")));
				}
				return _kinectCompatible;
			}
		}

		public static BitmapImage KinectRequired
		{
			get
			{
				if (_kinectRequired == null)
				{
					_kinectRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "kinect-required.png")));
				}
				return _kinectRequired;
			}
		}

		public static BitmapImage KinectXboxOneCompatible
		{
			get
			{
				if (_kinectXboxOneCompatible == null)
				{
					_kinectXboxOneCompatible = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "kinect-compatible-xbox-one.png")));
				}
				return _kinectXboxOneCompatible;
			}
		}

		public static BitmapImage KinectXboxOneRequired
		{
			get
			{
				if (_kinectXboxOneRequired == null)
				{
					_kinectXboxOneRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "kinect-required-xbox-one.png")));
				}
				return _kinectXboxOneRequired;
			}
		}

		public static BitmapImage Ps4CameraRequired
		{
			get
			{
				if (_ps4CameraRequired == null)
				{
					_ps4CameraRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "ps4-camera-required.png")));
				}
				return _ps4CameraRequired;
			}
		}

		public static BitmapImage Ps4MoveCompatible
		{
			get
			{
				if (_ps4MoveCompatible == null)
				{
					_ps4MoveCompatible = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "ps4-move-compatible.png")));
				}
				return _ps4MoveCompatible;
			}
		}

		public static BitmapImage Ps4MoveRequired
		{
			get
			{
				if (_ps4MoveRequired == null)
				{
					_ps4MoveRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "ps4-move-required.png")));
				}
				return _ps4MoveRequired;
			}
		}

		public static BitmapImage MoveCompatible
		{
			get
			{
				if (_moveCompatible == null)
				{
					_moveCompatible = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "move-required.png")));
				}
				return _moveCompatible;
			}
		}

		public static BitmapImage MoveRequired
		{
			get
			{
				if (_moveRequired == null)
				{
					_moveRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "move-required.png")));
				}
				return _moveRequired;
			}
		}

		public static BitmapImage WiiFitCompatible
		{
			get
			{
				if (_wiiFitCompatible == null)
				{
					_wiiFitCompatible = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "wii-fit-compatible.png")));
				}
				return _wiiFitCompatible;
			}
		}

		public static BitmapImage WiiFitRequired
		{
			get
			{
				if (_wiiFitRequired == null)
				{
					_wiiFitRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "wii-fit-required.png")));
				}
				return _wiiFitRequired;
			}
		}

		public static BitmapImage WiiMote
		{
			get
			{
				if (_wiiMote == null)
				{
					_wiiMote = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "wii-mote.png")));
				}
				return _wiiMote;
			}
		}

		public static BitmapImage NunchuckRequired
		{
			get
			{
				if (_nunchuckRequired == null)
				{
					_nunchuckRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "nunchuk.png")));
				}
				return _nunchuckRequired;
			}
		}

		public static BitmapImage WiiMotionPlusCompatible
		{
			get
			{
				if (_wiiMotionPlusCompatible == null)
				{
					_wiiMotionPlusCompatible = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "wii-motion-plus-compatible.png")));
				}
				return _wiiMotionPlusCompatible;
			}
		}

		public static BitmapImage WiiMotionPlusRequired
		{
			get
			{
				if (_wiiMotionPlusRequired == null)
				{
					_wiiMotionPlusRequired = new BitmapImage(new Uri(string.Format("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}", "wii-motion-plus-required.png")));
				}
				return _wiiMotionPlusRequired;
			}
		}

		public static BitmapImage GetSubPlatformImage(SubPlatforms sub)
		{
			return sub switch
			{
				SubPlatforms.KinectCompatible => KinectCompatible, 
				SubPlatforms.KinectRequired => KinectRequired, 
				SubPlatforms.KinectXboxOneCompatible => KinectXboxOneCompatible, 
				SubPlatforms.KinectXboxOneRequired => KinectXboxOneRequired, 
				SubPlatforms.MoveCompatible => MoveCompatible, 
				SubPlatforms.MoveRequired => MoveRequired, 
				SubPlatforms.Ps4CameraRequired => Ps4CameraRequired, 
				SubPlatforms.Ps4MoveCompatible => Ps4MoveCompatible, 
				SubPlatforms.Ps4MoveRequired => Ps4MoveRequired, 
				SubPlatforms.WiiFitCompatible => WiiFitCompatible, 
				SubPlatforms.WiiFitRequired => WiiFitRequired, 
				SubPlatforms.WiiMote => WiiMote, 
				SubPlatforms.WiiMotionPlusCompatible => WiiMotionPlusCompatible, 
				SubPlatforms.WiiMotionPlusRequired => WiiMotionPlusRequired, 
				SubPlatforms.NunchuckRequired => NunchuckRequired, 
				_ => null, 
			};
		}
	}

	private const string ImagesBaseUri = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}";
}
