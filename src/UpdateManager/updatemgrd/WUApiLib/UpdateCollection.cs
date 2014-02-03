using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(UpdateCollectionClass))]
[Guid("07F7438C-7709-4CA5-B518-91279288134E")]
internal interface UpdateCollection : IUpdateCollection
{
}
