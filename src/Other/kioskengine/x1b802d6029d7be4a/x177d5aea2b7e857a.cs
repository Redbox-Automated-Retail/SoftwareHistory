using System;
using System.Collections;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Skybound.VisualTips;

namespace x1b802d6029d7be4a;

internal class x177d5aea2b7e857a : IMessageFilter
{
	private static x177d5aea2b7e857a x9834ddb0e0bd5996;

	private static Hashtable x78e287737d2450c8;

	private static Hashtable xb4923b14cf4c9932;

	private static Control x148e9b7821dfbb19;

	private static System.Timers.Timer x846407aabb57ce0b;

	private static EventHandler x7d70300f07c15cc2;

	private static Control _x2ae3d24e2dae102a;

	private static IntPtr _x3e861d48c2df13f1;

	private static VisualTipProvider _xce630b15ef968765;

	public static Control x2ae3d24e2dae102a
	{
		get
		{
			if (_x2ae3d24e2dae102a != null)
			{
				return _x2ae3d24e2dae102a;
			}
			return _x2ae3d24e2dae102a = Control.FromHandle(_x3e861d48c2df13f1);
		}
	}

	public static VisualTipProvider xce630b15ef968765
	{
		get
		{
			return _xce630b15ef968765;
		}
		set
		{
			_xce630b15ef968765 = value;
		}
	}

	private x177d5aea2b7e857a()
	{
	}

	static x177d5aea2b7e857a()
	{
		x78e287737d2450c8 = new Hashtable();
		xb4923b14cf4c9932 = new Hashtable();
	}

	public static void x3178dfe58af38a3e()
	{
		if (x9834ddb0e0bd5996 == null)
		{
			Application.AddMessageFilter(x9834ddb0e0bd5996 = new x177d5aea2b7e857a());
		}
	}

	public static void x164d925049163070()
	{
		if (x9834ddb0e0bd5996 != null)
		{
			Application.RemoveMessageFilter(x9834ddb0e0bd5996);
			x9834ddb0e0bd5996 = null;
		}
	}

	public static void x4d4b6d982caf2ebd(int xc032bdbfd3143d7d, EventHandler x9a91410fb3d88434)
	{
		if (x148e9b7821dfbb19 == null)
		{
			x148e9b7821dfbb19 = new Control();
			x148e9b7821dfbb19.CreateControl();
		}
		EventHandler eventHandler = (EventHandler)x78e287737d2450c8[xc032bdbfd3143d7d];
		if (eventHandler == null)
		{
			System.Timers.Timer timer = new System.Timers.Timer();
			timer.SynchronizingObject = x148e9b7821dfbb19;
			timer.Interval = xc032bdbfd3143d7d;
			timer.Elapsed += xa6fdaa21b0fd72cc;
			timer.Start();
			xb4923b14cf4c9932[xc032bdbfd3143d7d] = timer;
			x78e287737d2450c8[xc032bdbfd3143d7d] = x9a91410fb3d88434;
		}
		else
		{
			x78e287737d2450c8[xc032bdbfd3143d7d] = (EventHandler)Delegate.Combine(eventHandler, x9a91410fb3d88434);
		}
	}

	public static void x00101a8773a2d606(int xc032bdbfd3143d7d, EventHandler x9a91410fb3d88434)
	{
		EventHandler eventHandler = (EventHandler)x78e287737d2450c8[xc032bdbfd3143d7d];
		if (eventHandler != null)
		{
			x78e287737d2450c8[xc032bdbfd3143d7d] = (EventHandler)Delegate.Remove(eventHandler, x9a91410fb3d88434);
		}
		if (x78e287737d2450c8[xc032bdbfd3143d7d] == null && xb4923b14cf4c9932[xc032bdbfd3143d7d] is System.Timers.Timer timer)
		{
			xb4923b14cf4c9932[xc032bdbfd3143d7d] = null;
			timer.Dispose();
		}
	}

	private static void xa6fdaa21b0fd72cc(object xe0292b9ed559da7d, ElapsedEventArgs xfbf34718e704c6bc)
	{
		((EventHandler)x78e287737d2450c8[(int)(xe0292b9ed559da7d as System.Timers.Timer).Interval])?.Invoke(null, xfbf34718e704c6bc);
	}

	public static void x5fe94b51388fcd2b(int xc032bdbfd3143d7d, EventHandler x7d1bf994956f1081)
	{
		lock (typeof(x177d5aea2b7e857a))
		{
			x7d70300f07c15cc2 = x7d1bf994956f1081;
			if (x846407aabb57ce0b == null)
			{
				x846407aabb57ce0b = new System.Timers.Timer();
				x846407aabb57ce0b.SynchronizingObject = x148e9b7821dfbb19;
			}
			else
			{
				x846407aabb57ce0b.Stop();
			}
			x846407aabb57ce0b.Interval = xc032bdbfd3143d7d;
			x846407aabb57ce0b.Elapsed += x44533cba2fe66dd3;
			x846407aabb57ce0b.Start();
		}
	}

	private static void x44533cba2fe66dd3(object xe0292b9ed559da7d, ElapsedEventArgs xfbf34718e704c6bc)
	{
		if (x846407aabb57ce0b.Enabled)
		{
			x846407aabb57ce0b.Stop();
			x846407aabb57ce0b.Elapsed -= x44533cba2fe66dd3;
			if (x7d70300f07c15cc2 != null)
			{
				x7d70300f07c15cc2(null, xfbf34718e704c6bc);
			}
		}
	}

	private static void x781480e1b79ff9f8()
	{
		if (xb4923b14cf4c9932 == null || xb4923b14cf4c9932.Values == null)
		{
			return;
		}
		foreach (System.Timers.Timer value in xb4923b14cf4c9932.Values)
		{
			if (value != null)
			{
				value.Stop();
				value.Start();
			}
		}
	}

	bool IMessageFilter.PreFilterMessage(ref Message m)
	{
		if (m.Msg == 512)
		{
			x781480e1b79ff9f8();
			_x3e861d48c2df13f1 = m.HWnd;
			_x2ae3d24e2dae102a = null;
		}
		else if ((m.Msg == 675 || m.Msg == 513) && _x3e861d48c2df13f1 == m.HWnd)
		{
			_x3e861d48c2df13f1 = IntPtr.Zero;
			_x2ae3d24e2dae102a = null;
		}
		if (xce630b15ef968765 != null)
		{
			switch (m.Msg)
			{
			case 512:
			{
				Point point = new Point(m.LParam.ToInt32());
				xce630b15ef968765.x59157bfb8127f817(new MouseEventArgs(Control.MouseButtons, 0, point.X, point.Y, 0));
				break;
			}
			case 513:
			case 514:
				xce630b15ef968765.x26745f26d8f8ce5c();
				break;
			case 675:
				xce630b15ef968765.x83e9335166b9fd89(EventArgs.Empty);
				break;
			case 256:
			{
				KeyEventArgs e = new KeyEventArgs((Keys)m.WParam.ToInt32());
				xce630b15ef968765.x0808834e03f64d3d(e);
				return e.Handled;
			}
			}
		}
		else if (m.Msg == 512)
		{
			x781480e1b79ff9f8();
		}
		else if (m.Msg == 256 && VisualTipProvider.xde1abe0aa61e3828.Count > 0)
		{
			return VisualTipProvider.xde1abe0aa61e3828.x5b2e42a68d835b31((Keys)m.WParam.ToInt32());
		}
		return false;
	}
}
