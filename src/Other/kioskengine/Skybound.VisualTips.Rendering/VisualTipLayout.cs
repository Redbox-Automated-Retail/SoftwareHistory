using System;
using System.ComponentModel;
using System.Drawing;

namespace Skybound.VisualTips.Rendering;

public class VisualTipLayout
{
	private Rectangle[] xd1de4e54b407a08b = new Rectangle[9];

	public Rectangle ShadowBounds
	{
		get
		{
			return xd1de4e54b407a08b[0];
		}
		set
		{
			xd1de4e54b407a08b[0] = value;
		}
	}

	public Rectangle WindowBounds
	{
		get
		{
			return xd1de4e54b407a08b[1];
		}
		set
		{
			xd1de4e54b407a08b[1] = value;
		}
	}

	public void Offset(int x, int y)
	{
		for (int i = 0; i < xd1de4e54b407a08b.Length; i++)
		{
			xd1de4e54b407a08b[i].Offset(x, y);
		}
	}

	internal void x3415cd70babaee65(int x08db3aeabb253cb1, int x1e218ceaee1bb583)
	{
		for (int i = 1; i < xd1de4e54b407a08b.Length; i++)
		{
			xd1de4e54b407a08b[i].Offset(x08db3aeabb253cb1, x1e218ceaee1bb583);
		}
	}

	public Size GetSize()
	{
		return Rectangle.Union(xd1de4e54b407a08b[0], xd1de4e54b407a08b[1]).Size;
	}

	public Rectangle GetElementBounds(VisualTipRenderElement element)
	{
		if (!Enum.IsDefined(typeof(VisualTipRenderElement), element))
		{
			throw new InvalidEnumArgumentException("element", (int)element, typeof(VisualTipRenderElement));
		}
		return xd1de4e54b407a08b[(int)(element + 2)];
	}

	public void SetElementBounds(VisualTipRenderElement element, Rectangle value)
	{
		if (!Enum.IsDefined(typeof(VisualTipRenderElement), element))
		{
			throw new InvalidEnumArgumentException("element", (int)element, typeof(VisualTipRenderElement));
		}
		xd1de4e54b407a08b[(int)(element + 2)] = value;
	}
}
