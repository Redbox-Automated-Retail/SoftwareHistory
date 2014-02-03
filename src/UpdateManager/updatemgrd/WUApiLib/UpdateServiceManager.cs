using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(UpdateServiceManagerClass))]
[Guid("0BB8531D-7E8D-424F-986C-A0B8F60A3E7B")]
internal interface UpdateServiceManager : IUpdateServiceManager2
{
}
