using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("ADE87BF7-7B56-4275-8FAB-B9B0E591844B")]
[CoClass(typeof(SystemInformationClass))]
internal interface SystemInformation : ISystemInformation
{
}
