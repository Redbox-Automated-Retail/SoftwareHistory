using System;
using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.SyntaxBox;

[ComVisible(true)]
public class ListItem : IComparable
{
	public string InsertText = "";

	public string Text = "";

	public string ToolTip = "";

	public int Type;

	public ListItem(string text, int type)
	{
		Text = text;
		Type = type;
		ToolTip = "";
	}

	public ListItem(string text, int type, string tooltip)
	{
		Text = text;
		Type = type;
		ToolTip = tooltip;
	}

	public ListItem(string text, int type, string tooltip, string inserttext)
	{
		Text = text;
		Type = type;
		ToolTip = tooltip;
		InsertText = inserttext;
	}

	public int CompareTo(object obj)
	{
		ListItem listItem = (ListItem)obj;
		return Text.CompareTo(listItem.Text);
	}

	public override string ToString()
	{
		return Text;
	}
}
