using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(UpdateInstallerClass))]
[Guid("3442D4FE-224D-4CEE-98CF-30E0C4D229E6")]
internal interface UpdateInstaller : IUpdateInstaller2
{
}
