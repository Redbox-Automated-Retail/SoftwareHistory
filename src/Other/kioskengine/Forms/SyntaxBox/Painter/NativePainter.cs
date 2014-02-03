using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Alsing.Drawing.GDI;
using Alsing.Globalization;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox.Painter;

public class NativePainter : IPainter, IDisposable
{
	private readonly EditViewControl Control;

	private Word BracketEnd;

	private Word BracketStart;

	private int FirstSpanRow;

	private RenderItems GFX;

	private int LastRow;

	private int LastSpanRow;

	private bool RenderCaretRowOnly;

	private int ResizeCount;

	private bool SpanFound;

	private int yOffset;

	public NativePainter(EditViewControl control)
	{
		Control = control;
		InitGraphics();
	}

	public void Resize()
	{
		ResizeCount++;
		InitGraphics();
	}

	public Size MeasureString(string s)
	{
		try
		{
			GFX.StringBuffer.Font = GFX.FontNormal;
			return GFX.StringBuffer.MeasureTabbedString(s, Control.TabSize);
		}
		catch
		{
			return new Size(0, 0);
		}
	}

	public void InitGraphics()
	{
		try
		{
			if (GFX.BackgroundBrush != null)
			{
				GFX.BackgroundBrush.Dispose();
			}
			if (GFX.GutterMarginBrush != null)
			{
				GFX.GutterMarginBrush.Dispose();
			}
			if (GFX.LineNumberMarginBrush != null)
			{
				GFX.LineNumberMarginBrush.Dispose();
			}
			if (GFX.HighLightLineBrush != null)
			{
				GFX.HighLightLineBrush.Dispose();
			}
			if (GFX.LineNumberMarginBorderBrush != null)
			{
				GFX.LineNumberMarginBorderBrush.Dispose();
			}
			if (GFX.GutterMarginBorderBrush != null)
			{
				GFX.GutterMarginBorderBrush.Dispose();
			}
			if (GFX.OutlineBrush != null)
			{
				GFX.OutlineBrush.Dispose();
			}
			GFX.BackgroundBrush = new GDIBrush(Control.BackColor);
			GFX.GutterMarginBrush = new GDIBrush(Control.GutterMarginColor);
			GFX.LineNumberMarginBrush = new GDIBrush(Control.LineNumberBackColor);
			GFX.HighLightLineBrush = new GDIBrush(Control.HighLightedLineColor);
			GFX.LineNumberMarginBorderBrush = new GDIBrush(Control.LineNumberBorderColor);
			GFX.GutterMarginBorderBrush = new GDIBrush(Control.GutterMarginBorderColor);
			GFX.OutlineBrush = new GDIBrush(Control.OutlineColor);
			if (GFX.FontNormal != null)
			{
				GFX.FontNormal.Dispose();
			}
			if (GFX.FontBold != null)
			{
				GFX.FontBold.Dispose();
			}
			if (GFX.FontItalic != null)
			{
				GFX.FontItalic.Dispose();
			}
			if (GFX.FontBoldItalic != null)
			{
				GFX.FontBoldItalic.Dispose();
			}
			if (GFX.FontUnderline != null)
			{
				GFX.FontUnderline.Dispose();
			}
			if (GFX.FontBoldUnderline != null)
			{
				GFX.FontBoldUnderline.Dispose();
			}
			if (GFX.FontItalicUnderline != null)
			{
				GFX.FontItalicUnderline.Dispose();
			}
			if (GFX.FontBoldItalicUnderline != null)
			{
				GFX.FontBoldItalicUnderline.Dispose();
			}
			string fontName = Control.FontName;
			float fontSize = Control.FontSize;
			GFX.FontNormal = new GDIFont(fontName, fontSize, bold: false, italic: false, underline: false, strikethrough: false);
			GFX.FontBold = new GDIFont(fontName, fontSize, bold: true, italic: false, underline: false, strikethrough: false);
			GFX.FontItalic = new GDIFont(fontName, fontSize, bold: false, italic: true, underline: false, strikethrough: false);
			GFX.FontBoldItalic = new GDIFont(fontName, fontSize, bold: true, italic: true, underline: false, strikethrough: false);
			GFX.FontUnderline = new GDIFont(fontName, fontSize, bold: false, italic: false, underline: true, strikethrough: false);
			GFX.FontBoldUnderline = new GDIFont(fontName, fontSize, bold: true, italic: false, underline: true, strikethrough: false);
			GFX.FontItalicUnderline = new GDIFont(fontName, fontSize, bold: false, italic: true, underline: true, strikethrough: false);
			GFX.FontBoldItalicUnderline = new GDIFont(fontName, fontSize, bold: true, italic: true, underline: true, strikethrough: false);
			InitIMEWindow();
		}
		catch (Exception)
		{
		}
		if (Control != null && Control.IsHandleCreated)
		{
			if (GFX.StringBuffer != null)
			{
				GFX.StringBuffer.Dispose();
			}
			if (GFX.SelectionBuffer != null)
			{
				GFX.SelectionBuffer.Dispose();
			}
			if (GFX.BackBuffer != null)
			{
				GFX.BackBuffer.Dispose();
			}
			GFX.StringBuffer = new GDISurface(1, 1, Control, BindControl: true)
			{
				Font = GFX.FontNormal
			};
			int height = GFX.StringBuffer.MeasureTabbedString("ABC", 0).Height + Control._SyntaxBox.RowPadding;
			GFX.BackBuffer = new GDISurface(Control.ClientWidth, height, Control, BindControl: true)
			{
				Font = GFX.FontNormal
			};
			GFX.SelectionBuffer = new GDISurface(Control.ClientWidth, height, Control, BindControl: true)
			{
				Font = GFX.FontNormal
			};
			Control.View.RowHeight = GFX.BackBuffer.MeasureTabbedString("ABC", 0).Height + Control._SyntaxBox.RowPadding;
			Control.View.CharWidth = GFX.BackBuffer.MeasureTabbedString(" ", 0).Width;
		}
	}

	public void RenderAll()
	{
		Control.View.RowHeight = GFX.BackBuffer.MeasureString("ABC").Height;
		Control.View.CharWidth = GFX.BackBuffer.MeasureString(" ").Width;
		Control.InitVars();
		Graphics graphics = Control.CreateGraphics();
		RenderAll(graphics);
		graphics.Dispose();
	}

	public void RenderCaret(Graphics g)
	{
		RenderCaretRowOnly = true;
		RenderAll(g);
		RenderCaretRowOnly = false;
	}

	public void RenderAll(Graphics g)
	{
		try
		{
			Control.InitVars();
			Control.InitScrollbars();
			SetBrackets();
			SetSpanIndicators();
			int firstVisibleRow = Control.View.FirstVisibleRow;
			int num = firstVisibleRow - LastRow;
			LastRow = firstVisibleRow;
			if (Control.SmoothScroll)
			{
				switch (num)
				{
				case 1:
				{
					for (int num2 = Control.View.RowHeight; num2 > 0; num2 -= Control.SmoothScrollSpeed)
					{
						yOffset = num2 + Control.View.YOffset;
						RenderAll2();
						g.Flush();
						Thread.Sleep(0);
					}
					break;
				}
				case -1:
				{
					for (int i = -Control.View.RowHeight; i < 0; i += Control.SmoothScrollSpeed)
					{
						yOffset = i + Control.View.YOffset;
						RenderAll2();
						g.Flush();
						Thread.Sleep(0);
					}
					break;
				}
				}
			}
			yOffset = Control.View.YOffset;
			RenderAll2();
		}
		catch
		{
		}
	}

