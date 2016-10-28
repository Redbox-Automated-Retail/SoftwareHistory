using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Alsing.Windows.Forms.IntelliMouse;
using Properties;

namespace Alsing.Windows.Forms.CoreLib;

[ComVisible(true)]
public class IntelliMouseControl : Control
{
	protected const int WM_CAPTURECHANGED = 533;

	protected const int WM_LBUTTONDOWN = 513;

	protected const int WM_MBUTTONDOWN = 519;

	protected const int WM_MBUTTONUP = 520;

	protected const int WM_MOUSELEAVE = 675;

	protected const int WM_RBUTTONDOWN = 516;

	protected bool Active;

	protected IContainer components;

	private bool IsDirty;

	protected WeakReference _CurrentParent;

	protected Point ActivationPoint = new Point(0, 0);

	protected Point CurrentDelta = new Point(0, 0);

	protected Bitmap _Image;

	protected PictureBox picImage;

	protected RegionHandler regionHandler1;

	protected Timer tmrFeedback;

	protected Color _TransparencyKey = Color.FromArgb(255, 0, 255);

	protected Control CurrentParent
	{
		get
		{
			if (_CurrentParent != null)
			{
				return (Control)_CurrentParent.Target;
			}
			return null;
		}
		set
		{
			_CurrentParent = new WeakReference(value);
		}
	}

	public Bitmap Image
	{
		get
		{
			return _Image;
		}
		set
		{
			_Image = value;
			IsDirty = true;
		}
	}

	public Color TransparencyKey
	{
		get
		{
			return _TransparencyKey;
		}
		set
		{
			_TransparencyKey = value;
			IsDirty = true;
		}
	}

	public event EventHandler BeginScroll;

	public event EventHandler EndScroll;

	public event Alsing.Windows.Forms.IntelliMouse.ScrollEventHandler Scroll;

	public IntelliMouseControl()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	~IntelliMouseControl()
	{
	}

	protected void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Alsing.Windows.Forms.CoreLib.IntelliMouseControl));
		this.tmrFeedback = new System.Windows.Forms.Timer();
		this.picImage = new System.Windows.Forms.PictureBox();
		this.regionHandler1 = new Alsing.Windows.Forms.CoreLib.RegionHandler(this.components);
		this.tmrFeedback.Enabled = true;
		this.tmrFeedback.Interval = 10;
		this.tmrFeedback.Tick += new System.EventHandler(tmrFeedback_Tick);
		this.picImage.Image = (System.Drawing.Bitmap)resourceManager.GetObject("picImage.Image");
		this.picImage.Location = new System.Drawing.Point(17, 17);
		this.picImage.Name = "picImage";
		this.picImage.TabIndex = 0;
		this.picImage.TabStop = false;
		this.regionHandler1.Control = null;
		this.regionHandler1.MaskImage = null;
		this.regionHandler1.TransparencyKey = System.Drawing.Color.FromArgb(255, 0, 255);
		base.ParentChanged += new System.EventHandler(IntelliMouseControl_ParentChanged);
	}

	protected void CreateRegion()
	{
		regionHandler1.ApplyRegion(this, Image, TransparencyKey);
		IsDirty = false;
	}

	public void Activate(int x, int y)
	{
		if (IsDirty)
		{
			CreateRegion();
		}
		base.Size = new Size(Image.Width, Image.Height);
		base.Location = new Point(x - Image.Width / 2, y - Image.Height / 2);
		ActivationPoint.X = x;
		ActivationPoint.Y = y;
		BringToFront();
		base.Visible = true;
		Focus();
		Active = false;
		Application.DoEvents();
		SetCursor(0, 0);
		tmrFeedback.Enabled = true;
		onBeginScroll(new EventArgs());
		NativeMethods.SendMessage(base.Handle, 519, 0, 0);
		Active = true;
	}

	protected void SetCursor(int x, int y)
	{
		CurrentDelta.X = x;
		CurrentDelta.Y = y;
		if (y > 16)
		{
			MemoryStream stream = new MemoryStream(Resources.MoveDown);
			Cursor = new Cursor(stream);
			CurrentDelta.Y -= 16;
		}
		else if (y < -16)
		{
			MemoryStream stream2 = new MemoryStream(Resources.MoveUp);
			Cursor = new Cursor(stream2);
			CurrentDelta.Y += 16;
		}
		else
		{
			MemoryStream stream3 = new MemoryStream(Resources.MoveUpDown);
			Cursor = new Cursor(stream3);
			CurrentDelta = new Point(0, 0);
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (Active && e.Button != MouseButtons.None && e.Button != MouseButtons.Middle && e.X != 0 && e.Y != 0)
		{
			Deactivate();
			Point point = new Point(e.X + base.Left, e.Y + base.Top);
			NativeMethods.SendMessage(base.Parent.Handle, 513, 0, point.Y * 65536 + point.X);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (Active)
		{
			if (e.Button != MouseButtons.Middle && e.Button != MouseButtons.None)
			{
				Deactivate();
				return;
			}
			int num = e.X;
			int num2 = e.Y;
			num -= Image.Width / 2;
			num2 -= Image.Height / 2;
			SetCursor(num, num2);
			NativeMethods.SendMessage(base.Handle, 519, 0, 0);
		}
		else
		{
			base.OnMouseMove(e);
		}
	}

	protected void Deactivate()
	{
		NativeMethods.SendMessage(base.Handle, 520, 0, 0);
		Active = false;
		tmrFeedback.Enabled = false;
		Hide();
		onEndScroll(new EventArgs());
		base.Parent.Focus();
	}

	protected override void OnResize(EventArgs e)
	{
		base.Size = ((Image != null) ? new Size(Image.Width, Image.Height) : new Size(32, 32));
	}

	protected void Parent_MouseDown(object s, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Middle)
		{
			Activate(e.X, e.Y);
		}
	}

	protected void tmrFeedback_Tick(object sender, EventArgs e)
	{
		Alsing.Windows.Forms.IntelliMouse.ScrollEventArgs e2 = new Alsing.Windows.Forms.IntelliMouse.ScrollEventArgs();
		e2.DeltaX = CurrentDelta.X;
		e2.DeltaY = CurrentDelta.Y;
		Alsing.Windows.Forms.IntelliMouse.ScrollEventArgs e3 = e2;
		onScroll(e3);
	}

	protected virtual void onBeginScroll(EventArgs e)
	{
		if (BeginScroll != null)
		{
			BeginScroll(this, e);
		}
	}

	protected virtual void onEndScroll(EventArgs e)
	{
		if (EndScroll != null)
		{
			EndScroll(this, e);
		}
	}

	protected virtual void onScroll(Alsing.Windows.Forms.IntelliMouse.ScrollEventArgs e)
	{
		if (Scroll != null)
		{
			Scroll(this, e);
		}
	}

	protected void IntelliMouseControl_ParentChanged(object sender, EventArgs e)
	{
		if (CurrentParent != null)
		{
			CurrentParent.MouseDown -= Parent_MouseDown;
		}
		if (base.Parent != null)
		{
			base.Parent.MouseDown += Parent_MouseDown;
			Deactivate();
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		Deactivate();
	}

	protected override void OnLeave(EventArgs e)
	{
		base.OnLeave(e);
		Deactivate();
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		Deactivate();
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 675)
		{
			base.WndProc(ref m);
			Deactivate();
		}
		else
		{
			base.WndProc(ref m);
		}
	}
}
