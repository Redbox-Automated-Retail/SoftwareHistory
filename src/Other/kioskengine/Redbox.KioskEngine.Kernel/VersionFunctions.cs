using System;
using System.Diagnostics;
using System.IO;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class VersionFunctions
{
	[KernelFunction(Name = "kernel.fileversion")]
	internal static string FileVersion(string filePath)
	{
		if (File.Exists(filePath))
		{
			return FileVersionInfo.GetVersionInfo(filePath).FileVersion;
		}
		return null;
	}

	[KernelFunction(Name = "kernel.osversion")]
	internal static string OSVersion()
	{
		return System.Environment.OSVersion.ToString();
	}
}
