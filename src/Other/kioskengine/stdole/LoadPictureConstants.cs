using System.Runtime.InteropServices;

namespace stdole;

[Guid("E6C8FA08-BD9F-11D0-985E-00C04FC29993")]
[ComVisible(true)]
public enum LoadPictureConstants
{
	Default = 0,
	Monochrome = 1,
	VgaColor = 2,
	Color = 4
}
