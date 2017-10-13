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
