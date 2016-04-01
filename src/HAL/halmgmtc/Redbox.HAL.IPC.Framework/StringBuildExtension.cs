using System.Text;

namespace Redbox.HAL.IPC.Framework;

internal static class StringBuildExtension
{
	internal static void Reset(StringBuilder builder)
	{
		if (builder.Length > 0)
		{
			builder.Length = 0;
		}
	}
}
