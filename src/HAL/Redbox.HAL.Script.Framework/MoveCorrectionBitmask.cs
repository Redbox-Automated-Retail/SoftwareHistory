using System;

namespace Redbox.HAL.Script.Framework;

[Flags]
internal enum MoveCorrectionBitmask
{
	Return = 1,
	Vend = 2,
	Unload = 4
}
