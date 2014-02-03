using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.CoreLib;

public class SplitViewControl : Control
{
	private enum SizeAction
	{
		None,
		SizeH,
		SizeV,
		SizeA
	}

	private Control _LowerLeft;

	private Control _LowerRight;

	private Control _UpperLeft;

	private Control _UpperRight;

	private SizeAction Action;

	private Panel Center;

	private Container components;

	private bool FirstTime;

	private Panel Horizontal;

	private Point PrevPos = new Point(0);

	private Point StartPos = new Point(0, 0);

	private Panel Vertical;

	public Control UpperLeft
	{
		get
		{
			return _UpperLeft;
		}
		set
		{
			_UpperLeft = value;
			DoResize();
		}
	}

	public Control UpperRight
	{
		get
		{
			return _UpperRight;
		}
		set
		{
			_UpperRight = value;
			DoResize();
		}
	}

	public Control LowerLeft
	{
		get
		{
			return _LowerLeft;
		}
		set
		{
			_LowerLeft = value;
			DoResize();
		}
	}

	public Control LowerRight
	{
		get
		{
			return _LowerRight;
		}
		set
		{
			_LowerRight = value;
			DoResize();
		}
	}

	public int SplitviewV
	{
		get
		{
			return Vertical.Left;
		}
		set
		{
			Vertical.Left = value;
			DoResize();
		}
	}

	public int SplitviewH
	{
		get
		{
			return Horizontal.Top;
		}
		set
		{
			Horizontal.Top = value;
			DoResize();
		}
	}

	public event EventHandler Resizing;

	public event EventHandler HideTop;

	public event EventHandler HideLeft;

	public SplitViewControl()
	{
		SetStyle(ControlStyles.ContainerControl, value: true);
		InitializeComponent();
		DoResize();
		ReSize(0, 0);
	}

	private void InitializeComponent()
	{
		this.Vertical = new System.Windows.Forms.Panel();
		this.Horizontal = new System.Windows.Forms.Panel();
		this.Center = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.Vertical.BackColor = System.Drawing.SystemColors.Control;
		this.Vertical.Cursor = System.Windows.Forms.Cursors.VSplit;
		this.Vertical.Name = "Vertical";
		this.Vertical.Size = new System.Drawing.Size(4, 264);
		this.Vertical.TabIndex = 0;
		this.Vertical.Resize += new System.EventHandler(Vertical_Resize);
		this.Vertical.MouseUp += new System.Windows.Forms.MouseEventHandler(Vertical_MouseUp);
		this.Vertical.DoubleClick += new System.EventHandler(Vertical_DoubleClick);
		this.Vertical.MouseMove += new System.Windows.Forms.MouseEventHandler(Vertical_MouseMove);
		this.Vertical.MouseDown += new System.Windows.Forms.MouseEventHandler(Vertical_MouseDown);
		this.Horizontal.BackColor = System.Drawing.SystemColors.Control;
		this.Horizontal.Cursor = System.Windows.Forms.Cursors.HSplit;
		this.Horizontal.Name = "Horizontal";
		this.Horizontal.Size = new System.Drawing.Size(320, 4);
		this.Horizontal.TabIndex = 1;
		this.Horizontal.MouseUp += new System.Windows.Forms.MouseEventHandler(Horizontal_MouseUp);
		this.Horizontal.DoubleClick += new System.EventHandler(Horizontal_DoubleClick);
		this.Horizontal.MouseMove += new System.Windows.Forms.MouseEventHandler(Horizontal_MouseMove);
		this.Horizontal.MouseDown += new System.Windows.Forms.MouseEventHandler(Horizontal_MouseDown);
		this.Center.BackColor = System.Drawing.SystemColors.Control;
		this.Center.Cursor = System.Windows.Forms.Cursors.SizeAll;
		this.Center.Location = new System.Drawing.Point(146, 69);
		this.Center.Name = "Center";
		this.Center.Size = new System.Drawing.Size(24, 24);
		this.Center.TabIndex = 2;
		this.Center.MouseUp += new System.Windows.Forms.MouseEventHandler(Center_MouseUp);
		this.Center.DoubleClick += new System.EventHandler(Center_DoubleClick);
		this.Center.MouseMove += new System.Windows.Forms.MouseEventHandler(Center_MouseMove);
		this.Center.MouseDown += new System.Windows.Forms.MouseEventHandler(Center_MouseDown);
		this.BackColor = System.Drawing.Color.Magenta;
		base.Controls.AddRange(new System.Windows.Forms.Control[3] { this.Center, this.Horizontal, this.Vertical });
		base.Size = new System.Drawing.Size(200, 200);
		base.VisibleChanged += new System.EventHandler(SplitViewControl_VisibleChanged);
		base.Enter += new System.EventHandler(SplitViewControl_Enter);
		base.Leave += new System.EventHandler(SplitViewControl_Leave);
		base.ResumeLayout(false);
	}

