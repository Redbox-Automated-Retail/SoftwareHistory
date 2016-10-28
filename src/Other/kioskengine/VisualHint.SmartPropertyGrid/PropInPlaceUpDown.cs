using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class PropInPlaceUpDown : Control, IInPlaceControl
{
	private ButtonID _captured;

	private ButtonID _mouseOver;

	private ButtonID _pushed;

	private Timer _timer;

	private int _timerInterval;

	private Control _wmFocusMissingArg;

	private PropInPlaceTextbox _edit;

	private PropertyValue.ValueValidationResult _currentValueValidationResult;

	private PropertyEnumerator _currentInvalidPropertyEnum;

	private PropertyEnumerator _ownerPropertyEnum;

	private decimal _increment = 1.0m;

	private bool _realtimeChange = true;

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
			UpDownSettingsAttribute upDownSettingsAttribute = (UpDownSettingsAttribute)_ownerPropertyEnum.Property.Value.GetAttribute(typeof(UpDownSettingsAttribute));
			if (upDownSettingsAttribute == null && _ownerPropertyEnum.Parent.Property != null && _ownerPropertyEnum.Parent.Property.Value != null)
			{
				ArrayList attributes = _ownerPropertyEnum.Parent.Property.Value.GetAttributes(typeof(UpDownSettingsAttribute));
				foreach (UpDownSettingsAttribute item in attributes)
				{
					if (item.ChildPropertyName == _ownerPropertyEnum.Property.Name)
					{
						upDownSettingsAttribute = item;
						break;
					}
				}
			}
			if (upDownSettingsAttribute != null)
			{
				Increment = upDownSettingsAttribute.Increment;
			}
			if (!_ownerPropertyEnum.Property.ParentGrid.IsInsideRefreshProperties)
			{
				if (!base.Visible)
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
				}
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
			}
		}
	}

	public bool ReadOnly => _ownerPropertyEnum.Property.Value.IsReadOnly(_ownerPropertyEnum);

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
			if (_edit == null)
			{
				base.Text = value;
				Invalidate();
			}
			else
			{
				_edit.Text = value;
			}
		}
	}

	public decimal Increment
	{
		get
		{
			return _increment;
		}
		set
		{
			_increment = value;
		}
	}

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

	private bool ShowIcon
	{
		get
		{
			PropertyValue value = _ownerPropertyEnum.Property.Value;
			ImageList imageList = null;
			if (value.ImageSource == PropertyGrid.ImageSources.Value)
			{
				imageList = value.ImageList;
			}
			else if (value.ImageSource == PropertyGrid.ImageSources.Grid)
			{
				imageList = value.Grid.ImageList;
			}
			if (imageList != null)
			{
				return _ownerPropertyEnum.Property.Value.ImageIndex != -1;
			}
			return false;
		}
	}

	public TextBox TextBox => _edit;

	public PropInPlaceUpDown(bool editable)
	{
		if (editable)
		{
			_edit = new PropInPlaceTextbox(isPassword: false);
			_edit.Parent = this;
			_edit.TabStop = false;
		}
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque | ControlStyles.ResizeRedraw, value: true);
		Cursor = Cursors.Arrow;
	}

	protected override void OnMove(EventArgs e)
	{
		base.OnMove(e);
		OnSizeChanged(e);
	}

	public virtual void RepositionChildren()
	{
		if (_edit == null)
		{
			return;
		}
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width <= 0)
		{
			return;
		}
		int doubleword = Win32Calls.LoWord(Win32Calls.SendMessage(_edit.Handle, 212, IntPtr.Zero, IntPtr.Zero));
		int num = Win32Calls.LoWord(doubleword) + Win32Calls.HiWord(doubleword);
		int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
		Rectangle clientRectangle2 = base.ClientRectangle;
		clientRectangle2.Width -= verticalScrollBarWidth + 1;
		clientRectangle2.Height++;
		Graphics graphics = CreateGraphics();
		clientRectangle2 = _ownerPropertyEnum.Property.Value.GetStringValueRect(graphics, clientRectangle2, Point.Empty);
		clientRectangle2.Height--;
		int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
		int num2 = clientRectangle2.Left - globalTextMargin + ((_ownerPropertyEnum.Property.Value.EditboxLeftMargin != -1) ? (globalTextMargin - _ownerPropertyEnum.Property.Value.EditboxLeftMargin) : 0);
		Rectangle rect = new Rectangle(num2, 0, clientRectangle.Width - verticalScrollBarWidth - num2, clientRectangle2.Height);
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
			int num3 = rect.Height - tm.tmHeight;
			rect.Y += num3 / 2;
			rect.Height -= num3 / 2;
			rect = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(rect, base.ClientSize.Width);
			_edit.Invalidate();
			_edit.Bounds = rect;
		}
		graphics.Dispose();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		RepositionChildren();
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (_edit != null && !Focused)
		{
			Win32Calls.PostMessage(_edit.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
		}
		else
		{
			if (_edit != null)
			{
				return;
			}
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			char[] value = new char[1] { e.KeyChar };
			string strB = new string(value);
			for (int i = 0; i < displayedValues.Length; i++)
			{
				if (!Text.Equals(displayedValues[i]))
				{
					continue;
				}
				for (int j = 0; j < displayedValues.Length; j++)
				{
					string text = displayedValues[(i + j + 1) % displayedValues.Length];
					if (text.Length > 0 && string.Compare(text.Substring(0, 1), strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
					{
						Text = text;
						if (RealtimeChange)
						{
							CommitChanges(final: false);
						}
						break;
					}
				}
				break;
			}
		}
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if (_edit != null)
		{
			if (_wmFocusMissingArg != _edit)
			{
				_edit.Focus();
			}
			else
			{
				base.Parent.Focus();
			}
		}
		else
		{
			Invalidate();
		}
		base.OnGotFocus(e);
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

	protected override void OnPaint(PaintEventArgs e)
	{
		PropertyGrid parentGrid = _ownerPropertyEnum.Property.ParentGrid;
		PropertyValue value = _ownerPropertyEnum.Property.Value;
		parentGrid.DrawManager.DrawPropertyValueBackground(e.Graphics, base.ClientRectangle, _ownerPropertyEnum, PropertyValue.DrawValueIn.InPlaceCtrl);
		int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
		if (!ReadOnly)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.X = base.ClientSize.Width - verticalScrollBarWidth;
			clientRectangle.Width = verticalScrollBarWidth;
			clientRectangle = OwnerPropertyEnumerator.Property.ParentGrid.TransformRectIfRTL(clientRectangle, base.ClientSize.Width);
			parentGrid.DrawManager.DrawUpDownButton(e.Graphics, this, clientRectangle, _pushed, _mouseOver);
		}
		Rectangle clientRectangle2 = base.ClientRectangle;
		if (!ReadOnly)
		{
			clientRectangle2.Width -= SystemInformation.VerticalScrollBarWidth + 1;
		}
		if (_edit == null && Focused)
		{
			Rectangle stringValueRect = value.GetStringValueRect(e.Graphics, clientRectangle2, Point.Empty);
			int globalTextMargin = _ownerPropertyEnum.Property.ParentGrid.GlobalTextMargin;
			stringValueRect.X -= (ShowIcon ? (globalTextMargin / 2) : globalTextMargin) - 1;
			stringValueRect.Width = clientRectangle2.Right - 1 - stringValueRect.Left;
			stringValueRect.Y++;
			stringValueRect.Height -= 2;
			stringValueRect = parentGrid.TransformRectIfRTL(stringValueRect, base.ClientSize.Width);
			using (Brush brush = new SolidBrush(parentGrid.SelectedBackColor))
			{
				e.Graphics.FillRectangle(brush, stringValueRect);
			}
			ControlPaint.DrawFocusRectangle(e.Graphics, stringValueRect);
		}
		Color textColor = ((!_ownerPropertyEnum.Property.Enabled) ? SystemColors.GrayText : ((!Focused) ? _ownerPropertyEnum.Property.Value.ForeColor : parentGrid.HighlightedTextColor));
		if (value.HasMultipleTexts() && Text.Length == 0)
		{
			value.DrawValue(e.Graphics, clientRectangle2, textColor, _ownerPropertyEnum, "", multipleTexts: true, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		else
		{
			value.DrawValue(e.Graphics, clientRectangle2, textColor, _ownerPropertyEnum, Text, multipleTexts: false, PropertyValue.DrawValueIn.InPlaceCtrl);
		}
		base.OnPaint(e);
	}

	public bool OnForbidClickThrough(Point pt)
	{
		return pt.X > base.ClientRectangle.Right - SystemInformation.VerticalScrollBarWidth;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (ReadOnly)
		{
			return;
		}
		if (_ownerPropertyEnum.Property.ParentGrid.TransformPointIfRTL(new Point(e.X, e.Y), base.ClientSize.Width).X > base.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth)
		{
			if (e.Button == MouseButtons.Left)
			{
				BeginButtonPress(e);
			}
			base.OnMouseDown(e);
			if (_edit != null)
			{
				_edit.Focus();
				_edit.SelectAll();
			}
		}
		else
		{
			if (e.Clicks == 2)
			{
				ScrollValueOnDoubleClick(Control.ModifierKeys != Keys.Shift, ScrollValueOnDoubleClickSource.FromInPlaceCtrl);
			}
			base.OnMouseDown(e);
		}
		if (_edit == null)
		{
			Focus();
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		_mouseOver = ButtonID.None;
		Invalidate();
		base.OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (base.Capture)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Height /= 2;
			if (_captured == ButtonID.Down)
			{
				clientRectangle.Y += clientRectangle.Height;
			}
			if (_ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(clientRectangle, base.ClientSize.Width).Contains(e.X, e.Y))
			{
				if (_pushed != _captured)
				{
					StartTimer();
					_pushed = _captured;
					Invalidate();
				}
			}
			else if (_pushed != ButtonID.None)
			{
				StopTimer();
				_pushed = ButtonID.None;
				Invalidate();
			}
		}
		int num = (int)Math.Round((double)base.ClientSize.Height / 2.0);
		int verticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
		int num2 = base.ClientSize.Width - verticalScrollBarWidth;
		Rectangle rectangle = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(new Rectangle(num2, 0, verticalScrollBarWidth, num), base.ClientSize.Width);
		Rectangle rectangle2 = _ownerPropertyEnum.Property.ParentGrid.TransformRectIfRTL(new Rectangle(num2, num, verticalScrollBarWidth, num), base.ClientSize.Width);
		if (rectangle.Contains(e.X, e.Y))
		{
			_mouseOver = ButtonID.Up;
			Invalidate();
		}
		else if (rectangle2.Contains(e.X, e.Y))
		{
			_mouseOver = ButtonID.Down;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_ownerPropertyEnum.Property.ParentGrid.TransformPointIfRTL(new Point(e.X, e.Y), base.ClientSize.Width).X > base.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth && e.Button == MouseButtons.Left)
		{
			EndButtonPress();
		}
		base.OnMouseUp(e);
	}

	private void BeginButtonPress(MouseEventArgs e)
	{
		if (e.Y < base.Size.Height / 2)
		{
			_captured = (_pushed = ButtonID.Up);
		}
		else
		{
			_captured = (_pushed = ButtonID.Down);
		}
		Invalidate();
		base.Capture = true;
		HandleUpDownButton(_pushed, rotate: false, ScrollValueOnDoubleClickSource.FromInPlaceCtrl);
		StartTimer();
	}

	private void EndButtonPress()
	{
		_pushed = ButtonID.None;
		_captured = ButtonID.None;
		StopTimer();
		base.Capture = false;
		Invalidate();
	}

	protected void StartTimer()
	{
		if (_timer == null)
		{
			_timer = new Timer();
			_timer.Tick += TimerHandler;
		}
		_timerInterval = 500;
		_timer.Interval = _timerInterval;
		_timer.Start();
	}

	protected void StopTimer()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer.Dispose();
			_timer = null;
		}
	}

	protected override void OnMouseCaptureChanged(EventArgs e)
	{
		if (!base.Capture)
		{
			EndButtonPress();
		}
	}

	private void TimerHandler(object source, EventArgs args)
	{
		if (!base.Capture)
		{
			return;
		}
		HandleUpDownButton(_pushed, rotate: false, ScrollValueOnDoubleClickSource.FromInPlaceCtrl);
		if (_timer != null)
		{
			_timerInterval *= 7;
			_timerInterval /= 10;
			if (_timerInterval < 1)
			{
				_timerInterval = 1;
			}
			_timer.Interval = _timerInterval;
		}
	}

	private bool HandleUpDownButton(ButtonID buttonId, bool rotate, ScrollValueOnDoubleClickSource source)
	{
		string text = Text;
		PropertyUpDownEventArgs e = new PropertyUpDownEventArgs(_ownerPropertyEnum, (buttonId != ButtonID.Up) ? PropertyUpDownEventArgs.UpDownButtons.Down : PropertyUpDownEventArgs.UpDownButtons.Up);
		e.Value = Text;
		_ = Text;
		_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyUpDown(e);
		bool flag = _ownerPropertyEnum.Property.Value.HasMultipleTexts();
		if (Text != e.Value)
		{
			text = e.Value;
		}
		else
		{
			string[] displayedValues = _ownerPropertyEnum.Property.Value.GetDisplayedValues();
			if (displayedValues.Length > 0)
			{
				text = Text;
				if (flag && text.Length == 0)
				{
					int num = -1;
					switch (buttonId)
					{
					case ButtonID.Down:
						num = displayedValues.Length - 1;
						break;
					case ButtonID.Up:
						num = 0;
						break;
					}
					if (num != -1)
					{
						text = displayedValues[num];
					}
				}
				else
				{
					for (int i = 0; i < displayedValues.Length; i++)
					{
						if (displayedValues[i] == text)
						{
							int num2 = -1;
							if (buttonId == ButtonID.Down && i > 0)
							{
								num2 = i - 1;
							}
							else if (buttonId == ButtonID.Down && rotate)
							{
								num2 = displayedValues.Length - 1;
							}
							else if (buttonId == ButtonID.Up && i < displayedValues.Length - 1)
							{
								num2 = i + 1;
							}
							else if (buttonId == ButtonID.Up && rotate)
							{
								num2 = 0;
							}
							if (num2 != -1)
							{
								text = displayedValues[num2];
							}
							break;
						}
					}
				}
			}
			else if ((object)_ownerPropertyEnum.Property.Value.UnderlyingType != typeof(string))
			{
				TypeConverter typeConverter = _ownerPropertyEnum.Property.Value.TypeConverter;
				PropertyTypeDescriptorContext typeDescriptorContext = _ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum);
				object obj;
				decimal num3;
				try
				{
					if (flag)
					{
						DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)_ownerPropertyEnum.Property.Value.GetAttribute(typeof(DefaultValueAttribute));
						obj = ((defaultValueAttribute == null) ? ((object)0) : defaultValueAttribute.Value);
						num3 = Convert.ToDecimal(obj);
					}
					else
					{
						obj = typeConverter.ConvertFromString(typeDescriptorContext, _ownerPropertyEnum.Property.Value.CultureInfo, Text);
						num3 = Convert.ToDecimal(obj);
					}
				}
				catch (Exception e2)
				{
					_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
					_currentValueValidationResult = PropertyValue.ValueValidationResult.TypeConverterFailed;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, Text, PropertyValue.ValueValidationResult.TypeConverterFailed, e2));
					return false;
				}
				switch (buttonId)
				{
				case ButtonID.Up:
					try
					{
						num3 += Increment;
					}
					catch (OverflowException)
					{
					}
					break;
				case ButtonID.Down:
					try
					{
						num3 -= Increment;
					}
					catch (OverflowException)
					{
					}
					break;
				}
				CultureInfo cultureInfo = _ownerPropertyEnum.Property.Value.CultureInfo;
				try
				{
					text = typeConverter.ConvertToString(typeDescriptorContext, cultureInfo, Convert.ChangeType(num3, obj.GetType(), cultureInfo));
				}
				catch (OverflowException)
				{
				}
				if (text == null)
				{
					text = "";
				}
			}
		}
		if (Text != text)
		{
			if (!flag)
			{
				_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
			}
			PropertyValidatorBase validator = _ownerPropertyEnum.Property.Value.Validator;
			if (_edit != null && (!flag || Text.Length > 0))
			{
				try
				{
					if (validator != null && !validator.Check(_ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(Text), modify: false))
					{
						if (_edit != null)
						{
							_edit.SelectAll();
						}
						_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
						_currentValueValidationResult = PropertyValue.ValueValidationResult.ValidatorFailed;
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, OwnerPropertyEnumerator, Text, _currentValueValidationResult));
						return false;
					}
				}
				catch (Exception e3)
				{
					if (_edit != null)
					{
						_edit.SelectAll();
					}
					_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
					_currentValueValidationResult = PropertyValue.ValueValidationResult.TypeConverterFailed;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, Text, PropertyValue.ValueValidationResult.TypeConverterFailed, e3));
					return false;
				}
			}
			if (validator != null && !validator.Check(_ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(text), modify: false))
			{
				return false;
			}
			if (_realtimeChange)
			{
				CommitChanges(text, final: false);
			}
			else
			{
				Text = text;
			}
		}
		if (source != ScrollValueOnDoubleClickSource.FromLabel)
		{
			OwnerPropertyEnumerator.Property.ParentGrid.SelectAndFocusProperty(OwnerPropertyEnumerator, selectAllText: true);
		}
		return true;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (!ReadOnly)
		{
			if (m.Msg == 256)
			{
				if ((int)m.WParam == 38 || (int)m.WParam == 40)
				{
					if ((int)m.WParam == 38)
					{
						ProcessArrowKey(ButtonID.Up);
					}
					else if ((int)m.WParam == 40)
					{
						ProcessArrowKey(ButtonID.Down);
					}
					return true;
				}
			}
			else if (m.Msg == 257 && _pushed != ButtonID.None)
			{
				EndProcessArrowKey();
			}
		}
		return base.ProcessKeyPreview(ref m);
	}

	private void ProcessArrowKey(ButtonID buttonId)
	{
		_pushed = buttonId;
		Invalidate();
		HandleUpDownButton(_pushed, rotate: false, ScrollValueOnDoubleClickSource.FromTextBox);
		StartTimer();
	}

	private void EndProcessArrowKey()
	{
		_pushed = ButtonID.None;
		StopTimer();
		Invalidate();
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Return:
		{
			PropertyValue.ValueValidationResult result2 = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result2) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus && _edit != null)
			{
				_edit.SelectAll();
				return true;
			}
			break;
		}
		case Keys.Tab:
		{
			PropertyValue.ValueValidationResult result = CommitChanges(final: true);
			if (PropertyValue.IsErrorCode(result) && _ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
			{
				if (_edit != null)
				{
					_edit.SelectAll();
				}
				return true;
			}
			break;
		}
		case Keys.Escape:
			Text = _ownerPropertyEnum.Property.Value.DisplayString;
			CommitChanges(final: true);
			break;
		}
		if (base.ContainsFocus)
		{
			return base.ProcessDialogKey(keyData);
		}
		return true;
	}

	protected override bool IsInputKey(Keys keyData)
	{
		if (_edit == null && (keyData == Keys.Up || keyData == Keys.Down))
		{
			return true;
		}
		return base.IsInputKey(keyData);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (_edit == null)
		{
			if (e.KeyData == Keys.Up)
			{
				ProcessArrowKey(ButtonID.Up);
			}
			else if (e.KeyData == Keys.Down)
			{
				ProcessArrowKey(ButtonID.Down);
			}
		}
		base.OnKeyDown(e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (_edit == null && (e.KeyData == Keys.Up || e.KeyData == Keys.Down))
		{
			EndProcessArrowKey();
		}
		base.OnKeyUp(e);
	}

	public void OnF4()
	{
		Focus();
	}

	public bool OnParentKeyDown(KeyEventArgs e)
	{
		if (_edit != null && _edit.OnParentKeyDown(e))
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
			ProcessArrowKey(ButtonID.Up);
		}
		else
		{
			ProcessArrowKey(ButtonID.Down);
		}
		EndProcessArrowKey();
		Invalidate();
	}

	public virtual bool GoToNextPropertyOnTab(bool forward)
	{
		return true;
	}

	public virtual bool ScrollValueOnDoubleClick(bool forward, ScrollValueOnDoubleClickSource source)
	{
		if (ReadOnly)
		{
			return false;
		}
		bool realtimeChange = _realtimeChange;
		_realtimeChange = true;
		bool result = HandleUpDownButton(forward ? ButtonID.Up : ButtonID.Down, rotate: true, source);
		_realtimeChange = realtimeChange;
		return result;
	}

	public PropertyValue.ValueValidationResult CommitChanges(string strToValidate, bool final)
	{
		PropertyValue.ValueValidationResult valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
		object obj = null;
		try
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyBeginCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, strToValidate, valueIsText: true));
			PropertyTypeDescriptorContext typeDescriptorContext = _ownerPropertyEnum.Property.Value.GetTypeDescriptorContext(_ownerPropertyEnum);
			bool flag = _ownerPropertyEnum.Property.Value.TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string));
			bool flag2 = false;
			if (flag)
			{
				flag2 = !_ownerPropertyEnum.Property.Value.DisplayString.Equals(strToValidate);
			}
			object obj2 = null;
			bool hasMultipleValues = _ownerPropertyEnum.Property.Value.HasMultipleValues;
			if (!flag || flag2)
			{
				if (!hasMultipleValues)
				{
					obj2 = _ownerPropertyEnum.Property.Value.GetValue();
				}
				try
				{
					if (!hasMultipleValues || strToValidate.Length != 0)
					{
						obj = _ownerPropertyEnum.Property.Value.ConvertDisplayedStringToValue(strToValidate);
					}
				}
				catch (Exception e)
				{
					_currentInvalidPropertyEnum = OwnerPropertyEnumerator;
					_currentValueValidationResult = PropertyValue.ValueValidationResult.TypeConverterFailed;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, Text, PropertyValue.ValueValidationResult.TypeConverterFailed, e));
					if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore)
					{
						Text = _ownerPropertyEnum.Property.Value.DisplayString;
						_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
					}
					return _currentValueValidationResult;
				}
			}
			if ((hasMultipleValues && flag2) || (obj != null && (obj2 == null || !obj.Equals(obj2))) || (obj == null && obj2 != null))
			{
				_ownerPropertyEnum.Property.ParentGrid.BeginUpdate();
				if (!hasMultipleValues)
				{
					_ownerPropertyEnum.Property.Value.PreviousValue = _ownerPropertyEnum.Property.Value.GetValue();
				}
				valueValidationResult = _ownerPropertyEnum.Property.Value.SetValueFromInPlaceCtrl(obj, out var invalidPropEnum, null);
				if (PropertyValue.IsErrorCode(valueValidationResult))
				{
					_currentInvalidPropertyEnum = invalidPropEnum;
					if (valueValidationResult == PropertyValue.ValueValidationResult.ValidatorFailed)
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, obj, PropertyValue.ValueValidationResult.ValidatorFailed));
					}
					else
					{
						OwnerPropertyEnumerator.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, obj, valueValidationResult, _ownerPropertyEnum.Property.Value.LastExceptionByClient));
					}
				}
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore || valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					Text = _ownerPropertyEnum.Property.Value.DisplayString;
				}
				if (_ownerPropertyEnum.Property.ParentGrid.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.IgnoreAndRestore && PropertyValue.IsErrorCode(valueValidationResult))
				{
					valueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
				}
				if (valueValidationResult == PropertyValue.ValueValidationResult.Validated)
				{
					if (PropertyValue.IsErrorCode(_currentValueValidationResult))
					{
						_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), valueValidationResult));
					}
					_ownerPropertyEnum.Property.ParentGrid.NotifyPropertyChanged(new PropertyChangedEventArgs(_ownerPropertyEnum));
				}
				_currentValueValidationResult = valueValidationResult;
				_ownerPropertyEnum.Property.ParentGrid.EndUpdate();
			}
			else
			{
				Text = _ownerPropertyEnum.Property.Value.DisplayString;
				if (PropertyValue.IsErrorCode(_currentValueValidationResult))
				{
					_currentValueValidationResult = PropertyValue.ValueValidationResult.PreviousValueRestored;
					_ownerPropertyEnum.Property.ParentGrid.NotifyValueValidation(new ValueValidationEventArgs(OwnerPropertyEnumerator, _currentInvalidPropertyEnum, _ownerPropertyEnum.Property.Value.GetValue(), _currentValueValidationResult));
				}
			}
		}
		finally
		{
			_ownerPropertyEnum.Property.ParentGrid.NotifyEndCommitChanges(new CommitChangesEventArgs(_ownerPropertyEnum, obj, valueIsText: false));
		}
		return valueValidationResult;
	}

	public virtual PropertyValue.ValueValidationResult CommitChanges(bool final)
	{
		return CommitChanges(Text, final);
	}

	public Control GetDropDownContainer()
	{
		return null;
	}
}
