using System.Drawing;
using System.Windows.Forms;

namespace x2de5aa54b5fc5109;

internal struct xdc8e11111d3c00f8(Size size, ColorDepth colorDepth)
{
	private Size _x437e3b626c0fdd43 = size;

	private ColorDepth _x94af564f5d0fedbf = colorDepth;

	public static xdc8e11111d3c00f8 x45260ad4b94166f2 = default(xdc8e11111d3c00f8);

	public Size x437e3b626c0fdd43
	{
		get
		{
			return _x437e3b626c0fdd43;
		}
		set
		{
			_x437e3b626c0fdd43 = value;
		}
	}

	public ColorDepth x94af564f5d0fedbf
	{
		get
		{
			return _x94af564f5d0fedbf;
		}
		set
		{
			_x94af564f5d0fedbf = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is xdc8e11111d3c00f8)
		{
			xdc8e11111d3c00f8 xdc8e11111d3c00f9 = (xdc8e11111d3c00f8)obj;
			if (xdc8e11111d3c00f9._x437e3b626c0fdd43 == _x437e3b626c0fdd43)
			{
				return xdc8e11111d3c00f9._x94af564f5d0fedbf == _x94af564f5d0fedbf;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _x437e3b626c0fdd43.GetHashCode() ^ _x94af564f5d0fedbf.GetHashCode();
	}

	public override string ToString()
	{
		return $"{x437e3b626c0fdd43.Width}x{x437e3b626c0fdd43.Height} {(int)x94af564f5d0fedbf} bpp";
	}
}