	private void OnResizing()
	{
		if (Resizing != null)
		{
			Resizing(this, new EventArgs());
		}
	}

	private void OnHideLeft()
	{
		if (HideLeft != null)
		{
			HideLeft(this, new EventArgs());
		}
	}

	private void OnHideTop()
	{
		if (HideTop != null)
		{
			HideTop(this, new EventArgs());
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void OnResize(EventArgs e)
	{
		DoResize();
	}

	private void DoResize()
	{
		int num = base.Height;
		int num2 = base.Width;
		if (num != 0 && num2 != 0)
		{
			SuspendLayout();
			ReSize2();
			OnResizing();
			if (Horizontal.Top < 15)
			{
				Horizontal.Top = -Horizontal.Height;
				OnHideTop();
			}
			if (Vertical.Left < 15)
			{
				Vertical.Left = -Vertical.Width;
				OnHideLeft();
			}
			Horizontal.Width = base.Width;
			Vertical.Height = base.Height;
			Horizontal.SendToBack();
			Vertical.SendToBack();
			Horizontal.BackColor = SystemColors.Control;
			Vertical.BackColor = SystemColors.Control;
			int num3 = Vertical.Left + Vertical.Width;
			int num4 = Horizontal.Top + Horizontal.Height;
			int num5 = base.Width - num3;
			int num6 = base.Height - num4;
			int top = Horizontal.Top;
			int left = Vertical.Left;
			if (LowerRight != null)
			{
				LowerRight.BringToFront();
				LowerRight.SetBounds(num3, num4, num5, num6);
			}
			if (UpperRight != null)
			{
				UpperRight.BringToFront();
				UpperRight.SetBounds(num3, 0, num5, top);
			}
			if (LowerLeft != null)
			{
				LowerLeft.BringToFront();
				LowerLeft.SetBounds(0, num4, left, num6);
			}
			if (UpperLeft != null)
			{
				UpperLeft.BringToFront();
				UpperLeft.SetBounds(0, 0, left, top);
			}
			ResumeLayout();
		}
	}

	protected override void OnControlAdded(ControlEventArgs e)
	{
		DoResize();
	}

	private void Vertical_Resize(object sender, EventArgs e)
	{
	}

	private void Horizontal_MouseDown(object sender, MouseEventArgs e)
	{
		Action = SizeAction.SizeH;
		StartPos = new Point(e.X, e.Y);
		Horizontal.BringToFront();
		Horizontal.BackColor = SystemColors.ControlDark;
		FirstTime = true;
	}

	private void Vertical_MouseDown(object sender, MouseEventArgs e)
	{
		Action = SizeAction.SizeV;
		StartPos = new Point(e.X, e.Y);
		Vertical.BringToFront();
		Vertical.BackColor = SystemColors.ControlDark;
		FirstTime = true;
	}

	private void Center_MouseDown(object sender, MouseEventArgs e)
	{
		Action = SizeAction.SizeA;
		StartPos = new Point(e.X, e.Y);
		Vertical.BringToFront();
		Horizontal.BringToFront();
		Vertical.BackColor = SystemColors.ControlDark;
		Horizontal.BackColor = SystemColors.ControlDark;
		FirstTime = true;
	}

	private void Horizontal_MouseUp(object sender, MouseEventArgs e)
	{
		int num = 0;
		int num2 = StartPos.Y - e.Y;
		ReSize(num, num2);
		Action = SizeAction.None;
		DoResize();
	}

	private void Vertical_MouseUp(object sender, MouseEventArgs e)
	{
		int num = StartPos.X - e.X;
		int num2 = 0;
		ReSize(num, num2);
		Action = SizeAction.None;
		DoResize();
		Refresh();
	}

	private void Center_MouseUp(object sender, MouseEventArgs e)
	{
		int num = StartPos.X - e.X;
		int num2 = StartPos.Y - e.Y;
		ReSize(num, num2);
		Action = SizeAction.None;
		DoResize();
	}

	private void Horizontal_MouseMove(object sender, MouseEventArgs e)
	{
		if (Action == SizeAction.SizeH && e.Button == MouseButtons.Left)
		{
			int num = e.X;
			int num2 = e.Y;
			if (num2 + Horizontal.Top > base.Height - 4)
			{
				num2 = base.Height - 4 - Horizontal.Top;
			}
			if (num2 + Horizontal.Top < 0)
			{
				num2 = -Horizontal.Top;
			}
			Point point;
			if (!FirstTime)
			{
				point = PointToScreen(base.Location);
				point.Y += PrevPos.Y + Horizontal.Location.Y;
				ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, base.Width, 3), Color.Black);
			}
			else
			{
				FirstTime = false;
			}
			point = PointToScreen(base.Location);
			point.Y += num2 + Horizontal.Location.Y;
			ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, base.Width, 3), Color.Black);
			PrevPos = new Point(num, num2);
		}
	}

	private void Vertical_MouseMove(object sender, MouseEventArgs e)
	{
		if (Action == SizeAction.SizeV && e.Button == MouseButtons.Left)
		{
			int num = e.X;
			int num2 = e.Y;
			if (num + Vertical.Left > base.Width - 4)
			{
				num = base.Width - 4 - Vertical.Left;
			}
			if (num + Vertical.Left < 0)
			{
				num = -Vertical.Left;
			}
			Point point;
			if (!FirstTime)
			{
				point = PointToScreen(base.Location);
				point.X += PrevPos.X + Vertical.Location.X;
				ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, 3, base.Height), Color.Black);
			}
			else
			{
				FirstTime = false;
			}
			point = PointToScreen(base.Location);
			point.X += num + Vertical.Location.X;
			ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, 3, base.Height), Color.Black);
			PrevPos = new Point(num, num2);
		}
	}

	private void Center_MouseMove(object sender, MouseEventArgs e)
	{
		if (Action == SizeAction.SizeA && e.Button == MouseButtons.Left)
		{
			int num = e.X - Center.Width / 2;
			int num2 = e.Y - Center.Height / 2;
			if (num2 + Horizontal.Top > base.Height - 4)
			{
				num2 = base.Height - 4 - Horizontal.Top;
			}
			if (num2 + Horizontal.Top < 0)
			{
				num2 = -Horizontal.Top;
			}
			if (num + Vertical.Left > base.Width - 4)
			{
				num = base.Width - 4 - Vertical.Left;
			}
			if (num + Vertical.Left < 0)
			{
				num = -Vertical.Left;
			}
			Point point;
			if (!FirstTime)
			{
				point = PointToScreen(base.Location);
				point.X += PrevPos.X + Vertical.Location.X;
				ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, 3, base.Height), Color.Black);
				point = PointToScreen(base.Location);
				point.Y += PrevPos.Y + Horizontal.Location.Y;
				ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, base.Width, 3), SystemColors.ControlDark);
			}
			else
			{
				FirstTime = false;
			}
			point = PointToScreen(base.Location);
			point.X += num + Vertical.Location.X;
			ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, 3, base.Height), Color.Black);
			point = PointToScreen(base.Location);
			point.Y += num2 + Horizontal.Location.Y;
			ControlPaint.FillReversibleRectangle(new Rectangle(point.X, point.Y, base.Width, 3), SystemColors.ControlDark);
			PrevPos = new Point(num, num2);
		}
	}

	private void ReSize(int x, int y)
	{
		SuspendLayout();
		int num = Vertical.Left - x;
		int num2 = Horizontal.Top - y;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > base.Height - Horizontal.Height - SystemInformation.VerticalScrollBarWidth * 3)
		{
			num2 = base.Height - Horizontal.Height - SystemInformation.VerticalScrollBarWidth * 3;
		}
		if (num > base.Width - Vertical.Width - SystemInformation.VerticalScrollBarWidth * 3)
		{
			num = base.Width - Vertical.Width - SystemInformation.VerticalScrollBarWidth * 3;
		}
		if (num != Vertical.Left)
		{
			Vertical.Left = num;
		}
		if (num2 != Horizontal.Top)
		{
			Horizontal.Top = num2;
		}
		int num3 = Horizontal.Top + Horizontal.Height / 2 - Center.Height / 2;
		int num4 = Vertical.Left + Vertical.Width / 2 - Center.Width / 2;
		Center.Location = new Point(num4, num3);
		ResumeLayout();
		Invalidate();
		try
		{
			if (UpperLeft != null)
			{
				UpperLeft.Refresh();
			}
			if (UpperLeft != null)
			{
				UpperLeft.Refresh();
			}
			if (UpperLeft != null)
			{
				UpperLeft.Refresh();
			}
			if (UpperLeft != null)
			{
				UpperLeft.Refresh();
			}
		}
		catch
		{
		}
		OnResizing();
	}

	private void ReSize2()
	{
		int num = Vertical.Left;
		int num2 = Horizontal.Top;
		if (num < 0)
		{
			num = 0;
		}
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num2 > base.Height - Horizontal.Height - SystemInformation.VerticalScrollBarWidth * 3)
		{
			num2 = base.Height - Horizontal.Height - SystemInformation.VerticalScrollBarWidth * 3;
			if (num2 != Horizontal.Top)
			{
				Horizontal.Top = num2;
			}
		}
		if (num > base.Width - Vertical.Width - SystemInformation.VerticalScrollBarWidth * 3)
		{
			num = base.Width - Vertical.Width - SystemInformation.VerticalScrollBarWidth * 3;
			if (num != Vertical.Left)
			{
				Vertical.Left = num;
			}
		}
		int num3 = Horizontal.Top + Horizontal.Height / 2 - Center.Height / 2;
		int num4 = Vertical.Left + Vertical.Width / 2 - Center.Width / 2;
		Center.Location = new Point(num4, num3);
	}

	private void Center_DoubleClick(object sender, EventArgs e)
	{
		Horizontal.Top = -100;
		Vertical.Left = -100;
		DoResize();
	}

	private void Vertical_DoubleClick(object sender, EventArgs e)
	{
		Vertical.Left = -100;
		DoResize();
	}

	private void Horizontal_DoubleClick(object sender, EventArgs e)
	{
		Horizontal.Top = -100;
		DoResize();
	}

	public void Split5050h()
	{
		Horizontal.Top = base.Height / 2;
		DoResize();
	}

	public void Split5050v()
	{
		Vertical.Left = base.Width / 2;
		DoResize();
	}

	public void ResetSplitview()
	{
		Vertical.Left = 0;
		Horizontal.Top = 0;
		DoResize();
	}

	public void InvokeMouseDownh()
	{
		IntPtr handle = Horizontal.Handle;
		NativeMethods.SendMessage(handle, 513, 0, 0);
	}

	public void InvokeMouseDownv()
	{
		IntPtr handle = Vertical.Handle;
		NativeMethods.SendMessage(handle, 513, 0, 0);
	}

	private void SplitViewControl_Leave(object sender, EventArgs e)
	{
	}

	private void SplitViewControl_Enter(object sender, EventArgs e)
	{
	}

	private void SplitViewControl_VisibleChanged(object sender, EventArgs e)
	{
	}
}
