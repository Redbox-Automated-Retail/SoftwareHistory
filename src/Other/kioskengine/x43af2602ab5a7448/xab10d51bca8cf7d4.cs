using System;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using Skybound.Windows.Forms;

namespace x43af2602ab5a7448;

internal class xab10d51bca8cf7d4 : IDisposable
{
	private System.Timers.Timer x89b320c0febbe503;

	private ISynchronizeInvoke _x061479d2a6161ad7;

	private double _xd2f68ee6f47e9dfb;

	private int _xf4d7b500db4ba600;

	private Motion _xec53cc296529fdea;

	private double x8ee060316474a897;

	public ISynchronizeInvoke x061479d2a6161ad7
	{
		get
		{
			return _x061479d2a6161ad7;
		}
		set
		{
			_x061479d2a6161ad7 = value;
			if (x89b320c0febbe503 != null)
			{
				x89b320c0febbe503.SynchronizingObject = value;
			}
		}
	}

	public double xd2f68ee6f47e9dfb => _xd2f68ee6f47e9dfb;

	public int xf4d7b500db4ba600 => _xf4d7b500db4ba600;

	public bool xda1d1aa1eef530a1 => x89b320c0febbe503 == null;

	public Motion xec53cc296529fdea => _xec53cc296529fdea;

	public event EventHandler xf61701b848da9540;

	public xab10d51bca8cf7d4(Motion motion, int duration)
	{
		_xec53cc296529fdea = motion;
		_xf4d7b500db4ba600 = duration;
	}

	public void x12cb12b5d2cad53d()
	{
		if (x89b320c0febbe503 != null)
		{
			throw new InvalidOperationException("INTERNAL ERROR: The animation timer is already running.");
		}
		_xd2f68ee6f47e9dfb = 0.08;
		x89b320c0febbe503 = new System.Timers.Timer();
		x89b320c0febbe503.SynchronizingObject = x061479d2a6161ad7;
		x89b320c0febbe503.Interval = 1.0;
		x89b320c0febbe503.Elapsed += xd87a89026f130e78;
		x89b320c0febbe503.Start();
		x8ee060316474a897 = DateTime.Now.TimeOfDay.TotalMilliseconds;
		xd87a89026f130e78(x89b320c0febbe503, null);
	}

	public void x74f5a1ef3906e23c()
	{
		_xd2f68ee6f47e9dfb = 1.0 - xd2f68ee6f47e9dfb;
	}

	public void xac2b8ecd963a9a83()
	{
		_xd2f68ee6f47e9dfb = 1.0;
		if (x89b320c0febbe503 != null)
		{
			x89b320c0febbe503.Stop();
			x89b320c0febbe503.Dispose();
			x89b320c0febbe503 = null;
		}
	}

	private void xd87a89026f130e78(object xe0292b9ed559da7d, ElapsedEventArgs xfbf34718e704c6bc)
	{
		if (!Monitor.TryEnter(this))
		{
			return;
		}
		try
		{
			if (x89b320c0febbe503 == null)
			{
				return;
			}
			double num = DateTime.Now.TimeOfDay.TotalMilliseconds - x8ee060316474a897;
			double num2 = num / (double)xf4d7b500db4ba600;
			if (xec53cc296529fdea != Motion.Constant)
			{
				double num3 = 0.0;
				if (xec53cc296529fdea == Motion.InOut)
				{
					num3 = (xd2f68ee6f47e9dfb - 0.5) * 1.8;
				}
				else if (xec53cc296529fdea == Motion.Decelerate)
				{
					num3 = xd2f68ee6f47e9dfb * 0.8;
				}
				else if (xec53cc296529fdea == Motion.Accelerate)
				{
					num3 = -0.8 + xd2f68ee6f47e9dfb * 0.8;
				}
				double num4 = Math.Cos(num3 * Math.PI);
				num2 += num2 * num4;
			}
			if (num2 < 0.01)
			{
				num2 = 0.01;
			}
			_xd2f68ee6f47e9dfb += num2;
			if (xd2f68ee6f47e9dfb >= 1.0)
			{
				xac2b8ecd963a9a83();
			}
			else
			{
				x8ee060316474a897 = DateTime.Now.TimeOfDay.TotalMilliseconds;
			}
			x392d5b0a051ea348(xfbf34718e704c6bc);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	protected virtual void x392d5b0a051ea348(EventArgs xfbf34718e704c6bc)
	{
		if (xf61701b848da9540 != null)
		{
			xf61701b848da9540(this, xfbf34718e704c6bc);
		}
	}

	public void Dispose()
	{
		if (x89b320c0febbe503 != null)
		{
			x89b320c0febbe503.Dispose();
			x89b320c0febbe503 = null;
		}
	}
}
