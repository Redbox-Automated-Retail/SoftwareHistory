using System.Drawing;

namespace Alsing.Windows.Forms.FormatLabel;

public class Word
{
	public Element Element;

	public int Height;

	public Image Image;

	public Rectangle ScreenArea = new Rectangle(0, 0, 0, 0);

	public string Text = "";

	public int Width;
}
