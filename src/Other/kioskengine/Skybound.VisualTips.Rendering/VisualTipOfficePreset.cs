using System.ComponentModel;
using System.Drawing;
using Microsoft.Win32;
using Skybound.ComponentModel;

namespace Skybound.VisualTips.Rendering;

[TypeConverter(typeof(TypeConverter))]
public class VisualTipOfficePreset
{
	internal class TypeConverter : StandardValueConverter
	{
		public TypeConverter()
			: base(typeof(VisualTipOfficePreset))
		{
		}
	}

	internal Color xf83c69bb98e96a69;

	internal Color x096ce6921a0d3679;

	internal Color xb64cd85cc9846947;

	internal Color x0cd10c3b6c8154d6;

	private static VisualTipOfficePreset _xa12904199a25f8c7;

	private static VisualTipOfficePreset _x291842826cb2d7e0;

	private static VisualTipOfficePreset _x83a24a61a9cc6289;

	private static VisualTipOfficePreset _xc5f41472af38670e;

	private static VisualTipOfficePreset _xd5a7a92b8cfb14b3;

	private static VisualTipOfficePreset _xe7c87f797bb4b411;

	private static VisualTipOfficePreset _x1fbfc81266439497;

	private static VisualTipOfficePreset _x87c77c027dc67f29;

	private static VisualTipOfficePreset _x2dd6a063e8d835cd;

	private static VisualTipOfficePreset _xa1c30e7dd5b3b43e;

	private static VisualTipOfficePreset _x46d2ee2a363fa637;

	private static VisualTipOfficePreset _x67748d6645aa8af0;

	private static VisualTipOfficePreset _x5dd39aeabb2bcad1;

	private static VisualTipOfficePreset _xf2974a62abb9777f;

	private static VisualTipOfficePreset _xb3f077f6e70063ba;

	[StandardValue]
	public static VisualTipOfficePreset AutoSelect => _xa12904199a25f8c7;

	[StandardValue]
	public static VisualTipOfficePreset XPBlue => _x291842826cb2d7e0;

	[StandardValue]
	public static VisualTipOfficePreset XPGreen => _x83a24a61a9cc6289;

	[StandardValue]
	public static VisualTipOfficePreset XPSilver => _xc5f41472af38670e;

	[StandardValue]
	public static VisualTipOfficePreset Control => _xd5a7a92b8cfb14b3;

	[StandardValue]
	public static VisualTipOfficePreset Brown => _xe7c87f797bb4b411;

	[StandardValue]
	public static VisualTipOfficePreset Hazel => _x1fbfc81266439497;

	[StandardValue]
	public static VisualTipOfficePreset Cyan => _x87c77c027dc67f29;

	[StandardValue]
	public static VisualTipOfficePreset Pink => _x2dd6a063e8d835cd;

	[StandardValue]
	public static VisualTipOfficePreset Violet => _xa1c30e7dd5b3b43e;

	[StandardValue]
	public static VisualTipOfficePreset Red => _x46d2ee2a363fa637;

	[StandardValue]
	public static VisualTipOfficePreset ToolTip => _x67748d6645aa8af0;

	[StandardValue]
	public static VisualTipOfficePreset DeepBlue => _x5dd39aeabb2bcad1;

	[StandardValue]
	public static VisualTipOfficePreset Smoke => _xf2974a62abb9777f;

	[StandardValue]
	public static VisualTipOfficePreset Midnight => _xb3f077f6e70063ba;

	private VisualTipOfficePreset()
	{
	}

	private VisualTipOfficePreset(Color backColor, Color backColorGradient, Color borderColor, Color textColor)
	{
		xf83c69bb98e96a69 = backColor;
		x096ce6921a0d3679 = backColorGradient;
		xb64cd85cc9846947 = borderColor;
		x0cd10c3b6c8154d6 = textColor;
	}

