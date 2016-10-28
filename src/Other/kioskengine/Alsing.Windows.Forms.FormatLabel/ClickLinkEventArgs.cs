using System;
using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.FormatLabel;

[ComVisible(true)]
public class ClickLinkEventArgs : EventArgs
{
	public string Link = "";

	public ClickLinkEventArgs(string Link)
	{
		this.Link = Link;
	}
}
