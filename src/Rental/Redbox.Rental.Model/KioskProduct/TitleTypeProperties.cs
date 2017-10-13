using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public static class TitleTypeProperties
{
	private static Dictionary<TitleType, TitleTypeProperty> _titleTypeProperties;

	public static Dictionary<TitleType, TitleTypeProperty> Properties
	{
		get
		{
			if (_titleTypeProperties == null)
			{
				_titleTypeProperties = new Dictionary<TitleType, TitleTypeProperty>
				{
					{
						TitleType.All,
						new TitleTypeProperty
						{
							Name = "All"
						}
					},
					{
						TitleType.DVD,
						new TitleTypeProperty
						{
							Name = "DVD"
						}
					},
					{
						TitleType.Bluray,
						new TitleTypeProperty
						{
							Name = "Blu-ray"
						}
					},
					{
						TitleType.Xbox360,
						new TitleTypeProperty
						{
							Name = "Xbox 360"
						}
					},
					{
						TitleType.PS2,
						new TitleTypeProperty
						{
							Name = "PS2"
						}
					},
					{
						TitleType.PS3,
						new TitleTypeProperty
						{
							Name = "PS3"
						}
					},
					{
						TitleType.PSP,
						new TitleTypeProperty
						{
							Name = "PSP"
						}
					},
					{
						TitleType.Wii,
						new TitleTypeProperty
						{
							Name = "Wii"
						}
					},
					{
						TitleType.DS,
						new TitleTypeProperty
						{
							Name = "DS"
						}
					},
					{
						TitleType.PC,
						new TitleTypeProperty
						{
							Name = "PC"
						}
					},
					{
						TitleType.WiiU,
						new TitleTypeProperty
						{
							Name = "Wii U"
						}
					},
					{
						TitleType.PS4,
						new TitleTypeProperty
						{
							Name = "PS4"
						}
					},
					{
						TitleType.XboxOne,
						new TitleTypeProperty
						{
							Name = "Xbox One"
						}
					},
					{
						TitleType.DigitalCode,
						new TitleTypeProperty
						{
							Name = "Digital Code"
						}
					},
					{
						TitleType.NintendoSwitch,
						new TitleTypeProperty
						{
							Name = "Nintendo Switch"
						}
					}
				};
			}
			return _titleTypeProperties;
		}
	}
}
