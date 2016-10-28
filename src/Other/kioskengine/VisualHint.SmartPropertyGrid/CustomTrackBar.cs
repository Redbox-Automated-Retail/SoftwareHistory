using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class CustomTrackBar : Control
{
	public enum Orientations
	{
		Horizontal,
		Vertical
	}

	private bool _dragInProgress;

	private Orientation _orientation;

	private string _label;

	private IColorComponentTracker _tracker = new SimpleLineTracker();

	private IColorComponentDrawingImplementer _drawingImplementer;

	private int _minimum;

	private int _maximum = 255;

	private bool _reversedOrigin;

	private int _value;

	private int _smallChange = 1;

	private int _largeChange = 10;

	private Rectangle _contentRect = Rectangle.Empty;

	private Win32Calls.RECT _margins;

	public Orientation Orientation
	{
		get
		{
			return _orientation;
		}
		set
		{
			_orientation = value;
		}
	}

	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			_label = value;
		}
	}

	public IColorComponentTracker Tracker
	{
		get
		{
			return _tracker;
		}
		set
		{
			if (value == null && (object)value.GetType() != typeof(SimpleLineTracker))
			{
				_tracker = new SimpleLineTracker();
			}
			else if ((object)_tracker.GetType() != value.GetType())
			{
				_tracker = value;
			}
		}
	}

	public int Minimum
	{
		get
		{
			return _minimum;
		}
		set
		{
			_minimum = value;
		}
	}

	public int Maximum
	{
		get
		{
			return _maximum;
		}
		set
		{
			_maximum = value;
		}
	}

	public bool ReversedOrigin
	{
		set
		{
			_reversedOrigin = value;
		}
	}

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			int value2 = _value;
			if (value >= _minimum && value <= _maximum)
			{
				_value = value;
			}
			else if (value < _minimum)
			{
				_value = _minimum;
			}
			else
			{
				_value = _maximum;
			}
			if (ValueChanged != null && value2 != _value)
			{
				ValueChanged(this, EventArgs.Empty);
			}
			Invalidate();
		}
	}

	public int SmallChange
	{
		get
		{
			return _smallChange;
		}
		set
		{
			_smallChange = value;
		}
	}

	public int LargeChange
	{
		get
		{
			return _largeChange;
		}
		set
		{
			_largeChange = value;
		}
	}

	public Win32Calls.RECT Margins
	{
		get
		{
			return _margins;
		}
		set
		{
			_margins = value;
		}
	}

	protected Rectangle ContentRect
	{
		get
		{
			if (_contentRect.IsEmpty)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				Rectangle contentRect = default(Rectangle);
				contentRect.X = _margins.Left;
				contentRect.Y = _margins.Top;
				contentRect.Width = clientRectangle.Right - _margins.Right - contentRect.Left;
				contentRect.Height = clientRectangle.Bottom - _margins.Bottom - contentRect.Top;
				_contentRect = contentRect;
				_contentRect.Inflate(-1, -1);
			}
			return _contentRect;
		}
	}

	public event EventHandler ValueChanged;

	public CustomTrackBar(IColorComponentDrawingImplementer implementer)
	{
		_drawingImplementer = implementer;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.Selectable, value: true);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		using (Brush brush = new SolidBrush(BackColor))
		{
			e.Graphics.FillRectangle(brush, clientRectangle);
		}
		if (_orientation == Orientation.Horizontal)
		{
			using Font font = new Font("Tahoma", 8f);
			Size size = Size.Round(e.Graphics.MeasureString(_label, font));
			Rectangle rectangle = clientRectangle;
			rectangle.Width = size.Width;
			rectangle.Y = (int)((double)(ContentRect.Top + ContentRect.Bottom - size.Height) / 2.0);
			rectangle.Height = size.Height;
			StringFormat stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();
			stringFormat.LineAlignment = StringAlignment.Center;
			e.Graphics.DrawString(_label, font, SystemBrushes.WindowText, rectangle, stringFormat);
		}
		Rectangle contentRect = ContentRect;
		contentRect.Inflate(1, 1);
		e.Graphics.DrawLine(SystemPens.ControlDarkDark, new Point(contentRect.Left, contentRect.Bottom - 2), contentRect.Location);
		e.Graphics.DrawLine(SystemPens.ControlDarkDark, contentRect.Location, new Point(contentRect.Right - 2, contentRect.Top));
		e.Graphics.DrawLine(SystemPens.ControlLightLight, new Point(contentRect.Left, contentRect.Bottom - 1), new Point(contentRect.Right - 1, contentRect.Bottom - 1));
		e.Graphics.DrawLine(SystemPens.ControlLightLight, new Point(contentRect.Right - 1, contentRect.Bottom - 1), new Point(contentRect.Right - 1, contentRect.Top));
		if (Focused)
		{
			contentRect.Inflate(1, 1);
			ControlPaint.DrawFocusRectangle(e.Graphics, contentRect);
		}
		_drawingImplementer.DrawContent(this, e.Graphics, ContentRect);
		GetPositions(out var _, out var _, out var trackerPos);
		if (_orientation == Orientation.Horizontal)
		{
			_tracker.Draw(e.Graphics, _value, new Point(trackerPos, ContentRect.Top), new Point(trackerPos, ContentRect.Bottom), clientRectangle);
		}
		else if (_orientation == Orientation.Vertical)
		{
			_tracker.Draw(e.Graphics, _value, new Point(ContentRect.Left, trackerPos), new Point(ContentRect.Right, trackerPos), clientRectangle);
		}
	}

	protected void GetPositions(out int leftOrTop, out int rightOrBottom, out int trackerPos)
	{
		leftOrTop = ContentRect.Left;
		rightOrBottom = ContentRect.Right;
		if (_reversedOrigin)
		{
			if (_orientation == Orientation.Horizontal)
			{
				trackerPos = ContentRect.Right - (int)Math.Round((double)ContentRect.Width * (double)(_value - _minimum) / (double)_maximum);
			}
			else
			{
				trackerPos = ContentRect.Bottom - (int)Math.Round((double)ContentRect.Height * (double)(_value - _minimum) / (double)_maximum);
			}
		}
		else if (_orientation == Orientation.Horizontal)
		{
			trackerPos = ContentRect.Left + (int)Math.Round((double)ContentRect.Width * (double)(_value - _minimum) / (double)_maximum);
		}
		else
		{
			trackerPos = ContentRect.Top + (int)Math.Round((double)ContentRect.Height * (double)(_value - _minimum) / (double)_maximum);
		}
	}

	protected int GetValueFromPosition(int trackerPos)
	{
		if (_reversedOrigin)
		{
			if (_orientation == Orientation.Horizontal)
			{
				return _minimum + (int)Math.Round((double)(ContentRect.Right - trackerPos) / (double)ContentRect.Width * (double)_maximum);
			}
			if (_orientation == Orientation.Vertical)
			{
				return _minimum + (int)Math.Round((double)(ContentRect.Bottom - trackerPos) / (double)ContentRect.Height * (double)_maximum);
			}
		}
		else
		{
			if (_orientation == Orientation.Horizontal)
			{
				return _minimum + (int)Math.Round((double)(trackerPos - ContentRect.Left) / (double)ContentRect.Width * (double)_maximum);
			}
			if (_orientation == Orientation.Vertical)
			{
				return _minimum + (int)Math.Round((double)(trackerPos - ContentRect.Top) / (double)ContentRect.Height * (double)_maximum);
			}
		}
		return _minimum;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			if (_orientation == Orientation.Horizontal)
			{
				if (e.X >= ContentRect.Left && e.X <= ContentRect.Right)
				{
					Focus();
					Value = GetValueFromPosition(e.X);
					_dragInProgress = true;
				}
			}
			else if (_orientation == Orientation.Vertical && e.Y >= ContentRect.Top && e.Y <= ContentRect.Bottom)
			{
				Focus();
				Value = GetValueFromPosition(e.Y);
				_dragInProgress = true;
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (_dragInProgress)
		{
			if (_orientation == Orientation.Horizontal)
			{
				int num = e.X;
				if (num < ContentRect.Left)
				{
					num = ContentRect.Left;
				}
				else if (num > ContentRect.Right)
				{
					num = ContentRect.Right;
				}
				Value = GetValueFromPosition(num);
			}
			else if (_orientation == Orientation.Vertical)
			{
				int num2 = e.Y;
				if (num2 < ContentRect.Top)
				{
					num2 = ContentRect.Top;
				}
				else if (num2 > ContentRect.Bottom)
				{
					num2 = ContentRect.Bottom;
				}
				Value = GetValueFromPosition(num2);
			}
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_dragInProgress)
		{
			_dragInProgress = false;
		}
		base.OnMouseUp(e);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (keyData == Keys.Left || keyData == Keys.Right || keyData == Keys.Up || keyData == Keys.Down)
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Left)
		{
			Value -= SmallChange;
		}
		else if (e.KeyData == Keys.Down)
		{
			Value -= SmallChange;
		}
		else if (e.KeyData == Keys.Right)
		{
			Value += SmallChange;
		}
		else if (e.KeyData == Keys.Up)
		{
			Value += SmallChange;
		}
		else if (e.KeyData == Keys.Home)
		{
			Value = Minimum;
		}
		else if (e.KeyData == Keys.End)
		{
			Value = Maximum;
		}
		else if (e.KeyData == Keys.Prior)
		{
			Value += LargeChange;
		}
		else if (e.KeyData == Keys.Next)
		{
			Value -= LargeChange;
		}
		base.OnKeyDown(e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		Invalidate();
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		Invalidate();
		base.OnLostFocus(e);
	}
}
