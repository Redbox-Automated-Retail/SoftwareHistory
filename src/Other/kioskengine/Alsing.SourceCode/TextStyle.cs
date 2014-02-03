using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Alsing.SourceCode;

[Editor(typeof(TextStyleUIEditor), typeof(UITypeEditor))]
public class TextStyle : ICloneable
{
	public string Name;

	private bool _Bold;

	private bool _Italic;

	private bool _Underline;

	private Color _ForeColor = Color.Black;

	private Color _BackColor = Color.Transparent;

	[Category("Font")]
	[Description("Gets or Sets if the style uses a BOLD font")]
	public bool Bold
	{
		get
		{
			return _Bold;
		}
		set
		{
			_Bold = value;
			OnChange();
		}
	}

	[Description("Gets or Sets if the style uses an ITALIC font")]
	[Category("Font")]
	public bool Italic
	{
		get
		{
			return _Italic;
		}
		set
		{
			_Italic = value;
			OnChange();
		}
	}

	[Category("Font")]
	[Description("Gets or Sets if the style uses an UNDERLINED font")]
	public bool Underline
	{
		get
		{
			return _Underline;
		}
		set
		{
			_Underline = value;
			OnChange();
		}
	}

	[Category("Color")]
	[Description("Gets or Sets the fore color of the style")]
	public Color ForeColor
	{
		get
		{
			return _ForeColor;
		}
		set
		{
			_ForeColor = value;
			OnChange();
		}
	}

	[Category("Color")]
	[Description("Gets or Sets the background color of the style")]
	public Color BackColor
	{
		get
		{
			return _BackColor;
		}
		set
		{
			_BackColor = value;
			OnChange();
		}
	}

	[Browsable(false)]
	public bool Transparent => BackColor.A == 0;

	public event EventHandler Change;

	public TextStyle()
	{
		ForeColor = Color.Black;
		BackColor = Color.Transparent;
	}

	protected virtual void OnChange()
	{
		if (Change != null)
		{
			Change(this, EventArgs.Empty);
		}
	}

	public override string ToString()
	{
		if (Name == null)
		{
			return "TextStyle";
		}
		return Name;
	}

	public object Clone()
	{
		TextStyle textStyle = new TextStyle();
		textStyle.BackColor = BackColor;
		textStyle.Bold = Bold;
		textStyle.ForeColor = ForeColor;
		textStyle.Italic = Italic;
		textStyle.Underline = Underline;
		textStyle.Name = Name;
		return textStyle;
	}
}
