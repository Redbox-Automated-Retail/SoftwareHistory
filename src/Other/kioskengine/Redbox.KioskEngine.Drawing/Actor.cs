using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Drawing;

public class Actor : IActor, IDisposable
{
	private int m_x;

	private int m_y;

	private Font m_font;

	private int m_width;

	private int m_height;

	private string m_text;

	private string m_html;

	private Image m_image;

	private bool m_enabled;

	private bool m_visible;

	private bool? m_locked;

	private float m_opacity;

	private int m_frameNumber;

	private string m_errorText;

	private Size? m_cornerSize;

	private Color? m_borderColor;

	private float m_strokeWeight;

	private float? m_gradientAngle;

	private Color m_foregroundColor;

	private Color m_backgroundColor;

	private IStyleSheetStyle m_style;

	private float? m_cornerSweepAngle;

	private static bool m_drawErrorTop;

	private float? m_textRotationAngle;

	private WebBrowser m_browserControl;

	private Color? m_gradientTargetColor;

	private Point? m_textTranslationPoint;

	private RenderOptionFlags m_optionFlags;

	private FrameDimension m_frameDimension;

	private StringAlignment m_verticalAlignment;

	private StringAlignment m_horizontalAlignment;

	public bool HitExists => Hit != null;

	public int X
	{
		get
		{
			return m_x;
		}
		set
		{
			if (m_x != value)
			{
				Rectangle rectangle = new Rectangle(m_x, Y, Width, Height);
				m_x = value;
				NotifyScene(new Rectangle[2]
				{
					rectangle,
					new Rectangle(m_x, Y, Width, Height)
				});
			}
		}
	}

	public int Y
	{
		get
		{
			return m_y;
		}
		set
		{
			if (m_y != value)
			{
				Rectangle rectangle = new Rectangle(X, m_y, Width, Height);
				m_y = value;
				NotifyScene(new Rectangle[2]
				{
					rectangle,
					new Rectangle(X, m_y, Width, Height)
				});
			}
		}
	}

	public Font Font
	{
		get
		{
			if (m_font == null)
			{
				IFontCacheService service = ServiceLocator.Instance.GetService<IFontCacheService>();
				if (service != null)
				{
					m_font = service.RegisterFont("DefaultActorFont", "Arial", 16f, FontStyle.Regular);
				}
			}
			return m_font;
		}
		set
		{
			SetFieldValue(ref m_font, value);
		}
	}

	public int Width
	{
		get
		{
			return m_width;
		}
		set
		{
			SetFieldValue(ref m_width, value);
		}
	}

