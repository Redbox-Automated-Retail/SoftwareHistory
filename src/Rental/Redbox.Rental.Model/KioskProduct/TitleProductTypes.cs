namespace Redbox.Rental.Model.KioskProduct;

public static class TitleProductTypes
{
	public static TitleType[] GameFamilyTitleTypes = new TitleType[10]
	{
		TitleType.Xbox360,
		TitleType.PS2,
		TitleType.PS3,
		TitleType.PSP,
		TitleType.Wii,
		TitleType.DS,
		TitleType.PC,
		TitleType.WiiU,
		TitleType.PS4,
		TitleType.XboxOne
	};

	public static TitleType[] MovieFamilyTitleTypes = new TitleType[2]
	{
		TitleType.DVD,
		TitleType.Bluray
	};
}
