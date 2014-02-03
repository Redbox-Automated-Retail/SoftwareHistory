using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("04C6895D-EAF2-4034-97F3-311DE9BE413A")]
[CoClass(typeof(UpdateSearcherClass))]
internal interface UpdateSearcher : IUpdateSearcher3
{
}
