using System.Drawing;
using System.Runtime.InteropServices;

namespace Alsing.Windows.Forms.FormatLabel;

[ComVisible(true)]
public class Element
{
	protected string _Tag = "";

	protected string _TagName = "";

	public Color BackColor = Color.Black;

	public TextEffect Effect;

	public Color EffectColor = Color.Black;

	public Font Font;

	public Color ForeColor = Color.Black;

	public Element Link;

	public bool NewLine;

	public string Text = "";

	public Word[] words;

	public string TagName => _TagName;

	public string Tag
	{
		get
		{
			return _Tag;
		}
		set
		{
			_Tag = value.ToLowerInvariant();
			_Tag = _Tag.Replace("\t", " ");
			if (_Tag.IndexOf(" ") >= 0)
			{
				_TagName = _Tag.Substring(0, _Tag.IndexOf(" "));
			}
			else
			{
				_TagName = _Tag;
			}
		}
	}
}
