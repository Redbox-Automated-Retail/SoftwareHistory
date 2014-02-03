using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Skybound.ComponentModel;

namespace Skybound.VisualTips.Rendering;

[Skybound.ComponentModel.DisplayName("Office")]
public class VisualTipOfficeRenderer : VisualTipRenderer
{
	private VisualTipOfficePreset _x86d76ee96fa83686;

	[ParenthesizePropertyName(true)]
	[Description("The preset which determines the default colors used by the renderer.")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[DefaultValue(typeof(VisualTipOfficePreset), "AutoSelect")]
	public VisualTipOfficePreset Preset
	{
		get
		{
			if (_x86d76ee96fa83686 != null)
			{
				return _x86d76ee96fa83686;
			}
			return VisualTipOfficePreset.AutoSelect;
		}
		set
		{
			_x86d76ee96fa83686 = value;
			x255a9125fad2d9bb("Preset");
		}
	}

	[PropertyListValue]
	[Description("Whether a tip is displayed with round corners.")]
	public bool RoundCorners
	{
		get
		{
			return (bool)x4e35c79189b28e26.x3f88a25febd23896("RoundCorners", true);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("RoundCorners", value, true);
		}
	}

	[Description("The effect used to draw a tip background.")]
	[PropertyListValue]
	public VisualTipOfficeBackgroundEffect BackgroundEffect
	{
		get
		{
			return (VisualTipOfficeBackgroundEffect)x4e35c79189b28e26.x3f88a25febd23896("BackgroundEffect", VisualTipOfficeBackgroundEffect.Gradient);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("BackgroundEffect", value, VisualTipOfficeBackgroundEffect.Gradient);
		}
	}

	[PropertyListValue]
	[Description("The background color of a tip.")]
	public Color BackColor
	{
		get
		{
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("BackColor", Preset.xf83c69bb98e96a69);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("BackColor", x794d88e66bc5597a(value), Color.Empty);
		}
	}

	[PropertyListValue]
	[Description("The color into which the background gradient blends on a tip.")]
	public Color BackColorGradient
	{
		get
		{
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("BackColorGradient", Preset.x096ce6921a0d3679);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("BackColorGradient", x794d88e66bc5597a(value), Color.Empty);
		}
	}

	[PropertyListValue]
	[Description("The angle of the background gradient, in degrees clockwise from the x-axis (0-360).")]
	public int GradientAngle
	{
		get
		{
			return (int)x4e35c79189b28e26.x3f88a25febd23896("GradientAngle", 90);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("GradientAngle", value, 90);
		}
	}

	[PropertyListValue]
	[Description("The color of the tip border.")]
	public Color BorderColor
	{
		get
		{
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("BorderColor", Preset.xb64cd85cc9846947);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("BorderColor", x794d88e66bc5597a(value), Color.Empty);
		}
	}

	[PropertyListValue]
	[Description("The color of the text on a tip.")]
	public Color TextColor
	{
		get
		{
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("TextColor", Preset.x0cd10c3b6c8154d6);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("TextColor", x794d88e66bc5597a(value), Color.Empty);
		}
	}

	protected override void OnDrawWindow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
	{
		Rectangle windowBounds = layout.WindowBounds;
		bool flag = BackgroundEffect == VisualTipOfficeBackgroundEffect.Glass;
		GraphicsPath graphicsPath;
		if (RoundCorners)
		{
			graphicsPath = VisualTipRenderer.xe4b7016dff2efe10(windowBounds, VisualTipRenderer.x9843c083bd22e3f5 ? 5 : 7, BorderCorners.All);
		}
		else
		{
			graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(new Rectangle(windowBounds.Location, windowBounds.Size - new Size(1, 1)));
		}
		using (Brush brush = new LinearGradientBrush(windowBounds, BackColor, BackColorGradient, GradientAngle + (flag ? 180 : 0), isAngleScaleable: false))
		{
			e.Graphics.FillPath(brush, graphicsPath);
		}
		if (flag)
		{
			int num = (int)((1f - (1f - BackColor.GetBrightness()) * 0.75f) * 255f);
			Color baseColor = Color.FromArgb(num, num, num);
			using LinearGradientBrush brush2 = new LinearGradientBrush(windowBounds, Color.FromArgb(128, baseColor), Color.FromArgb(0, baseColor), LinearGradientMode.Vertical);
			using GraphicsPath path = x08acb482965b2c70(windowBounds, RoundCorners ? (VisualTipRenderer.x9843c083bd22e3f5 ? 5 : 7) : 0);
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.FillPath(brush2, path);
			e.Graphics.SmoothingMode = SmoothingMode.Default;
		}
		using (Pen pen = new Pen(BorderColor))
		{
			if (VisualTipRenderer.x9843c083bd22e3f5)
			{
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			}
			else
			{
				pen.Alignment = PenAlignment.Center;
			}
			e.Graphics.DrawPath(pen, graphicsPath);
			if (VisualTipRenderer.x9843c083bd22e3f5)
			{
				e.Graphics.SmoothingMode = SmoothingMode.Default;
			}
		}
		if (HasFooter(tip))
		{
			int num2 = layout.GetElementBounds(VisualTipRenderElement.FooterImage).Top - 6;
			Color color = Color.FromArgb((int)((float)(int)BackColorGradient.R * 0.9f), (int)((float)(int)BackColorGradient.G * 0.9f), (int)((float)(int)BackColorGradient.B * 0.9f));
			using Pen pen2 = new Pen(color);
			e.Graphics.DrawLine(pen2, windowBounds.X + 5, num2, windowBounds.Right - 6, num2);
			pen2.Color = ControlPaint.Light(BackColor);
			e.Graphics.DrawLine(pen2, windowBounds.X + 5, num2 + 1, windowBounds.Right - 6, num2 + 1);
		}
	}

	private GraphicsPath x08acb482965b2c70(Rectangle xda73fcb97c77d998, int xd6ed827fa7f40115)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		if (xd6ed827fa7f40115 == 0)
		{
			graphicsPath.AddLine(xda73fcb97c77d998.X, xda73fcb97c77d998.Y, xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Y);
			graphicsPath.AddLine(xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Y, xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Bottom - 1);
		}
		else
		{
			graphicsPath.AddLine(xda73fcb97c77d998.X + xd6ed827fa7f40115, xda73fcb97c77d998.Y, xda73fcb97c77d998.Right - xd6ed827fa7f40115 - 1, xda73fcb97c77d998.Y);
			graphicsPath.AddArc(xda73fcb97c77d998.Right - xd6ed827fa7f40115 - 1, xda73fcb97c77d998.Y, xd6ed827fa7f40115, xd6ed827fa7f40115, 270f, 90f);
			graphicsPath.AddLine(xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Y + xd6ed827fa7f40115, xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Bottom - xd6ed827fa7f40115);
		}
		graphicsPath.AddBezier(xda73fcb97c77d998.Right - 1, xda73fcb97c77d998.Bottom - 1, xda73fcb97c77d998.Right, xda73fcb97c77d998.Y + xda73fcb97c77d998.Height / 2, xda73fcb97c77d998.X + xda73fcb97c77d998.Width / 2, xda73fcb97c77d998.Y, xda73fcb97c77d998.X, xda73fcb97c77d998.Y);
		graphicsPath.CloseAllFigures();
		return graphicsPath;
	}

	protected override Color GetElementTextColor(VisualTip tip, VisualTipRenderElement element)
	{
		return TextColor;
	}

	protected override Region OnCreateMaskRegion(VisualTip tip, VisualTipLayout layout)
	{
		if (RoundCorners)
		{
			Rectangle windowBounds = layout.WindowBounds;
			windowBounds.Width++;
			windowBounds.Height++;
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddPolygon(new Point[8]
			{
				new Point(windowBounds.X + 2, windowBounds.Y),
				new Point(windowBounds.Right - 3, windowBounds.Y),
				new Point(windowBounds.Right - 1, windowBounds.Y + 2),
				new Point(windowBounds.Right - 1, windowBounds.Bottom - 4),
				new Point(windowBounds.Right - 4, windowBounds.Bottom - 1),
				new Point(windowBounds.X + 2, windowBounds.Bottom - 1),
				new Point(windowBounds.X, windowBounds.Bottom - 4),
				new Point(windowBounds.X, windowBounds.Y + 2)
			});
			return new Region(graphicsPath);
		}
		return base.OnCreateMaskRegion(tip, layout);
	}
}
