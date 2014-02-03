using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("EFF90582-2DDC-480F-A06D-60F3FBC362C3")]
[CoClass(typeof(StringCollectionClass))]
internal interface StringCollection : IStringCollection
{
}