	static VisualTipOfficePreset()
	{
		_x291842826cb2d7e0 = new VisualTipOfficePreset(Color.FromArgb(255, 255, 255), Color.FromArgb(201, 217, 239), Color.FromArgb(111, 121, 133), Color.FromArgb(64, 64, 64));
		_x83a24a61a9cc6289 = new VisualTipOfficePreset(Color.FromArgb(245, 255, 215), Color.FromArgb(197, 210, 155), Color.FromArgb(123, 140, 119), Color.FromArgb(64, 64, 64));
		_xc5f41472af38670e = new VisualTipOfficePreset(Color.FromArgb(251, 251, 251), Color.FromArgb(220, 220, 220), Color.FromArgb(128, 128, 128), Color.FromArgb(64, 64, 64));
		_xd5a7a92b8cfb14b3 = new VisualTipOfficePreset(SystemColors.ControlLightLight, SystemColors.Control, SystemColors.ControlDark, SystemColors.ControlText);
		_xe7c87f797bb4b411 = new VisualTipOfficePreset(Color.FromArgb(255, 249, 249), Color.FromArgb(233, 222, 208), Color.FromArgb(138, 128, 118), Color.FromArgb(64, 64, 64));
		_x1fbfc81266439497 = new VisualTipOfficePreset(Color.FromArgb(255, 252, 249), Color.FromArgb(232, 233, 208), Color.FromArgb(140, 140, 119), Color.FromArgb(64, 64, 64));
		_x87c77c027dc67f29 = new VisualTipOfficePreset(Color.FromArgb(248, 254, 251), Color.FromArgb(208, 228, 233), Color.FromArgb(118, 135, 138), Color.FromArgb(64, 64, 64));
		_x2dd6a063e8d835cd = new VisualTipOfficePreset(Color.FromArgb(254, 248, 253), Color.FromArgb(233, 208, 212), Color.FromArgb(138, 118, 123), Color.FromArgb(64, 64, 64));
		_xa1c30e7dd5b3b43e = new VisualTipOfficePreset(Color.FromArgb(248, 248, 254), Color.FromArgb(227, 208, 233), Color.FromArgb(132, 118, 138), Color.FromArgb(64, 64, 64));
		_x46d2ee2a363fa637 = new VisualTipOfficePreset(Color.FromArgb(243, 217, 207), Color.IndianRed, Color.FromArgb(132, 96, 96), Color.FromArgb(48, 32, 32));
		_x67748d6645aa8af0 = new VisualTipOfficePreset(SystemColors.Info, SystemColors.Info, SystemColors.WindowFrame, SystemColors.InfoText);
		_x5dd39aeabb2bcad1 = new VisualTipOfficePreset(Color.FromArgb(221, 236, 254), Color.FromArgb(129, 169, 226), Color.FromArgb(59, 97, 156), Color.FromArgb(0, 0, 0));
		_xf2974a62abb9777f = new VisualTipOfficePreset(Color.FromArgb(240, 240, 240), Color.FromArgb(224, 224, 224), Color.FromArgb(128, 128, 128), Color.FromArgb(0, 0, 0));
		_xb3f077f6e70063ba = new VisualTipOfficePreset(Color.Gray, Color.Black, Color.Silver, Color.White);
		SystemEvents.UserPreferenceChanged += x1467c8d3688c2be6;
		x62b82ee93b5db6ed();
	}

	private static void x1467c8d3688c2be6(object xe0292b9ed559da7d, UserPreferenceChangedEventArgs xfbf34718e704c6bc)
	{
		if (xfbf34718e704c6bc.Category == UserPreferenceCategory.Color)
		{
			x62b82ee93b5db6ed();
		}
	}

	private static void x62b82ee93b5db6ed()
	{
		float hue = SystemColors.ActiveCaption.GetHue();
		VisualTipOfficePreset visualTipOfficePreset = ((hue >= 200f && hue <= 250f) ? XPBlue : ((hue >= 70f && hue <= 120f) ? XPGreen : ((hue != 0f && !(SystemColors.ActiveCaption.GetSaturation() < 0.1f)) ? Control : XPSilver)));
		_xa12904199a25f8c7 = (VisualTipOfficePreset)visualTipOfficePreset.MemberwiseClone();
	}
}
