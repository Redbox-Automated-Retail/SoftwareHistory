using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ComImport]
[Guid("D27CDB6C-AE6D-11CF-96B8-444553540000")]
[CoClass(typeof(ShockwaveFlashClass))]
public interface ShockwaveFlash : IShockwaveFlash, _IShockwaveFlashEvents_Event
{
}