	public void RenderRow(int RowIndex)
	{
		RenderRow(RowIndex, 10);
	}

	public TextPoint CharFromPixel(int X, int Y)
	{
		try
		{
			int val = Y / Control.View.RowHeight + Control.View.FirstVisibleRow;
			val = Math.Min(val, Control.Document.VisibleRows.Count);
			if (val == Control.Document.VisibleRows.Count)
			{
				val--;
				Row row = Control.Document.VisibleRows[val];
				if (row.IsCollapsed)
				{
					row = row.Expansion_EndRow;
				}
				return new TextPoint(row.Text.Length, row.Index);
			}
			val = Math.Max(val, 0);
			if (Control.Document.VisibleRows.Count != 0)
			{
				Row row2 = Control.Document.VisibleRows[val];
				val = Control.Document.IndexOf(row2);
				if (val == -1)
				{
					return new TextPoint(-1, -1);
				}
				if (!row2.IsCollapsed)
				{
					return ColumnFromPixel(val, X);
				}
				if (X < row2.Expansion_PixelEnd - Control.View.FirstVisibleColumn * Control.View.CharWidth)
				{
					return ColumnFromPixel(val, X);
				}
				if (X >= row2.Expansion_EndRow.Expansion_PixelStart - Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin)
				{
					return ColumnFromPixel(row2.Expansion_EndRow.Index, X - row2.Expansion_EndRow.Expansion_PixelStart + MeasureRow(row2.Expansion_EndRow, row2.Expansion_EndRow.Expansion_StartChar).Width);
				}
				return new TextPoint(row2.Expansion_EndChar, row2.Index);
			}
			return new TextPoint(0, 0);
		}
		catch
		{
			Control._SyntaxBox.FontName = "Courier New";
			Control._SyntaxBox.FontSize = 10f;
			return new TextPoint(0, 0);
		}
	}

	public int GetMaxCharWidth()
	{
		int num = 0;
		string text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		foreach (char c in text)
		{
			int width = MeasureString(string.Concat(c)).Width;
			if (width > num)
			{
				num = width;
			}
		}
		return num;
	}

	public void Dispose()
	{
		if (GFX.FontNormal != null)
		{
			GFX.FontNormal.Dispose();
		}
		if (GFX.FontBold != null)
		{
			GFX.FontBold.Dispose();
		}
		if (GFX.FontItalic != null)
		{
			GFX.FontItalic.Dispose();
		}
		if (GFX.FontBoldItalic != null)
		{
			GFX.FontBoldItalic.Dispose();
		}
		if (GFX.FontUnderline != null)
		{
			GFX.FontUnderline.Dispose();
		}
		if (GFX.FontBoldUnderline != null)
		{
			GFX.FontBoldUnderline.Dispose();
		}
		if (GFX.FontItalicUnderline != null)
		{
			GFX.FontItalicUnderline.Dispose();
		}
		if (GFX.FontBoldItalicUnderline != null)
		{
			GFX.FontBoldItalicUnderline.Dispose();
		}
		if (GFX.BackgroundBrush != null)
		{
			GFX.BackgroundBrush.Dispose();
		}
		if (GFX.GutterMarginBrush != null)
		{
			GFX.GutterMarginBrush.Dispose();
		}
		if (GFX.LineNumberMarginBrush != null)
		{
			GFX.LineNumberMarginBrush.Dispose();
		}
		if (GFX.HighLightLineBrush != null)
		{
			GFX.HighLightLineBrush.Dispose();
		}
		if (GFX.LineNumberMarginBorderBrush != null)
		{
			GFX.LineNumberMarginBorderBrush.Dispose();
		}
		if (GFX.GutterMarginBorderBrush != null)
		{
			GFX.GutterMarginBorderBrush.Dispose();
		}
		if (GFX.OutlineBrush != null)
		{
			GFX.OutlineBrush.Dispose();
		}
		if (GFX.StringBuffer != null)
		{
			GFX.StringBuffer.Dispose();
		}
		if (GFX.SelectionBuffer != null)
		{
			GFX.SelectionBuffer.Dispose();
		}
		if (GFX.BackBuffer != null)
		{
			GFX.BackBuffer.Dispose();
		}
	}

	private void InitIMEWindow()
	{
		if (Control.IMEWindow != null)
		{
			Control.IMEWindow.SetFont(Control.FontName, Control.FontSize);
		}
	}

	public Size MeasureRow(Row xtr, int Count)
	{
		int num = 0;
		int taborigin = -Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin;
		int num2 = Control.View.TextMargin - Control.View.ClientAreaStart;
		if (xtr.InQueue)
		{
			SetStringFont(bold: false, italic: false, underline: false);
			int count = Math.Max(Count - xtr.Text.Length, 0);
			string text = new string(' ', count);
			string text2 = xtr.Text + text;
			num = GFX.StringBuffer.MeasureTabbedString(text2.Substring(0, Count), Control.PixelTabSize).Width;
		}
		else
		{
			int num3 = 0;
			int num4 = 0;
			foreach (Word formattedWord in xtr.FormattedWords)
			{
				if (formattedWord.Type == WordType.Word && formattedWord.Style != null)
				{
					SetStringFont(formattedWord.Style.Bold, formattedWord.Style.Italic, formattedWord.Style.Underline);
				}
				else
				{
					SetStringFont(bold: false, italic: false, underline: false);
				}
				if (formattedWord.Text.Length + num3 >= Count || formattedWord == xtr.FormattedWords[xtr.FormattedWords.Count - 1])
				{
					int val = Count - num3;
					int length = Math.Min(val, formattedWord.Text.Length);
					num4 += GFX.StringBuffer.DrawTabbedString(formattedWord.Text.Substring(0, length), num2 + num4, 0, taborigin, Control.PixelTabSize).Width;
					num = num4;
					break;
				}
				num4 += GFX.StringBuffer.DrawTabbedString(formattedWord.Text, num2 + num4, 0, taborigin, Control.PixelTabSize).Width;
				num3 += formattedWord.Text.Length;
			}
			SetStringFont(bold: false, italic: false, underline: false);
			int count2 = Math.Max(Count - xtr.Text.Length, 0);
			string text3 = new string(' ', count2);
			num += GFX.StringBuffer.DrawTabbedString(text3, num2 + num4, 0, taborigin, Control.PixelTabSize).Width;
		}
		return new Size(num, 0);
	}

	private void SetBrackets()
	{
		BracketEnd = null;
		BracketStart = null;
		Word currentWord = Control.Caret.CurrentWord;
		if (currentWord == null)
		{
			return;
		}
		Span span = currentWord.Span;
		if (span == null)
		{
			return;
		}
		if ((currentWord == span.StartWord || currentWord == span.EndWord) && span.EndWord != null)
		{
			BracketEnd = span.EndWord;
			BracketStart = span.StartWord;
		}
		try
		{
			if (currentWord.Pattern != null)
			{
				if (currentWord.Pattern.BracketType == BracketType.EndBracket)
				{
					Word startBracketWord = Control.Document.GetStartBracketWord(currentWord, currentWord.Pattern.MatchingBracket, currentWord.Span);
					BracketEnd = currentWord;
					BracketStart = startBracketWord;
				}
				if (currentWord.Pattern.BracketType == BracketType.StartBracket)
				{
					Word endBracketWord = Control.Document.GetEndBracketWord(currentWord, currentWord.Pattern.MatchingBracket, currentWord.Span);
					BracketEnd = endBracketWord;
					BracketStart = currentWord;
				}
			}
		}
		catch
		{
		}
	}

