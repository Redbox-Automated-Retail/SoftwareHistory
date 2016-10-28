using System.Drawing;
using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.FormatLabel;

[ComVisible(true)]
public class Word
{
	public Element Element;

	public int Height;

	public Image Image;

	public Rectangle ScreenArea = new Rectangle(0, 0, 0, 0);

	public string Text = "";

	public int Width;
}
