using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Skybound.ComponentModel;

namespace Skybound.VisualTips.Rendering;

[Skybound.ComponentModel.DisplayName("Balloon")]
public class VisualTipBalloonRenderer : VisualTipRenderer
{
	[Description("The background color of a tip.")]
	[PropertyListValue]
	public Color BackColor
	{
		get
		{
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("BackColor", SystemColors.Info);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("BackColor", x794d88e66bc5597a(value), Color.Empty);
		}
	}

	[PropertyListValue]
	[Description("The color of the tip border.")]
	public Color BorderColor
	{
		get
		{
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("BorderColor", SystemColors.InfoText);
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
			return (Color)x4e35c79189b28e26.x3f88a25febd23896("TextColor", SystemColors.InfoText);
		}
		set
		{
			x4e35c79189b28e26.xc7b6e59bfbbe9301("TextColor", x794d88e66bc5597a(value), Color.Empty);
		}
	}

	protected override VisualTipLayout OnCreateLayout(VisualTip tip)
	{
		VisualTipLayout visualTipLayout = base.OnCreateLayout(tip);
		visualTipLayout.x3415cd70babaee65(4, 18);
		visualTipLayout.WindowBounds = Rectangle.Inflate(visualTipLayout.WindowBounds, 4, 2);
		visualTipLayout.ShadowBounds = new Rectangle(visualTipLayout.ShadowBounds.Location, visualTipLayout.ShadowBounds.Size + new Size(8, 32));
		return visualTipLayout;
	}

	protected override void OnDrawShadow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
	{
		if (HasShadow(tip) && Rectangle.Intersect(layout.ShadowBounds, layout.WindowBounds) != layout.ShadowBounds)
		{
			Rectangle shadowBounds = layout.ShadowBounds;
			shadowBounds.X += 4;
			shadowBounds.Width -= 4;
			shadowBounds.Y += 16;
			shadowBounds.Height -= 28;
			VisualTipRenderer.x777ac584ca37e79e(e.Graphics, shadowBounds, 12, Color.FromArgb(128, Color.Black));
		}
	}

	protected override void OnDrawWindow(PaintEventArgs e, VisualTip tip, VisualTipLayout layout)
	{
		Rectangle windowBounds = layout.WindowBounds;
		GraphicsPath path = VisualTipRenderer.xe4b7016dff2efe10(windowBounds, VisualTipRenderer.x9843c083bd22e3f5 ? 10 : 14, BorderCorners.All);
		using (Brush brush = new SolidBrush(BackColor))
		{
			e.Graphics.FillPath(brush, path);
		}
		using Pen pen = new Pen(BorderColor);
		if (VisualTipRenderer.x9843c083bd22e3f5)
		{
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		}
		else
		{
			pen.Alignment = PenAlignment.Inset;
		}
		e.Graphics.DrawPath(pen, path);
		if (VisualTipRenderer.x9843c083bd22e3f5)
		{
			e.Graphics.SmoothingMode = SmoothingMode.Default;
		}
		Point[] points = xc000ad67131b24d6(tip, windowBounds);
		using (Brush brush2 = new SolidBrush(BackColor))
		{
			e.Graphics.FillPolygon(brush2, points);
		}
		using (Pen pen2 = new Pen(BackColor))
		{
			e.Graphics.DrawPolygon(pen2, points);
		}
		e.Graphics.DrawLines(pen, points);
	}

	private Point[] xc000ad67131b24d6(VisualTip x2636ebb6bd343dac, Rectangle x76b3d9d2638e5ecd)
	{
		Rectangle rectangle = x2636ebb6bd343dac.xf8e45af07afc44e0();
		int num = Math.Min(Math.Max(rectangle.X, x76b3d9d2638e5ecd.X + 16), x76b3d9d2638e5ecd.Right - 32);
		bool flag = num > x76b3d9d2638e5ecd.X + x76b3d9d2638e5ecd.Width / 2;
		if (rectangle.Y <= 0)
		{
			return new Point[3]
			{
				new Point(num, x76b3d9d2638e5ecd.Y),
				new Point(num + (flag ? 16 : 0), x76b3d9d2638e5ecd.Y - 16),
				new Point(num + 16, x76b3d9d2638e5ecd.Y)
			};
		}
		return new Point[3]
		{
			new Point(num + 16, x76b3d9d2638e5ecd.Bottom - 1),
			new Point(num + (flag ? 16 : 0), x76b3d9d2638e5ecd.Bottom - 1 + 16),
			new Point(num, x76b3d9d2638e5ecd.Bottom - 1)
		};
	}

	protected override Region OnCreateMaskRegion(VisualTip tip, VisualTipLayout layout)
	{
		Rectangle windowBounds = layout.WindowBounds;
		Point[] array = xc000ad67131b24d6(tip, windowBounds);
		Point[] array2 = new Point[6];
		if (array[0].Y == windowBounds.Y)
		{
			array.CopyTo(array2, 0);
			ref Point reference = ref array2[3];
			ref Point reference2 = ref array2[4];
			ref Point reference3 = ref array2[5];
			reference3 = new Point(windowBounds.X + 10, windowBounds.Bottom);
			reference = (reference2 = reference3);
		}
		else
		{
			ref Point reference4 = ref array2[0];
			ref Point reference5 = ref array2[1];
			ref Point reference6 = ref array2[2];
			reference6 = new Point(windowBounds.X + 10, windowBounds.Y);
			reference4 = (reference5 = reference6);
			array.CopyTo(array2, 3);
		}
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLines(new Point[26]
		{
			new Point(windowBounds.X, windowBounds.Y + 5),
			new Point(windowBounds.X + 1, windowBounds.Y + 3),
			new Point(windowBounds.X + 2, windowBounds.Y + 2),
			new Point(windowBounds.X + 3, windowBounds.Y + 1),
			new Point(windowBounds.X + 5, windowBounds.Y),
			new Point(array2[0].X, array2[0].Y),
			new Point(array2[1].X, array2[1].Y),
			new Point(array2[1].X + 1, array2[1].Y),
			new Point(array2[2].X + 1, array2[2].Y),
			new Point(windowBounds.Right - 5, windowBounds.Y),
			new Point(windowBounds.Right - 2, windowBounds.Y + 2),
			new Point(windowBounds.Right - 1, windowBounds.Y + 4),
			new Point(windowBounds.Right, windowBounds.Bottom - 7),
			new Point(windowBounds.Right - 2, windowBounds.Bottom - 3),
			new Point(windowBounds.Right - 5, windowBounds.Bottom - 1),
			new Point(windowBounds.Right - 6, windowBounds.Bottom),
			new Point(array2[3].X + 1, array2[3].Y),
			new Point(array2[4].X + 1, array2[4].Y),
			new Point(array2[4].X, array2[4].Y),
			new Point(array2[5].X, array2[5].Y),
			new Point(windowBounds.X + 5, windowBounds.Bottom),
			new Point(windowBounds.X + 5, windowBounds.Bottom - 1),
			new Point(windowBounds.X + 4, windowBounds.Bottom - 1),
			new Point(windowBounds.X + 2, windowBounds.Bottom - 3),
			new Point(windowBounds.X + 1, windowBounds.Bottom - 4),
			new Point(windowBounds.X, windowBounds.Bottom - 6)
		});
		return new Region(graphicsPath);
	}
}
