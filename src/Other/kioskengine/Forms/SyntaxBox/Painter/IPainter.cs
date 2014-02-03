using System;
using System.Drawing;
using Alsing.SourceCode;

namespace Alsing.Windows.Forms.SyntaxBox.Painter;

public interface IPainter : IDisposable
{
	Size MeasureRow(Row xtr, int Count);

	void RenderAll();

	void RenderAll(Graphics g);

	void RenderCaret(Graphics g);

	void RenderRow(int RowIndex);

	TextPoint CharFromPixel(int X, int Y);

	void Resize();

	void InitGraphics();

	Size MeasureString(string str);

	int GetMaxCharWidth();
}
