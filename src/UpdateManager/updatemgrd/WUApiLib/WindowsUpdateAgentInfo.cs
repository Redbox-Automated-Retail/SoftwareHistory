using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("85713FA1-7796-4FA2-BE3B-E2D6124DD373")]
[CoClass(typeof(WindowsUpdateAgentInfoClass))]
internal interface WindowsUpdateAgentInfo : IWindowsUpdateAgentInfo
{
}
