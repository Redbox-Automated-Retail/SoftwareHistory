using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows.Forms;
using Alsing.Drawing.GDI;
using Alsing.Windows.Forms.FormatLabel;

namespace Alsing.Windows.Forms.CoreLib;

[ComVisible(true)]
public class FormatLabelControl : BaseControl
{
	private readonly Dictionary<string, GDIFont> _Fonts = new Dictionary<string, GDIFont>();

	private Element _ActiveElement;

	private Element[] _Elements;

	private bool _HasImageError;

	private ImageList _ImageList;

	private Color _Link_Color = Color.Blue;

	private Color _Link_Color_Hover = Color.Blue;

	private bool _Link_UnderLine;

	private bool _Link_UnderLine_Hover = true;

	private List<Row> _Rows;

	private ScrollBars _ScrollBars;

	private string _Text = "format <b>label</b>";

	private bool _WordWrap = true;

	private PictureBox Filler;

	private HScrollBar hScroll;

	private VScrollBar vScroll;

	private Container components;

	private int _Margin;

	public ImageList ImageList
	{
		get
		{
			return _ImageList;
		}
		set
		{
			_ImageList = value;
			Invalidate();
		}
	}

	public Color Link_Color
	{
		get
		{
			return _Link_Color;
		}
		set
		{
			_Link_Color = value;
			Invalidate();
		}
	}

	public Color Link_Color_Hover
	{
		get
		{
			return _Link_Color_Hover;
		}
		set
		{
			_Link_Color_Hover = value;
			Invalidate();
		}
	}

	public bool Link_UnderLine
	{
		get
		{
			return _Link_UnderLine;
		}
		set
		{
			_Link_UnderLine = value;
			Invalidate();
		}
	}

	public bool Link_UnderLine_Hover
	{
		get
		{
			return _Link_UnderLine_Hover;
		}
		set
		{
			_Link_UnderLine_Hover = value;
			Invalidate();
		}
	}

	public bool AutoSizeHorizontal { get; set; }

	public bool AutoSizeVertical { get; set; }

	public bool WordWrap
	{
		get
		{
			return _WordWrap;
		}
		set
		{
			_WordWrap = value;
			CreateRows();
			Invalidate();
		}
	}

	[EditorBrowsable(EditorBrowsableState.Always)]
	[Obsolete("", false)]
	[Browsable(true)]
	public override Image BackgroundImage
	{
		get
		{
			return base.BackgroundImage;
		}
		set
		{
			base.BackgroundImage = value;
		}
	}

	public ScrollBars ScrollBars
	{
		get
		{
			return _ScrollBars;
		}
		set
		{
			_ScrollBars = value;
			InitScrollbars();
		}
	}

