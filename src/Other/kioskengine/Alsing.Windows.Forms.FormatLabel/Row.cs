using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.FormatLabel;

[ComVisible(true)]
public class Row
{
	public int BottomPadd;

	public int Height;

	public bool RenderSeparator;

	public int Top;

	public bool Visible;

	public int Width;

	public List<Word> Words = new List<Word>();
}
