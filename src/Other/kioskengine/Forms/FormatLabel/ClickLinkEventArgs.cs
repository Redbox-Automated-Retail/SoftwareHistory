using System;

namespace Alsing.Windows.Forms.FormatLabel;

public class ClickLinkEventArgs : EventArgs
{
	public string Link = "";

	public ClickLinkEventArgs(string Link)
	{
		this.Link = Link;
	}
}
