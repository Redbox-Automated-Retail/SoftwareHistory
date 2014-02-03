using System;
using System.Reflection;

namespace x1b802d6029d7be4a;

internal sealed class x2a4816fc89870133
{
	public static readonly Type xdc0071b1d90f9481;

	public static readonly Type x8b354663120944da;

	private x2a4816fc89870133()
	{
	}

	static x2a4816fc89870133()
	{
		if (Environment.Version.Major >= 2)
		{
			xdc0071b1d90f9481 = Assembly.LoadWithPartialName("System.Windows.Forms").GetType("System.Windows.Forms.ToolStrip", throwOnError: false);
			x8b354663120944da = Assembly.LoadWithPartialName("System.Windows.Forms").GetType("System.Windows.Forms.ToolStripItem", throwOnError: false);
		}
	}
}