	[Browsable(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public override string Text
	{
		get
		{
			return _Text;
		}
		set
		{
			try
			{
				_Text = value;
				CreateAll();
				Invalidate();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Debugger.Break();
			}
		}
	}

	public new int Margin
	{
		get
		{
			return _Margin;
		}
		set
		{
			_Margin = value;
			CreateRows();
			Invalidate();
		}
	}

	public event ClickLinkEventHandler ClickLink;

	public FormatLabelControl()
	{
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.Opaque, value: true);
		InitializeComponent();
		Text = Text;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			foreach (GDIFont value in _Fonts.Values)
			{
				value.Dispose();
			}
			if (components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.Filler = new System.Windows.Forms.PictureBox();
		this.vScroll = new System.Windows.Forms.VScrollBar();
		this.hScroll = new System.Windows.Forms.HScrollBar();
		base.SuspendLayout();
		this.Filler.BackColor = System.Drawing.SystemColors.Control;
		this.Filler.Cursor = System.Windows.Forms.Cursors.Default;
		this.Filler.Location = new System.Drawing.Point(136, 112);
		this.Filler.Name = "Filler";
		this.Filler.Size = new System.Drawing.Size(16, 16);
		this.Filler.TabIndex = 5;
		this.Filler.TabStop = false;
		this.vScroll.Cursor = System.Windows.Forms.Cursors.Default;
		this.vScroll.LargeChange = 2;
		this.vScroll.Location = new System.Drawing.Point(136, -8);
		this.vScroll.Name = "vScroll";
		this.vScroll.Size = new System.Drawing.Size(16, 112);
		this.vScroll.TabIndex = 4;
		this.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(vScroll_Scroll);
		this.hScroll.Cursor = System.Windows.Forms.Cursors.Default;
		this.hScroll.LargeChange = 1;
		this.hScroll.Location = new System.Drawing.Point(0, 112);
		this.hScroll.Maximum = 600;
		this.hScroll.Name = "hScroll";
		this.hScroll.Size = new System.Drawing.Size(128, 16);
		this.hScroll.TabIndex = 3;
		this.BackColor = System.Drawing.SystemColors.Window;
		base.Controls.AddRange(new System.Windows.Forms.Control[3] { this.Filler, this.vScroll, this.hScroll });
		base.Name = "FormatLabelControl";
		base.Size = new System.Drawing.Size(160, 136);
		base.ResumeLayout(false);
	}

	private void CreateAll()
	{
		_Elements = CreateElements();
		ClearFonts();
		ApplyFormat(_Elements);
		CreateWords(_Elements);
		CreateRows();
		SetAutoSize();
	}

	private void ClearFonts()
	{
		foreach (GDIFont value in _Fonts.Values)
		{
			value.Dispose();
		}
		_Fonts.Clear();
	}

	protected void OnClickLink(string Link)
	{
		if (ClickLink != null)
		{
			ClickLink(this, new ClickLinkEventArgs(Link));
		}
	}

	private void SetAutoSize()
	{
		if (AutoSizeHorizontal)
		{
			base.Width = GetWidth();
		}
		if (AutoSizeVertical)
		{
			base.Height = GetHeight();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		SetAutoSize();
		if (_HasImageError)
		{
			CreateAll();
		}
		GDISurface gDISurface = new GDISurface(base.Width, base.Height, this, BindControl: true);
		Graphics graphics = Graphics.FromHdc(gDISurface.hDC);
		try
		{
			gDISurface.FontTransparent = true;
			if (BackgroundImage != null)
			{
				graphics.DrawImage(BackgroundImage, 0, 0, base.Width, base.Height);
			}
			else
			{
				gDISurface.Clear(BackColor);
			}
			int margin = Margin;
			int num = Margin;
			for (int i = vScroll.Value; i < _Rows.Count; i++)
			{
				Row row = _Rows[i];
				margin = Margin;
				row.Visible = true;
				row.Top = num;
				if (row.RenderSeparator)
				{
					Color color = Color.FromArgb(120, 0, 0, 0);
					Brush brush = new SolidBrush(color);
					graphics.FillRectangle(brush, 0, num, base.Width, 1);
					Color color2 = Color.FromArgb(120, 255, 255, 255);
					Brush brush2 = new SolidBrush(color2);
					graphics.FillRectangle(brush2, 0, num + 1, base.Width, 1);
					brush.Dispose();
					brush2.Dispose();
				}
				foreach (Word word in row.Words)
				{
					int num2 = row.Height - word.Height + num;
					if (word.Image != null)
					{
						graphics.DrawImage(word.Image, margin, num);
					}
					else
					{
						GDIFont font2;
						if (word.Element.Link != null)
						{
							Font font = null;
							FontStyle fontStyle = word.Element.Font.Style;
							if (word.Element.Link == _ActiveElement)
							{
								if (_Link_UnderLine_Hover)
								{
									fontStyle |= FontStyle.Underline;
								}
								font = new Font(word.Element.Font, fontStyle);
							}
							else
							{
								if (_Link_UnderLine)
								{
									fontStyle |= FontStyle.Underline;
								}
								font = new Font(word.Element.Font, fontStyle);
							}
							font2 = GetFont(font);
						}
						else
						{
							font2 = GetFont(word.Element.Font);
						}
						gDISurface.Font = font2;
						if (word.Element.Effect != TextEffect.None)
						{
							gDISurface.TextForeColor = word.Element.EffectColor;
							if (word.Element.Effect == TextEffect.Outline)
							{
								for (int j = -1; j <= 1; j++)
								{
									for (int k = -1; k <= 1; k++)
									{
										gDISurface.DrawTabbedString(word.Text, margin + j, num2 + k, 0, 0);
									}
								}
							}
							else if (word.Element.Effect != TextEffect.None)
							{
								gDISurface.DrawTabbedString(word.Text, margin + 1, num2 + 1, 0, 0);
							}
						}
						if (word.Element.Link != null)
						{
							if (word.Element.Link == _ActiveElement)
							{
								gDISurface.TextForeColor = Link_Color_Hover;
							}
							else
							{
								gDISurface.TextForeColor = Link_Color;
							}
						}
						else
						{
							gDISurface.TextForeColor = word.Element.ForeColor;
						}
						gDISurface.TextBackColor = word.Element.BackColor;
						gDISurface.DrawTabbedString(word.Text, margin, num2, 0, 0);
					}
					word.ScreenArea.X = margin;
					word.ScreenArea.Y = num2;
					margin += word.Width;
				}
				num += row.Height + row.BottomPadd;
				if (num > base.Height)
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
		gDISurface.RenderToControl(0, 0);
		gDISurface.Dispose();
		graphics.Dispose();
	}

	private Element[] CreateElements()
	{
		string text = Text.Replace("\n", "");
		text = text.Replace("\r", "");
		string[] array = text.Split('<');
		List<Element> list = new List<Element>();
		int num = 0;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			Element element = new Element();
			if (num == 0)
			{
				element.Text = text2;
			}
			else
			{
				string[] array3 = text2.Split('>');
				element.Tag = array3[0].ToLowerInvariant();
				if (element.Tag.IndexOfAny(" \t".ToCharArray()) >= 0)
				{
					int num2 = element.Tag.IndexOfAny(" \t".ToCharArray());
					string text3 = array3[0].Substring(0, num2).ToLowerInvariant();
					string text4 = array3[0].Substring(num2 + 1);
					element.Tag = text3 + " " + text4;
				}
				element.Text = array3[1];
				if (element.TagName == "img")
				{
					Element element2 = new Element();
					element2.Tag = element.Tag;
					Element item = element2;
					list.Add(item);
					element.Tag = "";
				}
				element.Text = element.Text.Replace("\t", "     ");
				element.Text = element.Text.Replace("&#145;", "'");
				element.Text = element.Text.Replace("&#146;", "'");
				element.Text = element.Text.Replace(" ", '\u0001'.ToString());
				element.Text = HttpUtility.HtmlDecode(element.Text);
				element.Text = element.Text.Replace('\u0001'.ToString(), " ");
			}
			list.Add(element);
			num++;
		}
		Element[] array4 = new Element[list.Count];
		list.CopyTo(array4);
		return array4;
	}

	private string GetAttrib(string attrib, string tag)
	{
		try
		{
			if (tag.IndexOf(attrib) < 0)
			{
				return "";
			}
			tag = tag.Replace("\t", " ");
			int num = tag.IndexOf(attrib);
			int startIndex = num + attrib.Length;
			int num2 = tag.IndexOf("=", startIndex);
			if (num2 < 0)
			{
				return "";
			}
			num2++;
			string text = tag.Substring(num2);
			while (text.StartsWith(" "))
			{
				text = text.Substring(1);
			}
			if (text.StartsWith("\""))
			{
				text = text.Substring(1);
				int length = text.IndexOf("\"");
				return text.Substring(0, length);
			}
			int num3 = text.IndexOf(" ");
			if (num3 < 0)
			{
				num3 = text.Length;
			}
			return text.Substring(0, num3);
		}
		catch
		{
			return "";
		}
	}

	private void ApplyFormat(Element[] Elements)
	{
		Stack stack = new Stack();
		Stack stack2 = new Stack();
		Stack stack3 = new Stack();
		Stack stack4 = new Stack();
		Stack stack5 = new Stack();
		Stack stack6 = new Stack();
		Stack stack7 = new Stack();
		Stack stack8 = new Stack();
		Stack stack9 = new Stack();
		Stack stack10 = new Stack();
		stack.Push(Font.Bold);
		stack2.Push(Font.Italic);
		stack3.Push(Font.Underline);
		stack4.Push(ForeColor);
		stack5.Push(Color.Transparent);
		stack6.Push((int)((double)Font.Size * 1.3));
		stack7.Push(Font.Name);
		stack10.Push(TextEffect.None);
		stack9.Push(Color.Black);
		stack8.Push(null);
		foreach (Element element in Elements)
		{
			switch (element.TagName)
			{
			case "b":
				stack.Push(true);
				break;
			case "a":
				stack8.Push(element);
				break;
			case "i":
			case "em":
				stack2.Push(true);
				break;
			case "u":
				stack3.Push(true);
				break;
			case "font":
			{
				string attrib = GetAttrib("face", element.Tag);
				string attrib2 = GetAttrib("size", element.Tag);
				string attrib3 = GetAttrib("color", element.Tag);
				string attrib4 = GetAttrib("effectcolor", element.Tag);
				string attrib5 = GetAttrib("effect", element.Tag);
				if (attrib2 == "")
				{
					stack6.Push(stack6.Peek());
				}
				else
				{
					stack6.Push(int.Parse(attrib2));
				}
				if (attrib == "")
				{
					stack7.Push(stack7.Peek());
				}
				else
				{
					stack7.Push(attrib);
				}
				if (attrib3 == "")
				{
					stack4.Push(stack4.Peek());
				}
				else
				{
					stack4.Push(Color.FromName(attrib3));
				}
				if (attrib4 == "")
				{
					stack9.Push(stack9.Peek());
				}
				else
				{
					stack9.Push(Color.FromName(attrib4));
				}
				if (attrib5 == "")
				{
					stack10.Push(stack10.Peek());
				}
				else
				{
					stack10.Push(Enum.Parse(typeof(TextEffect), attrib5, ignoreCase: true));
				}
				break;
			}
			case "br":
				element.NewLine = true;
				break;
			case "hr":
				element.NewLine = true;
				break;
			case "h3":
				stack6.Push((int)((double)Font.Size * 1.4));
				stack.Push(true);
				element.NewLine = true;
				break;
			case "h4":
				stack6.Push((int)((double)Font.Size * 1.2));
				stack.Push(true);
				element.NewLine = true;
				break;
			case "/b":
				stack.Pop();
				break;
			case "/a":
				stack8.Pop();
				break;
			case "/i":
			case "/em":
				stack2.Pop();
				break;
			case "/u":
				stack3.Pop();
				break;
			case "/font":
				stack7.Pop();
				stack6.Pop();
				stack4.Pop();
				stack10.Pop();
				stack9.Pop();
				break;
			case "/h3":
				stack6.Pop();
				stack.Pop();
				element.NewLine = true;
				break;
			case "/h4":
				stack6.Pop();
				stack.Pop();
				element.NewLine = true;
				break;
			}
			bool flag = (bool)stack.Peek();
			bool flag2 = (bool)stack2.Peek();
			bool flag3 = (bool)stack3.Peek();
			Element link = (Element)stack8.Peek();
			string familyName = (string)stack7.Peek();
			int num = (int)stack6.Peek();
			Color backColor = (Color)stack5.Peek();
			Color foreColor = (Color)stack4.Peek();
			TextEffect effect = (TextEffect)stack10.Peek();
			Color effectColor = (Color)stack9.Peek();
			FontStyle fontStyle = FontStyle.Regular;
			if (flag)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (flag2)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (flag3)
			{
				fontStyle |= FontStyle.Underline;
			}
			Font font = new Font(familyName, num, fontStyle);
			element.Font = font;
			element.BackColor = backColor;
			element.ForeColor = foreColor;
			element.Link = link;
			element.Effect = effect;
			element.EffectColor = effectColor;
		}
	}

	private bool IsIndex(string src)
	{
		int result;
		return int.TryParse(src, out result);
	}

	private void CreateWords(Element[] Elements)
	{
		GDISurface gDISurface = new GDISurface(1, 1, this, BindControl: false);
		_HasImageError = false;
		foreach (Element element in Elements)
		{
			if (element.TagName == "img")
			{
				element.words = new Word[1];
				element.words[0] = new Word();
				Image image = null;
				try
				{
					string text = GetAttrib("img", element.Tag).ToLowerInvariant();
					if (IsIndex(text))
					{
						int index = int.Parse(text);
						image = ImageList.Images[index];
					}
					else if (!text.StartsWith("http://"))
					{
						image = ((!text.StartsWith("file://")) ? Image.FromFile(text) : Image.FromFile(text.Substring(7)));
					}
				}
				catch
				{
					image = new Bitmap(20, 20);
					_HasImageError = true;
				}
				element.words[0].Image = image;
				element.words[0].Element = element;
				if (image != null)
				{
					element.words[0].Height = image.Height;
					element.words[0].Width = image.Width;
					element.words[0].ScreenArea.Width = image.Width;
					element.words[0].ScreenArea.Height = image.Height;
				}
				continue;
			}
			string[] array = element.Text.Split(' ');
			element.words = new Word[array.Length];
			int num = 0;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				element.words[num] = new Word();
				element.words[num].Element = element;
				string text3;
				if (num == array.Length - 1)
				{
					element.words[num].Text = text2;
					text3 = text2;
				}
				else
				{
					element.words[num].Text = text2 + " ";
					text3 = text2 + " ";
				}
				gDISurface.Font = GetFont(element.Font);
				Size size = gDISurface.MeasureTabbedString(text3, 0);
				element.words[num].Height = size.Height;
				element.words[num].Width = size.Width;
				element.words[num].ScreenArea.Width = element.words[num].Width;
				element.words[num].ScreenArea.Height = element.words[num].Height;
				num++;
			}
		}
		gDISurface.Dispose();
	}

	private GDIFont GetFont(Font font)
	{
		GDIFont value = null;
		if (!_Fonts.TryGetValue(GetFontKey(font), out value))
		{
			value = new GDIFont(font.Name, font.Size, font.Bold, font.Italic, font.Underline, strikethrough: false);
			_Fonts[GetFontKey(font)] = value;
		}
		return value;
	}

	private string GetFontKey(Font font)
	{
		return font.Name + font.Bold + font.Italic + font.Underline + font.Size;
	}

	private void CreateRows()
	{
		if (_Elements == null)
		{
			return;
		}
		int num = 0;
		_Rows = new List<Row>();
		Row row = new Row();
		_Rows.Add(row);
		bool flag = false;
		Element[] elements = _Elements;
		foreach (Element element in elements)
		{
			if (element.words == null)
			{
				return;
			}
			if (element.NewLine)
			{
				num = 0;
				row = new Row();
				_Rows.Add(row);
				flag = true;
			}
			if (element.TagName == "hr")
			{
				row.RenderSeparator = true;
			}
			Word[] words = element.words;
			foreach (Word word in words)
			{
				if (WordWrap)
				{
					int num2 = 0;
					if (ScrollBars == ScrollBars.Both || ScrollBars == ScrollBars.Vertical)
					{
						num2 = vScroll.Width;
					}
					if (word.Width + num > base.ClientWidth - Margin - num2)
					{
						num = 0;
						row = new Row();
						_Rows.Add(row);
						flag = true;
					}
				}
				if (word.Text.Replace(" ", "") != "" || word.Image != null)
				{
					flag = false;
				}
				if (!flag)
				{
					row.Words.Add(word);
					num += word.Width;
				}
			}
		}
		int num3 = 0;
		foreach (Row row3 in _Rows)
		{
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			if (num3 > 0)
			{
				int num7 = num3 - 1;
				Row row2 = _Rows[num7];
				while (num7 >= 0 && row2.Words.Count == 0)
				{
					row2 = _Rows[num7];
					num7--;
				}
				if (num7 >= 0)
				{
					row2 = _Rows[num7];
					if (row2.Words.Count > 0)
					{
						Word word2 = row2.Words[row2.Words.Count - 1];
						num5 = word2.Height;
					}
				}
			}
			foreach (Word word3 in row3.Words)
			{
				if (word3.Height > num5 && word3.Text != "")
				{
					num5 = word3.Height;
				}
				num4 += word3.Width;
			}
			row3.Height = num5;
			foreach (Word word4 in row3.Words)
			{
				if (word4.Image != null && word4.Height > num5)
				{
					_ = word4.Height;
				}
			}
			foreach (Word word5 in row3.Words)
			{
				int num8 = 0;
				int num9 = 0;
				if (word5.Image != null)
				{
					switch (GetAttrib("valign", word5.Element.Tag))
					{
					case "top":
						num8 = row3.Height;
						num9 = word5.Height - num8;
						break;
					case "middle":
					case "center":
					{
						num8 = row3.Height;
						int num10 = (word5.Height - num8) / 2;
						num8 += num10;
						num9 = num10;
						break;
					}
					case "bottom":
						num8 = word5.Height;
						num9 = 0;
						break;
					default:
						num8 = word5.Height;
						num9 = 0;
						break;
					}
					if (num8 > num5)
					{
						num5 = num8;
					}
					if (num9 > num6)
					{
						num6 = num9;
					}
					num4 += word5.Width;
				}
			}
			row3.Width = num4;
			row3.Height = num5;
			row3.BottomPadd = num6;
			num3++;
		}
		vScroll.Maximum = _Rows.Count;
	}

	private void InitScrollbars()
	{
		if (vScroll != null && hScroll != null)
		{
			if (ScrollBars == ScrollBars.Both)
			{
				vScroll.Left = base.ClientWidth - vScroll.Width;
				vScroll.Top = 0;
				vScroll.Height = base.ClientHeight - hScroll.Height;
				hScroll.Left = 0;
				hScroll.Top = base.ClientHeight - hScroll.Height;
				hScroll.Width = base.ClientWidth - vScroll.Width;
				Filler.Left = vScroll.Left;
				Filler.Top = hScroll.Top;
				Filler.Visible = true;
				vScroll.Visible = true;
				hScroll.Visible = true;
			}
			else if (ScrollBars == ScrollBars.Vertical)
			{
				vScroll.Left = base.ClientWidth - vScroll.Width;
				vScroll.Top = 0;
				vScroll.Height = base.ClientHeight;
				hScroll.Left = 0;
				hScroll.Top = base.ClientHeight - hScroll.Height;
				hScroll.Width = base.ClientWidth - vScroll.Width;
				Filler.Left = vScroll.Left;
				Filler.Top = hScroll.Top;
				Filler.Visible = false;
				vScroll.Visible = true;
				hScroll.Visible = false;
			}
			else if (ScrollBars == ScrollBars.Horizontal)
			{
				vScroll.Left = base.ClientWidth - vScroll.Width;
				vScroll.Top = 0;
				vScroll.Height = base.ClientHeight;
				hScroll.Left = 0;
				hScroll.Top = base.ClientHeight - hScroll.Height;
				hScroll.Width = base.ClientWidth;
				Filler.Left = vScroll.Left;
				Filler.Top = hScroll.Top;
				Filler.Visible = false;
				vScroll.Visible = false;
				hScroll.Visible = true;
			}
			else if (ScrollBars == ScrollBars.None)
			{
				vScroll.Left = base.ClientWidth - vScroll.Width;
				vScroll.Top = 0;
				vScroll.Height = base.ClientHeight;
				hScroll.Left = 0;
				hScroll.Top = base.ClientHeight - hScroll.Height;
				hScroll.Width = base.ClientWidth;
				Filler.Left = vScroll.Left;
				Filler.Top = hScroll.Top;
				Filler.Visible = false;
				vScroll.Visible = false;
				hScroll.Visible = false;
			}
		}
	}

	protected override void OnResize(EventArgs e)
	{
		try
		{
			InitScrollbars();
			SetAutoSize();
		}
		catch
		{
		}
		CreateRows();
		base.OnResize(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		int num = e.Y;
		int num2 = e.X;
		int num3 = 0;
		bool flag = false;
		_ActiveElement = null;
		if (_Rows != null)
		{
			foreach (Row row in _Rows)
			{
				if (num >= row.Top && num <= row.Top + row.Height)
				{
					foreach (Word word in row.Words)
					{
						if (num >= word.ScreenArea.Top && num <= word.ScreenArea.Bottom && num2 >= word.ScreenArea.Left && num2 <= word.ScreenArea.Right && word.Element.Link != null)
						{
							flag = true;
							_ActiveElement = word.Element.Link;
							break;
						}
					}
					break;
				}
				num3++;
			}
		}
		if (flag)
		{
			Cursor = Cursors.Hand;
			Invalidate();
			OnClickLink(GetAttrib("href", _ActiveElement.Tag));
		}
		else
		{
			Cursor = Cursors.Arrow;
			Invalidate();
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		int num = e.Y;
		int num2 = e.X;
		int num3 = 0;
		bool flag = false;
		_ActiveElement = null;
		if (_Rows != null)
		{
			foreach (Row row in _Rows)
			{
				if (num >= row.Top && num <= row.Top + row.Height)
				{
					foreach (Word word in row.Words)
					{
						if (num >= word.ScreenArea.Top && num <= word.ScreenArea.Bottom && num2 >= word.ScreenArea.Left && num2 <= word.ScreenArea.Right && word.Element.Link != null)
						{
							flag = true;
							_ActiveElement = word.Element.Link;
							break;
						}
					}
					break;
				}
				num3++;
			}
		}
		if (flag)
		{
			Cursor = Cursors.Hand;
			Invalidate();
		}
		else
		{
			Cursor = Cursors.Arrow;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	private void vScroll_Scroll(object sender, ScrollEventArgs e)
	{
		Invalidate();
	}

	public int GetWidth()
	{
		int num = 0;
		foreach (Row row in _Rows)
		{
			if (row.Width > num)
			{
				num = row.Width;
			}
		}
		return num + Margin * 2 + base.BorderWidth * 2;
	}

	public int GetHeight()
	{
		int num = 0;
		foreach (Row row in _Rows)
		{
			num += row.Height;
		}
		return num + Margin * 2 + base.BorderWidth * 2;
	}
}
