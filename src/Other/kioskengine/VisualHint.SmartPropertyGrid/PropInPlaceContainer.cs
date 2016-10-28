using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropInPlaceContainer : Form, IMessageHookListener
{
	private enum MoveDirection
	{
		None,
		Vertical,
		NESW,
		NWSE
	}

	private IDropDownContent _content;

	private MoveDirection _currentMoveDirection;

	private Point _resizeStartPoint = Point.Empty;

	private bool _resizable;

	private bool _preventResizable;

	public static readonly int ResizeGripHeight;

	public static readonly Size MinSize;

	private bool _contentIsCached;

	private bool _resizeTop;

	private bool _insideFireMustBeClosedEvent;

	private Bitmap _resizeGripImage;

	public bool Resizable
	{
		get
		{
			return _resizable;
		}
		set
		{
			_resizable = value;
		}
	}

	public bool ResizeTop
	{
		get
		{
			return _resizeTop;
		}
		set
		{
			base.DockPadding.Top = 0;
			base.DockPadding.Bottom = 0;
			_resizeTop = value;
			if (_resizable)
			{
				_resizeGripImage = null;
				if (_resizeTop)
				{
					base.DockPadding.Top = ResizeGripHeight + 1;
				}
				else
				{
					base.DockPadding.Bottom = ResizeGripHeight + 1;
				}
			}
		}
	}

	public IDropDownContent DropDownContent => _content;

	public Control ContentControl => _content as Control;

	public int SelectedIndex
	{
		get
		{
			if (_content != null)
			{
				return _content.GetSelectedIndex();
			}
			return -1;
		}
	}

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle = 136;
			createParams.Style = -2139095040;
			if (OSFeature.IsPresent(SystemParameter.DropShadow))
			{
				createParams.ClassStyle |= 131072;
			}
			return createParams;
		}
	}

	internal event EventHandler MustBeClosed;

	public void SetContent(PropertyEnumerator propEnum, string currentValue)
	{
		SetContent(propEnum, currentValue, null);
	}

	public void SetContent(PropertyEnumerator propEnum, string currentValue, object valueKey)
	{
		if (propEnum == null)
		{
			if (_content != null)
			{
				(_content as Control).MouseUp -= OnContentClicked;
				base.Controls.Remove(_content as Control);
				if (!_contentIsCached)
				{
					(_content as Control).Dispose();
				}
				_content = null;
			}
			return;
		}
		if (propEnum.Property.Value.FeelCache != null)
		{
			_contentIsCached = true;
			_content = propEnum.Property.Value.FeelCache as IDropDownContent;
		}
		else
		{
			_content = (IDropDownContent)propEnum.Property.ParentGrid.GetDropDownContent(propEnum, valueKey);
			if (propEnum.Property.Value.GetAttribute(typeof(UseFeelCacheAttribute)) != null)
			{
				_contentIsCached = true;
				propEnum.Property.Value.FeelCache = _content;
			}
		}
		if (!(_content is Control control))
		{
			return;
		}
		control.Parent = this;
		PropertyGrid parentGrid = propEnum.Property.ParentGrid;
		DropDownListboxVisualsAttribute dropDownListboxVisualsAttribute = (DropDownListboxVisualsAttribute)propEnum.Property.Value.GetAttribute(typeof(DropDownListboxVisualsAttribute));
		if ((dropDownListboxVisualsAttribute == null || dropDownListboxVisualsAttribute.UsePropertyValueVisuals) && !parentGrid.ShowDefaultValues)
		{
			control.BackColor = propEnum.Property.Value.BackColor;
			control.ForeColor = propEnum.Property.Value.ForeColor;
			control.Font = propEnum.Property.Value.Font;
		}
		else
		{
			control.BackColor = parentGrid.GridBackColor;
			control.ForeColor = parentGrid.GridForeColor;
			control.Font = parentGrid.Font;
		}
		control.MouseUp += OnContentClicked;
		_content.InitializeContent(propEnum, currentValue, valueKey);
		base.Size = new Size(control.Width + 2, control.Height + 2);
		if (ContentControl is DropDownContentListBox dropDownContentListBox)
		{
			if (!_preventResizable && dropDownContentListBox.Items.Count > dropDownContentListBox.Height / dropDownContentListBox.ItemHeight)
			{
				base.Height += SystemInformation.HorizontalScrollBarHeight + 1;
				Resizable = true;
			}
			else
			{
				Resizable = false;
			}
		}
	}

	static PropInPlaceContainer()
	{
		ResizeGripHeight = 0;
		ResizeGripHeight = SystemInformation.HorizontalScrollBarHeight;
		MinSize = new Size(SystemInformation.VerticalScrollBarWidth * 4, SystemInformation.HorizontalScrollBarHeight * 4);
	}

	public PropInPlaceContainer(bool preventResizable)
		: this()
	{
		_preventResizable = preventResizable;
	}

	public PropInPlaceContainer()
	{
		base.FormBorderStyle = FormBorderStyle.None;
		base.ShowInTaskbar = false;
		base.ControlBox = false;
		base.MinimizeBox = false;
		base.MaximizeBox = false;
		SetStyle(Utilities.DoubleBufferStyle, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: false);
		SetStyle(ControlStyles.UserMouse, value: true);
	}

	[CLSCompliant(false)]
	public bool OnListenMessageHook(Control control, uint message, IntPtr wParam, IntPtr lParam)
	{
		if (message == 8 && control != null && Contains(control))
		{
			Control control2 = Control.FromHandle(wParam);
			if (control2 != this && !Contains(control2))
			{
				FireMustBeClosedEvent(reasonLostFocus: true, control2);
			}
		}
		return false;
	}

	public void CloseDropDown(bool validateValue)
	{
		FireMustBeClosedEvent(validateValue);
	}

	private void FireMustBeClosedEvent(bool reasonLostFocus, Control wndTakingFocus)
	{
		if (!_insideFireMustBeClosedEvent)
		{
			_insideFireMustBeClosedEvent = true;
			if (MustBeClosed != null)
			{
				MustBeClosed(this, new DropDownContentClosedEventArgs(reasonLostFocus, wndTakingFocus));
			}
			_insideFireMustBeClosedEvent = false;
		}
	}

	internal void FireMustBeClosedEvent(bool toBeValidated)
	{
		if (!_insideFireMustBeClosedEvent)
		{
			_insideFireMustBeClosedEvent = true;
			if (MustBeClosed != null)
			{
				MustBeClosed(this, new DropDownContentClosedEventArgs(toBeValidated));
			}
			_insideFireMustBeClosedEvent = false;
		}
	}

	private Bitmap GetResizeGripImage(Graphics g)
	{
		if (_resizeGripImage == null)
		{
			_resizeGripImage = new Bitmap(ResizeGripHeight, ResizeGripHeight, g);
			using (Graphics graphics = Graphics.FromImage(_resizeGripImage))
			{
				Matrix matrix = new Matrix();
				if (RightToLeft == RightToLeft.Yes)
				{
					matrix.Translate(0f, ResizeTop ? ((float)(ResizeGripHeight + 1)) : 0f);
					matrix.Scale(1f, ResizeTop ? (-1f) : 1f);
				}
				else
				{
					matrix.Translate(ResizeGripHeight + 1, ResizeTop ? ((float)(ResizeGripHeight + 1)) : 0f);
					matrix.Scale(-1f, ResizeTop ? (-1f) : 1f);
				}
				graphics.Transform = matrix;
				ControlPaint.DrawSizeGrip(graphics, BackColor, 0, 0, ResizeGripHeight, ResizeGripHeight);
				graphics.ResetTransform();
			}
			_resizeGripImage.MakeTransparent(BackColor);
		}
		return _resizeGripImage;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (_resizable)
		{
			Bitmap resizeGripImage = GetResizeGripImage(e.Graphics);
			Rectangle rect = ((RightToLeft != RightToLeft.Yes) ? new Rectangle(0, ResizeTop ? (-1) : (base.Height - ResizeGripHeight - 3), ResizeGripHeight, ResizeGripHeight) : new Rectangle(base.Width - resizeGripImage.Width - 4, ResizeTop ? (-1) : (base.Height - ResizeGripHeight - 3), ResizeGripHeight, ResizeGripHeight));
			e.Graphics.DrawImage(resizeGripImage, rect);
			int num = (ResizeTop ? (ContentControl.Top - 1) : ContentControl.Bottom);
			e.Graphics.DrawLine(SystemPens.ControlDark, 0, num, base.Width, num);
		}
	}

	private MoveDirection GetMoveDirection(Point pt)
	{
		Rectangle rectangle = ((RightToLeft != RightToLeft.Yes) ? new Rectangle(0, (!ResizeTop) ? (base.Height - ResizeGripHeight - 3) : 0, ResizeGripHeight, ResizeGripHeight) : new Rectangle(base.Width - ResizeGripHeight - 4, (!ResizeTop) ? (base.Height - ResizeGripHeight) : 0, ResizeGripHeight + 4, ResizeGripHeight));
		if (ResizeTop)
		{
			if (rectangle.Contains(pt))
			{
				if (RightToLeft != RightToLeft.Yes)
				{
					return MoveDirection.NWSE;
				}
				return MoveDirection.NESW;
			}
		}
		else if (rectangle.Contains(pt))
		{
			if (RightToLeft != RightToLeft.Yes)
			{
				return MoveDirection.NESW;
			}
			return MoveDirection.NWSE;
		}
		int num = ResizeGripHeight / 2;
		Rectangle rectangle2 = new Rectangle(0, (!ResizeTop) ? (base.Height - num) : 0, base.Width, num);
		if (rectangle2.Contains(pt))
		{
			return MoveDirection.Vertical;
		}
		return MoveDirection.None;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_currentMoveDirection = GetMoveDirection(new Point(e.X, e.Y));
			if (_currentMoveDirection != MoveDirection.None)
			{
				_resizeStartPoint = PointToScreen(new Point(e.X, e.Y));
				base.Capture = true;
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (_currentMoveDirection != MoveDirection.None)
		{
			Point point = PointToScreen(new Point(e.X, e.Y));
			int num = point.Y - _resizeStartPoint.Y;
			int num2 = ((_currentMoveDirection != MoveDirection.Vertical) ? (point.X - _resizeStartPoint.X) : 0);
			if (ResizeTop)
			{
				if (base.Height - num < MinSize.Height)
				{
					num = base.Height - MinSize.Height;
					_resizeStartPoint.Y = base.Bottom - MinSize.Height + (_resizeStartPoint.Y - base.Top);
				}
				else
				{
					_resizeStartPoint.Y = point.Y;
				}
			}
			else if (base.Height + num < MinSize.Height)
			{
				num = MinSize.Height - base.Height;
				_resizeStartPoint.Y = base.Top + MinSize.Height + (_resizeStartPoint.Y - base.Bottom);
			}
			else
			{
				_resizeStartPoint.Y = point.Y;
			}
			Rectangle bounds = base.Bounds;
			if (RightToLeft == RightToLeft.Yes)
			{
				if (base.Width + num2 < MinSize.Width)
				{
					num2 = MinSize.Width - base.Width;
					_resizeStartPoint.X = base.Left + MinSize.Width - (base.Right - _resizeStartPoint.X);
				}
				else
				{
					_resizeStartPoint.X = point.X;
				}
				if (ResizeTop)
				{
					base.Bounds = new Rectangle(base.Left, base.Top + num, base.Width + num2, base.Height - num);
				}
				else
				{
					base.Bounds = new Rectangle(base.Left, base.Top, base.Width + num2, base.Height + num);
				}
			}
			else
			{
				if (base.Width - num2 < MinSize.Width)
				{
					num2 = base.Width - MinSize.Width;
					_resizeStartPoint.X = base.Right - MinSize.Width + (_resizeStartPoint.X - base.Left);
				}
				else
				{
					_resizeStartPoint.X = point.X;
				}
				if (ResizeTop)
				{
					base.Bounds = new Rectangle(base.Left + num2, base.Top + num, base.Width - num2, base.Height - num);
				}
				else
				{
					base.Bounds = new Rectangle(base.Left + num2, base.Top, base.Width - num2, base.Height + num);
				}
			}
			if (bounds != base.Bounds)
			{
				Refresh();
			}
		}
		else
		{
			switch (GetMoveDirection(new Point(e.X, e.Y)))
			{
			case MoveDirection.Vertical:
				Cursor = Cursors.SizeNS;
				break;
			case MoveDirection.NESW:
				Cursor = Cursors.SizeNESW;
				break;
			case MoveDirection.NWSE:
				Cursor = Cursors.SizeNWSE;
				break;
			default:
				Cursor = null;
				break;
			}
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		Cursor = null;
		base.OnMouseLeave(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button == MouseButtons.Left)
		{
			_currentMoveDirection = MoveDirection.None;
			base.Capture = false;
		}
	}

	private void OnContentClicked(object sender, MouseEventArgs e)
	{
		if (_content.HasToBeClosedOnClick())
		{
			FireMustBeClosedEvent(toBeValidated: true);
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == 256)
		{
			switch ((Keys)(int)m.WParam)
			{
			case Keys.Return:
			case Keys.F4:
				FireMustBeClosedEvent(toBeValidated: true);
				return true;
			case Keys.Escape:
				FireMustBeClosedEvent(toBeValidated: false);
				return true;
			}
		}
		else if (m.Msg == 260)
		{
			Keys keys = (Keys)(int)m.WParam;
			if (keys == Keys.Down || keys == Keys.Up)
			{
				FireMustBeClosedEvent(toBeValidated: true);
				return true;
			}
		}
		return false;
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		_resizeGripImage = null;
		base.OnVisibleChanged(e);
	}

	public void DoModalLoop()
	{
		Focus();
		MethodInfo methodInfo = null;
		object obj = null;
		if (PropertyGrid.HandleIdleDuringDropDown)
		{
			Type nestedType = typeof(Application).GetNestedType("ThreadContext", BindingFlags.NonPublic);
			if ((object)nestedType != null)
			{
				methodInfo = nestedType.GetMethod("System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FDoIdle", BindingFlags.Instance | BindingFlags.NonPublic);
				MethodInfo method = nestedType.GetMethod("FromCurrent", BindingFlags.Static | BindingFlags.NonPublic);
				if ((object)method != null)
				{
					obj = method.Invoke(null, new object[0]);
				}
			}
		}
		while (base.Visible)
		{
			Application.DoEvents();
			if (obj != null && (object)methodInfo != null)
			{
				Win32Calls.WaitMessage();
				methodInfo.Invoke(obj, new object[1] { -1 });
			}
			else
			{
				Win32Calls.MsgWaitForMultipleObjects(0, IntPtr.Zero, fWaitAll: true, 250, 255);
			}
		}
	}
}
