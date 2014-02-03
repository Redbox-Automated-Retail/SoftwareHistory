using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Skybound.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceUITypeEditor : Control, IInPlaceControl, IMouseHookListener
{
	internal class DropDownForm : Form, IMessageHookListener
	{
		private enum MoveDirection
		{
			None,
			Vertical,
			NESW,
			NWSE
		}

		private bool _canceled;

		private PropInPlaceUITypeEditor _inPlaceCtrl;

		private MoveDirection _currentMoveDirection;

		private bool _resizable;

		public static readonly int ResizeGripHeight;

		public static readonly Size MinSize;

		private Point _resizeStartPoint = Point.Empty;

		private bool _resizeTop;

		private Control _innerControl;

		private TypeConverter _typeConverterForFindString;

		private string _autoCompleteBuffer = "";

		private Timer _timer;

		private ListBox _listbox;

		private Bitmap _resizeGripImage;

		public bool ResizeInProgress => _currentMoveDirection != MoveDirection.None;

		public bool ResizeTop
		{
			get
			{
				return _resizeTop;
			}
			set
			{
				_resizeTop = value;
				if (_resizable)
				{
					_resizeGripImage = null;
					if (value)
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

		public bool Canceled => _canceled;

		static DropDownForm()
		{
			ResizeGripHeight = 0;
			ResizeGripHeight = SystemInformation.HorizontalScrollBarHeight;
			MinSize = new Size(SystemInformation.VerticalScrollBarWidth * 4, SystemInformation.HorizontalScrollBarHeight * 4);
		}

		public DropDownForm()
		{
			base.FormBorderStyle = FormBorderStyle.None;
			base.ShowInTaskbar = false;
			base.ControlBox = false;
			base.MinimizeBox = false;
			base.MaximizeBox = false;
			base.KeyPreview = true;
			base.StartPosition = FormStartPosition.Manual;
		}

		public void Init(PropInPlaceUITypeEditor inPlaceCtrl, bool resizable)
		{
			_inPlaceCtrl = inPlaceCtrl;
			_inPlaceCtrl.SetDropDownContainer(this);
			RightToLeft = _inPlaceCtrl.OwnerPropertyEnumerator.Property.ParentGrid.RightToLeft;
			Win32Calls.SetWindowLongPtr(base.Handle, -8, inPlaceCtrl.Parent.Handle);
			_resizable = resizable;
			base.DockPadding.Bottom = 0;
			base.DockPadding.Top = 0;
		}

		public void SetControl(Control ctl)
		{
			base.Controls.Clear();
			_innerControl = ctl;
			base.Controls.Add(_innerControl);
		}

		public Control GetControl()
		{
			return _innerControl;
		}

		public void CloseDropDown()
		{
			Hide();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			if (base.Visible)
			{
				_canceled = false;
			}
			_resizeGripImage = null;
			base.OnVisibleChanged(e);
		}

		public bool OnListenMessageHook(Control control, uint message, IntPtr wParam, IntPtr lParam)
		{
			if (message == 8 && control != null && Contains(control))
			{
				Control control2 = Control.FromHandle(wParam);
				if (control2 != null && control2 != this && !Contains(control2))
				{
					_inPlaceCtrl.Parent.Focus();
				}
			}
			return false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.KeyCode == Keys.Escape)
			{
				_canceled = true;
			}
			if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.F4 || (e.Alt && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up)))
			{
				Hide();
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (_listbox != null)
			{
				if (_timer != null)
				{
					_timer.Stop();
					_timer.Start();
				}
				else
				{
					int autoCompletionTimerDuration = _inPlaceCtrl.OwnerPropertyEnumerator.Property.ParentGrid.AutoCompletionTimerDuration;
					if (autoCompletionTimerDuration > 0)
					{
						_timer = new Timer();
						_timer.Tick += _timer_Tick;
						_timer.Interval = autoCompletionTimerDuration;
						_timer.Start();
					}
				}
				_autoCompleteBuffer += new string(e.KeyChar, 1);
				int num = FindString(_autoCompleteBuffer, _listbox.Items, _listbox.SelectedIndex - ((_autoCompleteBuffer.Length != 1) ? 1 : 0));
				if (num == -1)
				{
					_autoCompleteBuffer = new string(e.KeyChar, 1);
					num = FindString(_autoCompleteBuffer, _listbox.Items, _listbox.SelectedIndex);
				}
				if (num != -1)
				{
					_listbox.SelectedIndex = num;
				}
			}
			_listbox = null;
			base.OnKeyPress(e);
		}

		private int FindString(string str, IList items, int startIndex)
		{
			if (str != null && items != null)
			{
				if (startIndex < -1 || startIndex >= items.Count)
				{
					return -1;
				}
				int length = str.Length;
				int num = 0;
				Type type = null;
				int num2 = (startIndex + 1) % items.Count;
				while (num < items.Count)
				{
					num++;
					object obj = items[num2];
					Type type2 = obj.GetType();
					if (type == null || !type.Equals(type2))
					{
						_typeConverterForFindString = TypeDescriptor.GetConverter(obj);
						type = type2;
					}
					if (string.Compare(str, 0, _typeConverterForFindString.ConvertToString(obj), 0, length, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
					{
						return num2;
					}
					num2 = (num2 + 1) % items.Count;
				}
			}
			return -1;
		}

		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (m.Msg == 256)
			{
				_listbox = Control.FromHandle(m.HWnd) as ListBox;
			}
			return base.ProcessKeyPreview(ref m);
		}

		private void _timer_Tick(object sender, EventArgs e)
		{
			_timer.Stop();
			_timer.Tick -= _timer_Tick;
			_timer = null;
			_autoCompleteBuffer = "";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _timer != null)
			{
				_timer.Stop();
				_timer.Tick -= _timer_Tick;
				_timer = null;
			}
			base.Dispose(disposing);
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

		protected override void OnDeactivate(EventArgs e)
		{
			base.Owner = null;
			base.OnDeactivate(e);
			Hide();
			if (Form.ActiveForm == null)
			{
				_inPlaceCtrl.OwnerPropertyEnumerator.Property.ParentGrid.Grid.HideInPlaceControl();
				ContainerControl topLevelContainer = Utilities.GetTopLevelContainer(_inPlaceCtrl);
				if (topLevelContainer != null)
				{
					topLevelContainer.ActiveControl = _inPlaceCtrl.Parent;
				}
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (_resizable)
			{
				Bitmap resizeGripImage = GetResizeGripImage(e.Graphics);
				Rectangle rect = ((RightToLeft != RightToLeft.Yes) ? new Rectangle(0, ResizeTop ? (-1) : (base.Height - ResizeGripHeight - 3), ResizeGripHeight, ResizeGripHeight) : new Rectangle(base.Width - resizeGripImage.Width - 4, ResizeTop ? (-1) : (base.Height - ResizeGripHeight - 3), ResizeGripHeight, ResizeGripHeight));
				e.Graphics.DrawImage(resizeGripImage, rect);
				int num = (ResizeTop ? (GetControl().Top - 1) : GetControl().Bottom);
				e.Graphics.DrawLine(SystemPens.ControlDark, 0, num, base.Width, num);
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
	}

	private class EditorService : IWindowsFormsEditorService, IServiceProvider
	{
		private PropInPlaceUITypeEditor _inPlaceCtrl;

		private DropDownForm _dropDownHolder;

		private DialogResult _dialogResult = DialogResult.Cancel;

		private bool _canceled;

		internal DropDownForm DropDownHolder => _dropDownHolder;

		public DialogResult DialogResult
		{
			get
			{
				return _dialogResult;
			}
			set
			{
				_dialogResult = value;
			}
		}

		public bool Canceled => _canceled;

		public EditorService(PropInPlaceUITypeEditor inPlaceCtrl)
		{
			_inPlaceCtrl = inPlaceCtrl;
		}

		public void CloseDropDown()
		{
			if (_dropDownHolder != null)
			{
				_dropDownHolder.CloseDropDown();
			}
		}

		public void DropDownControl(Control control)
		{
			if (_dropDownHolder == null)
			{
				_dropDownHolder = new DropDownForm();
			}
			_dropDownHolder.Init(_inPlaceCtrl, _inPlaceCtrl.Resizable);
			_canceled = false;
			int width = control.PreferredSize.Width;
			Rectangle bounds = new Rectangle(0, 0, (width != 0) ? control.PreferredSize.Width : control.Bounds.Width, control.Bounds.Height);
			_dropDownHolder.SetControl(control);
			bounds.Inflate(1, 1);
			if (_inPlaceCtrl.Resizable)
			{
				bounds.Height += DropDownForm.ResizeGripHeight + 1;
			}
			_dropDownHolder.Bounds = bounds;
			control.Dock = DockStyle.Fill;
			control.Resize += OnUIControlResize;
			_dropDownHolder.Width = Math.Max(_inPlaceCtrl.Width + 1, _dropDownHolder.Width);
			Control parentForm = GetParentForm(_inPlaceCtrl);
			if (parentForm != null && parentForm is Form)
			{
				_dropDownHolder.Owner = (Form)parentForm;
			}
			PositionDropDownHolder();
			Win32Calls.ShowWindow(new HandleRef(_dropDownHolder, _dropDownHolder.Handle), 8);
			_dropDownHolder.Visible = true;
			control.Visible = true;
			control.Focus();
			((PropertyGrid)_inPlaceCtrl.Parent.Parent).SetDropDownTopWindow(_dropDownHolder);
			DoModalLoop();
			((PropertyGrid)_inPlaceCtrl.Parent.Parent).SetDropDownTopWindow(null);
			_canceled = _dropDownHolder.Canceled;
		}

		private void OnUIControlResize(object sender, EventArgs e)
		{
			if (!_dropDownHolder.ResizeInProgress)
			{
				Rectangle bounds = _dropDownHolder.GetControl().Bounds;
				bounds.Inflate(1, 1);
				if (_inPlaceCtrl.Resizable)
				{
					int num = DropDownForm.ResizeGripHeight + 1;
					bounds.Height += num;
				}
				_dropDownHolder.Size = bounds.Size;
				PositionDropDownHolder();
			}
		}

		public DialogResult ShowDialog(Form dialog)
		{
			_inPlaceCtrl.Parent.Focus();
			Control parentForm = GetParentForm(_inPlaceCtrl);
			_dialogResult = dialog.ShowDialog(parentForm);
			return _dialogResult;
		}

		public object GetService(Type serviceType)
		{
			if (serviceType.Equals(typeof(IWindowsFormsEditorService)))
			{
				return this;
			}
			return null;
		}

		private void DoModalLoop()
		{
			MethodInfo methodInfo = null;
			object obj = null;
			if (PropertyGrid.HandleIdleDuringDropDown)
			{
				Type nestedType = typeof(Application).GetNestedType("ThreadContext", BindingFlags.NonPublic);
				if (nestedType != null)
				{
					methodInfo = nestedType.GetMethod("System.Windows.Forms.UnsafeNativeMethods.IMsoComponent.FDoIdle", BindingFlags.Instance | BindingFlags.NonPublic);
					MethodInfo method = nestedType.GetMethod("FromCurrent", BindingFlags.Static | BindingFlags.NonPublic);
					if (method != null)
					{
						obj = method.Invoke(null, new object[0]);
					}
				}
			}
			while (_dropDownHolder.Visible)
			{
				Application.DoEvents();
				if (obj != null && methodInfo != null)
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

		private void PositionDropDownHolder()
		{
			Point location;
			if (_inPlaceCtrl.Parent.RightToLeft == RightToLeft.Yes)
			{
				location = _inPlaceCtrl.Parent.PointToScreen(_inPlaceCtrl.Location);
			}
			else
			{
				location = _inPlaceCtrl.Parent.PointToScreen(_inPlaceCtrl.Location);
				location.X -= _dropDownHolder.Width - _inPlaceCtrl.Width;
			}
			Rectangle workingArea = Screen.FromControl(_inPlaceCtrl).WorkingArea;
			if (location.X < workingArea.X)
			{
				location.X = workingArea.X;
			}
			else if (location.X + _dropDownHolder.Width > workingArea.Right)
			{
				location.X = workingArea.Right - _dropDownHolder.Width;
			}
			if (location.Y + _inPlaceCtrl.Height + _dropDownHolder.Height > workingArea.Bottom)
			{
				location.Offset(0, -_dropDownHolder.Height);
				_dropDownHolder.ResizeTop = true;
			}
			else
			{
				location.Offset(0, _inPlaceCtrl.Height);
				_dropDownHolder.ResizeTop = false;
			}
			_dropDownHolder.Location = location;
			_dropDownHolder.Location = location;
		}

		private Control GetParentForm(Control ctl)
		{
			while (ctl.Parent != null)
			{
				ctl = ctl.Parent;
			}
			return ctl;
		}
	}

	private UITypeEditor _editor;

	private EditorService _editorService;

	private PropInPlaceTextbox _edit;

	private bool _dontTransferFocus;

	private PropertyValue.ValueValidationResult _currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private bool _realtimeChange = true;

	private bool _pushed;

	private bool _mouseOver;

	private Control _wmFocusMissingArg;

	private string _buttonText = "...";

	private bool _fullWidthButton;

	private bool _clickedWhileDroppedDown;

	private DropDownForm _editorContainer;

	public PropertyEnumerator OwnerPropertyEnumerator
	{
		get
		{
			return _ownerPropertyEnum;
		}
		set
		{
			_ownerPropertyEnum = (PropertyEnumerator)value.Clone();
			if (_edit != null)
			{
				_edit.OwnerPropertyEnumerator = value;
			}
			ButtonSettingsAttribute buttonSettingsAttribute = (ButtonSettingsAttribute)_ownerPropertyEnum.Property.Value.GetAttribute(typeof(ButtonSettingsAttribute));
			if (buttonSettingsAttribute == null && _ownerPropertyEnum.Parent.Property != null && _ownerPropertyEnum.Parent.Property.Value != null)
			{
				ArrayList attributes = _ownerPropertyEnum.Parent.Property.Value.GetAttributes(typeof(ButtonSettingsAttribute));
				foreach (ButtonSettingsAttribute item in attributes)
				{
					if (item.ChildPropertyName == _ownerPropertyEnum.Property.Name)
					{
						buttonSettingsAttribute = item;
						break;
					}
				}
			}
			if (buttonSettingsAttribute != null)
			{
				FullWidthButton = buttonSettingsAttribute.FullWidth;
				_buttonText = buttonSettingsAttribute.ButtonText;
			}
			if (!_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				_editor = _ownerPropertyEnum.Property.Value.UITypeEditor;
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
			}
		}
	}

	public bool ReadOnly => _ownerPropertyEnum.Property.Value.IsReadOnly(_ownerPropertyEnum);

	public bool RealtimeChange
	{
		get
		{
			return _realtimeChange;
		}
		set
		{
			_realtimeChange = value;
		}
	}

	public override string Text
	{
		get
		{
			if (_edit == null)
			{
				return base.Text;
			}
			return _edit.Text;
		}
		set
		{
			base.Text = value;
			if (_edit != null)
			{
				_edit.Text = value;
			}
			Refresh();
		}
	}

	public string ButtonText
	{
		get
		{
			return _buttonText;
		}
		set
		{
			_buttonText = value;
			RepositionChildren();
		}
	}

	public bool FullWidthButton
	{
		get
		{
			if (_edit != null)
			{
				return false;
			}
			return _fullWidthButton;
		}
		set
		{
			_fullWidthButton = value;
		}
	}

	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
			if (_edit != null)
			{
				_edit.Font = value;
			}
		}
	}

	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
			if (_edit != null)
			{
				_edit.BackColor = value;
			}
		}
	}

	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
			if (_edit != null)
			{
				_edit.ForeColor = value;
			}
		}
	}

	public bool TextBoxVisibility
	{
		set
		{
			if (value)
			{
				if (_edit == null)
				{
					_edit = new PropInPlaceTextbox(isPassword: false);
					_edit.Parent = this;
					_edit.Text = Text;
					_edit.TabStop = false;
					_edit.TextChanged += OnTextboxTextChanged;
				}
			}
			else if (_edit != null)
			{
				_edit.TextChanged -= OnTextboxTextChanged;
				base.Controls.Remove(_edit);
				string text = Text;
				_edit = null;
				Text = text;
			}
		}
	}

	public bool Resizable => _editor.IsDropDownResizable;

	public TextBox TextBox => _edit;

	public PropInPlaceUITypeEditor()
	{
		_editorService = new EditorService(this);
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	private UITypeEditorEditStyle GetVisualStyle(PropertyTypeDescriptorContext context)
	{
		return ((UITypeEditorVisualStyleAttribute)_ownerPropertyEnum.Property.Value.GetAttribute(typeof(UITypeEditorVisualStyleAttribute)))?.VisualStyle ?? _editor.GetEditStyle(context);
	}

	protected int GetButtonWidth(Graphics graphics)
	{
		if (ReadOnly)
		{
			return 0;
		}
		PropertyTypeDescriptorContext typeDescriptorContext = _ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum);
		UITypeEditorEditStyle visualStyle = GetVisualStyle(typeDescriptorContext);
		if (visualStyle == UITypeEditorEditStyle.DropDown)
		{
			return SystemInformation.VerticalScrollBarWidth;
		}
		if (FullWidthButton)
		{
			return base.ClientSize.Width;
		}
		Graphics graphics2 = ((graphics != null) ? graphics : CreateGraphics());
		int result = Size.Round(graphics2.MeasureString(_buttonText, Font)).Width + 6;
		if (graphics == null)
		{
			graphics2.Dispose();
		}
		return result;
	}

	protected Rectangle GetButtonRect(Graphics graphics)
	{
		if (ReadOnly)
		{
			return Rectangle.Empty;
		}
		int buttonWidth = GetButtonWidth(graphics);
		return new Rectangle(base.Width - buttonWidth, 0, buttonWidth, base.Height);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PropertyGrid parentGrid = _ownerPropertyEnum.Property.ParentGrid;
		PropertyValue value = _ownerPropertyEnum.Property.Value;
		_ownerPropertyEnum.Property.ParentGrid.DrawManager.DrawPropertyValueBackground(e.Graphics, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		Rectangle buttonRect = GetButtonRect(e.Graphics);
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.Width -= buttonRect.Width + 1;
		PropertyTypeDescriptorContext typeDescriptorContext = value.GetTypeDescriptorContext(_ownerPropertyEnum);
		UITypeEditorEditStyle visualStyle = GetVisualStyle(typeDescriptorContext);
		if (!FullWidthButton || visualStyle != UITypeEditorEditStyle.Modal)
		{
			if (_edit == null && Focused && _ownerPropertyEnum.Property.InPlaceCtrlInAction == null)
			{
				Rectangle stringValueRect = value.GetStringValueRect(e.Graphics, clientRectangle, Point.Empty);
				stringValueRect.Y++;
				stringValueRect.Height -= 2;
				stringValueRect.X -= _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin / 2;
				stringValueRect.Width = buttonRect.Left - 1 - stringValueRect.Left;
				using Brush brush = new SolidBrush(parentGrid.SelectedBackColor);
				e.Graphics.FillRectangle(brush, OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(stringValueRect, base.ClientSize.Width));
			}
			Color textColor = ((!_ownerPropertyEnum.Property.Enabled) ? SystemColors.GrayText : ((!Focused || _ownerPropertyEnum.Property.InPlaceCtrlInAction != null) ? value.ForeColor : parentGrid.HighlightedTextColor));
			if (value.HasMultipleTexts() && Text.Length == 0)
			{
				value.DrawValue(e.Graphics, clientRectangle, textColor, _ownerPropertyEnum, "", multipleTexts: true, PropertyValue.DrawValueIn.InPlaceCtrl);
			}
			else
			{
				value.DrawValue(e.Graphics, clientRectangle, textColor, _ownerPropertyEnum, Text, multipleTexts: false, PropertyValue.DrawValueIn.InPlaceCtrl);
			}
		}
		if (!ReadOnly)
		{
			buttonRect = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(buttonRect, base.ClientSize.Width);
			if (ThemeRenderer.Enabled)
			{
				switch (visualStyle)
				{
				case UITypeEditorEditStyle.DropDown:
					parentGrid.DrawManager.DrawDropDownButton(e.Graphics, this, drawDown: true, buttonRect, _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
					break;
				case UITypeEditorEditStyle.Modal:
					parentGrid.DrawManager.DrawButton(e.Graphics, this, buttonRect, _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
					Win32Calls.DrawText(e.Graphics, (_buttonText == null) ? value.DisplayString : _buttonText, ref buttonRect, Font, parentGrid.DrawManager.GetButtonTextColor(), 37);
					break;
				}
			}
			else
			{
				switch (visualStyle)
				{
				case UITypeEditorEditStyle.DropDown:
					parentGrid.DrawManager.DrawDropDownButton(e.Graphics, this, drawDown: true, buttonRect, _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
					break;
				case UITypeEditorEditStyle.Modal:
				{
					parentGrid.DrawManager.DrawButton(e.Graphics, this, buttonRect, _pushed, _mouseOver, PropertyValue.DrawValueIn.InPlaceCtrl);
					StringFormat stringFormat = (StringFormat)StringFormat.GenericDefault.Clone();
					stringFormat.Alignment = StringAlignment.Center;
					stringFormat.LineAlignment = StringAlignment.Center;
					using (Brush brush2 = new SolidBrush(parentGrid.DrawManager.GetButtonTextColor()))
					{
						e.Graphics.DrawString((_buttonText == null) ? value.DisplayString : _buttonText, Font, brush2, buttonRect, stringFormat);
					}
					break;
				}
				}
			}
		}
		base.OnPaint(e);
	}

	private void OnTextboxTextChanged(object sender, EventArgs e)
	{
		Invalidate();
	}

	public bool OnForbidClickThrough(Point pt)
	{
		bool flag = true;
		using Graphics graphics = CreateGraphics();
		return _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(pt);
	}

	public virtual void RepositionChildren()
	{
		if (_edit != null)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (clientRectangle.Width > 0)
			{
				int doubleword = Win32Calls.LoWord(Win32Calls.SendMessage(_edit.Handle, 212, IntPtr.Zero, IntPtr.Zero));
				int num = Win32Calls.LoWord(doubleword) + Win32Calls.HiWord(doubleword);
				PropertyTypeDescriptorContext typeDescriptorContext = _ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum);
				UITypeEditorEditStyle visualStyle = GetVisualStyle(typeDescriptorContext);
				int num2 = 0;
				if (visualStyle != UITypeEditorEditStyle.None)
				{
					num2 = GetButtonWidth(null);
				}
				Rectangle clientRectangle2 = base.ClientRectangle;
				clientRectangle2.Width -= num2 + 1;
				clientRectangle2.Height++;
				Graphics graphics = CreateGraphics();
				clientRectangle2 = _ownerPropertyEnum.Property.Value.GetStringValueRect(graphics, clientRectangle2, Point.Empty);
				clientRectangle2.Height--;
				int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
				int num3 = clientRectangle2.Left - globalTextMargin + ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? (globalTextMargin - _ownerPropertyEnum.Property.Value.EditboxLeftMargin) : 0);
				Rectangle rect = new Rectangle(num3, 0, clientRectangle.Width - num2 - num3, clientRectangle2.Height);
				rect.Y++;
				rect.Height--;
				if (rect.Width <= num)
				{
					if (_edit.Focused)
					{
						base.Parent.Focus();
					}
					_edit.Visible = false;
				}
				else
				{
					_edit.Visible = true;
					Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
					Win32Calls.GetTextMetrics(graphics, Font, ref tm);
					int num4 = rect.Height - tm.tmHeight;
					rect.Y += num4 / 2;
					rect.Height -= num4 / 2;
					rect = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
					_edit.Invalidate();
					_edit.Bounds = rect;
				}
				graphics.Dispose();
			}
		}
		Invalidate(invalidateChildren: false);
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		RepositionChildren();
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if (_edit != null)
		{
			if (!_dontTransferFocus)
			{
				if (_wmFocusMissingArg != _edit)
				{
					if (_wmFocusMissingArg != null && _editorService.DropDownHolder != _wmFocusMissingArg.TopLevelControl)
					{
						_edit.Focus();
					}
				}
				else
				{
					base.Parent.Focus();
				}
			}
		}
		else
		{
			Invalidate();
		}
		base.OnGotFocus(e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		_pushed = false;
		_mouseOver = false;
		base.OnLostFocus(e);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 7)
		{
			_wmFocusMissingArg = Control.FromHandle(m.WParam);
		}
		else if (m.Msg == 8)
		{
			_wmFocusMissingArg = Control.FromHandle(m.WParam);
		}
		else if (m.Msg == 522)
		{
			Point p = new Point(Win32Calls.LoWord((int)m.LParam), Win32Calls.HiWord((int)m.LParam));
			p = PointToClient(p);
			OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, Win32Calls.HiWord((int)m.WParam)));
			m.Result = IntPtr.Zero;
			return;
		}
		base.WndProc(ref m);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (_edit != null && !Focused)
		{
			Win32Calls.PostMessage(_edit.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
		}
		else if (_edit == null && OwnerPropertyEnumerator.Property.Value.UseAutoCompletion)
		{
			int currentIndex = -1;
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			for (int i = 0; i < displayedValues.Length; i++)
			{
				if (_ownerPropertyEnum.Property.Value.DisplayString.Equals(displayedValues[i]))
				{
					currentIndex = i;
					break;
				}
			}
			string text = OwnerPropertyEnumerator.Property.Value.IncrementalFind(e.KeyChar, ref currentIndex);
			if (currentIndex != -1 && Text != text)
			{
				Text = text;
				if (RealtimeChange)
				{
					CommitChanges(final: false);
				}
			}
		}
		base.OnKeyPress(e);
	}

	[CLSCompliant(false)]
	public bool OnListenMouseHook(Control control, uint message, Point pt, uint hitTest)
	{
		_clickedWhileDroppedDown = true;
		return false;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			Graphics graphics = CreateGraphics();
			Rectangle rectangle = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width);
			graphics.Dispose();
			if (e.Clicks == 2 && !rectangle.Contains(new Point(e.X, e.Y)))
			{
				ScrollValueOnDoubleClick(Control.ModifierKeys != Keys.Shift, ScrollValueOnDoubleClickSource.FromInPlaceCtrl);
			}
			else if (!_clickedWhileDroppedDown)
			{
				if (_edit == null || _edit.Focused)
				{
					PropertyValue.ValueValidationResult result = CommitChanges(final: false);
					if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
					{
						if (_edit != null)
						{
							_edit.Focus();
							_edit.SelectAll();
						}
						return;
					}
				}
				if (rectangle.Contains(new Point(e.X, e.Y)))
				{
					_pushed = true;
					_mouseOver = true;
					base.Capture = true;
				}
				_dontTransferFocus = true;
				Focus();
				_dontTransferFocus = false;
				Invalidate(invalidateChildren: false);
				Update();
			}
		}
		_clickedWhileDroppedDown = false;
		base.OnMouseDown(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		_clickedWhileDroppedDown = false;
		_mouseOver = false;
		Invalidate(invalidateChildren: false);
		base.OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		_clickedWhileDroppedDown = false;
		bool mouseOver = _mouseOver;
		Graphics graphics = CreateGraphics();
		_mouseOver = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(new Point(e.X, e.Y));
		graphics.Dispose();
		if (mouseOver != _mouseOver)
		{
			Invalidate(invalidateChildren: false);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_pushed && e.Button == MouseButtons.Left)
		{
			base.Capture = false;
			_pushed = false;
			_mouseOver = false;
			Invalidate(invalidateChildren: false);
			using Graphics graphics = CreateGraphics();
			if (_ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(GetButtonRect(graphics), base.ClientSize.Width).Contains(new Point(e.X, e.Y)))
			{
				RunButton();
			}
		}
		base.OnMouseUp(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Space)
		{
			_mouseOver = true;
			_pushed = true;
			Invalidate(invalidateChildren: false);
		}
		else if (e.KeyData == Keys.Up || e.KeyData == Keys.Down)
		{
			ProcessArrowKey(e.KeyData, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
			Invalidate();
		}
		base.OnKeyDown(e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (e.KeyData == Keys.Space)
		{
			_mouseOver = false;
			_pushed = false;
			Invalidate(invalidateChildren: false);
			RunButton();
		}
		base.OnKeyUp(e);
	}

	protected bool ProcessArrowKey(Keys key, ScrollValueOnDoubleClickSource source, bool rotate)
	{
		if (ReadOnly)
		{
			return false;
		}
		string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
		if (displayedValues.Length > 0)
		{
			int num = -1;
			if (_ownerPropertyEnum.Property.Value.HasMultipleTexts())
			{
				num = ((key != Keys.Down) ? (displayedValues.Length - 1) : 0);
			}
			else if (Text.Length == 0)
			{
				num = ((key != Keys.Down) ? (displayedValues.Length - 1) : 0);
			}
			else
			{
				for (int i = 0; i < displayedValues.Length; i++)
				{
					if (Text.Equals(displayedValues[i]))
					{
						num = ((key != Keys.Down) ? ((i > 0) ? (i - 1) : (rotate ? (displayedValues.Length - 1) : 0)) : ((i < displayedValues.Length - 1) ? (i + 1) : ((!rotate) ? (-1) : 0)));
						break;
					}
				}
			}
			if (num != -1)
			{
				_ownerPropertyEnum.Property.Value.ImageIndex = num;
				Text = displayedValues[num];
				if (source == ScrollValueOnDoubleClickSource.FromLabel || RealtimeChange)
				{
					CommitChanges(source == ScrollValueOnDoubleClickSource.FromLabel);
				}
				if (source != ScrollValueOnDoubleClickSource.FromLabel)
				{
					OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: true);
				}
				return true;
			}
		}
		return false;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		if (forward)
		{
			return ProcessArrowKey(Keys.Down, source, rotate: true);
		}
		return ProcessArrowKey(Keys.Up, source, rotate: true);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (!ReadOnly && (keyData == Keys.Up || keyData == Keys.Down))
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if (keyData == Keys.Return)
		{
			_pushed = false;
			_mouseOver = false;
			PropertyValue.ValueValidationResult result = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus && _edit != null)
			{
				_edit.SelectAll();
				return true;
			}
		}
		else if (keys == Keys.Escape)
		{
			_pushed = false;
			_mouseOver = false;
			Text = _ownerPropertyEnum.Property.Value.DisplayString;
			CommitChanges(final: true);
		}
		else
		{
			if (keys == Keys.F4 || (keys == Keys.Down && (keyData & Keys.Alt) != Keys.None))
			{
				_pushed = false;
				_mouseOver = false;
				if (_edit != null && _edit.Focused)
				{
					PropertyValue.ValueValidationResult result2 = CommitChanges(final: false);
					if (!PropertyValue.IsErrorCode(result2) || _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
					{
						OnF4();
					}
					else
					{
						_edit.SelectAll();
					}
				}
				else
				{
					OnF4();
				}
				return true;
			}
			if (keys == Keys.Tab)
			{
				PropertyValue.ValueValidationResult result3 = CommitChanges(Focused);
				if (PropertyValue.IsErrorCode(result3) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
				{
					if (_edit != null)
					{
						_edit.SelectAll();
					}
					return true;
				}
				if (_edit != null)
				{
					bool flag = (keyData & Keys.Shift) == 0;
					if (flag && _edit.Focused)
					{
						if (_ownerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys || (_ownerPropertyEnum.Property.ParentGrid.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyInSubControls) != PropertyGrid.TabKeyNavigationModes.NoSubMode)
						{
							OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: false);
							_dontTransferFocus = true;
							Focus();
							Invalidate(invalidateChildren: false);
							_dontTransferFocus = false;
							return true;
						}
					}
					else if (!flag && _edit.Focused)
					{
						if (_ownerPropertyEnum.Property.ParentGrid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys)
						{
							base.Parent.Focus();
							return true;
						}
					}
					else if (!flag && Focused)
					{
						_edit.Focus();
						_edit.SelectAll();
						Invalidate(invalidateChildren: false);
						return true;
					}
				}
			}
		}
		if (base.ContainsFocus)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == 256)
		{
			Keys keys = (Keys)(int)m.WParam;
			if (keys == Keys.Down || keys == Keys.Up)
			{
				ProcessArrowKey(keys, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
				return true;
			}
		}
		return base.ProcessKeyPreview(ref m);
	}

	private void SetDropDownContainer(DropDownForm container)
	{
		_editorContainer = container;
	}

	protected void RunButton()
	{
		if (ReadOnly)
		{
			return;
		}
		Property property = _ownerPropertyEnum.Property;
		PropertyValue value = property.Value;
		PropertyTypeDescriptorContext typeDescriptorContext = value.GetTypeDescriptorContext(_ownerPropertyEnum);
		UITypeEditorEditStyle editStyle = _editor.GetEditStyle(typeDescriptorContext);
		try
		{
			if (editStyle == UITypeEditorEditStyle.DropDown)
			{
				property.InPlaceCtrlInAction = this;
			}
			object value2;
			if (value.HasMultipleValues)
			{
				value2 = (value.UnderlyingType.IsArray ? new object[0] : ((!typeof(IList).IsAssignableFrom(value.UnderlyingType)) ? null : TypeDescriptor.CreateInstance(null, value.UnderlyingType, null, null)));
			}
			else if (!RealtimeChange && value.TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)))
			{
				try
				{
					value2 = value.ConvertDisplayedStringToValue(Text);
				}
				catch (Exception)
				{
					value2 = value.GetValue();
				}
			}
			else
			{
				value2 = value.GetValue();
			}
			PropertyEnumerator ownerPropertyEnumerator = OwnerPropertyEnumerator;
			object newValue = _editor.EditValue(typeDescriptorContext, _editorService, value2);
			if (_ownerPropertyEnum == ownerPropertyEnumerator)
			{
				PropertyValue.DontConvertStringValues = true;
				CommitChanges(newValue, fromEditor: true);
				PropertyValue.DontConvertStringValues = false;
			}
		}
		finally
		{
			_editorService.DialogResult = DialogResult.Cancel;
			base.Parent.Invalidate();
			if (editStyle == UITypeEditorEditStyle.Modal && !base.Parent.Focused)
			{
				base.Parent.Focus();
			}
		}
		if (editStyle == UITypeEditorEditStyle.DropDown)
		{
			property.InPlaceCtrlInAction = null;
		}
		property.ParentGrid.OnInPlaceCtrlFinishedEdition();
	}

	public void OnF4()
	{
		if (!ReadOnly)
		{
			_dontTransferFocus = true;
		}
		Focus();
		_dontTransferFocus = false;
		RunButton();
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		if (_edit != null && !_edit.ReadOnly && _edit.OnParentKeyDown(e))
		{
			return true;
		}
		if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
		{
			string[] displayedValues = OwnerPropertyEnumerator.Property.Value.GetDisplayedValues();
			string[] array = displayedValues;
			foreach (string text in array)
			{
				if (text.Length == 0)
				{
					if (_edit != null)
					{
						_edit.Clear();
						_edit.Focus();
					}
					else
					{
						Text = "";
						Focus();
					}
					if (RealtimeChange)
					{
						CommitChanges(final: false);
					}
					return true;
				}
			}
		}
		return false;
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (e.Delta > 0)
		{
			ProcessArrowKey(Keys.Up, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
		}
		else
		{
			ProcessArrowKey(Keys.Down, ScrollValueOnDoubleClickSource.FromInPlaceCtrl, rotate: false);
		}
		Invalidate();
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		if (forward)
		{
			if (_edit == null || ReadOnly)
			{
				return true;
			}
			return Focused;
		}
		if (_edit == null)
		{
			return true;
		}
		return _edit.Focused;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		return CommitChanges(Text, fromEditor: false);
	}

	protected PropertyValue.ValueValidationResult CommitChanges(object newValue, bool fromEditor)
	{
		PropertyValue.ValueValidationResult valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		try
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, newValue, !fromEditor));
			PropertyTypeDescriptorContext typeDescriptorContext = _ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum);
			bool flag = _ownerPropertyEnum.Property.Value.TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)) && newValue is string;
			bool flag2 = false;
			object obj = null;
			bool hasMultipleValues = _ownerPropertyEnum.Property.Value.HasMultipleValues;
			if (flag)
			{
				obj = _ownerPropertyEnum.Property.Value.DisplayString;
				flag2 = !_ownerPropertyEnum.Property.Value.DisplayString.Equals(newValue as string);
			}
			PropertyGrid parentGrid = OwnerPropertyEnumerator.Property.ParentGrid;
			if (!flag || flag2)
			{
				if (!hasMultipleValues)
				{
					obj = _ownerPropertyEnum.Property.Value.GetValue();
				}
				if (!fromEditor)
				{
					try
					{
						newValue = ((hasMultipleValues && newValue is string && (newValue as string).Length == 0) ? null : _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(newValue as string));
					}
					catch (Exception e)
					{
						_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
						_currentValueValidationResult = PropertyValue.ValueValidationResult.TypeConverterFailed;
						parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, Text, PropertyValue.ValueValidationResult.TypeConverterFailed, e));
						if (parentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
						{
							Text = _ownerPropertyEnum.Property.Value.DisplayString;
							_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
							parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
						}
						return _currentValueValidationResult;
					}
				}
			}
			if ((hasMultipleValues && flag2) || (newValue != null && (_editorService.DialogResult == DialogResult.OK || obj == null || !newValue.Equals(obj))) || (newValue == null && obj != null))
			{
				parentGrid.BeginUpdate();
				_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
				valueValidationResult = _ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(newValue, out var invalidPropEnum, null);
				if (PropertyValue.IsErrorCode(valueValidationResult))
				{
					_currentInvalidPropertyEnum = invalidPropEnum;
					if (valueValidationResult == PropertyValue.ValueValidationResult.ValidatorFailed)
					{
						parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, newValue, PropertyValue.ValueValidationResult.ValidatorFailed));
					}
					else
					{
						parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, newValue, valueValidationResult, _ownerPropertyEnum.Property.Value.LastExceptionByClient));
					}
				}
				if (parentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore || valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
				}
				if (parentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore && PropertyValue.IsErrorCode(valueValidationResult))
				{
					valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
				}
				if (valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					if (PropertyValue.IsErrorCode(_currentValueValidationResult))
					{
						parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
					}
					parentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum));
				}
				_currentValueValidationResult = valueValidationResult;
				parentGrid.EndUpdate();
			}
			else
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
				if (PropertyValue.IsErrorCode(_currentValueValidationResult))
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					parentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
				}
			}
		}
		finally
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyEndCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, newValue, valueIsText: false));
		}
		return valueValidationResult;
	}

	public Control GetDropDownContainer()
	{
		return _editorContainer;
	}
}
