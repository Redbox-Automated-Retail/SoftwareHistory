using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class DiskfileHandlerFactory
{
	internal static DiskfileHandlerFactory Instance => Singleton<DiskfileHandlerFactory>.Instance;

	internal IFileDiskHandler MakeHandler(string script)
	{
		if (string.Compare(script, "return", ignoreCase: true) == 0)
		{
			return new ReturnHandler();
		}
		if (string.Compare(script, "quick-return", ignoreCase: true) == 0)
		{
			return new ReturnHandler();
		}
		if (string.Compare(script, "check-picker", ignoreCase: true) == 0)
		{
			return new CheckPickerHandler();
		}
		return new DefaultHandler();
	}

	private DiskfileHandlerFactory()
	{
	}
}
