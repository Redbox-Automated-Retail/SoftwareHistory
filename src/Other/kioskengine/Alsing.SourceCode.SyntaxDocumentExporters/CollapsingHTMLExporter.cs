using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Alsing.SourceCode.SyntaxDocumentExporters;

[ComVisible(true)]
public class CollapsingHTMLExporter
{
	private StringBuilder sb;

	public string Export(SyntaxDocument doc, string ImagePath)
	{
		return Export(doc, Color.Transparent, ImagePath, "");
	}

	public string Export(SyntaxDocument doc, Color BGColor, string ImagePath, string Style)
	{
		sb = new StringBuilder();
		doc.ParseAll(ParseKeywords: true);
		int num = 0;
		string text = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture);
		if (BGColor.A == 0)
		{
			Out("<table  ><tr><td nowrap><div style=\"" + Style + "\">");
		}
		else
		{
			Out("<table  style=\"background-color:" + GetHTMLColor(BGColor) + ";" + Style + "\"><tr><td nowrap><div>");
		}
		foreach (Row item in doc)
		{
			num++;
			if (item.CanFold)
			{
				RenderCollapsed(item.VirtualCollapsedRow, item, num, ImagePath, text);
				Out("<div style=\"display:block;\" id=\"open" + text + "_" + num.ToString(CultureInfo.InvariantCulture) + "\">");
				string text2 = "minus.gif";
				try
				{
					if (item.expansion_StartSpan.Parent.Parent == null)
					{
						text2 = "minusNoTopLine.gif";
					}
				}
				catch
				{
				}
				Out("<img src=\"" + ImagePath + text2 + "\" align=top onclick=\"open" + text + "_" + num.ToString(CultureInfo.InvariantCulture) + ".style.display='none'; closed" + text + "_" + num.ToString(CultureInfo.InvariantCulture) + ".style.display='block'; \">");
			}
			else if (item.CanFoldEndPart)
			{
				Out("<img src=\"" + ImagePath + "L.gif\"  align=top>");
			}
			else if (item.HasExpansionLine)
			{
				Out("<img src=\"" + ImagePath + "I.gif\"  align=top>");
			}
			else
			{
				Out("<img src=\"" + ImagePath + "clear.gif\"  align=top>");
			}
			foreach (Word item2 in item)
			{
				write(item2.Text, item2.Style);
			}
			if (item.CanFoldEndPart)
			{
				Out("</div>\n");
			}
			else
			{
				Out("<br>\n");
			}
		}
		Out("</div></td></tr></table>");
		return sb.ToString();
	}

	private void RenderCollapsed(Row r, Row TrueRow, int i, string ImagePath, string guid)
	{
		Out("<div style=\"display:none;\" id=\"closed" + guid + "_" + i.ToString(CultureInfo.InvariantCulture) + "\">");
		string text = "plus.gif";
		try
		{
			if (TrueRow.expansion_StartSpan.Parent.Parent == null)
			{
				text = "PlusNoLines.gif";
			}
		}
		catch
		{
		}
		Out("<img src=\"" + ImagePath + text + "\" align=top onclick=\"open" + guid + "_" + i.ToString(CultureInfo.InvariantCulture) + ".style.display='block'; closed" + guid + "_" + i.ToString(CultureInfo.InvariantCulture) + ".style.display='none'; \">");
		foreach (Word item in r)
		{
			write(item.Text, item.Style);
		}
		Out("</div>");
	}

	private void write(string text, TextStyle s)
	{
		if (s != null)
		{
			if (s.Bold)
			{
				Out("<b>");
			}
			if (s.Italic)
			{
				Out("<i>");
			}
			if (s.Transparent)
			{
				Out("<span style=\"color:" + GetHTMLColor(s.ForeColor) + "\">");
			}
			else
			{
				Out("<span style=\"color:" + GetHTMLColor(s.ForeColor) + ";background-color:" + GetHTMLColor(s.BackColor) + ";\">");
			}
		}
		text = text.Replace("&", "&amp;");
		text = text.Replace("<", "&lt;");
		text = text.Replace(">", "&gt;");
		text = text.Replace(" ", "&nbsp;");
		text = text.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
		Out(text);
		if (s != null)
		{
			Out("</span>");
			if (s.Italic)
			{
				Out("</i>");
			}
			if (s.Bold)
			{
				Out("</b>");
			}
		}
	}

	private static string GetHTMLColor(Color c)
	{
		return string.Format("#{0}{1}{2}", c.R.ToString("x2"), c.G.ToString("x2"), c.B.ToString("x2"));
	}

	private void Out(string text)
	{
		sb.Append(text);
	}
}
