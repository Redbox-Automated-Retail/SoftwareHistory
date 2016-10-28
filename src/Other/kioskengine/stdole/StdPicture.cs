using System.Runtime.InteropServices;

namespace stdole;

[ComImport]
[CoClass(typeof(StdPictureClass))]
[Guid("7BF80981-BF32-101A-8BBB-00AA00300CAB")]
[ComVisible(true)]
public interface StdPicture : Picture
{
}
