using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComImport]
[Guid("D27CDB72-AE6D-11CF-96B8-444553540000")]
[CoClass(typeof(FlashObjectInterfaceClass))]
public interface FlashObjectInterface : IFlashObjectInterface
{
}
