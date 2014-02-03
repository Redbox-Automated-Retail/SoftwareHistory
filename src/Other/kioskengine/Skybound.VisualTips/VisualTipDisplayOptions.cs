using System;

namespace Skybound.VisualTips;

[Flags]
public enum VisualTipDisplayOptions
{
	Default = 0,
	PositionMask = 3,
	PositionBottom = 0,
	PositionRight = 1,
	PositionLeft = 2,
	PositionTop = PositionMask,
	HideOnMouseLeave = 8,
	HideOnKeyDown = 0x10,
	HideOnKeyPress = 0x20,
	HideOnMouseDown = 0x40,
	HideOnLostFocus = 0x80,
	HideOnTextChanged = 0x100,
	ForwardEscapeKey = 0x200
}
