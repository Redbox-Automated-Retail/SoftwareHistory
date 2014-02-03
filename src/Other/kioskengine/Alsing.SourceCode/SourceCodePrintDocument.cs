using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;

namespace Alsing.SourceCode;

[ToolboxItem(true)]
public class SourceCodePrintDocument : PrintDocument
{
	private Font fontBreak;

	private Font fontNormal;

	private RowList rc;

	private int RowIndex;

	public SyntaxDocument Document { get; set; }

	public SourceCodePrintDocument()
	{
	}

	public SourceCodePrintDocument(SyntaxDocument document)
	{
		Document = document;
	}

	protected override void OnBeginPrint(PrintEventArgs ev)
	{
		base.OnBeginPrint(ev);
		fontNormal = new Font("Courier new", 8f, FontStyle.Regular);
		fontBreak = new Font("Symbol", 8f, FontStyle.Bold);
		RowIndex = 0;
	}

	protected override void OnPrintPage(PrintPageEventArgs ev)
	{
		int num = 0;
		float num2 = ev.MarginBounds.Left;
		float num3 = ev.MarginBounds.Right;
		float num4 = ev.MarginBounds.Top;
		if (rc == null)
		{
			Document.ParseAll();
			Document.ParseAll(ParseKeywords: true);
			rc = new RowList();
			foreach (Row item in Document)
			{
				bool flag = false;
				float num5 = num2;
				Row row2 = new Row();
				rc.Add(row2);
				foreach (Word item2 in item)
				{
					Font font = fontNormal;
					if (item2.Style != null)
					{
						FontStyle fontStyle = FontStyle.Regular;
						if (item2.Style.Bold)
						{
							fontStyle |= FontStyle.Bold;
						}
						if (item2.Style.Italic)
						{
							fontStyle |= FontStyle.Italic;
						}
						if (item2.Style.Underline)
						{
							fontStyle |= FontStyle.Underline;
						}
						font = new Font("Courier new", 8f, fontStyle);
					}
					SizeF sizeF = ev.Graphics.MeasureString(item2.Text, font);
					if (num5 + sizeF.Width > num3)
					{
						char c = '¿';
						Word word2 = new Word();
						word2.Text = string.Concat(c);
						word2.InfoTip = "break char";
						Word word3 = word2;
						row2.Add(word3);
						flag = true;
						row2 = new Row();
						rc.Add(row2);
						num5 = num2;
					}
					num5 += sizeF.Width;
					row2.Add(item2);
				}
				if (flag)
				{
					rc.Add(new Row());
				}
			}
		}
		base.OnPrintPage(ev);
		float num6 = (float)ev.MarginBounds.Height / fontNormal.GetHeight(ev.Graphics);
		while ((float)num < num6 && RowIndex < rc.Count)
		{
			float num7 = num2;
			float y = num4 + (float)num * fontNormal.GetHeight(ev.Graphics);
			Row row3 = rc[RowIndex];
			foreach (Word item3 in row3)
			{
				if (item3.InfoTip != null && item3.InfoTip == "break char")
				{
					ev.Graphics.DrawString(item3.Text, fontBreak, Brushes.Black, num7, y, new StringFormat());
					continue;
				}
				SizeF sizeF2 = ev.Graphics.MeasureString(item3.Text, fontNormal);
				if (item3.Text != null && ".,:;".IndexOf(item3.Text) >= 0)
				{
					sizeF2.Width = 6f;
					num7 -= 4f;
				}
				if (item3.Text == "\t")
				{
					sizeF2.Width = ev.Graphics.MeasureString("...", fontNormal).Width;
				}
				Color color = Color.Black;
				Font font2 = fontNormal;
				if (item3.Style != null)
				{
					color = item3.Style.ForeColor;
					FontStyle fontStyle2 = FontStyle.Regular;
					if (item3.Style.Bold)
					{
						fontStyle2 |= FontStyle.Bold;
					}
					if (item3.Style.Italic)
					{
						fontStyle2 |= FontStyle.Italic;
					}
					if (item3.Style.Underline)
					{
						fontStyle2 |= FontStyle.Underline;
					}
					font2 = new Font("Courier new", 8f, fontStyle2);
					if (!item3.Style.Transparent)
					{
						Color backColor = item3.Style.BackColor;
						ev.Graphics.FillRectangle(new SolidBrush(backColor), num7, y, sizeF2.Width, fontNormal.GetHeight(ev.Graphics));
					}
				}
				color = Color.FromArgb(color.R, color.G, color.B);
				ev.Graphics.DrawString(item3.Text, font2, new SolidBrush(color), num7, y, new StringFormat());
				num7 += sizeF2.Width;
			}
			num++;
			RowIndex++;
		}
		ev.HasMorePages = RowIndex < rc.Count;
	}
}
