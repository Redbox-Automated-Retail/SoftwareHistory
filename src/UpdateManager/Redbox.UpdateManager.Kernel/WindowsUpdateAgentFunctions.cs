using Redbox.Core;
using Redbox.WUA;

namespace Redbox.UpdateManager.Kernel;

public static class WindowsUpdateAgentFunctions
{
	private const string Type = "Info";

	private const string Key = "WUAList";

	private const bool Encode = true;

	[KernelFunction(Name = "kernel.ListAll")]
	internal static bool ListAll()
	{
		Manager.Instance.ListAll(out var updates);
		return UpdateServiceFunctions.StatusMessage("Info", "WUAList", "All", "All updates", ObjectExtensions.ToJson(updates), encode: true);
	}

	[KernelFunction(Name = "kernel.ListInstalled")]
	internal static bool ListInstalled()
	{
		Manager.Instance.ListInstalled(out var updates);
		return UpdateServiceFunctions.StatusMessage("Info", "WUAList", "Installed", "Only installed updates", ObjectExtensions.ToJson(updates), encode: true);
	}

	[KernelFunction(Name = "kernel.ListAvailable")]
	internal static bool ListAvailable()
	{
		Manager.Instance.ListAvailable(out var updates);
		return UpdateServiceFunctions.StatusMessage("Info", "WUAList", "Available", "Only available updates", ObjectExtensions.ToJson(updates), encode: true);
	}

	[KernelFunction(Name = "kernel.ListDownloadedNotInstalled")]
	internal static bool ListDownloadedNotInstalled()
	{
		Manager.Instance.ListDownloadedNotInstalled(out var updates);
		return UpdateServiceFunctions.StatusMessage("Info", "WUAList", "DownloadedNotInstalled", "Only downloaded but not yet installed updates", ObjectExtensions.ToJson(updates), encode: true);
	}
}
