using System;
using System.Management;
using System.Runtime.InteropServices;

namespace HALUtilities;

internal sealed class ComputerInfo
{
	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

	internal void Display()
	{
		ManagementClass managementClass = new ManagementClass("Win32_ComputerSystem");
		foreach (ManagementObject instance in managementClass.GetInstances())
		{
			Console.WriteLine("PC {0}/{1}", instance["Manufacturer"].ToString(), instance["Model"].ToString());
		}
		if (GetPhysicallyInstalledSystemMemory(out var totalMemoryInKilobytes))
		{
			Console.WriteLine(totalMemoryInKilobytes / 1024 / 1024 + " GB of RAM installed.");
		}
		else
		{
			Console.WriteLine("Unable to obtain memory: GLE = {0}", Marshal.GetLastWin32Error());
		}
	}
}
