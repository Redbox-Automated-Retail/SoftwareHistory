using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(UpdateSessionClass))]
[Guid("918EFD1E-B5D8-4C90-8540-AEB9BDC56F9D")]
internal interface UpdateSession : IUpdateSession3
{
}
