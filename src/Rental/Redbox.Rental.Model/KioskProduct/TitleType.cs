using System.ComponentModel.DataAnnotations;

namespace Redbox.Rental.Model.KioskProduct;

public enum TitleType
{
	[Display(Name = "All", Description = "All")]
	All = 0,
	[Display(Name = "DVD", Description = "DVD")]
	DVD = 1,
	[Display(Name = "Bluray", Description = "Blu-ray")]
	Bluray = 2,
	[Display(Name = "Xbox360", Description = "Xbox 360")]
	Xbox360 = 3,
	[Display(Name = "PS2", Description = "PS2")]
	PS2 = 4,
	[Display(Name = "PS3", Description = "PS3")]
	PS3 = 5,
	[Display(Name = "PSP", Description = "PSP")]
	PSP = 6,
	[Display(Name = "Wii", Description = "Wii")]
	Wii = 7,
	[Display(Name = "DS", Description = "Nintendo DS")]
	DS = 8,
	[Display(Name = "PC", Description = "PC")]
	PC = 9,
	[Display(Name = "WiiU", Description = "Wii U")]
	WiiU = 10,
	[Display(Name = "PS4", Description = "PS4")]
	PS4 = 11,
	[Display(Name = "XboxOne", Description = "Xbox One")]
	XboxOne = 12,
	[Display(Name = "DigitalCode", Description = "Digital Code")]
	DigitalCode = 17,
	[Display(Name = "NintendoSwitch", Description = "Nintendo Switch")]
	NintendoSwitch = 18,
	[Display(Name = "4K UHD", Description = "4K UHD")]
	_4KUHD = 19
}
