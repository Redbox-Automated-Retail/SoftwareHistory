using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[CoClass(typeof(InstallationAgentClass))]
[Guid("925CBC18-A2EA-4648-BF1C-EC8BADCFE20A")]
internal interface InstallationAgent : IInstallationAgent
{
}