	private void SetSpanIndicators()
	{
		SpanFound = false;
		try
		{
			Span span = Control.Caret.CurrentSegment();
			if (span != null && span.StartWord != null && span.StartWord.Row != null && span.EndWord != null && span.EndWord.Row != null)
			{
				FirstSpanRow = span.StartWord.Row.Index;
				LastSpanRow = span.EndWord.Row.Index;
				SpanFound = true;
			}
		}
		catch
		{
		}
	}

	private void RenderAll2()
	{
		try
		{
			int num = Control.View.FirstVisibleRow;
			if (Control.AutoListStartPos != null)
			{
				try
				{
					if (Control.AutoListVisible)
					{
						Point textPointPixelPos = GetTextPointPixelPos(Control.AutoListStartPos);
						if (textPointPixelPos == new Point(-1, -1))
						{
							Control.AutoList.Visible = false;
						}
						else
						{
							textPointPixelPos.Y += Control.View.RowHeight + 2;
							textPointPixelPos.X += -20;
							textPointPixelPos = Control.PointToScreen(textPointPixelPos);
							Screen screen = Screen.FromPoint(new Point(Control.Right, textPointPixelPos.Y));
							if (textPointPixelPos.Y + Control.AutoList.Height > screen.WorkingArea.Height)
							{
								textPointPixelPos.Y -= Control.View.RowHeight + 2 + Control.AutoList.Height;
							}
							if (textPointPixelPos.X + Control.AutoList.Width > screen.WorkingArea.Width)
							{
								textPointPixelPos.X -= textPointPixelPos.X + Control.AutoList.Width - screen.WorkingArea.Width;
							}
							Control.AutoList.Location = textPointPixelPos;
							Control.Focus();
						}
					}
				}
				catch
				{
				}
			}
			if (Control.InfoTipStartPos != null)
			{
				try
				{
					if (Control.InfoTipVisible)
					{
						Point textPointPixelPos2 = GetTextPointPixelPos(Control.InfoTipStartPos);
						if (textPointPixelPos2 == new Point(-1, -1))
						{
							Control.InfoTip.Visible = false;
						}
						else
						{
							textPointPixelPos2.Y += Control.View.RowHeight + 2;
							textPointPixelPos2.X += -20;
							textPointPixelPos2 = Control.PointToScreen(textPointPixelPos2);
							Screen screen2 = Screen.FromPoint(new Point(Control.Right, textPointPixelPos2.Y));
							if (textPointPixelPos2.Y + Control.InfoTip.Height > screen2.WorkingArea.Height)
							{
								textPointPixelPos2.Y -= Control.View.RowHeight + 2 + Control.InfoTip.Height;
							}
							if (textPointPixelPos2.X + Control.InfoTip.Width > screen2.WorkingArea.Width)
							{
								textPointPixelPos2.X -= textPointPixelPos2.X + Control.InfoTip.Width - screen2.WorkingArea.Width;
							}
							Control.InfoTip.Location = textPointPixelPos2;
							Control.InfoTip.Visible = true;
						}
					}
					else
					{
						Control.InfoTip.Visible = false;
					}
				}
				catch
				{
				}
			}
			for (int i = 0; i < Control.View.VisibleRowCount; i++)
			{
				if (num >= 0 && num < Control.Document.VisibleRows.Count)
				{
					Row row = Control.Document.VisibleRows[num];
					if (RenderCaretRowOnly)
					{
						if (row == Control.Caret.CurrentRow)
						{
							RenderRow(Control.Document.IndexOf(row), i);
						}
						if (Control.Caret.CurrentRow.expansion_EndSpan != null && Control.Caret.CurrentRow.expansion_EndSpan.StartRow != null && Control.Caret.CurrentRow.expansion_EndSpan.StartRow == row)
						{
							RenderRow(Control.Document.IndexOf(row), i);
						}
					}
					else
					{
						RenderRow(Control.Document.IndexOf(row), i);
					}
				}
				else if (!RenderCaretRowOnly)
				{
					RenderRow(Control.Document.Count, i);
				}
				num++;
			}
		}
		catch
		{
		}
	}