	public int Height
	{
		get
		{
			return m_height;
		}
		set
		{
			SetFieldValue(ref m_height, value);
		}
	}

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			SetFieldValue(ref m_text, value);
		}
	}

	public string Html
	{
		get
		{
			return m_html;
		}
		set
		{
			SetFieldValue(ref m_html, value);
			if (m_html != null && m_html != string.Empty)
			{
				m_browserControl = new WebBrowser();
				m_browserControl.ScrollBarsEnabled = false;
				m_browserControl.DocumentCompleted += BrowserDocumentCompleted;
				m_browserControl.Height = Height;
				m_browserControl.Width = Width;
				m_browserControl.DocumentText = m_html;
				MarkDirty();
			}
		}
	}

	public string Name { get; set; }

	public IScene Scene { get; set; }

	public bool IsDirty { get; internal set; }

	public float Opacity
	{
		get
		{
			return m_opacity;
		}
		set
		{
			if (SetFieldValue(ref m_opacity, value, notifyScene: false, processStyle: false))
			{
				if (m_opacity < 0f)
				{
					m_opacity = 0f;
				}
				if (m_opacity >= 1f)
				{
					m_opacity = 1f;
				}
				NotifyScene(new Rectangle[1]
				{
					new Rectangle(X, Y, Width, Height)
				});
			}
		}
	}

	public int FrameCount { get; internal set; }

	public int? TabOrder { get; set; }

	public int FrameNumber
	{
		get
		{
			return m_frameNumber;
		}
		set
		{
			if (SetFieldValue(ref m_frameNumber, value, notifyScene: false, processStyle: false))
			{
				if (m_frameNumber >= FrameCount)
				{
					m_frameNumber = 0;
				}
				if (m_frameNumber < 0)
				{
					m_frameNumber = 0;
				}
				NotifyScene(new Rectangle[1]
				{
					new Rectangle(X, Y, Width, Height)
				});
			}
		}
	}

	public Image Image
	{
		get
		{
			return m_image;
		}
		set
		{
			if (!SetFieldValue(ref m_image, value, notifyScene: false, processStyle: false))
			{
				return;
			}
			if (m_image != null)
			{
				if (Width == 0)
				{
					Width = m_image.Width;
				}
				if (Height == 0)
				{
					Height = m_image.Height;
				}
				m_frameDimension = new FrameDimension(m_image.FrameDimensionsList[0]);
				FrameCount = m_image.GetFrameCount(m_frameDimension);
			}
			NotifyScene(new Rectangle[1]
			{
				new Rectangle(X, Y, Width, Height)
			});
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			SetFieldValue(ref m_visible, value);
		}
	}

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			SetFieldValue(ref m_enabled, value, notifyScene: true, processStyle: true);
		}
	}

	public bool? Locked
	{
		get
		{
			return m_locked;
		}
		set
		{
			if (!m_locked.HasValue)
			{
				m_locked = value;
				return;
			}
			throw new ArgumentException("Locked can only be set once.");
		}
	}

	public object Tag { get; set; }

	public Rectangle Bounds => new Rectangle(X, Y, Width, Height);

	public string ErrorText
	{
		get
		{
			return m_errorText;
		}
		set
		{
			SetFieldValue(ref m_errorText, value);
		}
	}

	public float[] TabStops { get; set; }

	public float StrokeWeight
	{
		get
		{
			return m_strokeWeight;
		}
		set
		{
			SetFieldValue(ref m_strokeWeight, value);
		}
	}

	public string StyleName { get; set; }

	public Rectangle? HotSpot { get; set; }

	public Rectangle? TextRegion { get; set; }

	public HitTestFlags HitFlags { get; set; }

	public Size? CornerSize
	{
		get
		{
			return m_cornerSize;
		}
		set
		{
			SetFieldValue(ref m_cornerSize, value);
		}
	}

	public Color? BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			SetFieldValue(ref m_borderColor, value);
		}
	}

	public Color ForegroundColor
	{
		get
		{
			return m_foregroundColor;
		}
		set
		{
			SetFieldValue(ref m_foregroundColor, value);
		}
	}

	public Color BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			SetFieldValue(ref m_backgroundColor, value);
		}
	}

	public float? GradientAngle
	{
		get
		{
			return m_gradientAngle;
		}
		set
		{
			SetFieldValue(ref m_gradientAngle, value);
		}
	}

	public float? CornerSweepAngle
	{
		get
		{
			return m_cornerSweepAngle;
		}
		set
		{
			SetFieldValue(ref m_cornerSweepAngle, value);
		}
	}

	public float? TextRotationAngle
	{
		get
		{
			return m_textRotationAngle;
		}
		set
		{
			SetFieldValue(ref m_textRotationAngle, value);
		}
	}

	public Color? GradientTargetColor
	{
		get
		{
			return m_gradientTargetColor;
		}
		set
		{
			SetFieldValue(ref m_gradientTargetColor, value);
		}
	}

	public string RelativeToActorName { get; set; }

	public Point? TextTranslationPoint
	{
		get
		{
			return m_textTranslationPoint;
		}
		set
		{
			SetFieldValue(ref m_textTranslationPoint, value);
		}
	}

	public RenderStateFlags StateFlags { get; set; }

	public RenderOptionFlags OptionFlags
	{
		get
		{
			return m_optionFlags;
		}
		set
		{
			SetFieldValue(ref m_optionFlags, value);
		}
	}

	public StringAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignment;
		}
		set
		{
			SetFieldValue(ref m_verticalAlignment, value);
		}
	}

	public StringAlignment HorizontalAlignment
	{
		get
		{
			return m_horizontalAlignment;
		}
		set
		{
			SetFieldValue(ref m_horizontalAlignment, value);
		}
	}

	public IStyleSheetStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			SetFieldValue(ref m_style, value);
		}
	}

	public event EventHandler Hit;

	public Actor()
	{
		Opacity = 1f;
		StrokeWeight = 1f;
		Enabled = true;
		Visible = true;
		IsDirty = true;
		BackgroundColor = Color.Black;
		ForegroundColor = Color.White;
		OptionFlags = RenderOptionFlags.Text | RenderOptionFlags.Image;
	}

	public void RaiseHit()
	{
		if (Hit != null)
		{
			Hit(this, EventArgs.Empty);
		}
	}

	public void ClearHit()
	{
		Hit = null;
	}

	public void MarkDirty()
	{
		NotifyScene(new Rectangle[1]
		{
			new Rectangle(X, Y, Width, Height)
		});
	}

	public virtual void Dispose()
	{
		ClearHit();
	}

	public MeasureTextResult MeasureText()
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		using Graphics graphics = Graphics.FromHwnd(Application.OpenForms[0].Handle);
		graphics.PixelOffsetMode = PixelOffsetMode.Half;
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		Rectangle textRegion = GetTextRegion(forTranslation: false);
		StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
		SizeF size = graphics.MeasureString(service.ExpandProperties(Text), Font, new SizeF(textRegion.Width, textRegion.Height), stringFormat, out var charactersFitted, out var linesFilled);
		return new MeasureTextResult(size, charactersFitted, linesFilled);
	}

	public void CenterHorizontally()
	{
		if (Scene != null)
		{
			X = (Scene.Width - Width) / 2;
		}
	}

	public void CenterVertically()
	{
		if (Scene != null)
		{
			Y = (Scene.Height - Height) / 2;
		}
	}

	public void Render(Graphics context)
	{
		OnRender(context);
	}

	protected void NotifyScene(Rectangle[] rectangles)
	{
		if (Scene != null)
		{
			IsDirty = Visible;
			Scene.MakeDirty(rectangles);
		}
	}

	protected virtual void OnRender(Graphics context)
	{
		if (!Visible)
		{
			IsDirty = false;
			return;
		}
		context.ResetTransform();
		if ((OptionFlags & RenderOptionFlags.DisableAntiAlias) == 0)
		{
			context.SmoothingMode = SmoothingMode.AntiAlias;
		}
		bool hasValue = TextRotationAngle.HasValue;
		bool hasValue2 = TextTranslationPoint.HasValue;
		if ((OptionFlags & RenderOptionFlags.Background) != RenderOptionFlags.None || (OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
		{
			Color alphaRampedColor = GetAlphaRampedColor(BackgroundColor, Opacity);
			if (CornerSize.HasValue)
			{
				using Pen p = new Pen(alphaRampedColor, StrokeWeight);
				DrawRoundedRect(context, p, X, Y, Width, Height, CornerSize.Value.Width, fill: true);
			}
			else
			{
				Brush brush = null;
				try
				{
					if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
					{
						Color alphaRampedColor2 = GetAlphaRampedColor(GradientTargetColor ?? BackgroundColor, Opacity);
						brush = new LinearGradientBrush(new Rectangle(X, Y, Width, Height), alphaRampedColor, alphaRampedColor2, GradientAngle ?? 180f);
					}
					else
					{
						brush = new SolidBrush(alphaRampedColor);
					}
					context.FillRectangle(brush, X, Y, Width, Height);
				}
				finally
				{
					brush?.Dispose();
				}
			}
		}
		if ((OptionFlags & RenderOptionFlags.Image) != RenderOptionFlags.None && Image != null)
		{
			ImageAttributes imageAttributes = new ImageAttributes();
			if (FrameCount > 1)
			{
				Image.SelectActiveFrame(m_frameDimension, FrameNumber);
			}
			else
			{
				imageAttributes.SetColorMatrix(new ColorMatrix
				{
					Matrix33 = Opacity
				});
			}
			if ((OptionFlags & RenderOptionFlags.Tile) != RenderOptionFlags.None)
			{
				imageAttributes.SetWrapMode(WrapMode.Tile);
			}
			context.DrawImage(Image, new Rectangle(X, Y, Width, Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes);
		}
		if ((OptionFlags & RenderOptionFlags.DrawBorder) != RenderOptionFlags.None)
		{
			using Pen pen = new Pen(BorderColor ?? Color.Red, StrokeWeight);
			if (CornerSize.HasValue)
			{
				DrawRoundedRect(context, pen, X, Y, Width, Height, CornerSize.Value.Width, fill: false);
			}
			else
			{
				context.DrawRectangle(pen, new Rectangle(X, Y, Width, Height));
			}
		}
		if ((OptionFlags & RenderOptionFlags.Text) != RenderOptionFlags.None && !string.IsNullOrEmpty(Text))
		{
			using SolidBrush brush2 = new SolidBrush(GetAlphaRampedColor(ForegroundColor, Opacity));
			StringFormat stringFormat = (StringFormat)StringFormat.GenericTypographic.Clone();
			stringFormat.Alignment = HorizontalAlignment;
			stringFormat.LineAlignment = VerticalAlignment;
			if ((OptionFlags & RenderOptionFlags.DisableAntiAlias) == 0)
			{
				context.PixelOffsetMode = PixelOffsetMode.Half;
				context.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			if (TabStops != null && TabStops.Length > 0)
			{
				stringFormat.SetTabStops(0f, TabStops);
			}
			if ((OptionFlags & RenderOptionFlags.ElideText) != RenderOptionFlags.None)
			{
				stringFormat.Trimming |= StringTrimming.EllipsisCharacter;
			}
			Rectangle textRegion = GetTextRegion(hasValue2);
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string s = service.ExpandProperties(Text);
			if (hasValue2)
			{
				context.TranslateTransform(TextTranslationPoint.Value.X, TextTranslationPoint.Value.Y);
			}
			if (hasValue)
			{
				context.RotateTransform(TextRotationAngle.Value);
			}
			if ((OptionFlags & RenderOptionFlags.TextShadow) != RenderOptionFlags.None)
			{
				using SolidBrush brush3 = new SolidBrush(GetAlphaRampedColor(Color.Black, Opacity));
				context.DrawString(s, Font, brush3, new Rectangle(textRegion.Left + 1, textRegion.Top + 1, textRegion.Width, textRegion.Height), stringFormat);
			}
			context.DrawString(s, Font, brush2, textRegion, stringFormat);
			if (hasValue2 || hasValue)
			{
				context.ResetTransform();
			}
		}
		if ((OptionFlags & RenderOptionFlags.HtmlToImage) != RenderOptionFlags.None && Image != null)
		{
			ImageAttributes imageAttributes2 = new ImageAttributes();
			imageAttributes2.SetColorMatrix(new ColorMatrix
			{
				Matrix33 = Opacity
			});
			context.DrawImage(Image, new Rectangle(X, Y, Width, Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, imageAttributes2);
		}
		if ((HitFlags & HitTestFlags.DrawHotSpot) != HitTestFlags.None)
		{
			context.DrawRectangle(Pens.HotPink, GetHitTestRectangle());
		}
		if (!string.IsNullOrEmpty(ErrorText))
		{
			using (Font font = new Font("Tahoma", 7f, FontStyle.Bold))
			{
				int num = Y + Height + 2;
				if (m_drawErrorTop)
				{
					num = Y - font.Height - 2;
				}
				context.DrawString(ErrorText, font, Brushes.Red, X, num);
			}
			m_drawErrorTop = !m_drawErrorTop;
		}
		StateFlags |= RenderStateFlags.Drawn;
		IsDirty = false;
	}

	internal Rectangle GetTextRegion(bool forTranslation)
	{
		int num = ((!forTranslation) ? X : 0);
		int num2 = ((!forTranslation) ? Y : 0);
		int width = Width;
		int height = Height;
		if (TextRegion.HasValue)
		{
			if (!forTranslation)
			{
				num += TextRegion.Value.Left;
				num2 += TextRegion.Value.Top;
			}
			width = TextRegion.Value.Width;
			height = TextRegion.Value.Height;
		}
		return new Rectangle(num, num2, width, height);
	}

	internal Rectangle GetHitTestRectangle()
	{
		int num = X;
		int num2 = Y;
		int width = Width;
		int height = Height;
		if (HotSpot.HasValue)
		{
			num += HotSpot.Value.Left;
			num2 += HotSpot.Value.Top;
			width = HotSpot.Value.Width;
			height = HotSpot.Value.Height;
		}
		return new Rectangle(num, num2, width, height);
	}

	private void BrowserDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
	{
		Bitmap bitmap = new Bitmap(Width, Height);
		m_browserControl.DrawToBitmap(bitmap, new Rectangle(0, 0, Width, Height));
		Image = bitmap;
	}

	private bool SetFieldValue<T>(ref T field, T value)
	{
		return SetFieldValue(ref field, value, notifyScene: true, processStyle: false);
	}

	private bool SetFieldValue<T>(ref T field, T value, bool notifyScene, bool processStyle)
	{
		if ((field == null && value != null) || (field != null && !field.Equals(value)))
		{
			field = value;
			if (processStyle && Style != null)
			{
				IStyleSheetState state = Style.GetState(Enabled ? "enabled" : "disabled");
				if (state != null)
				{
					PropertyInfo[] properties = typeof(Actor).GetProperties();
					foreach (PropertyInfo propertyInfo in properties)
					{
						string name = propertyInfo.Name;
						name = name.Substring(0, 1).ToLower() + name.Substring(1);
						object obj = state[name];
						if (obj == null)
						{
							continue;
						}
						try
						{
							if (name != "image")
							{
								object value2 = ConversionHelper.ChangeType(obj, propertyInfo.PropertyType);
								propertyInfo.SetValue(this, value2, null);
								continue;
							}
							string text = obj as string;
							if (text != null && text.StartsWith("cache:"))
							{
								IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
								Image = service.GetImage(text.Substring(6));
							}
							else if (text != null)
							{
								IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
								Image = service2.GetBitmap(text);
							}
						}
						catch (Exception e)
						{
							LogHelper.Instance.Log($"Unable to set actor property '{propertyInfo.Name}' with value '{obj}'.", e);
						}
					}
				}
			}
			if (notifyScene && (Width > 0 || Height > 0))
			{
				NotifyScene(new Rectangle[1]
				{
					new Rectangle(X, Y, Width, Height)
				});
			}
			return true;
		}
		return false;
	}

	private static Color GetAlphaRampedColor(Color color, float opacity)
	{
		int alpha = (int)Math.Ceiling(255f * opacity);
		return Color.FromArgb(alpha, color);
	}

	private void DrawRoundedRect(Graphics g, Pen p, float x, float y, float width, float height, float radius, bool fill)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		if (radius * 2f > height || radius * 2f > width)
		{
			radius = ((height > width) ? ((int)(width / 2f)) : ((int)(height / 2f)));
		}
		float num = radius * 2f;
		float sweepAngle = CornerSweepAngle ?? 90f;
		graphicsPath.AddArc(x + width - num, y, num, num, 270f, sweepAngle);
		if (y + height - radius < y + radius)
		{
			graphicsPath.AddLine(x + width, y + radius, x + width, y + height - radius);
		}
		graphicsPath.AddArc(x + width - num, y + height - num, num, num, 0f, sweepAngle);
		if (x + width - radius < x + radius)
		{
			graphicsPath.AddLine(x + width - radius, y + height, x + radius, y + height);
		}
		graphicsPath.AddArc(x, y + height - num, num, num, 90f, sweepAngle);
		if (y + height - radius < y + radius)
		{
			graphicsPath.AddLine(x, y + height - radius, x, y + radius);
		}
		graphicsPath.AddArc(x, y, num, num, 180f, sweepAngle);
		if (x + width - radius > x + radius)
		{
			graphicsPath.AddLine(x + radius, y, x + width - radius, y);
		}
		graphicsPath.CloseFigure();
		if (fill)
		{
			Brush brush = null;
			try
			{
				if ((OptionFlags & RenderOptionFlags.BackgroundGradient) != RenderOptionFlags.None)
				{
					Color alphaRampedColor = GetAlphaRampedColor(GradientTargetColor ?? BackgroundColor, Opacity);
					brush = new LinearGradientBrush(new Rectangle((int)x, (int)y, (int)width, (int)height), p.Color, alphaRampedColor, GradientAngle ?? 180f);
				}
				else
				{
					brush = new SolidBrush(p.Color);
				}
				g.FillPath(brush, graphicsPath);
				return;
			}
			finally
			{
				brush?.Dispose();
			}
		}
		g.DrawPath(p, graphicsPath);
	}
}
