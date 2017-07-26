using System;
using System.Windows.Media.Imaging;

namespace Redbox.Rental.UI.Views;

public class Constants
{
	public class ControlNames
	{
		public const string StartView = "StartView";
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

	private const string ImagesBaseUri = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/{0}";
}