	private void RenderRow(int RowIndex, int RowPos)
	{
		if (RowIndex >= 0 && RowIndex < Control.Document.Count && Control.Document[RowIndex].RowState == RowState.SpanParsed)
		{
			Control.Document.Parser.ParseRow(RowIndex, ParseKeywords: true);
			Control.Document[RowIndex].RowState = RowState.AllParsed;
		}
		try
		{
			GDISurface backBuffer = GFX.BackBuffer;
			bool flag = false;
			GDIBrush gDIBrush = GFX.BackgroundBrush;
			try
			{
				if (RowIndex < Control.Document.Count && RowIndex >= 0)
				{
					Row row = Control.Document[RowIndex];
					if (SpanFound && RowIndex >= FirstSpanRow && RowIndex <= LastSpanRow && Control._SyntaxBox.ScopeBackColor != Color.Transparent)
					{
						gDIBrush = new GDIBrush(Control._SyntaxBox.ScopeBackColor);
						flag = true;
					}
					else if (row.BackColor != Color.Transparent)
					{
						gDIBrush = new GDIBrush(row.BackColor);
						flag = true;
					}
					else if (row.endSpan != null)
					{
						for (Span span = row.expansion_EndSpan; span != null; span = span.Parent)
						{
							if (!span.spanDefinition.Transparent)
							{
								gDIBrush = new GDIBrush(span.spanDefinition.BackColor);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							for (Span span = row.endSpan; span != null; span = span.Parent)
							{
								if (!span.spanDefinition.Transparent)
								{
									gDIBrush = new GDIBrush(span.spanDefinition.BackColor);
									flag = true;
									break;
								}
							}
						}
						if (!flag)
						{
							for (Span span = row.expansion_EndSpan; span != null; span = span.Parent)
							{
								if (!span.spanDefinition.Transparent)
								{
									gDIBrush = new GDIBrush(span.spanDefinition.BackColor);
									flag = true;
									break;
								}
							}
						}
					}
				}
			}
			catch
			{
			}
			if (RowIndex == Control.Caret.Position.Y && Control.HighLightActiveLine)
			{
				backBuffer.Clear(GFX.HighLightLineBrush);
			}
			else if (RowIndex >= 0 && RowIndex < Control.Document.Count)
			{
				if (Control.Document[RowIndex].IsCollapsed)
				{
					if (Control.Document[RowIndex].Expansion_EndRow.Index == Control.Caret.Position.Y && Control.HighLightActiveLine)
					{
						backBuffer.Clear(GFX.HighLightLineBrush);
					}
					else
					{
						backBuffer.Clear(gDIBrush);
					}
				}
				else
				{
					backBuffer.Clear(gDIBrush);
				}
			}
			else
			{
				backBuffer.Clear(gDIBrush);
			}
			if (RowIndex <= Control.Selection.LogicalBounds.FirstRow || RowIndex >= Control.Selection.LogicalBounds.LastRow)
			{
				RenderText(RowIndex);
			}
			if (Control.Selection.IsValid && RowIndex >= Control.Selection.LogicalBounds.FirstRow && RowIndex <= Control.Selection.LogicalBounds.LastRow)
			{
				if (Control.ContainsFocus)
				{
					GFX.SelectionBuffer.Clear(Control.SelectionBackColor);
				}
				else
				{
					GFX.SelectionBuffer.Clear(Control.InactiveSelectionBackColor);
				}
				RenderSelectedText(RowIndex);
			}
			if (Control.ContainsFocus || Control.View.Action == EditAction.DragText)
			{
				RenderCaret(RowIndex, RowPos * Control.View.RowHeight + yOffset);
			}
			RenderSelection(RowIndex);
			RenderMargin(RowIndex);
			if (Control.Document.Folding)
			{
				RenderExpansion(RowIndex);
			}
			RowPaintEventArgs e = new RowPaintEventArgs();
			Rectangle bounds = new Rectangle(0, 0, Control.Width, Control.View.RowHeight);
			e.Graphics = Graphics.FromHdc(backBuffer.hDC);
			e.Bounds = bounds;
			e.Row = null;
			if (RowIndex >= 0 && RowIndex < Control.Document.Count)
			{
				e.Row = Control.Document[RowIndex];
			}
			Control._SyntaxBox.OnRenderRow(e);
			backBuffer.Flush();
			backBuffer.RenderToControl(0, RowPos * Control.View.RowHeight + yOffset);
			if (flag)
			{
				gDIBrush.Dispose();
			}
		}
		catch
		{
		}
	}

	private void SetFont(bool bold, bool italic, bool underline, GDISurface surface)
	{
		if (bold && italic && underline)
		{
			surface.Font = GFX.FontBoldItalicUnderline;
		}
		else if (bold && italic)
		{
			surface.Font = GFX.FontBoldItalic;
		}
		else if (bold && underline)
		{
			surface.Font = GFX.FontBoldUnderline;
		}
		else if (bold)
		{
			surface.Font = GFX.FontBold;
		}
		else if (italic && underline)
		{
			surface.Font = GFX.FontItalicUnderline;
		}
		else if (!italic && underline)
		{
			surface.Font = GFX.FontUnderline;
		}
		else if (italic)
		{
			surface.Font = GFX.FontItalic;
		}
		else
		{
			surface.Font = GFX.FontNormal;
		}
	}

	private void SetStringFont(bool bold, bool italic, bool underline)
	{
		SetFont(bold, italic, underline, GFX.StringBuffer);
	}

	private void RenderCollapsedSelectedText(int RowIndex, int xPos)
	{
		GDISurface selectionBuffer = GFX.SelectionBuffer;
		selectionBuffer.Font = GFX.FontBold;
		selectionBuffer.FontTransparent = true;
		selectionBuffer.TextForeColor = (Control.ContainsFocus ? Control.SelectionForeColor : Control.InactiveSelectionForeColor);
		Row row = Control.Document[RowIndex];
		string collapsedText = row.CollapsedText;
		xPos++;
		int taborigin = -Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin;
		GFX.StringBuffer.Font = GFX.FontBold;
		int width = GFX.StringBuffer.DrawTabbedString(collapsedText, xPos + 1, 0, taborigin, Control.PixelTabSize).Width;
		if (Control.ContainsFocus)
		{
			selectionBuffer.FillRect(Control.SelectionForeColor, xPos, 0, width + 2, Control.View.RowHeight);
			selectionBuffer.FillRect(Control.SelectionBackColor, xPos + 1, 1, width, Control.View.RowHeight - 2);
		}
		else
		{
			selectionBuffer.FillRect(Control.InactiveSelectionForeColor, xPos, 0, width + 2, Control.View.RowHeight);
			selectionBuffer.FillRect(Control.InactiveSelectionBackColor, xPos + 1, 1, width, Control.View.RowHeight - 2);
		}
		width = selectionBuffer.DrawTabbedString(collapsedText, xPos + 1, 0, taborigin, Control.PixelTabSize).Width;
		try
		{
			if (row.expansion_StartSpan.EndRow != null)
			{
				if (row.expansion_StartSpan.EndRow.RowState == RowState.SpanParsed)
				{
					Control.Document.Parser.ParseRow(row.expansion_StartSpan.EndRow.Index, ParseKeywords: true);
				}
				Word endWord = row.expansion_StartSpan.EndWord;
				xPos += Control.View.FirstVisibleColumn * Control.View.CharWidth;
				row.expansion_StartSpan.EndRow.Expansion_PixelStart = xPos + width - Control.View.TextMargin + 2;
				row.Expansion_PixelEnd = xPos - 1;
				RenderSelectedText(Control.Document.IndexOf(row.expansion_StartSpan.EndRow), row.expansion_StartSpan.EndRow.Expansion_PixelStart, endWord);
			}
		}
		catch
		{
		}
	}

	private void RenderCollapsedText(int RowIndex, int xPos)
	{
		GDISurface backBuffer = GFX.BackBuffer;
		backBuffer.Font = GFX.FontBold;
		backBuffer.FontTransparent = true;
		backBuffer.TextForeColor = Control.OutlineColor;
		Row row = Control.Document[RowIndex];
		string collapsedText = row.CollapsedText;
		xPos++;
		int taborigin = -Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin;
		GFX.StringBuffer.Font = GFX.FontBold;
		int width = GFX.StringBuffer.DrawTabbedString(collapsedText, xPos + 1, 0, taborigin, Control.PixelTabSize).Width;
		backBuffer.FillRect(GFX.OutlineBrush, xPos, 0, width + 2, Control.View.RowHeight);
		backBuffer.FillRect(GFX.BackgroundBrush, xPos + 1, 1, width, Control.View.RowHeight - 2);
		width = backBuffer.DrawTabbedString(collapsedText, xPos + 1, 0, taborigin, Control.PixelTabSize).Width;
		try
		{
			if (row.expansion_StartSpan.EndRow != null)
			{
				if (row.expansion_StartSpan.EndRow.RowState == RowState.SpanParsed)
				{
					Control.Document.Parser.ParseRow(row.expansion_StartSpan.EndRow.Index, ParseKeywords: true);
				}
				Word endWord = row.expansion_StartSpan.EndWord;
				xPos += Control.View.FirstVisibleColumn * Control.View.CharWidth;
				row.expansion_StartSpan.EndRow.Expansion_PixelStart = xPos + width - Control.View.TextMargin + 2;
				row.Expansion_PixelEnd = xPos - 1;
				RenderText(Control.Document.IndexOf(row.expansion_StartSpan.EndRow), row.expansion_StartSpan.EndRow.Expansion_PixelStart, endWord);
			}
		}
		catch
		{
		}
	}

	private void RenderText(int RowIndex)
	{
		RenderText(RowIndex, 0, null);
	}

	private void RenderText(int RowIndex, int XOffset, Word StartWord)
	{
		GDISurface backBuffer = GFX.BackBuffer;
		backBuffer.Font = GFX.FontNormal;
		backBuffer.FontTransparent = true;
		bool flag = false;
		if (RowIndex > Control.Document.Count - 1)
		{
			return;
		}
		backBuffer.TextForeColor = Color.Black;
		Row row = Control.Document[RowIndex];
		int num = Control.View.TextMargin - Control.View.ClientAreaStart + XOffset;
		int num2 = 0;
		int taborigin = -Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin;
		bool showWhitespace = Control.ShowWhitespace;
		bool flag2 = false;
		if (StartWord == null)
		{
			flag2 = true;
		}
		row.Expansion_StartChar = 0;
		row.Expansion_EndChar = 0;
		bool flag3 = false;
		foreach (Word formattedWord in row.FormattedWords)
		{
			if (flag2)
			{
				if (formattedWord.Span == row.expansion_StartSpan && row.expansion_StartSpan != null && !row.expansion_StartSpan.Expanded)
				{
					RenderCollapsedText(RowIndex, num);
					flag3 = true;
					break;
				}
				if ((formattedWord.Type == WordType.Space || formattedWord.Type == WordType.Tab) && !flag && Control.ShowTabGuides)
				{
					int num3 = num - (Control.View.TextMargin - Control.View.ClientAreaStart + XOffset);
					if ((double)num3 / (double)Control.PixelTabSize == (double)(num3 / Control.PixelTabSize))
					{
						backBuffer.FillRect(Control.TabGuideColor, num, 0, 1, Control.View.RowHeight);
					}
				}
				if (formattedWord.Type == WordType.Word || !showWhitespace)
				{
					if (formattedWord.Style != null)
					{
						SetFont(formattedWord.Style.Bold, formattedWord.Style.Italic, formattedWord.Style.Underline, backBuffer);
						backBuffer.TextBackColor = formattedWord.Style.BackColor;
						backBuffer.TextForeColor = formattedWord.Style.ForeColor;
						backBuffer.FontTransparent = formattedWord.Style.Transparent;
					}
					else
					{
						backBuffer.Font = GFX.FontNormal;
						backBuffer.TextForeColor = Color.Black;
						backBuffer.FontTransparent = true;
					}
					if (formattedWord.Type == WordType.Word)
					{
						flag = true;
					}
					if (row.Breakpoint && flag)
					{
						backBuffer.TextForeColor = Control.BreakPointForeColor;
						backBuffer.TextBackColor = Control.BreakPointBackColor;
						backBuffer.FontTransparent = false;
					}
					if (Control.BracketMatching && (formattedWord == BracketEnd || formattedWord == BracketStart))
					{
						backBuffer.TextForeColor = Control.BracketForeColor;
						if (Control.BracketBackColor != Color.Transparent)
						{
							backBuffer.TextBackColor = Control.BracketBackColor;
							backBuffer.FontTransparent = false;
						}
						num2 = backBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
						if (Control.BracketBorderColor != Color.Transparent)
						{
							backBuffer.DrawRect(Control.BracketBorderColor, num - 1, 0, num2, Control.View.RowHeight - 1);
						}
					}
					else
					{
						num2 = backBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
					}
					if (formattedWord.HasError)
					{
						int num4 = Control.View.RowHeight - 1;
						Color errorColor = formattedWord.ErrorColor;
						for (int i = 0; i < num2 + 3; i += 4)
						{
							backBuffer.DrawLine(errorColor, new Point(num + i, num4), new Point(num + i + 2, num4 - 2));
							backBuffer.DrawLine(errorColor, new Point(num + i + 2, num4 - 2), new Point(num + i + 4, num4));
						}
					}
				}
				else if (formattedWord.Type == WordType.Space)
				{
					backBuffer.Font = GFX.FontNormal;
					backBuffer.TextForeColor = Control.WhitespaceColor;
					backBuffer.FontTransparent = true;
					if (row.Breakpoint && flag)
					{
						backBuffer.TextForeColor = Control.BreakPointForeColor;
						backBuffer.TextBackColor = Control.BreakPointBackColor;
						backBuffer.FontTransparent = false;
					}
					backBuffer.DrawTabbedString("·", num, 0, taborigin, Control.PixelTabSize);
					num2 = backBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
				}
				else if (formattedWord.Type == WordType.Tab)
				{
					backBuffer.Font = GFX.FontNormal;
					backBuffer.TextForeColor = Control.WhitespaceColor;
					backBuffer.FontTransparent = true;
					if (row.Breakpoint && flag)
					{
						backBuffer.TextForeColor = Control.BreakPointForeColor;
						backBuffer.TextBackColor = Control.BreakPointBackColor;
						backBuffer.FontTransparent = false;
					}
					backBuffer.DrawTabbedString("»", num, 0, taborigin, Control.PixelTabSize);
					num2 = backBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
				}
				if (formattedWord.Pattern != null && formattedWord.Pattern.IsSeparator)
				{
					backBuffer.FillRect(Control.SeparatorColor, Control.View.TextMargin - 4, Control.View.RowHeight - 1, Control.View.ClientAreaWidth, 1);
				}
				num += num2;
			}
			if (!flag2)
			{
				row.Expansion_StartChar += formattedWord.Text.Length;
			}
			if (formattedWord == StartWord)
			{
				flag2 = true;
			}
			row.Expansion_EndChar += formattedWord.Text.Length;
		}
		if (!row.IsCollapsed && row.endSpan != null && row.endSpan.spanDefinition != null && row.endSpan.spanDefinition.Style != null)
		{
			backBuffer.FillRect(row.endSpan.spanDefinition.Style.BackColor, num, 0, Control.Width - num, Control.View.RowHeight);
		}
		if (Control._SyntaxBox.ShowEOLMarker && !flag3)
		{
			backBuffer.Font = GFX.FontNormal;
			backBuffer.TextForeColor = Control._SyntaxBox.EOLMarkerColor;
			backBuffer.FontTransparent = true;
			backBuffer.DrawTabbedString("¶", num, 0, taborigin, Control.PixelTabSize);
		}
	}

	private void RenderSelectedText(int RowIndex)
	{
		RenderSelectedText(RowIndex, 0, null);
	}

	private void RenderSelectedText(int RowIndex, int XOffset, Word StartWord)
	{
		GDISurface selectionBuffer = GFX.SelectionBuffer;
		selectionBuffer.Font = GFX.FontNormal;
		selectionBuffer.FontTransparent = true;
		if (RowIndex > Control.Document.Count - 1)
		{
			return;
		}
		selectionBuffer.TextForeColor = (Control.ContainsFocus ? Control.SelectionForeColor : Control.InactiveSelectionForeColor);
		Row row = Control.Document[RowIndex];
		int num = Control.View.TextMargin - Control.View.ClientAreaStart + XOffset;
		int num2 = 0;
		int taborigin = -Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin;
		bool showWhitespace = Control.ShowWhitespace;
		bool flag = false;
		if (StartWord == null)
		{
			flag = true;
		}
		row.Expansion_StartChar = 0;
		row.Expansion_EndChar = 0;
		bool flag2 = false;
		foreach (Word formattedWord in row.FormattedWords)
		{
			if (flag)
			{
				if (formattedWord.Span == row.expansion_StartSpan && row.expansion_StartSpan != null && !row.expansion_StartSpan.Expanded)
				{
					RenderCollapsedSelectedText(RowIndex, num);
					flag2 = true;
					break;
				}
				if (formattedWord.Type == WordType.Word || !showWhitespace)
				{
					if (formattedWord.Style != null)
					{
						SetFont(formattedWord.Style.Bold, formattedWord.Style.Italic, formattedWord.Style.Underline, selectionBuffer);
					}
					else
					{
						selectionBuffer.Font = GFX.FontNormal;
					}
					num2 = selectionBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
					if (formattedWord.HasError)
					{
						int num3 = Control.View.RowHeight - 1;
						Color errorColor = formattedWord.ErrorColor;
						for (int i = 0; i < num2 + 3; i += 4)
						{
							selectionBuffer.DrawLine(errorColor, new Point(num + i, num3), new Point(num + i + 2, num3 - 2));
							selectionBuffer.DrawLine(errorColor, new Point(num + i + 2, num3 - 2), new Point(num + i + 4, num3));
						}
					}
				}
				else if (formattedWord.Type == WordType.Space)
				{
					selectionBuffer.Font = GFX.FontNormal;
					selectionBuffer.DrawTabbedString("·", num, 0, taborigin, Control.PixelTabSize);
					num2 = selectionBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
				}
				else if (formattedWord.Type == WordType.Tab)
				{
					selectionBuffer.Font = GFX.FontNormal;
					selectionBuffer.DrawTabbedString("»", num, 0, taborigin, Control.PixelTabSize);
					num2 = selectionBuffer.DrawTabbedString(formattedWord.Text, num, 0, taborigin, Control.PixelTabSize).Width;
				}
				if (formattedWord.Pattern != null && formattedWord.Pattern.IsSeparator)
				{
					selectionBuffer.FillRect(Control.SeparatorColor, Control.View.TextMargin - 4, Control.View.RowHeight - 1, Control.View.ClientAreaWidth, 1);
				}
				num += num2;
			}
			if (!flag)
			{
				row.Expansion_StartChar += formattedWord.Text.Length;
			}
			if (formattedWord == StartWord)
			{
				flag = true;
			}
			row.Expansion_EndChar += formattedWord.Text.Length;
		}
		if (!row.IsCollapsed && row.endSpan != null && row.endSpan.spanDefinition != null && row.endSpan.spanDefinition.Style != null)
		{
			GFX.BackBuffer.FillRect(row.endSpan.spanDefinition.Style.BackColor, num, 0, Control.Width - num, Control.View.RowHeight);
		}
		if (Control._SyntaxBox.ShowEOLMarker && !flag2)
		{
			selectionBuffer.Font = GFX.FontNormal;
			selectionBuffer.TextForeColor = Control.SelectionForeColor;
			selectionBuffer.FontTransparent = true;
			selectionBuffer.DrawTabbedString("¶", num, 0, taborigin, Control.PixelTabSize);
		}
	}

	private void RenderCaret(int RowIndex, int ypos)
	{
		int num = -1;
		int y = Control.Caret.Position.Y;
		if (y >= 0 && y <= Control.Document.Count - 1)
		{
			Row row = Control.Document[y];
			if (row.expansion_EndSpan != null && !row.expansion_EndSpan.Expanded)
			{
				row = row.expansion_EndSpan.StartRow;
				num = row.Index;
			}
		}
		bool flag = RowIndex == num;
		if (RowIndex != y && RowIndex != num)
		{
			return;
		}
		if (Control.View.Action == EditAction.DragText)
		{
			Row row2 = Control.Document[y];
			int num2 = MeasureRow(row2, Control.Caret.Position.X).Width + 1;
			if (flag)
			{
				num2 += row2.Expansion_PixelStart;
				num2 -= MeasureRow(row2, row2.Expansion_StartChar).Width;
			}
			GFX.BackBuffer.InvertRect(num2 + Control.View.TextMargin - Control.View.ClientAreaStart - 1, 0, 3, Control.View.RowHeight);
			GFX.BackBuffer.InvertRect(num2 + Control.View.TextMargin - Control.View.ClientAreaStart, 1, 1, Control.View.RowHeight - 2);
			return;
		}
		Row row3 = Control.Document[y];
		if (!Control.OverWrite)
		{
			int num3 = Control.View.TextMargin - Control.View.ClientAreaStart;
			num3 += MeasureRow(row3, Control.Caret.Position.X).Width + 1;
			if (flag)
			{
				num3 += row3.Expansion_PixelStart;
				num3 -= MeasureRow(row3, row3.Expansion_StartChar).Width;
			}
			int num4 = Control.View.CharWidth / 12 + 1;
			if (num4 < 2)
			{
				num4 = 2;
			}
			if (Control.Caret.Blink)
			{
				GFX.BackBuffer.InvertRect(num3, 0, num4, Control.View.RowHeight);
			}
			if (Control.IMEWindow == null)
			{
				Control.IMEWindow = new IMEWindow(Control.Handle, Control.FontName, Control.FontSize);
				InitIMEWindow();
			}
			Control.IMEWindow.Loation = new Point(num3, ypos);
		}
		else
		{
			int num5 = MeasureRow(row3, Control.Caret.Position.X).Width;
			int width = MeasureRow(row3, Control.Caret.Position.X + 1).Width;
			int width2 = width - num5;
			if (flag)
			{
				num5 += row3.Expansion_PixelStart;
				num5 -= MeasureRow(row3, row3.Expansion_StartChar).Width;
			}
			int x = num5 + Control.View.TextMargin - Control.View.ClientAreaStart;
			if (Control.Caret.Blink)
			{
				GFX.BackBuffer.InvertRect(x, 0, width2, Control.View.RowHeight);
			}
			Control.IMEWindow.Loation = new Point(x, ypos);
		}
	}

	private void RenderMargin(int RowIndex)
	{
		GDISurface backBuffer = GFX.BackBuffer;
		if (Control.ShowGutterMargin)
		{
			backBuffer.FillRect(GFX.GutterMarginBrush, 0, 0, Control.View.GutterMarginWidth, Control.View.RowHeight);
			backBuffer.FillRect(GFX.GutterMarginBorderBrush, Control.View.GutterMarginWidth - 1, 0, 1, Control.View.RowHeight);
			if (RowIndex <= Control.Document.Count - 1)
			{
				Row row = Control.Document[RowIndex];
				if (Control.View.RowHeight >= Control._SyntaxBox.GutterIcons.ImageSize.Height)
				{
					if (row.Bookmarked)
					{
						Control._SyntaxBox.GutterIcons.Draw(Graphics.FromHdc(backBuffer.hDC), 0, 0, 1);
					}
					if (row.Breakpoint)
					{
						Control._SyntaxBox.GutterIcons.Draw(Graphics.FromHdc(backBuffer.hDC), 0, 0, 0);
					}
				}
				else
				{
					int rowHeight = Control.View.RowHeight;
					if (row.Bookmarked)
					{
						Control._SyntaxBox.GutterIcons.Draw(Graphics.FromHdc(backBuffer.hDC), 0, 0, rowHeight, rowHeight, 1);
					}
					if (row.Breakpoint)
					{
						Control._SyntaxBox.GutterIcons.Draw(Graphics.FromHdc(backBuffer.hDC), 0, 0, rowHeight, rowHeight, 0);
					}
				}
				if (row.Images != null)
				{
					foreach (int image in row.Images)
					{
						if (Control.View.RowHeight >= Control._SyntaxBox.GutterIcons.ImageSize.Height)
						{
							Control._SyntaxBox.GutterIcons.Draw(Graphics.FromHdc(backBuffer.hDC), 0, 0, image);
							continue;
						}
						int rowHeight2 = Control.View.RowHeight;
						Control._SyntaxBox.GutterIcons.Draw(Graphics.FromHdc(backBuffer.hDC), 0, 0, rowHeight2, rowHeight2, image);
					}
				}
			}
		}
		if (Control.ShowLineNumbers)
		{
			backBuffer.FillRect(GFX.LineNumberMarginBrush, Control.View.GutterMarginWidth, 0, Control.View.LineNumberMarginWidth + 1, Control.View.RowHeight);
			for (int i = 0; i < Control.View.RowHeight; i += 2)
			{
				backBuffer.FillRect(GFX.LineNumberMarginBorderBrush, Control.View.GutterMarginWidth + Control.View.LineNumberMarginWidth, i, 1, 1);
			}
		}
		if (!Control.ShowLineNumbers || !Control.ShowGutterMargin)
		{
			backBuffer.FillRect(GFX.BackgroundBrush, Control.View.TotalMarginWidth, 0, Control.View.TextMargin - Control.View.TotalMarginWidth - 3, Control.View.RowHeight);
		}
		else
		{
			backBuffer.FillRect(GFX.BackgroundBrush, Control.View.TotalMarginWidth + 1, 0, Control.View.TextMargin - Control.View.TotalMarginWidth - 4, Control.View.RowHeight);
		}
		if (Control.ShowLineNumbers)
		{
			backBuffer.Font = GFX.FontNormal;
			backBuffer.FontTransparent = true;
			backBuffer.TextForeColor = Control.LineNumberForeColor;
			if (RowIndex <= Control.Document.Count - 1)
			{
				int width = MeasureString((RowIndex + 1).ToString(CultureInfo.InvariantCulture)).Width;
				backBuffer.DrawTabbedString((RowIndex + 1).ToString(CultureInfo.InvariantCulture), Control.View.GutterMarginWidth + Control.View.LineNumberMarginWidth - width - 1, 1, 0, Control.PixelTabSize);
			}
		}
	}

	private void RenderExpansion(int RowIndex)
	{
		if (Control == null)
		{
			throw new NullReferenceException("Control may not be null");
		}
		if (RowIndex > Control.Document.Count - 1)
		{
			return;
		}
		Row row = Control.Document[RowIndex];
		GDISurface backBuffer = GFX.BackBuffer;
		if (row.endSpan != null)
		{
			if (row.expansion_StartSpan != null && row.startSpan.Parent == null)
			{
				if (!row.IsCollapsed)
				{
					backBuffer.FillRect(GFX.OutlineBrush, Control.View.TotalMarginWidth + 6, 4, 1, Control.View.RowHeight - 4);
				}
			}
			else if (row.endSpan.Parent != null || row.expansion_EndSpan != null)
			{
				backBuffer.FillRect(GFX.OutlineBrush, Control.View.TotalMarginWidth + 6, 0, 1, Control.View.RowHeight);
			}
			if (row.expansion_StartSpan != null)
			{
				backBuffer.FillRect(GFX.OutlineBrush, Control.View.TotalMarginWidth + 2, 4, 9, 9);
				backBuffer.FillRect(GFX.BackgroundBrush, Control.View.TotalMarginWidth + 3, 5, 7, 7);
				backBuffer.FillRect(GFX.OutlineBrush, Control.View.TotalMarginWidth + 4, 8, 5, 1);
				if (!row.expansion_StartSpan.Expanded)
				{
					backBuffer.FillRect(GFX.OutlineBrush, Control.View.TotalMarginWidth + 6, 6, 1, 5);
				}
			}
			if (row.expansion_EndSpan != null)
			{
				backBuffer.FillRect(GFX.OutlineBrush, Control.View.TotalMarginWidth + 7, Control.View.RowHeight - 1, 5, 1);
			}
		}
		if (SpanFound && Control._SyntaxBox.ScopeIndicatorColor != Color.Transparent && Control._SyntaxBox.ShowScopeIndicator)
		{
			if (RowIndex >= FirstSpanRow && RowIndex <= LastSpanRow)
			{
				backBuffer.FillRect(Control._SyntaxBox.ScopeIndicatorColor, Control.View.TotalMarginWidth + 14, 0, 2, Control.View.RowHeight);
			}
			if (RowIndex == FirstSpanRow)
			{
				backBuffer.FillRect(Control._SyntaxBox.ScopeIndicatorColor, Control.View.TotalMarginWidth + 14, 0, 4, 2);
			}
			if (RowIndex == LastSpanRow)
			{
				backBuffer.FillRect(Control._SyntaxBox.ScopeIndicatorColor, Control.View.TotalMarginWidth + 14, Control.View.RowHeight - 2, 4, 2);
			}
		}
	}

	private void RenderSelection(int RowIndex)
	{
		if (RowIndex > Control.Document.Count - 1 || !Control.Selection.IsValid)
		{
			return;
		}
		Row row = Control.Document[RowIndex];
		if (!row.IsCollapsed)
		{
			if (RowIndex > Control.Selection.LogicalBounds.FirstRow && RowIndex < Control.Selection.LogicalBounds.LastRow)
			{
				int num = MeasureRow(row, row.Text.Length).Width + MeasureString("¶").Width + 3;
				RenderBox(Control.View.TextMargin, 0, Math.Max(num - Control.View.ClientAreaStart, 0), Control.View.RowHeight);
			}
			else if (RowIndex == Control.Selection.LogicalBounds.FirstRow && RowIndex == Control.Selection.LogicalBounds.LastRow)
			{
				int width = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.FirstColumn)).Width;
				int width2 = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.LastColumn)).Width - width;
				RenderBox(Control.View.TextMargin + width - Control.View.ClientAreaStart, 0, width2, Control.View.RowHeight);
			}
			else if (RowIndex == Control.Selection.LogicalBounds.LastRow)
			{
				int width3 = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.LastColumn)).Width;
				RenderBox(Control.View.TextMargin, 0, Math.Max(width3 - Control.View.ClientAreaStart, 0), Control.View.RowHeight);
			}
			else if (RowIndex == Control.Selection.LogicalBounds.FirstRow)
			{
				int width4 = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.FirstColumn)).Width;
				int width5 = MeasureRow(row, row.Text.Length).Width + MeasureString("¶").Width + 3 - width4;
				RenderBox(Control.View.TextMargin + width4 - Control.View.ClientAreaStart, 0, width5, Control.View.RowHeight);
			}
		}
		else
		{
			RenderCollapsedSelection(RowIndex);
		}
	}

	private void RenderCollapsedSelection(int RowIndex)
	{
		Row row = Control.Document[RowIndex];
		if (RowIndex > Control.Selection.LogicalBounds.FirstRow && RowIndex < Control.Selection.LogicalBounds.LastRow)
		{
			int width = MeasureRow(row, row.Expansion_EndChar).Width;
			RenderBox(Control.View.TextMargin, 0, Math.Max(width - Control.View.ClientAreaStart, 0), Control.View.RowHeight);
		}
		else if (RowIndex == Control.Selection.LogicalBounds.FirstRow && RowIndex == Control.Selection.LogicalBounds.LastRow)
		{
			int width2 = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.FirstColumn)).Width;
			int val = Math.Min(row.Text.Length, Control.Selection.LogicalBounds.LastColumn);
			val = Math.Min(val, row.Expansion_EndChar);
			int width3 = MeasureRow(row, val).Width - width2;
			RenderBox(Control.View.TextMargin + width2 - Control.View.ClientAreaStart, 0, width3, Control.View.RowHeight);
		}
		else if (RowIndex == Control.Selection.LogicalBounds.LastRow)
		{
			int width4 = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.LastColumn)).Width;
			RenderBox(Control.View.TextMargin, 0, Math.Max(width4 - Control.View.ClientAreaStart, 0), Control.View.RowHeight);
		}
		else if (RowIndex == Control.Selection.LogicalBounds.FirstRow)
		{
			int width5 = MeasureRow(row, Math.Min(row.Text.Length, Control.Selection.LogicalBounds.FirstColumn)).Width;
			int width6 = MeasureRow(row, Math.Min(row.Text.Length, row.Expansion_EndChar)).Width - width5;
			RenderBox(Control.View.TextMargin + width5 - Control.View.ClientAreaStart, 0, width6, Control.View.RowHeight);
		}
		if (Control.Selection.LogicalBounds.LastRow > RowIndex && Control.Selection.LogicalBounds.FirstRow <= RowIndex)
		{
			int expansion_PixelEnd = row.Expansion_PixelEnd;
			int width7 = row.Expansion_EndRow.Expansion_PixelStart - expansion_PixelEnd + Control.View.TextMargin;
			RenderBox(expansion_PixelEnd - Control.View.ClientAreaStart, 0, width7, Control.View.RowHeight);
		}
		RowIndex = row.Expansion_EndRow.Index;
		row = row.Expansion_EndRow;
		if (Control.Selection.LogicalBounds.FirstRow <= RowIndex && Control.Selection.LogicalBounds.LastRow >= RowIndex)
		{
			int count = ((Control.Selection.LogicalBounds.LastRow != RowIndex) ? row.Text.Length : Math.Min(Control.Selection.LogicalBounds.LastColumn, row.Text.Length));
			int width8 = MeasureRow(row, count).Width;
			width8 += row.Expansion_PixelStart;
			width8 -= MeasureRow(row, row.Expansion_StartChar).Width;
			int width9;
			if (Control.Selection.LogicalBounds.FirstRow == RowIndex)
			{
				int count2 = Math.Max(Control.Selection.LogicalBounds.FirstColumn, row.Expansion_StartChar);
				width9 = MeasureRow(row, count2).Width;
				width9 += row.Expansion_PixelStart;
				width9 -= MeasureRow(row, row.Expansion_StartChar).Width;
			}
			else
			{
				width9 = MeasureRow(row, row.Expansion_StartChar).Width;
				width9 += row.Expansion_PixelStart;
				width9 -= MeasureRow(row, row.Expansion_StartChar).Width;
			}
			width8 -= width9;
			if (Control.Selection.LogicalBounds.LastRow != RowIndex)
			{
				width8 += 6;
			}
			RenderBox(Control.View.TextMargin + width9 - Control.View.ClientAreaStart, 0, width8, Control.View.RowHeight);
		}
	}

	private void RenderBox(int x, int y, int width, int height)
	{
		GFX.SelectionBuffer.RenderTo(GFX.BackBuffer, x, y, width, height, x, y);
	}

	private TextPoint ColumnFromPixel(int RowIndex, int X)
	{
		Row row = Control.Document[RowIndex];
		X -= Control.View.TextMargin - 2 - Control.View.FirstVisibleColumn * Control.View.CharWidth;
		if (row.Count == 0)
		{
			if (Control.VirtualWhitespace && Control.View.CharWidth > 0)
			{
				return new TextPoint(X / Control.View.CharWidth, RowIndex);
			}
			return new TextPoint(0, RowIndex);
		}
		int taborigin = -Control.View.FirstVisibleColumn * Control.View.CharWidth + Control.View.TextMargin;
		int num = Control.View.TextMargin - Control.View.ClientAreaStart;
		int num2 = 0;
		int num3 = 0;
		Word word = null;
		int num4 = 0;
		foreach (Word formattedWord in row.FormattedWords)
		{
			word = formattedWord;
			num4 = num3;
			if (formattedWord.Type == WordType.Word && formattedWord.Style != null)
			{
				SetStringFont(formattedWord.Style.Bold, formattedWord.Style.Italic, formattedWord.Style.Underline);
			}
			else
			{
				SetStringFont(bold: false, italic: false, underline: false);
			}
			int width = GFX.StringBuffer.DrawTabbedString(formattedWord.Text, num + num3, 0, taborigin, Control.PixelTabSize).Width;
			if (num3 + width < X)
			{
				if (formattedWord != row.FormattedWords[row.FormattedWords.Count - 1])
				{
					num3 += width;
					num2 += formattedWord.Text.Length;
				}
				continue;
			}
			break;
		}
		if (word != null)
		{
			if (word.Type == WordType.Word && word.Style != null)
			{
				SetStringFont(word.Style.Bold, word.Style.Italic, word.Style.Underline);
			}
			else
			{
				SetStringFont(bold: false, italic: false, underline: false);
			}
		}
		bool flag = false;
		if (word != null)
		{
			string text = word.Text;
			foreach (char c in text)
			{
				int width2 = GFX.StringBuffer.DrawTabbedString(string.Concat(c), num + num4, 0, taborigin, Control.PixelTabSize).Width;
				if (num4 + width2 >= X)
				{
					flag = true;
					break;
				}
				num2++;
				num4 += width2;
			}
		}
		if (!flag && Control.View.CharWidth > 0 && Control.VirtualWhitespace)
		{
			int num5 = X - num4;
			int num6 = num5 / Control.View.CharWidth;
			num2 += num6;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		return new TextPoint(num2, RowIndex);
	}

	private Point GetTextPointPixelPos(TextPoint tp)
	{
		Row row = Control.Document[tp.Y];
		if (row.RowState == RowState.SpanParsed)
		{
			Control.Document.Parser.ParseRow(row.Index, ParseKeywords: true);
		}
		Row row2 = (row.IsCollapsedEndPart ? row.Expansion_StartRow : row);
		int visibleIndex = row2.VisibleIndex;
		int num = visibleIndex - Control.View.FirstVisibleRow;
		if (num < 0 || num > Control.View.VisibleRowCount)
		{
			return new Point(-1, -1);
		}
		num *= Control.View.RowHeight;
		bool isCollapsedEndPart = row.IsCollapsedEndPart;
		int num2 = MeasureRow(row, tp.X).Width + 1;
		if (isCollapsedEndPart)
		{
			num2 += row.Expansion_PixelStart;
			num2 -= MeasureRow(row, row.Expansion_StartChar).Width;
		}
		int num3 = num2 + Control.View.TextMargin - Control.View.ClientAreaStart;
		if (num3 < Control.View.TextMargin || num3 > Control.View.ClientAreaWidth + Control.View.TextMargin)
		{
			return new Point(-1, -1);
		}
		return new Point(num3, num);
	}
}
