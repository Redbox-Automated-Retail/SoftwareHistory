using System;

namespace Skybound.VisualTips;

[Flags]
public enum VisualTipDisplayPosition
{
	Bottom = 0,
	Right = 1,
	Left = 2,
	Top = Right | Left
}
