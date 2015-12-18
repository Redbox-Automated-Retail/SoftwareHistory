namespace Redbox.Rental.Model;

public interface IStoreManager
{
	long KioskId { get; }

	string GetDefaultCulture();

	string GetAlternateCulture();

	string GetWallpaperPath();
}
