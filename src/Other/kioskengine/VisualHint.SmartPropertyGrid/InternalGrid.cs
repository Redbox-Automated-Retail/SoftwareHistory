using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
internal class InternalGrid : Control, IMouseHookClient, IMessageHookClient, IKeyboardHookClient
{
	public class DisplayNameSortComparer : IComparer
	{
		public int Compare(object left, object right)
		{
			return string.Compare(((Property)left).DisplayName, ((Property)right).DisplayName, ignoreCase: true, CultureInfo.CurrentCulture);
		}
	}

	private const int MinimumLabelColumnWidth = 20;

	private bool _validationWhileAppLoosingFocus;

	private int _beginVerticalFreezeCounter;

	private MouseHook _mouseHook;

	private MessageHook _messageHook;

	private KeyboardHook _keyboardHook;

	private Keys _lastLeaveKey;

	private Control _focusedControlAfterKillFocus;

	private Control _dropDownTopWindow;

	private long _lastTimeClickedFromParent;

	private long _lastTimeClicked;

	private Point _lastPosClickedInParent;

	private Point _lastPosClicked;

	private bool _gridOrCurrentInPlaceCtrlClickedDuringDropDown;

	private bool _doubleClickDone;

	private bool _columnsLock;

	private bool _displayModeChangeInProgress;

	private bool _dontShowInPlaceCtrlInGotFocus;

	private int _propertyVerticalMargin = 4;

	private Point _toolTipLocation = Point.Empty;

	private bool _lockRefresh;

	private bool _visibleItemCountValid;

	private bool _clickWithControl;

	private PropertyGrid _parentCtrl;

	private PropertyTree _categorizedPropertyCollection = new PropertyTree(0);

	private PropertyTree _flatPropertyCollection = new PropertyTree(1);

	private PropertyTree _activePropertyCollection;

	private int _firstDisplayedLine = 1;

	private int _basicPropertyHeight = -1;

	private bool _resizingColumns;

	private int _visibleItemCount;

	private int _visibleLineCount;

	private ArrayList _multiSelectedItems = new ArrayList();

	private Color _disabledForeColor = SystemColors.GrayText;

	private Color _readOnlyForeColor = SystemColors.GrayText;

	private Color _propertyLabelBackColor = Color.Empty;

	private Color _propertyValueBackColor = Color.Empty;

	private double _labelColumnWidthRatio = 0.5;

	private PropertyGrid.DisableModes _disableMode = PropertyGrid.DisableModes.Simple;

	private bool _disableModeGrayedOut = true;

	private bool _enabled = true;

	private ImageList _imageList;

	private ToolTipCtrl _tooltipCtrl;

	private PropertyGrid.ToolTipModes _toolTipMode;

	private PropertyGrid.ReadOnlyVisuals _readOnlyVisual = PropertyGrid.ReadOnlyVisuals.Disabled;

	private CustomDrawManager _drawManager;

	private PropertyEnumerator _propEnumToConvertDuringDisplayMode;

	private PropertyGrid.DisplayModes _displayMode;

	private PropertyGrid.MultiSelectModes _multiSelectMode;

	private PropertyVisibleDeepEnumerator _selectedPropertyEnum;

	private Control _currentInPlaceControl;

	private PropertyVisibleDeepEnumerator _firstDisplayedPropertyEnumerator;

	private int _leftColumnWidth;

	private Color _refColor;

	private Color _selectedBkgColor = SystemColors.Highlight;

	private Color _selectedNotFocusedBkgColor = Color.Empty;

	private Color _highlightedTextColor = SystemColors.HighlightText;

	private bool _hideRemainingCategory;

	private bool _scrollingInProgress;

	private object[] _currentTargetInstancesToAdd;

	private bool _hideInPlaceControlInProgress;

	private int _expandAllMaxLevels = 10;

	private int _scrollPosBeforeVerticalFreeze = -1;

	private bool _initializing;

	internal bool ValidationWhileAppLoosingFocus => _validationWhileAppLoosingFocus;

	public int PropertyVerticalMargin
	{
		get
		{
			return _propertyVerticalMargin;
		}
		set
		{
			_propertyVerticalMargin = value;
			RecalcPropertyItemHeight();
		}
	}

	public int LabelColumnWidth
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int num = (int)Math.Round(_labelColumnWidthRatio * (double)(clientRectangle.Width - LeftColumnWidth));
			if (num < 20)
			{
				num = 20;
			}
			else if (LeftColumnWidth + num + 20 > clientRectangle.Width)
			{
				num = clientRectangle.Width - LeftColumnWidth - 20;
			}
			return num;
		}
	}

	public int ValueColumnWidth => base.ClientRectangle.Width - LeftColumnWidth - LabelColumnWidth;

	public double LabelColumnWidthRatio
	{
		get
		{
			return _labelColumnWidthRatio;
		}
		set
		{
			if (value < 0.0)
			{
				value = 0.0;
			}
			else if (value > 1.0)
			{
				value = 1.0;
			}
			int labelColumnWidth = LabelColumnWidth;
			_labelColumnWidthRatio = value;
			Rectangle clientRectangle = base.ClientRectangle;
			if (labelColumnWidth > LabelColumnWidth)
			{
				Invalidate(_parentCtrl.TransformRectIfRTL(new Rectangle(LeftColumnWidth + 1, 0, clientRectangle.Width - LeftColumnWidth, clientRectangle.Height)), invalidateChildren: false);
				Update();
			}
			if (_currentInPlaceControl != null)
			{
				Property property = SelectedPropertyEnumerator.Property;
				Rectangle itemRect = GetItemRect(SelectedPropertyEnumerator);
				itemRect = property.GetValueRect(itemRect);
				PropertyFeel feel = property.Feel;
				feel.MoveControl(itemRect, SelectedPropertyEnumerator);
			}
			if (labelColumnWidth < LabelColumnWidth)
			{
				Invalidate(_parentCtrl.TransformRectIfRTL(new Rectangle(LeftColumnWidth + 1, 0, clientRectangle.Width - LeftColumnWidth, clientRectangle.Height)), invalidateChildren: false);
				Update();
			}
		}
	}

	public PropertyGrid.DisableModes DisableMode
	{
		get
		{
			return _disableMode;
		}
		set
		{
			if (_disableMode != value)
			{
				_disableMode = value;
				Invalidate();
				ProcessDisable();
			}
		}
	}

	public bool DisableModeGrayedOut
	{
		get
		{
			return _disableModeGrayedOut;
		}
		set
		{
			if (_disableModeGrayedOut != value)
			{
				_disableModeGrayedOut = value;
				Invalidate();
				ProcessDisable();
			}
		}
	}

	public new bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
			Invalidate();
			ProcessDisable();
		}
	}

	private bool CanSelectProperties
	{
		get
		{
			if (Enabled || DisableMode == PropertyGrid.DisableModes.None)
			{
				return true;
			}
			if (_disableMode == PropertyGrid.DisableModes.NoPropertySelection)
			{
				return false;
			}
			return true;
		}
	}

	public ImageList ImageList
	{
		get
		{
			return _imageList;
		}
		set
		{
			_imageList = value;
		}
	}

	public PropertyGrid.ToolTipModes ToolTipMode
	{
		get
		{
			return _toolTipMode;
		}
		set
		{
			_toolTipMode = value;
			if (_toolTipMode == PropertyGrid.ToolTipModes.None && _tooltipCtrl != null)
			{
				_tooltipCtrl.Dispose();
				_tooltipCtrl = null;
			}
		}
	}

	public PropertyGrid.ReadOnlyVisuals ReadOnlyVisual
	{
		get
		{
			return _readOnlyVisual;
		}
		set
		{
			_readOnlyVisual = value;
			PropertyEnumerator firstProperty = FirstProperty;
			while (firstProperty != RightBound)
			{
				PropertyValue value2 = firstProperty.Property.Value;
				if (value2 != null && value2.ReadOnly)
				{
					EnableSingleProperty(firstProperty, _readOnlyVisual == PropertyGrid.ReadOnlyVisuals.ReadOnlyFeel);
				}
				firstProperty.MoveNext();
			}
			Refresh();
		}
	}

	public CustomDrawManager DrawManager
	{
		get
		{
			return _drawManager;
		}
		set
		{
			_drawManager = value;
			_drawManager.Grid = _parentCtrl;
			if (base.IsHandleCreated)
			{
				Win32Calls.SetWindowPos(base.Handle, IntPtr.Zero, 0, 0, 0, 0, 55);
			}
			Refresh();
		}
	}

	public PropertyEnumerator PropEnumToConvertDuringDisplayMode
	{
		get
		{
			return _propEnumToConvertDuringDisplayMode;
		}
		set
		{
			_propEnumToConvertDuringDisplayMode = value;
		}
	}

	public PropertyGrid.DisplayModes DisplayMode
	{
		get
		{
			return _displayMode;
		}
		set
		{
			if (_displayMode == value)
			{
				return;
			}
			_parentCtrl.BeginUpdate();
			HideInPlaceControl();
			_displayMode = value;
			int num = -1;
			if (SelectedPropertyEnumerator != RightBound)
			{
				num = GetPropertyVisibleIndex(SelectedPropertyEnumerator);
			}
			if (_displayMode == PropertyGrid.DisplayModes.Categorized)
			{
				InvalidateVisibleItemCount();
				_activePropertyCollection = _categorizedPropertyCollection;
				PropertyEnumerator propertyEnumerator = _activePropertyCollection.GetDeepEnumerator().MoveFirst();
				while (propertyEnumerator != propertyEnumerator.RightBound)
				{
					Property property = propertyEnumerator.Property;
					if (property != null)
					{
						if (property.Value != null)
						{
							property.Value.OwnerEnumerator = propertyEnumerator;
						}
						if (property.Selected)
						{
							_selectedPropertyEnum = propertyEnumerator.GetVisibleDeepEnumerator();
						}
						if (_propEnumToConvertDuringDisplayMode != null && _propEnumToConvertDuringDisplayMode.Property == property)
						{
							_propEnumToConvertDuringDisplayMode = (PropertyEnumerator)propertyEnumerator.Clone();
						}
					}
					propertyEnumerator.MoveNext();
				}
				_firstDisplayedPropertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator();
				_firstDisplayedLine = 1;
				_firstDisplayedPropertyEnumerator.MoveFirst();
				_flatPropertyCollection.Clear();
			}
			else if (_displayMode == PropertyGrid.DisplayModes.Flat || _displayMode == PropertyGrid.DisplayModes.FlatSorted)
			{
				_displayModeChangeInProgress = true;
				Property property2 = null;
				if (_selectedPropertyEnum.Property != null)
				{
					property2 = _selectedPropertyEnum.Property;
					SelectProperty(_activePropertyCollection.GetVisibleDeepEnumerator().RightBound);
				}
				_flatPropertyCollection.Clear();
				_activePropertyCollection = _flatPropertyCollection;
				_firstDisplayedPropertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator().RightBound.GetVisibleDeepEnumerator();
				_selectedPropertyEnum = _firstDisplayedPropertyEnumerator;
				PropertyEnumerator propertyEnumerator2 = _categorizedPropertyCollection.GetDeepEnumerator().MoveFirst();
				PropertyEnumerator newSelectedEnum = null;
				while (propertyEnumerator2 != propertyEnumerator2.RightBound)
				{
					Property property3 = propertyEnumerator2.Property;
					if ((property3.Value != null || property3 is PropertyHyperLink) && propertyEnumerator2.Parent.Property.Value == null)
					{
						PropertyEnumerator propertyEnumerator3 = AddPropertyBefore(_flatPropertyCollection.GetDeepEnumerator().RightBound, property3);
						propertyEnumerator3.CategorizedPropertyEnumerator = propertyEnumerator2;
						propertyEnumerator2 = CloneHierarchyUnder(propertyEnumerator2, propertyEnumerator3, property2, ref newSelectedEnum);
						if (property3 == property2)
						{
							newSelectedEnum = propertyEnumerator3;
						}
						if (_propEnumToConvertDuringDisplayMode != null && _propEnumToConvertDuringDisplayMode.Property == property3)
						{
							_propEnumToConvertDuringDisplayMode = (PropertyEnumerator)propertyEnumerator3.Clone();
						}
					}
					propertyEnumerator2.MoveNext();
				}
				if (_displayMode == PropertyGrid.DisplayModes.FlatSorted)
				{
					_flatPropertyCollection.Sort(new DisplayNameSortComparer());
				}
				_firstDisplayedPropertyEnumerator = _flatPropertyCollection.GetVisibleDeepEnumerator();
				_firstDisplayedLine = 1;
				_firstDisplayedPropertyEnumerator.MoveFirst();
				if (newSelectedEnum != null)
				{
					SelectProperty(newSelectedEnum);
				}
				else
				{
					SelectProperty(_firstDisplayedPropertyEnumerator);
				}
				_displayModeChangeInProgress = false;
			}
			ResetScrollPosToFirstDisplayedProperty();
			if (num != -1)
			{
				MovePropertyToVisibleIndex(SelectedPropertyEnumerator, num, finalize: false);
			}
			else
			{
				EnsureVisible(SelectedPropertyEnumerator);
			}
			ProcessDisable();
			_parentCtrl.EndUpdate();
			Refresh();
			CheckScrollbar();
			_parentCtrl.OnDisplayModeChanged(EventArgs.Empty);
		}
	}

	public PropertyGrid.MultiSelectModes MultiSelectMode
	{
		get
		{
			return _multiSelectMode;
		}
		set
		{
			_multiSelectMode = value;
		}
	}

	public PropertyVisibleDeepEnumerator SelectedPropertyEnumerator => _selectedPropertyEnum;

	public Control InPlaceControl => _currentInPlaceControl;

	public PropertyVisibleDeepEnumerator FirstDisplayedProperty
	{
		get
		{
			return (PropertyVisibleDeepEnumerator)_firstDisplayedPropertyEnumerator.Clone();
		}
		set
		{
			_firstDisplayedPropertyEnumerator = new PropertyVisibleDeepEnumerator(value.Node);
			ResetScrollPosToFirstDisplayedProperty();
		}
	}

	public int LeftColumnWidth
	{
		get
		{
			return _leftColumnWidth;
		}
		set
		{
			_leftColumnWidth = value;
			Invalidate();
		}
	}

	public Color GridColor
	{
		get
		{
			return _refColor;
		}
		set
		{
			_refColor = value;
			Refresh();
		}
	}

	public Color SelectedBackColor
	{
		get
		{
			return _selectedBkgColor;
		}
		set
		{
			_selectedBkgColor = value;
			Refresh();
		}
	}

	public Color SelectedNotFocusedBackColor
	{
		get
		{
			if (_selectedNotFocusedBkgColor == Color.Empty)
			{
				return GridColor;
			}
			return _selectedNotFocusedBkgColor;
		}
		set
		{
			_selectedNotFocusedBkgColor = value;
			Refresh();
		}
	}

	public Color HighlightedTextColor
	{
		get
		{
			return _highlightedTextColor;
		}
		set
		{
			_highlightedTextColor = value;
			Refresh();
		}
	}

	public int BasicPropertyHeight => _basicPropertyHeight;

	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
			if (_currentInPlaceControl != null && _currentInPlaceControl.Visible)
			{
				_currentInPlaceControl.BackColor = value;
			}
			Refresh();
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
			if (_currentInPlaceControl != null && _currentInPlaceControl.Visible)
			{
				_currentInPlaceControl.ForeColor = value;
			}
			Refresh();
		}
	}

	public Color PropertyLabelBackColor
	{
		get
		{
			if (_propertyLabelBackColor.IsEmpty)
			{
				return BackColor;
			}
			return _propertyLabelBackColor;
		}
		set
		{
			_propertyLabelBackColor = value;
			Refresh();
		}
	}

	public Color PropertyValueBackColor
	{
		get
		{
			if (_propertyValueBackColor.IsEmpty)
			{
				return BackColor;
			}
			return _propertyValueBackColor;
		}
		set
		{
			_propertyValueBackColor = value;
			if (_currentInPlaceControl != null && _currentInPlaceControl.Visible)
			{
				_currentInPlaceControl.BackColor = value;
			}
			Refresh();
		}
	}

	internal bool IsSetPropertyValueBackColor => _propertyValueBackColor != Color.Empty;

	public Color DisabledForeColor
	{
		get
		{
			return _disabledForeColor;
		}
		set
		{
			_disabledForeColor = value;
			Refresh();
		}
	}

	public Color ReadOnlyForeColor
	{
		get
		{
			return _readOnlyForeColor;
		}
		set
		{
			_readOnlyForeColor = value;
			Refresh();
		}
	}

	public bool ColumnsLock
	{
		get
		{
			return _columnsLock;
		}
		set
		{
			_columnsLock = value;
		}
	}

	public bool HideRemainingCategory
	{
		get
		{
			return _hideRemainingCategory;
		}
		set
		{
			if (_hideRemainingCategory != value)
			{
				_hideRemainingCategory = value;
				_ = HasSingleRootCategory;
			}
		}
	}

	private bool HasSingleRootCategory
	{
		get
		{
			if (HideRemainingCategory)
			{
				return _categorizedPropertyCollection.GetSiblingEnumerator().Count == 1;
			}
			return false;
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
			if (Font != value)
			{
				base.Font = value;
			}
			bool flag = false;
			if (_currentInPlaceControl != null && _currentInPlaceControl.Visible)
			{
				flag = true;
				HideInPlaceControl();
			}
			if (ToolTip != null)
			{
				ToolTip.Font = Font;
			}
			RecalcPropertyItemHeight();
			using (TextBox textBox = new TextBox())
			{
				textBox.Font = Font;
				Win32Calls.SendMessage(textBox.Handle, 211, (IntPtr)3, (IntPtr)Win32Calls.MakeLong(65535, 65535));
				_parentCtrl.SetGlobalTextMargin(2 + Win32Calls.LoWord(Win32Calls.SendMessage(textBox.Handle, 212, IntPtr.Zero, IntPtr.Zero)));
				if (_parentCtrl.GlobalTextMargin < 5)
				{
					_parentCtrl.SetGlobalTextMargin(5);
				}
			}
			if (flag)
			{
				ShowInPlaceControl(SelectedPropertyEnumerator);
			}
			CheckScrollbar();
		}
	}

	public int ExpandAllMaxLevels
	{
		get
		{
			return _expandAllMaxLevels;
		}
		set
		{
			_expandAllMaxLevels = value;
		}
	}

	internal bool IsVerticallyFrozen => _beginVerticalFreezeCounter > 0;

	private ToolTipCtrl ToolTip
	{
		get
		{
			if (ToolTipMode == PropertyGrid.ToolTipModes.None)
			{
				return null;
			}
			if (_tooltipCtrl == null)
			{
				_tooltipCtrl = new ToolTipCtrl(this);
			}
			return _tooltipCtrl;
		}
	}

	public PropertyEnumerator RightBound => _activePropertyCollection.GetDeepEnumerator().RightBound;

	public PropertyEnumerator LeftBound => _activePropertyCollection.GetDeepEnumerator().LeftBound;

	public PropertyEnumerator FirstProperty
	{
		get
		{
			PropertyEnumerator deepEnumerator = _activePropertyCollection.GetDeepEnumerator();
			return deepEnumerator.MoveFirst();
		}
	}

	public PropertyEnumerator LastProperty
	{
		get
		{
			PropertyEnumerator deepEnumerator = _activePropertyCollection.GetDeepEnumerator();
			return deepEnumerator.MoveLast();
		}
	}

	internal void SetLabelColumnWidthRatio(double value)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		int num = (int)Math.Round(value * (double)(clientRectangle.Width - LeftColumnWidth));
		if (num < 20)
		{
			LabelColumnWidthRatio = (double)num / (double)(clientRectangle.Width - LeftColumnWidth);
		}
		else if (clientRectangle.Width - LeftColumnWidth - num < 20)
		{
			LabelColumnWidthRatio = (double)(clientRectangle.Width - LeftColumnWidth - 20) / (double)(clientRectangle.Width - LeftColumnWidth);
		}
		else
		{
			LabelColumnWidthRatio = value;
		}
	}

	private PropertyEnumerator CloneHierarchyUnder(PropertyEnumerator oldParentEnum, PropertyEnumerator newParentEnum, Property selectedProperty, ref PropertyEnumerator newSelectedEnum)
	{
		PropertyEnumerator siblingEnumerator = oldParentEnum.Children.GetSiblingEnumerator();
		PropertyEnumerator result = oldParentEnum;
		while (siblingEnumerator != siblingEnumerator.RightBound)
		{
			PropertyEnumerator propertyEnumerator = AddPropertyUnder(newParentEnum, siblingEnumerator.Property);
			propertyEnumerator.CategorizedPropertyEnumerator = siblingEnumerator;
			if (siblingEnumerator.Property == selectedProperty)
			{
				newSelectedEnum = propertyEnumerator;
			}
			result = CloneHierarchyUnder(siblingEnumerator, propertyEnumerator, selectedProperty, ref newSelectedEnum).GetDeepEnumerator();
			siblingEnumerator.MoveNext();
		}
		return result;
	}

	public InternalGrid(PropertyGrid parentCtrl)
	{
		_parentCtrl = parentCtrl;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque, value: true);
		UpdateStyles();
		LeftColumnWidth = 16;
		GridColor = SystemColors.ActiveBorder;
		_activePropertyCollection = _categorizedPropertyCollection;
		_firstDisplayedPropertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator().RightBound.GetVisibleDeepEnumerator();
		_selectedPropertyEnum = _activePropertyCollection.GetVisibleDeepEnumerator().RightBound.GetVisibleDeepEnumerator();
		base.TabStop = true;
		BackColor = SystemColors.Window;
		_mouseHook = new MouseHook(this, this);
		_messageHook = new MessageHook(this, this);
		_keyboardHook = new KeyboardHook(this, this);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		DrawManager.DrawGridBackground(e.Graphics, clientRectangle);
		Rectangle rect = clientRectangle;
		rect.Width = LeftColumnWidth + 1;
		Rectangle clientRectangle2 = base.ClientRectangle;
		clientRectangle2.Y -= (_firstDisplayedLine - 1) * _basicPropertyHeight;
		PropertyVisibleDeepEnumerator propertyVisibleDeepEnumerator = null;
		Rectangle itemRect = Rectangle.Empty;
		PropertyVisibleDeepEnumerator firstDisplayedProperty = FirstDisplayedProperty;
		int num = 0;
		while (firstDisplayedProperty != firstDisplayedProperty.RightBound)
		{
			Property property = firstDisplayedProperty.Property;
			int num2 = (clientRectangle2.Height = _basicPropertyHeight * property.HeightMultiplier);
			if (clientRectangle2.Top > e.ClipRectangle.Bottom)
			{
				break;
			}
			property.DisplayedAtPosition = num++;
			rect.Y = clientRectangle2.Y;
			rect.Height = clientRectangle2.Height;
			DrawManager.DrawLeftColumnBackground(e.Graphics, rect, firstDisplayedProperty);
			if (!Rectangle.Intersect(clientRectangle2, e.ClipRectangle).IsEmpty)
			{
				if (property.Selected)
				{
					propertyVisibleDeepEnumerator = (PropertyVisibleDeepEnumerator)firstDisplayedProperty.Clone();
					itemRect = clientRectangle2;
				}
				else
				{
					property.Draw(e.Graphics, clientRectangle2, firstDisplayedProperty);
				}
			}
			clientRectangle2.Offset(0, num2);
			if (clientRectangle2.Top > clientRectangle.Bottom)
			{
				break;
			}
			firstDisplayedProperty.MoveNext();
		}
		if (propertyVisibleDeepEnumerator != null)
		{
			propertyVisibleDeepEnumerator.Property.Draw(e.Graphics, itemRect, propertyVisibleDeepEnumerator);
		}
		base.OnPaint(e);
	}

	private void RecalcPropertyItemHeight()
	{
		Graphics graphics = CreateGraphics();
		_basicPropertyHeight = Win32Calls.GetTextExtent(graphics, "A0j", Font).Height + _propertyVerticalMargin;
		foreach (PropertyFeel value in _parentCtrl._registeredFeels.Values)
		{
			value?.UpdateFont();
		}
		graphics.Dispose();
		Invalidate();
	}

	private bool IsScrollbarVisible()
	{
		return (Win32Calls.GetWindowLong32(base.Handle, -16) & 0x200000) != 0;
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 33)
		{
			if (!Focused)
			{
				Point point = PointToClient(Control.MousePosition);
				if (base.ClientRectangle.Contains(point))
				{
					PropertyEnumerator propertyEnumerator = PropertyItemFromPoint(point, out var _);
					if (!(SelectedPropertyEnumerator != propertyEnumerator))
					{
					}
				}
			}
		}
		else if (m.Msg == 513)
		{
			_doubleClickDone = false;
			long ticks = DateTime.Now.Ticks;
			int num = Win32Calls.LoWord((int)m.LParam);
			int num2 = Win32Calls.HiWord((int)m.LParam);
			if (_lastTimeClicked != 0 && (int)((ticks - _lastTimeClicked) / 10000) < SystemInformation.DoubleClickTime)
			{
				if (Math.Abs(num - _lastPosClicked.X) < SystemInformation.DoubleClickSize.Width && Math.Abs(num2 - _lastPosClicked.Y) < SystemInformation.DoubleClickSize.Height)
				{
					_doubleClickDone = true;
				}
				_lastTimeClicked = 0L;
				_lastPosClicked = Point.Empty;
			}
			if (!_doubleClickDone)
			{
				_lastTimeClicked = ticks;
				_lastPosClicked = new Point(num, num2);
			}
		}
		else if (m.Msg == 515)
		{
			_lastTimeClicked = 0L;
		}
		else if (m.Msg == 131)
		{
			Win32Calls.NCCALCSIZE_PARAMS nCCALCSIZE_PARAMS = (Win32Calls.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(Win32Calls.NCCALCSIZE_PARAMS));
			Rectangle rect = new Rectangle(nCCALCSIZE_PARAMS.rgrc0.Left, nCCALCSIZE_PARAMS.rgrc0.Top, nCCALCSIZE_PARAMS.rgrc0.Right - nCCALCSIZE_PARAMS.rgrc0.Left, nCCALCSIZE_PARAMS.rgrc0.Bottom - nCCALCSIZE_PARAMS.rgrc0.Top);
			DrawManager.NcCalcSize(ref rect);
			nCCALCSIZE_PARAMS.rgrc0.Left = rect.X;
			nCCALCSIZE_PARAMS.rgrc0.Right = rect.X + rect.Width;
			nCCALCSIZE_PARAMS.rgrc0.Top = rect.Y;
			nCCALCSIZE_PARAMS.rgrc0.Bottom = rect.Y + rect.Height;
			Marshal.StructureToPtr(nCCALCSIZE_PARAMS, m.LParam, fDeleteOld: false);
		}
		else if (m.Msg == 78)
		{
			if (m.LParam != IntPtr.Zero)
			{
				Win32Calls.NMHDR nMHDR = (Win32Calls.NMHDR)m.GetLParam(typeof(Win32Calls.NMHDR));
				if (nMHDR.code == -521 && ToolTip != null && nMHDR.hwndFrom == ToolTip.Handle)
				{
					if (_toolTipLocation.X < 0)
					{
						_toolTipLocation.X = 0;
					}
					ToolTip.Location = _toolTipLocation;
					m.Result = (IntPtr)1;
					return;
				}
			}
		}
		else if (m.Msg == 133)
		{
			IntPtr windowDC = Win32Calls.GetWindowDC(m.HWnd);
			Graphics graphics = Graphics.FromHdc(windowDC);
			Win32Calls.RECT rc = default(Win32Calls.RECT);
			Win32Calls.GetWindowRect(m.HWnd, ref rc);
			Rectangle rect2 = new Rectangle(0, 0, rc.Right - rc.Left, rc.Bottom - rc.Top);
			DrawManager.DrawGridNonClientArea(graphics, rect2);
			Win32Calls.ReleaseDC(m.HWnd, windowDC);
			graphics.Dispose();
		}
		else if (m.Msg == 277)
		{
			int num3 = Win32Calls.LoWord((int)m.WParam);
			int scrollPos = Win32Calls.GetScrollPos(base.Handle, 1);
			if (num3 == 8)
			{
				EndScroll();
				return;
			}
			_scrollingInProgress = true;
			Rectangle clientRectangle = base.ClientRectangle;
			int num4 = clientRectangle.Height / _basicPropertyHeight;
			Win32Calls.SCROLLINFO lpScrollInfo = default(Win32Calls.SCROLLINFO);
			lpScrollInfo.cbSize = Marshal.SizeOf(lpScrollInfo);
			lpScrollInfo.fMask = 3;
			Win32Calls.GetScrollInfo(base.Handle, 1, ref lpScrollInfo);
			switch (num3)
			{
			case 1:
				ScrollLineDown();
				break;
			case 0:
				ScrollLineUp();
				break;
			case 3:
			{
				int num6 = 0;
				PropertyVisibleDeepEnumerator visibleDeepEnumerator = FirstDisplayedProperty.GetVisibleDeepEnumerator();
				while (visibleDeepEnumerator != visibleDeepEnumerator.RightBound)
				{
					Rectangle absoluteItemRect = GetAbsoluteItemRect(visibleDeepEnumerator);
					if (absoluteItemRect.Bottom > clientRectangle.Bottom)
					{
						PropertyVisibleDeepEnumerator visibleDeepEnumerator2 = visibleDeepEnumerator.GetVisibleDeepEnumerator();
						int num7 = (clientRectangle.Bottom - absoluteItemRect.Top) / _basicPropertyHeight + 1;
						do
						{
							if (_firstDisplayedLine == FirstDisplayedProperty.Property.HeightMultiplier)
							{
								_firstDisplayedPropertyEnumerator.MoveNext();
								_firstDisplayedLine = 1;
							}
							else
							{
								_firstDisplayedLine++;
							}
							if (num7 == visibleDeepEnumerator2.Property.HeightMultiplier)
							{
								visibleDeepEnumerator2.MoveNext();
								num7 = 1;
							}
							else
							{
								num7++;
							}
							num6++;
						}
						while (num6 < num4 - 1 && visibleDeepEnumerator2 != visibleDeepEnumerator2.RightBound);
						break;
					}
					visibleDeepEnumerator.MoveNext();
				}
				Win32Calls.SetScrollPos(base.Handle, 1, Win32Calls.GetScrollPos(base.Handle, 1) + num6, bRedraw: true);
				Invalidate();
				break;
			}
			case 2:
			{
				int num8 = Win32Calls.GetScrollPos(base.Handle, 1) - num4 + 1;
				if (num8 < 0)
				{
					num8 = 0;
				}
				for (int j = 0; j < Win32Calls.GetScrollPos(base.Handle, 1) - num8; j++)
				{
					if (_firstDisplayedLine > 1)
					{
						_firstDisplayedLine--;
						continue;
					}
					_firstDisplayedPropertyEnumerator.MovePrev();
					_firstDisplayedLine = FirstDisplayedProperty.Property.HeightMultiplier;
				}
				Win32Calls.SetScrollPos(base.Handle, 1, num8, bRedraw: true);
				Invalidate();
				break;
			}
			case 4:
			case 5:
			{
				if (!(FirstDisplayedProperty != RightBound.GetVisibleDeepEnumerator()))
				{
					break;
				}
				int num5 = Win32Calls.HiWord((int)m.WParam);
				int scrollPos2 = Win32Calls.GetScrollPos(base.Handle, 1);
				int i;
				for (i = 0; i < Math.Abs(num5 - scrollPos2); i++)
				{
					if (num5 > scrollPos2)
					{
						if (FirstDisplayedProperty.Property.HeightMultiplier == _firstDisplayedLine)
						{
							_firstDisplayedPropertyEnumerator.MoveNext();
							_firstDisplayedLine = 1;
						}
						else
						{
							_firstDisplayedLine++;
						}
						if (_firstDisplayedPropertyEnumerator == RightBound.GetVisibleDeepEnumerator())
						{
							break;
						}
					}
					else if (_firstDisplayedLine > 1)
					{
						_firstDisplayedLine--;
					}
					else
					{
						_firstDisplayedPropertyEnumerator.MovePrev();
						_firstDisplayedLine = FirstDisplayedProperty.Property.HeightMultiplier;
					}
				}
				if (num5 < scrollPos2)
				{
					Win32Calls.SetScrollPos(base.Handle, 1, scrollPos2 - i, bRedraw: true);
				}
				else
				{
					Win32Calls.SetScrollPos(base.Handle, 1, scrollPos2 + i, bRedraw: true);
				}
				Invalidate();
				break;
			}
			}
			if (!Focused && base.ContainsFocus)
			{
				Focus();
			}
			CheckScrollbar();
			ScrollEventArgs e = new ScrollEventArgs((ScrollEventType)num3, scrollPos, Win32Calls.GetScrollPos(base.Handle, 1), ScrollOrientation.VerticalScroll);
			_parentCtrl.NotifyScroll(e);
		}
		base.WndProc(ref m);
	}

	public void ScrollLineDown()
	{
		_scrollingInProgress = true;
		Win32Calls.SCROLLINFO lpScrollInfo = default(Win32Calls.SCROLLINFO);
		lpScrollInfo.cbSize = Marshal.SizeOf(lpScrollInfo);
		lpScrollInfo.fMask = 3;
		Win32Calls.GetScrollInfo(base.Handle, 1, ref lpScrollInfo);
		if (lpScrollInfo.nPage > 0 && Win32Calls.GetScrollPos(base.Handle, 1) < lpScrollInfo.nMax - (lpScrollInfo.nPage - 1))
		{
			if (_firstDisplayedLine == FirstDisplayedProperty.Property.HeightMultiplier)
			{
				_firstDisplayedPropertyEnumerator.MoveNext();
				_firstDisplayedLine = 1;
			}
			else
			{
				_firstDisplayedLine++;
			}
			Win32Calls.SetScrollPos(base.Handle, 1, Win32Calls.GetScrollPos(base.Handle, 1) + 1, bRedraw: true);
			Invalidate();
		}
		if (!Focused && base.ContainsFocus)
		{
			Focus();
		}
		CheckScrollbar();
	}

	public void ScrollLineUp()
	{
		_scrollingInProgress = true;
		Win32Calls.SCROLLINFO lpScrollInfo = default(Win32Calls.SCROLLINFO);
		lpScrollInfo.cbSize = Marshal.SizeOf(lpScrollInfo);
		lpScrollInfo.fMask = 3;
		Win32Calls.GetScrollInfo(base.Handle, 1, ref lpScrollInfo);
		if (Win32Calls.GetScrollPos(base.Handle, 1) > 0)
		{
			if (_firstDisplayedLine > 1)
			{
				_firstDisplayedLine--;
			}
			else
			{
				_firstDisplayedPropertyEnumerator.MovePrev();
				_firstDisplayedLine = FirstDisplayedProperty.Property.HeightMultiplier;
			}
			Win32Calls.SetScrollPos(base.Handle, 1, Win32Calls.GetScrollPos(base.Handle, 1) - 1, bRedraw: true);
			Invalidate();
		}
		if (!Focused && base.ContainsFocus)
		{
			Focus();
		}
		CheckScrollbar();
	}

	public void EndScroll()
	{
		_scrollingInProgress = false;
		if (_parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey && (_parentCtrl.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyWithAutoFocus) != PropertyGrid.TabKeyNavigationModes.NoSubMode)
		{
			ShowInPlaceControl(SelectedPropertyEnumerator);
		}
	}

	internal PropertyFeel FindFeel(PropertyEnumerator parentEnum, PropertyEnumerator propEnum)
	{
		PropertyFeel propertyFeel = null;
		ArrayList attributes = propEnum.Property.Value.GetAttributes(typeof(PropertyFeelAttribute));
		foreach (PropertyFeelAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0)
			{
				if (item.PropertyFeelName == null)
				{
					return null;
				}
				propertyFeel = _parentCtrl.GetRegisteredFeel(item.PropertyFeelName);
				if (propertyFeel == PropertyFeel.Empty)
				{
					return null;
				}
				if (propertyFeel != null)
				{
					break;
				}
			}
		}
		if (propertyFeel == null && parentEnum.Property.Value != null)
		{
			attributes = parentEnum.Property.Value.GetAttributes(typeof(PropertyFeelAttribute));
			foreach (PropertyFeelAttribute item2 in attributes)
			{
				if (item2.ChildPropertyName.Length > 0 && item2.ChildPropertyName == propEnum.Property.Value.DisplayName)
				{
					if (item2.PropertyFeelName == null)
					{
						return null;
					}
					propertyFeel = _parentCtrl.GetRegisteredFeel(item2.PropertyFeelName);
					break;
				}
			}
		}
		if (propertyFeel == null)
		{
			PasswordPropertyTextAttribute passwordPropertyTextAttribute = (PasswordPropertyTextAttribute)propEnum.Property.Value.GetAttribute(typeof(PasswordPropertyTextAttribute));
			if (passwordPropertyTextAttribute != null && passwordPropertyTextAttribute.Password)
			{
				propertyFeel = _parentCtrl.GetRegisteredFeel("editpwd");
			}
		}
		if (propertyFeel == null)
		{
			propertyFeel = _parentCtrl.GetRegisteredFeelAttachment(propEnum.Property.Value.UnderlyingType);
		}
		if (propertyFeel == null)
		{
			object[] customAttributes = propEnum.Property.Value.UnderlyingType.GetCustomAttributes(typeof(PropertyFeelAttribute), inherit: true);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				PropertyFeelAttribute propertyFeelAttribute3 = (PropertyFeelAttribute)customAttributes[i];
				if (propertyFeelAttribute3.ChildPropertyName.Length == 0)
				{
					propertyFeel = _parentCtrl.GetRegisteredFeel(propertyFeelAttribute3.PropertyFeelName);
					break;
				}
			}
		}
		if (propertyFeel == null)
		{
			UITypeEditor uITypeEditor = propEnum.Property.Value.UITypeEditor;
			if (uITypeEditor != null && !uITypeEditor.GetType().Equals(typeof(UITypeEditor)))
			{
				PropertyTypeDescriptorContext typeDescriptorContext = propEnum.Property.Value.GetTypeDescriptorContext(propEnum);
				if (uITypeEditor.GetEditStyle(typeDescriptorContext) == UITypeEditorEditStyle.None && propEnum.Property.Value.TypeConverter.GetStandardValuesSupported(typeDescriptorContext))
				{
					propertyFeel = _parentCtrl.GetRegisteredFeel("editlist");
				}
				else
				{
					propertyFeel = _parentCtrl.GetRegisteredFeel("FeelUITypeEditor");
					if (propertyFeel == null)
					{
						propertyFeel = new PropertyUITypeEditorFeel(_parentCtrl);
						_parentCtrl.RegisterFeel("FeelUITypeEditor", propertyFeel);
					}
				}
			}
		}
		if (propertyFeel == null)
		{
			PropertyTypeDescriptorContext typeDescriptorContext2 = propEnum.Property.Value.GetTypeDescriptorContext(propEnum);
			if (propEnum.Property.Value.TypeConverter.GetStandardValuesSupported(typeDescriptorContext2))
			{
				propertyFeel = _parentCtrl.GetRegisteredFeel("editlist");
			}
			if (propertyFeel == null)
			{
				string[] displayedValues = propEnum.Property.Value.GetDisplayedValues();
				if (displayedValues.Length > 0)
				{
					propertyFeel = _parentCtrl.GetRegisteredFeel("editlist");
				}
			}
		}
		if (propertyFeel == null)
		{
			propertyFeel = _parentCtrl.DefaultFeel;
		}
		return propertyFeel;
	}

	private PropertyLook FindLook(PropertyEnumerator parentEnum, PropertyEnumerator propEnum)
	{
		PropertyLook propertyLook = null;
		ArrayList attributes = propEnum.Property.Value.GetAttributes(typeof(PropertyLookAttribute));
		foreach (PropertyLookAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0)
			{
				if (item.LookType == null)
				{
					return null;
				}
				return (PropertyLook)Activator.CreateInstance(item.LookType, item.Parameters);
			}
		}
		if (propertyLook == null && parentEnum.Property != null && parentEnum.Property.Value != null)
		{
			attributes = parentEnum.Property.Value.GetAttributes(typeof(PropertyLookAttribute));
			foreach (PropertyLookAttribute item2 in attributes)
			{
				if (item2.ChildPropertyName.Length > 0 && item2.ChildPropertyName.CompareTo(propEnum.Property.Value.DisplayName) == 0)
				{
					return (PropertyLook)Activator.CreateInstance(item2.LookType, item2.Parameters);
				}
			}
		}
		if (propertyLook == null)
		{
			PasswordPropertyTextAttribute passwordPropertyTextAttribute = (PasswordPropertyTextAttribute)propEnum.Property.Value.GetAttribute(typeof(PasswordPropertyTextAttribute));
			if (passwordPropertyTextAttribute != null && passwordPropertyTextAttribute.Password)
			{
				propertyLook = new PropertyPasswordLook();
			}
		}
		if (propertyLook == null)
		{
			propertyLook = _parentCtrl.GetRegisteredLookAttachment(propEnum.Property.Value.UnderlyingType);
		}
		if (propertyLook == null)
		{
			object[] customAttributes = propEnum.Property.Value.UnderlyingType.GetCustomAttributes(typeof(PropertyLookAttribute), inherit: true);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				PropertyLookAttribute propertyLookAttribute3 = (PropertyLookAttribute)customAttributes[i];
				if (propertyLookAttribute3.ChildPropertyName.Length == 0)
				{
					propertyLook = (PropertyLook)Activator.CreateInstance(propertyLookAttribute3.LookType, propertyLookAttribute3.Parameters);
					break;
				}
			}
		}
		return propertyLook;
	}

	public PropertyEnumerator AppendRootCategory(int id, string catName)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		RootCategory rootCategory = new RootCategory(id, catName);
		rootCategory.ParentGridInternal = _parentCtrl;
		PropertyEnumerator propertyEnumerator = _activePropertyCollection.Append(_activePropertyCollection.GetDeepEnumerator(), rootCategory);
		if (FirstDisplayedProperty == _activePropertyCollection.GetVisibleDeepEnumerator().RightBound)
		{
			FirstDisplayedProperty = propertyEnumerator.GetVisibleDeepEnumerator();
		}
		InvalidateVisibleItemCount();
		if (base.Handle != IntPtr.Zero)
		{
			Invalidate();
		}
		if (FirstProperty.Count == 1)
		{
			SelectProperty(FirstProperty);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator InsertRootCategory(PropertyEnumerator beforeCategory, int id, string catName)
	{
		if (beforeCategory == _activePropertyCollection.GetDeepEnumerator().RightBound)
		{
			return AppendRootCategory(id, catName);
		}
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		if (beforeCategory.HasParent)
		{
			throw new Exception("Root categories can only be inserted at the root.");
		}
		RootCategory rootCategory = new RootCategory(id, catName);
		rootCategory.ParentGridInternal = _parentCtrl;
		PropertyEnumerator propertyEnumerator = _activePropertyCollection.InsertBefore(beforeCategory, rootCategory);
		PropertyEnumerator propertyEnumerator2 = ((PropertyEnumerator)propertyEnumerator.Clone()).MovePrev();
		if (FirstDisplayedProperty == _activePropertyCollection.GetVisibleDeepEnumerator().RightBound || (_parentCtrl.IsInsideSelectedObjects && propertyEnumerator2 == propertyEnumerator.LeftBound))
		{
			FirstDisplayedProperty = propertyEnumerator.GetVisibleDeepEnumerator();
		}
		InvalidateVisibleItemCount();
		if (base.Handle != IntPtr.Zero)
		{
			Invalidate();
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	internal bool AddChildProperties(PropertyEnumerator underParent)
	{
		PropertyValue value = underParent.Property.Value;
		if (value == null)
		{
			return false;
		}
		if (!value.TypeConverter.GetPropertiesSupported(value.GetTypeDescriptorContext(underParent)))
		{
			ShowChildPropertiesAttribute showChildPropertiesAttribute = (ShowChildPropertiesAttribute)underParent.Property.Value.GetAttribute(typeof(ShowChildPropertiesAttribute));
			if (showChildPropertiesAttribute == null || !showChildPropertiesAttribute.ShowChildProperties)
			{
				return false;
			}
		}
		else
		{
			ShowChildPropertiesAttribute showChildPropertiesAttribute2 = (ShowChildPropertiesAttribute)underParent.Property.Value.GetAttribute(typeof(ShowChildPropertiesAttribute));
			if (showChildPropertiesAttribute2 != null && !showChildPropertiesAttribute2.ShowChildProperties)
			{
				return false;
			}
		}
		object[] targetInstances = underParent.Property.Value.TargetInstances;
		object[] array = new object[targetInstances.Length];
		PropertyDescriptor propertyDescriptor = underParent.Property.Value.PropertyDescriptor;
		bool flag = false;
		if (targetInstances.Length == 1)
		{
			try
			{
				array[0] = underParent.Property.Value.UnderlyingValue;
				if (array[0] == null)
				{
					flag = true;
				}
			}
			catch (Exception)
			{
			}
		}
		else
		{
			for (int i = 0; i < targetInstances.Length; i++)
			{
				object component = targetInstances[i];
				try
				{
					MultiplePropertyDescriptor multiplePropertyDescriptor = (MultiplePropertyDescriptor)propertyDescriptor;
					array[i] = multiplePropertyDescriptor[i].GetValue(component);
					if (array[i] == null)
					{
						flag = true;
						break;
					}
				}
				catch (Exception)
				{
				}
			}
		}
		PropertyValue[] array2 = null;
		if (!flag)
		{
			if (_parentCtrl.LazyLoading && underParent.Property.LazyLoadingState == Property.LazyLoadingStates.None)
			{
				if (value.HasChildPropertyDescriptors(underParent, array, propertyDescriptor))
				{
					underParent.Property.LazyLoadingState = Property.LazyLoadingStates.HasChildren;
				}
				else
				{
					underParent.Property.LazyLoadingState = Property.LazyLoadingStates.HasNoChildren;
				}
			}
			else
			{
				array2 = value.GetChildValues(underParent, array, propertyDescriptor);
				underParent.Property.LazyLoadingState = Property.LazyLoadingStates.ContentLoaded;
			}
		}
		else
		{
			underParent.Property.LazyLoadingState = Property.LazyLoadingStates.ContentLoaded;
		}
		if (array2 != null && array2.Length > 0)
		{
			underParent.Property.Value.NoLinkWithChildren = false;
			Hashtable hashtable = new Hashtable();
			ArrayList attributes = underParent.Property.Value.GetAttributes(typeof(PropertyHideAttribute));
			ArrayList attributes2 = underParent.Property.Value.GetAttributes(typeof(PropertyDisableAttribute));
			PropertyValue[] array3 = array2;
			foreach (PropertyValue propertyValue in array3)
			{
				bool flag2 = false;
				foreach (PropertyHideAttribute item in attributes)
				{
					if (item.ChildPropertyName.CompareTo(propertyValue.DisplayName) == 0)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					continue;
				}
				ValueAddedToPropertyAttribute valueAddedToPropertyAttribute = (ValueAddedToPropertyAttribute)propertyValue.GetAttribute(typeof(ValueAddedToPropertyAttribute));
				if (valueAddedToPropertyAttribute != null)
				{
					hashtable.Add(valueAddedToPropertyAttribute.PropertyName, new KeyValuePair(valueAddedToPropertyAttribute.ValueKey, propertyValue));
					continue;
				}
				Property property = new Property(propertyValue.Id, propertyValue.DisplayName);
				property.ParentGridInternal = _parentCtrl;
				property.ValueInternal = propertyValue;
				property.Value.Grid = _parentCtrl;
				if (property.Value.PropertyDescriptor != null)
				{
					property.Comment = property.Value.PropertyDescriptor.Description;
				}
				PropertyEnumerator propEnum = AddPropertyUnder(underParent, property);
				if (IsAncestorDisabledByCheckbox(propEnum))
				{
					EnableProperty(propEnum, enable: false);
				}
				else
				{
					foreach (PropertyDisableAttribute item2 in attributes2)
					{
						if (item2.ChildPropertyName.CompareTo(propertyValue.DisplayName) == 0)
						{
							EnableProperty(propEnum, enable: false);
							break;
						}
					}
				}
				_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propEnum));
			}
			if (hashtable.Count > 0)
			{
				PropertyEnumerator siblingEnumerator = underParent.Children.GetSiblingEnumerator();
				while (siblingEnumerator != siblingEnumerator.RightBound)
				{
					KeyValuePair keyValuePair = (KeyValuePair)hashtable[siblingEnumerator.Property.Name];
					if (keyValuePair != null)
					{
						PropertyValue propertyValue2 = (PropertyValue)keyValuePair.Value;
						if (propertyValue2.TargetInstances.Length == 1)
						{
							siblingEnumerator.Property.AddValue(keyValuePair.Key, propertyValue2.TargetInstance, propertyValue2.PropertyDescriptor, (Attribute[])null);
						}
						else
						{
							object[] targetInstances2 = propertyValue2.TargetInstances;
							for (int k = 0; k < targetInstances2.Length; k++)
							{
								siblingEnumerator.Property.AddValue(keyValuePair.Key, targetInstances2[k], ((MultiplePropertyDescriptor)propertyValue2.PropertyDescriptor)[k], (Attribute[])null);
							}
						}
					}
					siblingEnumerator.MoveNext();
				}
			}
		}
		return HasOneVisibleChild(underParent);
	}

	private PropertyEnumerator AddPropertyBefore(PropertyEnumerator beforeEnum, Property property)
	{
		Invalidate();
		PropertyEnumerator propertyEnumerator = _activePropertyCollection.InsertBefore(beforeEnum, property);
		if (FirstDisplayedProperty == _activePropertyCollection.GetVisibleDeepEnumerator().RightBound)
		{
			_firstDisplayedLine = 1;
			FirstDisplayedProperty = propertyEnumerator.GetVisibleDeepEnumerator();
		}
		AddTargetInstances(propertyEnumerator, null, null);
		if (property.Value != null)
		{
			property.Value.OwnerEnumerator = propertyEnumerator;
			if (!_displayModeChangeInProgress && AddChildProperties(propertyEnumerator))
			{
				ExpandProperty(propertyEnumerator, expand: false);
			}
		}
		InvalidateVisibleItemCount();
		if (property.Value != null)
		{
			if (property.Look == null)
			{
				property.Value.Look = FindLook(beforeEnum.Parent, propertyEnumerator);
				if (propertyEnumerator.Property.Look != null)
				{
					propertyEnumerator.Property.Look.ControlHeightMultiplier();
				}
			}
			if (property.Feel == null && DisplayMode == PropertyGrid.DisplayModes.Categorized)
			{
				property.Value.Feel = FindFeel(beforeEnum.Parent, propertyEnumerator);
			}
			if (property.Value.IsReadOnly(propertyEnumerator) && ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.Disabled)
			{
				EnableSingleProperty(propertyEnumerator, enable: false);
			}
			AssignValidator(propertyEnumerator);
			HandleAttributes(propertyEnumerator);
		}
		InvalidateVisibleItemCount();
		return propertyEnumerator;
	}

	private PropertyEnumerator AddPropertyUnder(PropertyEnumerator underParent, Property property)
	{
		Invalidate();
		PropertyEnumerator propertyEnumerator = _activePropertyCollection.Append(underParent.Children, property);
		if (property.Value != null && property.Value.NoCustomAttributesSet)
		{
			PropertyValueAttributesNeededEventArgs e = new PropertyValueAttributesNeededEventArgs(propertyEnumerator);
			_parentCtrl.OnPropertyValueAttributesNeeded(e);
			property.Value.Attributes = e.Attributes;
		}
		if (FirstDisplayedProperty == _activePropertyCollection.GetVisibleDeepEnumerator().RightBound)
		{
			_firstDisplayedLine = 1;
			FirstDisplayedProperty = propertyEnumerator.GetVisibleDeepEnumerator();
		}
		if (property.Value != null)
		{
			property.Value.OwnerEnumerator = propertyEnumerator;
			if (!_displayModeChangeInProgress && AddChildProperties(propertyEnumerator))
			{
				ExpandProperty(propertyEnumerator, expand: false);
			}
		}
		InvalidateVisibleItemCount();
		if (property.Value != null)
		{
			if (propertyEnumerator.Property.Value.Look == null)
			{
				propertyEnumerator.Property.Value.Look = FindLook(underParent, propertyEnumerator);
				if (propertyEnumerator.Property.Value.Look != null)
				{
					propertyEnumerator.Property.Value.Look.ControlHeightMultiplier();
				}
			}
			if (propertyEnumerator.Property.Feel == null)
			{
				property.Value.Feel = FindFeel(underParent, propertyEnumerator);
			}
			if (property.Value.IsReadOnly(propertyEnumerator) && ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.Disabled)
			{
				EnableSingleProperty(propertyEnumerator, enable: false);
			}
			AssignValidator(propertyEnumerator);
			HandleAttributes(propertyEnumerator);
		}
		return propertyEnumerator;
	}

	private void HandleAttributes(PropertyEnumerator propEnum)
	{
		ArrayList attributes = propEnum.Property.Value.GetAttributes(typeof(PropertyDisableAttribute));
		foreach (PropertyDisableAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0)
			{
				EnableProperty(propEnum, enable: false);
				break;
			}
		}
		if (propEnum.Property.Value.GetAttribute(typeof(PropertyHeightMultiplierAttribute)) is PropertyHeightMultiplierAttribute propertyHeightMultiplierAttribute)
		{
			propEnum.Property.HeightMultiplier = propertyHeightMultiplierAttribute.HeightMultiplier;
		}
		if (propEnum.Property.Value.GetAttribute(typeof(PropertyManuallyDisabledAttribute)) is PropertyManuallyDisabledAttribute)
		{
			propEnum.Property.SetManuallyDisabled(propEnum, disabled: true);
		}
		ArrayList attributes2 = propEnum.Property.Value.GetAttributes(typeof(PropertyHideAttribute));
		foreach (PropertyHideAttribute item2 in attributes2)
		{
			if (item2.ChildPropertyName.Length == 0)
			{
				ShowProperty(propEnum, show: false);
				break;
			}
		}
	}

	private void AddTargetInstances(PropertyEnumerator propEnum, object[] targets, PropertyDescriptor propertyDescriptor)
	{
		if (targets == null && _currentTargetInstancesToAdd != null)
		{
			targets = _currentTargetInstancesToAdd;
		}
		if (targets != null && targets.Length > 1)
		{
			PropertyValueIndirect propertyValueIndirect = propEnum.Property.Value as PropertyValueIndirect;
			for (int i = 1; i < targets.Length; i++)
			{
				propertyValueIndirect.AddTargetInstance(targets[i], propertyDescriptor);
			}
		}
	}

	public PropertyEnumerator AppendProperty(PropertyEnumerator underCategory, int id, string propName, object targetInstance, PropertyDescriptor propertyDescriptor, string comment, Attribute[] attributes)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		Property property = new Property(id, propName);
		property.Comment = comment;
		property.ParentGridInternal = _parentCtrl;
		property.ValueInternal = new PropertyValueIndirect(_parentCtrl, null, targetInstance, propertyDescriptor, attributes);
		PropertyEnumerator propertyEnumerator = AddPropertyUnder(underCategory, property);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			property.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator InsertProperty(PropertyEnumerator beforeProperty, int id, string propName, object targetInstance, PropertyDescriptor propertyDescriptor, string comment, Attribute[] attributes)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		Property property = new Property(id, propName);
		property.Comment = comment;
		property.ParentGridInternal = _parentCtrl;
		property.ValueInternal = new PropertyValueIndirect(_parentCtrl, null, targetInstance, propertyDescriptor, attributes);
		PropertyEnumerator propertyEnumerator = AddPropertyBefore(beforeProperty, property);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			property.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public void AddTargetInstance(PropertyEnumerator propEnum, object targetInstance, string memberName)
	{
		if (!(propEnum.Property.Value is PropertyValueIndirect))
		{
			throw new ArgumentException("The property must already have a target instance.", "propEnum");
		}
		((PropertyValueIndirect)propEnum.Property.Value).AddTargetInstance(targetInstance, memberName);
	}

	public PropertyEnumerator AppendManagedProperty(PropertyEnumerator underCategory, int id, string propName, Type valueType, object initialValue, string comment, params Attribute[] attributes)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		Property property = new Property(id, propName);
		property.Comment = comment;
		property.ParentGridInternal = _parentCtrl;
		property.ValueInternal = new PropertyValueManaged(_parentCtrl, id, valueType, initialValue, attributes);
		PropertyEnumerator propertyEnumerator = AddPropertyUnder(underCategory, property);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			property.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator InsertManagedProperty(PropertyEnumerator beforeProperty, int id, string propName, Type valueType, object initialValue, string comment, Attribute[] attributes)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		Property property = new Property(id, propName);
		property.Comment = comment;
		property.ParentGridInternal = _parentCtrl;
		property.ValueInternal = new PropertyValueManaged(_parentCtrl, id, valueType, initialValue, attributes);
		PropertyEnumerator propertyEnumerator = AddPropertyBefore(beforeProperty, property);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			property.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator AppendHyperLinkProperty(PropertyEnumerator underCategory, int id, string propName, string comment)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		Property property = new PropertyHyperLink(id);
		property.DisplayName = propName;
		property.Comment = comment;
		property.ParentGridInternal = _parentCtrl;
		PropertyEnumerator propertyEnumerator = AddPropertyUnder(underCategory, property);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			property.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator InsertHyperLinkProperty(PropertyEnumerator beforeProperty, int id, string propName, string comment)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		Property property = new PropertyHyperLink(id);
		property.DisplayName = propName;
		property.Comment = comment;
		property.ParentGridInternal = _parentCtrl;
		PropertyEnumerator propertyEnumerator = AddPropertyBefore(beforeProperty, property);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			property.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator AppendSubCategory(PropertyEnumerator underCategory, int id, string catName)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		SubCategory subCategory = new SubCategory(id, catName);
		subCategory.ParentGridInternal = _parentCtrl;
		PropertyEnumerator propertyEnumerator = AddPropertyUnder(underCategory, subCategory);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			subCategory.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	public PropertyEnumerator InsertSubCategory(PropertyEnumerator beforeProperty, int id, string catName)
	{
		if (DisplayMode != PropertyGrid.DisplayModes.Categorized)
		{
			throw new Exception("Properties can only be inserted in categorized mode.");
		}
		SubCategory subCategory = new SubCategory(id, catName);
		subCategory.ParentGridInternal = _parentCtrl;
		PropertyEnumerator propertyEnumerator = AddPropertyBefore(beforeProperty, subCategory);
		if (IsAncestorDisabled(propertyEnumerator))
		{
			subCategory.Enable(propertyEnumerator, enable: false, ancestorDisabled: true, direct: true);
		}
		CheckScrollbar();
		_parentCtrl.OnPropertyCreated(new PropertyCreatedEventArgs(propertyEnumerator));
		return propertyEnumerator;
	}

	internal bool IsAncestorDisabled(PropertyEnumerator propEnum)
	{
		PropertyEnumerator propertyEnumerator = propEnum.Parent;
		while (propertyEnumerator != propertyEnumerator.RightBound)
		{
			if (!propertyEnumerator.Property.GetManuallyDisabledVariable())
			{
				return true;
			}
			propertyEnumerator = propertyEnumerator.Parent;
		}
		return false;
	}

	internal bool IsAncestorDisabledByCheckbox(PropertyEnumerator propEnum)
	{
		PropertyEnumerator propertyEnumerator = propEnum.Parent;
		while (propertyEnumerator != propertyEnumerator.RightBound)
		{
			if (propertyEnumerator.Property.CanBeDisabledManually && !propertyEnumerator.Property.GetManuallyDisabledVariable())
			{
				return true;
			}
			propertyEnumerator = propertyEnumerator.Parent;
		}
		return false;
	}

	public bool HasOneVisibleChild(PropertyEnumerator propEnum)
	{
		if (_parentCtrl.LazyLoading && propEnum.Property.LazyLoadingState == Property.LazyLoadingStates.HasChildren)
		{
			return true;
		}
		PropertyEnumerator children = propEnum.Children;
		while (children != children.RightBound)
		{
			if (children.Property.Visible)
			{
				return true;
			}
			children.MoveNext();
		}
		return false;
	}

	public bool DrawImage(int imageIndex, Graphics graphics, int x, int y)
	{
		try
		{
			Image image = _imageList.Images[imageIndex];
			Rectangle rectangle = _parentCtrl.TransformRectIfRTL(new Rectangle(x, y, image.Width, image.Height));
			Win32Calls.ImageListDraw(_imageList, imageIndex, graphics, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		PropertyGrid.HitTests hitTests = HitTest(new Point(e.X, e.Y), out var propEnum, out var itemRect);
		if ((e.Clicks == 2 || _doubleClickDone) && hitTests != PropertyGrid.HitTests.TreeGlyph && hitTests != PropertyGrid.HitTests.ManualDisable)
		{
			if (e.Button == MouseButtons.Left)
			{
				Property property = propEnum.Property;
				if (property != null)
				{
					if (property is PropertyHyperLink || property.HyperLinkFormat.Length > 0)
					{
						property.OnPropertyClicked(propEnum, new Point(e.X, e.Y), itemRect);
					}
					else if (hitTests == PropertyGrid.HitTests.ColumnSplit)
					{
						AdjustLabelColumn();
					}
					else if (hitTests == PropertyGrid.HitTests.Label && propEnum == SelectedPropertyEnumerator && InPlaceControl != null && !HasOneVisibleChild(propEnum))
					{
						bool flag = ((IInPlaceControl)InPlaceControl).ScrollValueOnDoubleClick(Control.ModifierKeys != Keys.Shift, ScrollValueOnDoubleClickSource.FromLabel);
						if (!flag && !_currentInPlaceControl.ContainsFocus)
						{
							_parentCtrl.ActiveControl = _currentInPlaceControl;
						}
						if (!flag || (_parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey && (_parentCtrl.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyWithAutoFocus) != PropertyGrid.TabKeyNavigationModes.NoSubMode))
						{
							SelectAllText();
						}
					}
					else if (propEnum.Children.Count > 0 || propEnum.Property.LazyLoadingState == Property.LazyLoadingStates.HasChildren)
					{
						ExpandProperty(propEnum, !property.Expanded);
					}
					else if (property.Enabled && !(property is RootCategory) && property is PropertyHyperLink)
					{
					}
				}
			}
		}
		else
		{
			if (_parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey && (_parentCtrl.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyWithAutoFocus) != PropertyGrid.TabKeyNavigationModes.NoSubMode && hitTests != PropertyGrid.HitTests.TreeGlyph && propEnum == SelectedPropertyEnumerator && base.ContainsFocus)
			{
				base.OnMouseDown(e);
				return;
			}
			bool flag2 = true;
			if (!Focused && CanSelectProperties)
			{
				if (!base.ContainsFocus)
				{
					flag2 = false;
				}
				if (SelectedPropertyEnumerator != propEnum)
				{
					_dontShowInPlaceCtrlInGotFocus = true;
				}
				if (_parentCtrl.NavigationKeyMode != PropertyGrid.NavigationKeyModes.TabKey || (_parentCtrl.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyWithAutoFocus) == 0 || hitTests != PropertyGrid.HitTests.TreeGlyph || !base.ContainsFocus || propEnum != SelectedPropertyEnumerator)
				{
					Focus();
				}
				Invalidate();
			}
			if (hitTests == PropertyGrid.HitTests.ColumnSplit)
			{
				if (e.Button == MouseButtons.Left)
				{
					Cursor = Cursors.VSplit;
					base.Capture = true;
					_resizingColumns = true;
				}
			}
			else
			{
				Property property2 = propEnum.Property;
				if (property2 != null)
				{
					if (hitTests == PropertyGrid.HitTests.ManualDisable && (Enabled || DisableMode == PropertyGrid.DisableModes.None) && e.Button == MouseButtons.Left)
					{
						if (!propEnum.HasParent)
						{
							EnableProperty(propEnum, !property2.Enabled);
						}
						else if (propEnum.Parent.Property.Enabled)
						{
							EnableProperty(propEnum, !property2.Enabled);
						}
					}
					if (hitTests == PropertyGrid.HitTests.TreeGlyph)
					{
						if (e.Button == MouseButtons.Left)
						{
							ExpandProperty(propEnum, !property2.Expanded);
						}
					}
					else if (CanSelectProperties)
					{
						if (Control.ModifierKeys == Keys.Shift && _multiSelectMode != PropertyGrid.MultiSelectModes.None)
						{
							PropertyEnumerator propertyEnumerator = null;
							if (_multiSelectMode == PropertyGrid.MultiSelectModes.SameLevel)
							{
								propertyEnumerator = ((SelectedPropertyEnumerator.Children.Count != 0) ? SelectedPropertyEnumerator : SelectedPropertyEnumerator.Parent);
							}
							foreach (PropertyVisibleDeepEnumerator multiSelectedItem in _multiSelectedItems)
							{
								multiSelectedItem.Property.SelectedInternal = false;
							}
							_multiSelectedItems.Clear();
							Rectangle absoluteItemRect = GetAbsoluteItemRect(SelectedPropertyEnumerator);
							PropertyVisibleDeepEnumerator visibleDeepEnumerator = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
							do
							{
								if (absoluteItemRect.Bottom < e.Y)
								{
									visibleDeepEnumerator.MoveNext();
								}
								else if (absoluteItemRect.Top > e.Y)
								{
									visibleDeepEnumerator.MovePrev();
								}
								if (_multiSelectMode == PropertyGrid.MultiSelectModes.SameLevel && !visibleDeepEnumerator.IsDescendantOf(propertyEnumerator.GetDeepEnumerator()))
								{
									break;
								}
								if (visibleDeepEnumerator.Children.Count == 0 && _parentCtrl.OnValidateMultiSelection(SelectedPropertyEnumerator, visibleDeepEnumerator))
								{
									visibleDeepEnumerator.Property.SelectedInternal = true;
									_multiSelectedItems.Add(visibleDeepEnumerator);
								}
							}
							while (visibleDeepEnumerator != propEnum);
							Invalidate();
						}
						else if (Control.ModifierKeys == Keys.Control && _multiSelectMode != PropertyGrid.MultiSelectModes.None)
						{
							bool flag3 = false;
							if (_multiSelectMode == PropertyGrid.MultiSelectModes.SameLevel)
							{
								PropertyEnumerator propertyEnumerator2 = ((SelectedPropertyEnumerator.Children.Count != 0) ? SelectedPropertyEnumerator : SelectedPropertyEnumerator.Parent);
								if (!propEnum.IsDescendantOf(propertyEnumerator2.GetDeepEnumerator()))
								{
									flag3 = true;
								}
							}
							_clickWithControl = true;
							if (!flag3)
							{
								if (propEnum == SelectedPropertyEnumerator && _multiSelectedItems.Count > 0)
								{
									SelectProperty((PropertyEnumerator)_multiSelectedItems[0], new Point(e.X, e.Y));
									_multiSelectedItems.RemoveAt(0);
								}
								else if (propEnum.Children.Count == 0)
								{
									if (propEnum.Property.Selected)
									{
										if (propEnum != SelectedPropertyEnumerator)
										{
											propEnum.Property.SelectedInternal = false;
											_multiSelectedItems.Remove(propEnum);
										}
									}
									else if (_parentCtrl.OnValidateMultiSelection(SelectedPropertyEnumerator, propEnum.GetVisibleDeepEnumerator()))
									{
										propEnum.Property.SelectedInternal = true;
										_multiSelectedItems.Add(propEnum);
									}
								}
								Invalidate();
							}
							_clickWithControl = false;
						}
						else if (_multiSelectedItems.Count > 0 && ((propEnum.Property.Selected && hitTests == PropertyGrid.HitTests.Value) || !flag2))
						{
							_clickWithControl = true;
							_multiSelectedItems.Add(SelectedPropertyEnumerator);
							PropertyVisibleDeepEnumerator visibleDeepEnumerator2 = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
							SelectProperty(propEnum, new Point(e.X, e.Y));
							visibleDeepEnumerator2.Property.SelectedInternal = true;
							_multiSelectedItems.Remove(propEnum);
							_clickWithControl = false;
						}
						else
						{
							SelectProperty(propEnum, new Point(e.X, e.Y));
						}
						if (_currentInPlaceControl != null && (Control.ModifierKeys != Keys.Control || property2.HyperLinkFormat.Length == 0) && e.Button == MouseButtons.Left)
						{
							Rectangle bounds = _currentInPlaceControl.Bounds;
							Point p = PointToScreen(new Point(e.X, e.Y));
							p = _currentInPlaceControl.PointToClient(p);
							if (bounds.Contains(new Point(e.X, e.Y)) && !TryOnForbidClickThrough(p))
							{
								Point p2 = PointToScreen(new Point(e.X, e.Y));
								IntPtr intPtr = IntPtr.Zero;
								if (_currentInPlaceControl.HasChildren)
								{
									intPtr = Win32Calls.WindowFromPoint(new Win32Calls.POINT(p2.X, p2.Y));
									if (intPtr != IntPtr.Zero)
									{
										Control control = Control.FromHandle(intPtr);
										if (control != null)
										{
											p2 = control.PointToClient(p2);
										}
									}
								}
								else
								{
									p2 = _currentInPlaceControl.PointToClient(p2);
								}
								if (intPtr == IntPtr.Zero)
								{
									intPtr = _currentInPlaceControl.Handle;
								}
								_parentCtrl.ClickThroughInProgress = true;
								int num = Win32Calls.MakeLong(p2.X, p2.Y);
								Win32Calls.SendMessage(intPtr, 513, IntPtr.Zero, (IntPtr)num);
								_parentCtrl.ClickThroughInProgress = false;
								_lastTimeClickedFromParent = DateTime.Now.Ticks;
								_lastPosClickedInParent = PointToScreen(new Point(e.X, e.Y));
							}
						}
						if (hitTests != PropertyGrid.HitTests.ManualDisable && property2.Enabled && e.Clicks == 1 && e.Button == MouseButtons.Left)
						{
							property2.OnPropertyClicked(propEnum, new Point(e.X, e.Y), itemRect);
						}
					}
				}
			}
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (_resizingColumns)
		{
			SetLabelColumnWidthRatio((double)(_parentCtrl.TransformPointIfRTL(new Point(e.X, e.Y)).X - LeftColumnWidth) / (double)(base.ClientRectangle.Width - LeftColumnWidth));
		}
		else
		{
			Rectangle itemRect = Rectangle.Empty;
			PropertyEnumerator propertyEnumerator = null;
			if (ToolTipMode != PropertyGrid.ToolTipModes.None)
			{
				propertyEnumerator = PropertyItemFromPoint(new Point(e.X, e.Y), out itemRect);
			}
			_toolTipLocation = Point.Empty;
			if ((ToolTipMode & PropertyGrid.ToolTipModes.ToolTipsOnValues) != PropertyGrid.ToolTipModes.None)
			{
				Property property = propertyEnumerator.Property;
				if (property != null && !(property is RootCategory) && !property.Selected)
				{
					Rectangle valueRect = property.GetValueRect(itemRect);
					if (valueRect.Contains(_parentCtrl.TransformPointIfRTL(new Point(e.X, e.Y))) && property.Value != null)
					{
						Graphics graphics = CreateGraphics();
						Rectangle stringValueRect = property.Value.GetStringValueRect(graphics, valueRect, new Point(e.X, e.Y));
						stringValueRect.Height--;
						if (_parentCtrl.TransformRectIfRTL(stringValueRect).Contains(new Point(e.X, e.Y)))
						{
							stringValueRect.Height++;
							Rectangle workingArea = Screen.GetWorkingArea(this);
							Rectangle rect = stringValueRect;
							rect = RectangleToScreen(_parentCtrl.TransformRectIfRTL(rect));
							if (stringValueRect.Right > valueRect.Right || rect.Right > workingArea.Right || rect.Left < workingArea.Left || stringValueRect.Bottom > valueRect.Bottom || rect.Bottom > workingArea.Bottom)
							{
								ToolTip.Font = property.Value.Font;
								stringValueRect = _parentCtrl.TransformRectIfRTL(stringValueRect);
								Point p = new Point(stringValueRect.Left - 1, stringValueRect.Top);
								p = PointToScreen(p);
								if (PropertyVerticalMargin == 0 || PropertyVerticalMargin == 2)
								{
									p.Y++;
								}
								int line = (e.Y - valueRect.Top) / _basicPropertyHeight;
								Size tooltipStringSize = property.Value.GetTooltipStringSize(graphics, valueRect, new Point(e.X, e.Y), line);
								int num = stringValueRect.Height + 1 - tooltipStringSize.Height - 4;
								int top = num % 2 + num / 2;
								int bottom = num / 2;
								ToolTip.SetMargins(_parentCtrl.GlobalTextMargin - 2, top, _parentCtrl.GlobalTextMargin - 2, bottom);
								Win32Calls.SendMessage(ToolTip.Handle, 1048, IntPtr.Zero, (IntPtr)tooltipStringSize.Width);
								if (rect.Left < workingArea.Left)
								{
									p.X = workingArea.Left;
								}
								else if (rect.Right > workingArea.Right)
								{
									p.X = workingArea.Right - tooltipStringSize.Width - _parentCtrl.GlobalTextMargin;
								}
								p.X -= _parentCtrl.GlobalTextMargin;
								_toolTipLocation = new Point(p.X, p.Y - 1);
								ToolTip.SetText(property.Value.GetTooltipString(line), propertyEnumerator);
							}
						}
						graphics.Dispose();
					}
				}
			}
			if ((ToolTipMode & PropertyGrid.ToolTipModes.ToolTipsOnLabels) != PropertyGrid.ToolTipModes.None)
			{
				Property property2 = propertyEnumerator.Property;
				if (property2 != null && !(property2 is RootCategory) && property2.Value != null)
				{
					Rectangle labelTextRect = property2.GetLabelTextRect(itemRect, propertyEnumerator);
					Graphics graphics2 = CreateGraphics();
					string displayName = property2.DisplayName;
					Size textExtent = Win32Calls.GetTextExtent(graphics2, displayName, _parentCtrl.DrawManager.GetPropertyLabelFont(property2));
					Rectangle rectangle = labelTextRect;
					rectangle.Width = textExtent.Width;
					rectangle.Height = _basicPropertyHeight;
					if (rectangle.Right > LeftColumnWidth + 1 + LabelColumnWidth)
					{
						rectangle.Width = LeftColumnWidth + 1 + LabelColumnWidth - rectangle.Left;
					}
					if (_parentCtrl.TransformRectIfRTL(rectangle).Contains(new Point(e.X, e.Y)))
					{
						Rectangle workingArea2 = Screen.GetWorkingArea(this);
						rectangle.Width = textExtent.Width;
						Rectangle rect2 = rectangle;
						rect2 = RectangleToScreen(_parentCtrl.TransformRectIfRTL(rect2));
						int num2 = LeftColumnWidth + 1 + LabelColumnWidth;
						if ((_parentCtrl.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None)
						{
							num2 -= _parentCtrl.GlobalTextMargin;
						}
						if (rectangle.Right > num2 || rect2.Right > workingArea2.Right || rect2.Left < workingArea2.Left)
						{
							ToolTip.Font = _parentCtrl.DrawManager.GetPropertyLabelFont(property2);
							rectangle = _parentCtrl.TransformRectIfRTL(rectangle);
							Point p2 = new Point(rectangle.Left - 1, rectangle.Top);
							p2 = PointToScreen(p2);
							if (PropertyVerticalMargin == 0 || PropertyVerticalMargin == 2)
							{
								p2.Y++;
							}
							int num3 = 0;
							if (ToolTip.Font.Bold || ToolTip.Font.Italic)
							{
								IntPtr hdc = graphics2.GetHdc();
								IntPtr hFont = Win32Calls.GetHFont(ToolTip.Font);
								IntPtr hgdiObj = Win32Calls.SelectObject(hdc, hFont);
								Win32Calls.ABCFLOAT[] array = new Win32Calls.ABCFLOAT[1];
								uint num4 = displayName[displayName.Length - 1];
								Win32Calls.GetCharABCWidthsFloat(hdc, num4, num4, array);
								Win32Calls.SelectObject(hdc, hgdiObj);
								graphics2.ReleaseHdc(hdc);
								double num5 = array[0].abcfC;
								if (num5 < 0.0)
								{
									num3 = -(int)num5;
								}
							}
							int num6 = _basicPropertyHeight + 1 - textExtent.Height - 4;
							int top2 = num6 % 2 + num6 / 2;
							int bottom2 = num6 / 2;
							ToolTip.SetMargins(_parentCtrl.GlobalTextMargin - 2, top2, _parentCtrl.GlobalTextMargin - 2 + num3, bottom2);
							Win32Calls.SendMessage(ToolTip.Handle, 1048, IntPtr.Zero, (IntPtr)textExtent.Width);
							if (rect2.Right > workingArea2.Right)
							{
								p2.X = workingArea2.Right - textExtent.Width - _parentCtrl.GlobalTextMargin;
							}
							else if (rect2.Left < workingArea2.Left)
							{
								p2.X = workingArea2.Left;
							}
							p2.X -= _parentCtrl.GlobalTextMargin;
							_toolTipLocation = new Point(p2.X, p2.Y - 1);
							ToolTip.SetText(displayName, propertyEnumerator);
						}
					}
					graphics2.Dispose();
				}
			}
			if (ToolTip != null && _toolTipLocation == Point.Empty)
			{
				ToolTip.SetText("", propertyEnumerator);
			}
		}
		PropertyGrid.HitTests hitTests = HitTest(new Point(e.X, e.Y), out var propEnum, out var itemRect2);
		if (!_resizingColumns)
		{
			if (hitTests == PropertyGrid.HitTests.ColumnSplit)
			{
				Cursor = Cursors.VSplit;
			}
			else
			{
				bool flag = false;
				if ((Enabled || DisableMode == PropertyGrid.DisableModes.None) && propEnum.Property != null && propEnum.Property.OverrideCursor(new Point(e.X, e.Y), itemRect2, propEnum))
				{
					flag = true;
				}
				if (!flag)
				{
					Cursor = Cursors.Arrow;
				}
			}
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_resizingColumns)
		{
			_resizingColumns = false;
			base.Capture = false;
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (Focused)
		{
			if (IsScrollbarVisible())
			{
				int num = 0;
				if (SystemInformation.MouseWheelScrollLines != -1)
				{
					num = SystemInformation.MouseWheelScrollLines;
				}
				else if (IsScrollbarVisible())
				{
					Win32Calls.SCROLLINFO lpScrollInfo = default(Win32Calls.SCROLLINFO);
					lpScrollInfo.cbSize = Marshal.SizeOf(lpScrollInfo);
					lpScrollInfo.fMask = 2;
					Win32Calls.GetScrollInfo(base.Handle, 1, ref lpScrollInfo);
					num = lpScrollInfo.nPage - 1;
				}
				int num2 = e.Delta / 120 * num;
				if (num2 > 0)
				{
					for (int i = 0; i < num2; i++)
					{
						Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(0, 0), IntPtr.Zero);
					}
					Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(8, 0), IntPtr.Zero);
				}
				else if (num2 < 0)
				{
					for (int j = 0; j < -num2; j++)
					{
						Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(1, 0), IntPtr.Zero);
					}
					Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(8, 0), IntPtr.Zero);
				}
			}
			Point point = PointToClient(new Point(e.X, e.Y));
			Win32Calls.PostMessage(base.Handle, 512, IntPtr.Zero, (IntPtr)Win32Calls.MakeLong(point.X, point.Y));
		}
		base.OnMouseWheel(e);
	}

	public PropertyGrid.HitTests HitTest(Point point, out PropertyEnumerator propEnum, out Rectangle itemRect)
	{
		propEnum = PropertyItemFromPoint(point, out itemRect);
		point = _parentCtrl.TransformPointIfRTL(point);
		if (propEnum.Property != null)
		{
			if (point.X >= LeftColumnWidth + 1 + LabelColumnWidth - 1 && point.X <= LeftColumnWidth + 1 + LabelColumnWidth + 1 && (_parentCtrl.DrawManager.ShowCategoryVerticalLine || !(propEnum.Property is RootCategory)) && !(propEnum.Property is PropertyHyperLink) && !_columnsLock)
			{
				return PropertyGrid.HitTests.ColumnSplit;
			}
			Rectangle empty = Rectangle.Empty;
			empty = propEnum.Property.GetSignRect(itemRect, propEnum);
			if (empty != Rectangle.Empty && empty.Contains(point))
			{
				return PropertyGrid.HitTests.TreeGlyph;
			}
			if (DisplayMode == PropertyGrid.DisplayModes.Categorized && point.X <= LeftColumnWidth)
			{
				return PropertyGrid.HitTests.LeftColumn;
			}
			if (propEnum.Property.CanBeDisabledManually && propEnum.Property.GetManuallyDisableRect(itemRect, propEnum).Contains(point))
			{
				return PropertyGrid.HitTests.ManualDisable;
			}
			if (point.X > LeftColumnWidth + 1 + LabelColumnWidth + 1 && !(propEnum.Property is RootCategory))
			{
				return PropertyGrid.HitTests.Value;
			}
			if (propEnum.Property.GetLabelColumnRect(itemRect, propEnum).Contains(point))
			{
				return PropertyGrid.HitTests.Label;
			}
		}
		return PropertyGrid.HitTests.Client;
	}

	public PropertyEnumerator PropertyItemFromPoint(Point point, out Rectangle itemRect)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		itemRect = new Rectangle(-1, -1, -1, -1);
		if (point.Y > clientRectangle.Bottom || point.Y < clientRectangle.Top)
		{
			return _activePropertyCollection.GetDeepEnumerator().RightBound;
		}
		PropertyEnumerator firstDisplayedProperty = FirstDisplayedProperty;
		while (firstDisplayedProperty != firstDisplayedProperty.RightBound)
		{
			Rectangle itemRect2 = GetItemRect(firstDisplayedProperty);
			if (itemRect2.Contains(point))
			{
				itemRect = itemRect2;
				break;
			}
			firstDisplayedProperty.MoveNext();
		}
		return firstDisplayedProperty;
	}

	public bool IsPropertyDisplayed(PropertyEnumerator propEnum)
	{
		return GetItemRect(propEnum) != new Rectangle(-1, -1, -1, -1);
	}

	public Rectangle GetItemRect(PropertyEnumerator propEnum)
	{
		if (propEnum == propEnum.RightBound)
		{
			return new Rectangle(-1, -1, -1, -1);
		}
		Property property = propEnum.Property;
		Rectangle clientRectangle = base.ClientRectangle;
		Rectangle result = clientRectangle;
		result.Y -= (_firstDisplayedLine - 1) * _basicPropertyHeight;
		result.Height += (_firstDisplayedLine - 1) * _basicPropertyHeight;
		PropertyEnumerator firstDisplayedProperty = FirstDisplayedProperty;
		while (firstDisplayedProperty.Property != null)
		{
			int num = (result.Height = _basicPropertyHeight * firstDisplayedProperty.Property.HeightMultiplier);
			if (property == firstDisplayedProperty.Property && result.Top < clientRectangle.Bottom)
			{
				break;
			}
			if (result.Top >= clientRectangle.Bottom)
			{
				result = new Rectangle(-1, -1, -1, -1);
				break;
			}
			result.Offset(0, num);
			firstDisplayedProperty.MoveNext();
		}
		if (firstDisplayedProperty.Property == null)
		{
			result = new Rectangle(-1, -1, -1, -1);
		}
		return result;
	}

	private Rectangle GetAbsoluteItemRect(PropertyEnumerator propEnum)
	{
		Property property = propEnum.Property;
		PropertyVisibleDeepEnumerator firstDisplayedProperty = FirstDisplayedProperty;
		Property property2 = firstDisplayedProperty.Property;
		if (property2 == null)
		{
			return new Rectangle(-1, -1, -1, -1);
		}
		Rectangle clientRectangle = base.ClientRectangle;
		Rectangle result = clientRectangle;
		result.Y -= (_firstDisplayedLine - 1) * _basicPropertyHeight;
		result.Height = property2.HeightMultiplier * _basicPropertyHeight;
		while (property2 != null)
		{
			int num = _basicPropertyHeight * property2.HeightMultiplier;
			result.Y = result.Bottom - num;
			result.Height = num;
			if (property2 == property)
			{
				break;
			}
			firstDisplayedProperty.MovePrev();
			property2 = firstDisplayedProperty.Property;
			result.Height = 0;
		}
		if (property2 != null)
		{
			return result;
		}
		result = clientRectangle;
		result.Y -= (_firstDisplayedLine - 1) * _basicPropertyHeight;
		firstDisplayedProperty = FirstDisplayedProperty;
		for (property2 = firstDisplayedProperty.Property; property2 != null; property2 = firstDisplayedProperty.Property)
		{
			int num2 = (result.Height = _basicPropertyHeight * property2.HeightMultiplier);
			if (property2 == property)
			{
				break;
			}
			result.Offset(0, num2);
			firstDisplayedProperty.MoveNext();
		}
		if (property2 != null)
		{
			return result;
		}
		return new Rectangle(-1, -1, -1, -1);
	}

	private void _EnableCategoryItem(PropertyEnumerator propEnum, bool enable, bool ancestorDisabled, bool direct)
	{
		Property property = propEnum.Property;
		property.Enable(propEnum, enable, ancestorDisabled, direct);
		PropertyEnumerator siblingEnumerator = propEnum.Children.GetSiblingEnumerator();
		while (siblingEnumerator != siblingEnumerator.RightBound)
		{
			Property property2 = siblingEnumerator.Property;
			if (siblingEnumerator.Children.Count > 0)
			{
				_EnableCategoryItem(siblingEnumerator, enable, !property.GetManuallyDisabledVariable() || ancestorDisabled, direct: false);
			}
			else
			{
				property2.Enable(siblingEnumerator, enable, !property.GetManuallyDisabledVariable() || ancestorDisabled, direct: false);
			}
			siblingEnumerator.MoveNext();
		}
	}

	public void EnableProperty(PropertyEnumerator propEnum, bool enable)
	{
		if (propEnum == propEnum.RightBound)
		{
			return;
		}
		Property property = propEnum.Property;
		if (property.Enabled != enable)
		{
			if (HasOneVisibleChild(propEnum))
			{
				_EnableCategoryItem(propEnum, enable, IsAncestorDisabled(propEnum), direct: true);
			}
			else
			{
				property.Enable(propEnum, enable, IsAncestorDisabled(propEnum), direct: true);
			}
			if (SelectedPropertyEnumerator.Property == property && !enable && _currentInPlaceControl != null)
			{
				HideInPlaceControl();
			}
			else if (enable && SelectedPropertyEnumerator.Property == property && _currentInPlaceControl == null)
			{
				ShowInPlaceControl(SelectedPropertyEnumerator);
			}
			Invalidate();
		}
	}

	public void EnableSingleProperty(PropertyEnumerator propEnum, bool enable)
	{
		if (propEnum == propEnum.RightBound)
		{
			return;
		}
		Property property = propEnum.Property;
		if (property.Enabled != enable)
		{
			property.Enable(propEnum, enable, IsAncestorDisabled(propEnum), direct: true);
			if (SelectedPropertyEnumerator.Property == property && !enable && _currentInPlaceControl != null)
			{
				HideInPlaceControl();
			}
			else if (enable && SelectedPropertyEnumerator.Property == property && _currentInPlaceControl == null)
			{
				ShowInPlaceControl(SelectedPropertyEnumerator);
			}
			Invalidate();
		}
	}

	public void SelectProperty(PropertyEnumerator enumerator)
	{
		SelectProperty(enumerator, Point.Empty);
	}

	private void SelectProperty(PropertyEnumerator enumerator, Point mousePos)
	{
		if (enumerator.Property != null && !CheckPropertyVisibility(enumerator))
		{
			return;
		}
		bool flag = false;
		if (!_clickWithControl && _multiSelectedItems.Count > 0)
		{
			foreach (PropertyVisibleDeepEnumerator multiSelectedItem in _multiSelectedItems)
			{
				if (multiSelectedItem != _selectedPropertyEnum)
				{
					multiSelectedItem.Property.SelectedInternal = false;
				}
			}
			_multiSelectedItems.Clear();
			flag = true;
		}
		bool flag2 = _selectedPropertyEnum != enumerator || flag;
		PropertyEnumerator selectedPropertyEnum = _selectedPropertyEnum;
		if (enumerator != enumerator.RightBound && enumerator.RightBound != null)
		{
			if (flag2)
			{
				if (_selectedPropertyEnum != _selectedPropertyEnum.RightBound)
				{
					_selectedPropertyEnum.Property.SelectedInternal = false;
				}
				_selectedPropertyEnum = enumerator.GetVisibleDeepEnumerator();
				_selectedPropertyEnum.RestrictedToThisLevelAndUnder = false;
				_selectedPropertyEnum.RestrictedToThisPropertyAndUnder = false;
				if (base.IsHandleCreated)
				{
					Rectangle clientRectangle = base.ClientRectangle;
					Rectangle itemRect = GetItemRect(enumerator);
					HandleDefaultValue(enumerator);
					if (base.ContainsFocus)
					{
						if (itemRect.Top < clientRectangle.Bottom && itemRect.Bottom > clientRectangle.Top)
						{
							ShowInPlaceControl(enumerator, mousePos);
						}
						else
						{
							HideInPlaceControl();
							Focus();
						}
					}
				}
				if (flag2)
				{
					_selectedPropertyEnum.Property.SelectedInternal = true;
				}
			}
			else if (_parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey && (_parentCtrl.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyWithAutoFocus) != PropertyGrid.TabKeyNavigationModes.NoSubMode && base.ContainsFocus && _currentInPlaceControl != null)
			{
				if (!_currentInPlaceControl.ContainsFocus)
				{
					FocusTextBox();
				}
				SelectAllText();
			}
		}
		else if (flag2)
		{
			if (base.IsHandleCreated)
			{
				HideInPlaceControl();
			}
			if (_selectedPropertyEnum.Property != null)
			{
				_selectedPropertyEnum.Property.SelectedInternal = false;
			}
			_selectedPropertyEnum = enumerator.GetVisibleDeepEnumerator();
			_selectedPropertyEnum.RestrictedToThisLevelAndUnder = false;
			_selectedPropertyEnum.RestrictedToThisPropertyAndUnder = false;
		}
		if (!flag2)
		{
			return;
		}
		if (base.IsHandleCreated)
		{
			if (_parentCtrl.CommentsVisibility)
			{
				_parentCtrl.Refresh();
			}
			else
			{
				Refresh();
			}
		}
		if (_selectedPropertyEnum != RightBound)
		{
			_parentCtrl.OnPropertySelected(new PropertySelectedEventArgs(_selectedPropertyEnum, selectedPropertyEnum));
		}
	}

	public void SelectAndFocusProperty(PropertyEnumerator enumerator, bool selectAllText)
	{
		SelectProperty(enumerator, Point.Empty);
		if (enumerator.Property != null && InPlaceControl != null)
		{
			TextBox textBox = (InPlaceControl as IInPlaceControl).TextBox;
			if (!InPlaceControl.ContainsFocus)
			{
				_parentCtrl.ActiveControl = InPlaceControl;
			}
			else if (textBox != null && !textBox.Focused)
			{
				textBox.Focus();
			}
			if (selectAllText)
			{
				SelectAllText();
			}
			else
			{
				textBox?.Select(textBox.Text.Length, 0);
			}
		}
	}

	public void HideInPlaceControl()
	{
		if (_hideInPlaceControlInProgress)
		{
			return;
		}
		_hideInPlaceControlInProgress = true;
		if (_currentInPlaceControl != null && _currentInPlaceControl.IsHandleCreated)
		{
			if (_currentInPlaceControl.ContainsFocus)
			{
				_parentCtrl.ActiveControl = this;
			}
			_currentInPlaceControl.Hide();
			_currentInPlaceControl.GotFocus -= OnChildGotFocus;
			_parentCtrl.OnInPlaceCtrlHidden(new InPlaceCtrlVisibleEventArgs((_currentInPlaceControl as IInPlaceControl).OwnerPropertyEnumerator, _currentInPlaceControl));
			_currentInPlaceControl = null;
			_mouseHook.HookMessages = false;
			_keyboardHook.HookMessages = false;
			_messageHook.HookMessages = false;
		}
		_hideInPlaceControlInProgress = false;
	}

	public void ShowInPlaceControl(PropertyEnumerator enumerator)
	{
		ShowInPlaceControl(enumerator, Point.Empty);
	}

	public void ShowInPlaceControl(PropertyEnumerator enumerator, Point mousePos)
	{
		if (enumerator.Property == null || !base.ContainsFocus || (!Enabled && DisableMode == PropertyGrid.DisableModes.NoValueSelection))
		{
			return;
		}
		if (enumerator.Property.Enabled && (base.Parent.ContainsFocus || base.Parent.Focused))
		{
			HideInPlaceControl();
			Rectangle itemRect = GetItemRect(enumerator);
			Rectangle clientRectangle = base.ClientRectangle;
			if (itemRect.Top >= clientRectangle.Bottom || itemRect.Bottom <= clientRectangle.Top)
			{
				return;
			}
			itemRect.X += LeftColumnWidth + 1 + LabelColumnWidth + 1;
			itemRect.Width -= LeftColumnWidth + 1 + LabelColumnWidth + 1;
			if (enumerator.Property.Feel == PropertyFeel.Empty || enumerator.Property.Feel == null)
			{
				return;
			}
			_keyboardHook.HookMessages = true;
			_mouseHook.HookMessages = true;
			_messageHook.HookMessages = true;
			_currentInPlaceControl = enumerator.Property.Feel.ShowControl(itemRect, enumerator);
			if (_currentInPlaceControl != null)
			{
				_parentCtrl.OnInPlaceCtrlVisible(new InPlaceCtrlVisibleEventArgs(enumerator, _currentInPlaceControl));
				_currentInPlaceControl.GotFocus += OnChildGotFocus;
			}
			if (!_scrollingInProgress && InPlaceControl != null && _parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey && (_parentCtrl.TabKeyNavigationMode & PropertyGrid.TabKeyNavigationModes.TabKeyWithAutoFocus) != PropertyGrid.TabKeyNavigationModes.NoSubMode)
			{
				itemRect = _currentInPlaceControl.Bounds;
				Point p = PointToScreen(mousePos);
				p = _currentInPlaceControl.PointToClient(p);
				if (itemRect.Contains(mousePos) && !TryOnForbidClickThrough(p))
				{
					_parentCtrl.ClickThroughInProgress = true;
				}
				else
				{
					_parentCtrl.ActiveControl = InPlaceControl;
				}
				if (_parentCtrl.ClickThroughInProgress)
				{
					_parentCtrl.ClickThroughInProgress = false;
				}
				else
				{
					SelectAllText();
				}
			}
		}
		else
		{
			HideInPlaceControl();
		}
	}

	public void FocusTextBox()
	{
		if (_currentInPlaceControl != null)
		{
			if ((_currentInPlaceControl as IInPlaceControl).TextBox != null)
			{
				(_currentInPlaceControl as IInPlaceControl).TextBox.Focus();
			}
			else if (_currentInPlaceControl is TextBoxBase)
			{
				_currentInPlaceControl.Focus();
			}
		}
	}

	public void SelectAllText()
	{
		if (_currentInPlaceControl != null)
		{
			if ((_currentInPlaceControl as IInPlaceControl).TextBox != null)
			{
				(_currentInPlaceControl as IInPlaceControl).TextBox.SelectAll();
			}
			else if (_currentInPlaceControl is TextBoxBase)
			{
				(_currentInPlaceControl as TextBoxBase).SelectAll();
			}
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		return keyData switch
		{
			Keys.Down => true, 
			Keys.Up => true, 
			Keys.Right => true, 
			Keys.Left => true, 
			_ => base.IsInputKey(keyData), 
		};
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			if (_currentInPlaceControl != null)
			{
				Win32Calls.RECT rc = default(Win32Calls.RECT);
				Win32Calls.GetWindowRect(_currentInPlaceControl.Handle, ref rc);
				if (Rectangle.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Bottom).Contains(Control.MousePosition))
				{
					base.OnKeyDown(e);
					return;
				}
			}
			Point point = PointToClient(Control.MousePosition);
			PropertyEnumerator propertyEnumerator = PropertyItemFromPoint(point, out var itemRect);
			if (propertyEnumerator.Property != null)
			{
				propertyEnumerator.Property.OverrideCursor(point, itemRect, propertyEnumerator);
			}
			base.OnKeyDown(e);
			return;
		}
		Property property = _selectedPropertyEnum.Property;
		if (property != null)
		{
			if (_currentInPlaceControl != null && _currentInPlaceControl is IInPlaceControl inPlaceControl && inPlaceControl.OnParentKeyDown(e))
			{
				e.SuppressKeyPress = true;
				return;
			}
			switch (e.KeyCode)
			{
			case Keys.F4:
				if (_currentInPlaceControl != null && e.Modifiers == Keys.None && _currentInPlaceControl is IInPlaceControl inPlaceControl3)
				{
					inPlaceControl3.OnF4();
				}
				break;
			case Keys.Return:
			case Keys.Space:
				if (SelectedPropertyEnumerator.Property.CanBeDisabledManually && e.KeyCode == Keys.Space)
				{
					if (!SelectedPropertyEnumerator.HasParent)
					{
						EnableProperty(SelectedPropertyEnumerator, !SelectedPropertyEnumerator.Property.Enabled);
						e.SuppressKeyPress = true;
					}
					else if (SelectedPropertyEnumerator.Parent.Property.Enabled)
					{
						EnableProperty(SelectedPropertyEnumerator, !SelectedPropertyEnumerator.Property.Enabled);
						e.SuppressKeyPress = true;
					}
				}
				else if (SelectedPropertyEnumerator.Property is PropertyHyperLink)
				{
					if (SelectedPropertyEnumerator.Property.Enabled)
					{
						PropertyHyperLinkClickedEventArgs e2 = new PropertyHyperLinkClickedEventArgs(SelectedPropertyEnumerator, SelectedPropertyEnumerator.Property.HyperLink);
						_parentCtrl.OnHyperLinkPropertyClicked(e2);
					}
				}
				else if ((HasOneVisibleChild(SelectedPropertyEnumerator) && (SelectedPropertyEnumerator.Property.Feel == null || !SelectedPropertyEnumerator.Property.Enabled)) || SelectedPropertyEnumerator.Property is RootCategory)
				{
					Property property2 = SelectedPropertyEnumerator.Property;
					ExpandProperty(SelectedPropertyEnumerator, !property2.Expanded);
				}
				break;
			case Keys.Right:
			case Keys.Add:
				if (e.KeyCode == Keys.Right && Control.ModifierKeys == Keys.Control)
				{
					SetLabelColumnWidthRatio((double)(LabelColumnWidth + 3) / (double)(base.ClientRectangle.Width - LeftColumnWidth));
					break;
				}
				if (!SelectedPropertyEnumerator.Property.Expanded && HasOneVisibleChild(SelectedPropertyEnumerator))
				{
					ExpandProperty(SelectedPropertyEnumerator, expand: true);
					break;
				}
				if (e.KeyCode == Keys.Add)
				{
					break;
				}
				goto case Keys.Down;
			case Keys.Down:
			{
				if (e.Modifiers == Keys.Alt && _currentInPlaceControl != null && _currentInPlaceControl is IInPlaceControl inPlaceControl2 && !SelectedPropertyEnumerator.Property.Value.IsReadOnly(SelectedPropertyEnumerator))
				{
					inPlaceControl2.OnF4();
					break;
				}
				PropertyVisibleDeepEnumerator propertyVisibleDeepEnumerator = new PropertyVisibleDeepEnumerator(_selectedPropertyEnum.Node);
				propertyVisibleDeepEnumerator.MoveNext();
				Property property3 = propertyVisibleDeepEnumerator.Property;
				if (property3 == null)
				{
					break;
				}
				Rectangle clientRectangle3 = base.ClientRectangle;
				Rectangle absoluteItemRect2 = GetAbsoluteItemRect(propertyVisibleDeepEnumerator);
				if (absoluteItemRect2.Top < clientRectangle3.Top)
				{
					do
					{
						ScrollLineUp();
					}
					while (GetAbsoluteItemRect(propertyVisibleDeepEnumerator).Top != clientRectangle3.Top);
					EndScroll();
				}
				else if (absoluteItemRect2.Bottom > clientRectangle3.Bottom)
				{
					_parentCtrl.BeginUpdate();
					do
					{
						ScrollLineDown();
						absoluteItemRect2 = GetAbsoluteItemRect(propertyVisibleDeepEnumerator);
					}
					while (absoluteItemRect2.Bottom > clientRectangle3.Bottom && absoluteItemRect2.Top > clientRectangle3.Top);
					EndScroll();
					_parentCtrl.EndUpdate();
				}
				SelectProperty(propertyVisibleDeepEnumerator);
				break;
			}
			case Keys.Left:
			case Keys.Subtract:
				if (e.KeyCode == Keys.Left && Control.ModifierKeys == Keys.Control)
				{
					SetLabelColumnWidthRatio((double)(LabelColumnWidth - 3) / (double)(base.ClientRectangle.Width - LeftColumnWidth));
					break;
				}
				if (SelectedPropertyEnumerator.Property.Expanded && HasOneVisibleChild(SelectedPropertyEnumerator))
				{
					ExpandProperty(SelectedPropertyEnumerator, expand: false);
					break;
				}
				if (e.KeyCode == Keys.Subtract)
				{
					break;
				}
				goto case Keys.Up;
			case Keys.Up:
			{
				PropertyVisibleDeepEnumerator propertyVisibleDeepEnumerator2 = new PropertyVisibleDeepEnumerator(_selectedPropertyEnum.Node);
				propertyVisibleDeepEnumerator2.MovePrev();
				Property property4 = propertyVisibleDeepEnumerator2.Property;
				if (property4 == null)
				{
					break;
				}
				Rectangle clientRectangle4 = base.ClientRectangle;
				Rectangle absoluteItemRect3 = GetAbsoluteItemRect(propertyVisibleDeepEnumerator2);
				if (absoluteItemRect3.Top < clientRectangle4.Top)
				{
					_firstDisplayedLine = 1;
					FirstDisplayedProperty = propertyVisibleDeepEnumerator2;
					CheckScrollbar();
				}
				else if (absoluteItemRect3.Bottom > clientRectangle4.Bottom)
				{
					do
					{
						ScrollLineDown();
					}
					while (GetAbsoluteItemRect(propertyVisibleDeepEnumerator2).Bottom >= clientRectangle4.Bottom);
					EndScroll();
				}
				SelectProperty(propertyVisibleDeepEnumerator2);
				break;
			}
			case Keys.Next:
			{
				Rectangle clientRectangle5 = base.ClientRectangle;
				int num3 = clientRectangle5.Height / _basicPropertyHeight - 1;
				Rectangle absoluteItemRect4 = GetAbsoluteItemRect(SelectedPropertyEnumerator);
				int j = 0;
				PropertyEnumerator visibleDeepEnumerator7 = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
				int num4 = ((!(visibleDeepEnumerator7 == FirstDisplayedProperty)) ? 1 : _firstDisplayedLine);
				for (; j < num3; j++)
				{
					if (!(visibleDeepEnumerator7 != visibleDeepEnumerator7.RightBound))
					{
						break;
					}
					if (num4 == visibleDeepEnumerator7.Property.HeightMultiplier)
					{
						visibleDeepEnumerator7.MoveNext();
						num4 = 1;
					}
					else
					{
						num4++;
					}
				}
				if (absoluteItemRect4.Bottom < clientRectangle5.Top)
				{
					if (visibleDeepEnumerator7 == visibleDeepEnumerator7.RightBound)
					{
						visibleDeepEnumerator7.MovePrev();
						_firstDisplayedLine = visibleDeepEnumerator7.Property.HeightMultiplier;
					}
					else
					{
						_firstDisplayedLine = num4;
					}
					SelectProperty(visibleDeepEnumerator7);
					EnsureVisible(visibleDeepEnumerator7);
				}
				else if (visibleDeepEnumerator7 == visibleDeepEnumerator7.RightBound)
				{
					visibleDeepEnumerator7.MovePrev();
					num4 = visibleDeepEnumerator7.Property.HeightMultiplier;
					PropertyVisibleDeepEnumerator visibleDeepEnumerator8 = visibleDeepEnumerator7.GetVisibleDeepEnumerator();
					j = num3;
					while (visibleDeepEnumerator7 != _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst() && j > 0)
					{
						if (num4 > 1)
						{
							num4--;
						}
						else
						{
							visibleDeepEnumerator7.MovePrev();
							num4 = visibleDeepEnumerator7.Property.HeightMultiplier;
						}
						j--;
					}
					_firstDisplayedLine = num4;
					FirstDisplayedProperty = visibleDeepEnumerator7 as PropertyVisibleDeepEnumerator;
					SelectProperty(visibleDeepEnumerator8);
				}
				else if (num3 < visibleDeepEnumerator7.Property.HeightMultiplier)
				{
					if (visibleDeepEnumerator7 != _activePropertyCollection.GetVisibleDeepEnumerator().MoveLast())
					{
						if (visibleDeepEnumerator7 == SelectedPropertyEnumerator)
						{
							visibleDeepEnumerator7.MoveNext();
						}
						_firstDisplayedLine = 1;
						FirstDisplayedProperty = visibleDeepEnumerator7 as PropertyVisibleDeepEnumerator;
						SelectProperty(visibleDeepEnumerator7);
					}
				}
				else
				{
					FirstDisplayedProperty = SelectedPropertyEnumerator;
					SelectProperty(visibleDeepEnumerator7);
				}
				Invalidate();
				break;
			}
			case Keys.Prior:
			{
				Rectangle clientRectangle2 = base.ClientRectangle;
				int num = clientRectangle2.Height / _basicPropertyHeight - 1;
				int i = 0;
				PropertyVisibleDeepEnumerator visibleDeepEnumerator5 = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
				int num2 = visibleDeepEnumerator5.Property.HeightMultiplier;
				for (; i < num; i++)
				{
					if (!(visibleDeepEnumerator5 != _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst()))
					{
						break;
					}
					if (num2 == 1)
					{
						visibleDeepEnumerator5.MovePrev();
						num2 = visibleDeepEnumerator5.Property.HeightMultiplier;
					}
					else
					{
						num2--;
					}
				}
				if (i == 0 && visibleDeepEnumerator5 != _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst())
				{
					visibleDeepEnumerator5.MovePrev();
					i = 1;
				}
				Rectangle absoluteItemRect = GetAbsoluteItemRect(visibleDeepEnumerator5);
				if (absoluteItemRect.Top >= clientRectangle2.Top && absoluteItemRect.Bottom < clientRectangle2.Bottom)
				{
					SelectProperty(visibleDeepEnumerator5);
				}
				else if (absoluteItemRect.Bottom > clientRectangle2.Bottom)
				{
					PropertyEnumerator visibleDeepEnumerator6 = visibleDeepEnumerator5.GetVisibleDeepEnumerator();
					num2 = 1;
					while (i > 0 && visibleDeepEnumerator6 != _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst())
					{
						if (num2 == 1)
						{
							visibleDeepEnumerator6.MovePrev();
							num2 = visibleDeepEnumerator6.Property.HeightMultiplier;
						}
						else
						{
							num2--;
						}
						i--;
					}
					FirstDisplayedProperty = visibleDeepEnumerator6 as PropertyVisibleDeepEnumerator;
					SelectProperty(visibleDeepEnumerator5);
				}
				else if (absoluteItemRect.Top < clientRectangle2.Top)
				{
					_firstDisplayedLine = 1;
					FirstDisplayedProperty = visibleDeepEnumerator5;
					SelectProperty(FirstDisplayedProperty);
				}
				Refresh();
				break;
			}
			case Keys.Home:
			{
				int scrollPos2 = Win32Calls.GetScrollPos(base.Handle, 1);
				Win32Calls.SetScrollPos(base.Handle, 1, 0, bRedraw: true);
				_firstDisplayedPropertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator();
				_firstDisplayedPropertyEnumerator.MoveFirst();
				_firstDisplayedLine = 1;
				SelectProperty(FirstDisplayedProperty);
				ScrollEventArgs e4 = new ScrollEventArgs(ScrollEventType.ThumbPosition, scrollPos2, 0, ScrollOrientation.VerticalScroll);
				_parentCtrl.NotifyScroll(e4);
				break;
			}
			case Keys.End:
			{
				Rectangle clientRectangle = base.ClientRectangle;
				_firstDisplayedPropertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator();
				_firstDisplayedPropertyEnumerator.MoveLast();
				_firstDisplayedLine = 1;
				PropertyVisibleDeepEnumerator visibleDeepEnumerator3 = _firstDisplayedPropertyEnumerator.GetVisibleDeepEnumerator();
				Rectangle itemRect2 = GetItemRect(visibleDeepEnumerator3);
				while (_firstDisplayedPropertyEnumerator != _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst() && clientRectangle.Bottom - itemRect2.Bottom > _basicPropertyHeight)
				{
					if (_firstDisplayedLine > 1)
					{
						_firstDisplayedLine--;
					}
					else
					{
						_firstDisplayedPropertyEnumerator.MovePrev();
						_firstDisplayedLine = _firstDisplayedPropertyEnumerator.Property.HeightMultiplier;
					}
					itemRect2 = GetItemRect(visibleDeepEnumerator3);
				}
				Win32Calls.SCROLLINFO lpScrollInfo = default(Win32Calls.SCROLLINFO);
				lpScrollInfo.cbSize = Marshal.SizeOf(lpScrollInfo);
				lpScrollInfo.fMask = 3;
				Win32Calls.GetScrollInfo(base.Handle, 1, ref lpScrollInfo);
				int scrollPos = Win32Calls.GetScrollPos(base.Handle, 1);
				Win32Calls.SetScrollPos(base.Handle, 1, lpScrollInfo.nMax - (lpScrollInfo.nPage - 1), bRedraw: true);
				PropertyVisibleDeepEnumerator visibleDeepEnumerator4 = _activePropertyCollection.GetVisibleDeepEnumerator();
				visibleDeepEnumerator4.MoveLast();
				SelectProperty(visibleDeepEnumerator4);
				ScrollEventArgs e3 = new ScrollEventArgs(ScrollEventType.ThumbPosition, scrollPos, lpScrollInfo.nMax - (lpScrollInfo.nPage - 1), ScrollOrientation.VerticalScroll);
				_parentCtrl.NotifyScroll(e3);
				break;
			}
			case Keys.A:
				if (!e.Control)
				{
					break;
				}
				if (_multiSelectMode == PropertyGrid.MultiSelectModes.SameLevel)
				{
					PropertyEnumerator propertyEnumerator2 = ((SelectedPropertyEnumerator.Children.Count != 0) ? SelectedPropertyEnumerator : SelectedPropertyEnumerator.Parent);
					PropertyEnumerator visibleDeepEnumerator = propertyEnumerator2.Children.GetVisibleDeepEnumerator();
					visibleDeepEnumerator.RestrictedToThisLevelAndUnder = true;
					while (visibleDeepEnumerator != propertyEnumerator2.Children.RightBound)
					{
						if (_parentCtrl.OnValidateMultiSelection(SelectedPropertyEnumerator, visibleDeepEnumerator as PropertyVisibleDeepEnumerator))
						{
							visibleDeepEnumerator.Property.SelectedInternal = true;
							_multiSelectedItems.Add(visibleDeepEnumerator);
						}
						visibleDeepEnumerator.MoveNext();
					}
					Invalidate();
				}
				else
				{
					if (_multiSelectMode != PropertyGrid.MultiSelectModes.Global)
					{
						break;
					}
					PropertyEnumerator visibleDeepEnumerator2 = _activePropertyCollection.GetVisibleDeepEnumerator();
					visibleDeepEnumerator2.MoveFirst();
					while (visibleDeepEnumerator2 != visibleDeepEnumerator2.RightBound)
					{
						if (visibleDeepEnumerator2 != SelectedPropertyEnumerator && visibleDeepEnumerator2.Children.Count == 0 && _parentCtrl.OnValidateMultiSelection(SelectedPropertyEnumerator, visibleDeepEnumerator2 as PropertyVisibleDeepEnumerator))
						{
							visibleDeepEnumerator2.Property.SelectedInternal = true;
							_multiSelectedItems.Add(visibleDeepEnumerator2);
						}
						visibleDeepEnumerator2.MoveNext();
					}
					Invalidate();
				}
				break;
			}
		}
		base.OnKeyDown(e);
	}

	protected override void OnKeyUp(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.ControlKey)
		{
			if (_currentInPlaceControl != null)
			{
				Win32Calls.RECT rc = default(Win32Calls.RECT);
				Win32Calls.GetWindowRect(_currentInPlaceControl.Handle, ref rc);
				if (Rectangle.FromLTRB(rc.Left, rc.Top, rc.Right, rc.Bottom).Contains(Control.MousePosition))
				{
					base.OnKeyUp(e);
					return;
				}
			}
			Point point = PointToClient(Control.MousePosition);
			PropertyEnumerator propertyEnumerator = PropertyItemFromPoint(point, out var _);
			if (!(propertyEnumerator.Property is PropertyHyperLink))
			{
				Cursor = Cursors.Arrow;
			}
		}
		base.OnKeyUp(e);
	}

	protected override bool ProcessDialogChar(char charCode)
	{
		switch (charCode)
		{
		case '+':
		case '-':
			if (SelectedPropertyEnumerator != RightBound && SelectedPropertyEnumerator.Children.Count > 0 && HasOneVisibleChild(SelectedPropertyEnumerator))
			{
				return true;
			}
			break;
		case '\b':
			return true;
		}
		return base.ProcessDialogChar(charCode);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		bool flag = false;
		if (e.KeyChar == '*')
		{
			Property property = SelectedPropertyEnumerator.Property;
			if (SelectedPropertyEnumerator.Children.Count > 0 || property.LazyLoadingState == Property.LazyLoadingStates.HasChildren)
			{
				PropertyEnumerator deepEnumerator = SelectedPropertyEnumerator.GetDeepEnumerator();
				deepEnumerator.RestrictedToThisPropertyAndUnder = true;
				ExpandAllProperties(deepEnumerator, expand: true);
				flag = true;
			}
		}
		else if (e.KeyChar == '/')
		{
			Property property2 = SelectedPropertyEnumerator.Property;
			if (property2.Expanded && SelectedPropertyEnumerator.Children.Count > 0)
			{
				PropertyEnumerator deepEnumerator2 = SelectedPropertyEnumerator.GetDeepEnumerator();
				deepEnumerator2.RestrictedToThisPropertyAndUnder = true;
				ExpandAllProperties(deepEnumerator2, expand: false);
				flag = true;
			}
		}
		if (!flag && _currentInPlaceControl != null)
		{
			_parentCtrl.ActiveControl = _currentInPlaceControl;
			SelectAllText();
			Win32Calls.PostMessage(_currentInPlaceControl.Handle, 258, (IntPtr)e.KeyChar, IntPtr.Zero);
		}
		base.OnKeyPress(e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		Invalidate();
		if (_dontShowInPlaceCtrlInGotFocus)
		{
			_dontShowInPlaceCtrlInGotFocus = false;
			return;
		}
		Property property = SelectedPropertyEnumerator.Property;
		if (property != null && property.InPlaceCtrlInAction == null)
		{
			ShowInPlaceControl(SelectedPropertyEnumerator);
		}
	}

	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		if (_currentInPlaceControl != null && !_currentInPlaceControl.ContainsFocus)
		{
			HideInPlaceControl();
		}
		Invalidate();
	}

	public void ExpandProperty(PropertyEnumerator propEnum, bool expand)
	{
		if (propEnum == propEnum.RightBound)
		{
			return;
		}
		Property property = propEnum.Property;
		if (expand == property.Expanded)
		{
			return;
		}
		if (property.Expanded && !expand)
		{
			if (SelectedPropertyEnumerator.IsDescendantOf(propEnum))
			{
				SelectProperty(propEnum);
			}
			if (FirstDisplayedProperty.IsDescendantOf(propEnum))
			{
				FirstDisplayedProperty = propEnum.GetVisibleDeepEnumerator();
			}
		}
		if (expand && _parentCtrl.LazyLoading && propEnum.Property.LazyLoadingState != Property.LazyLoadingStates.ContentLoaded)
		{
			if (DisplayMode == PropertyGrid.DisplayModes.Categorized)
			{
				AddChildProperties(propEnum);
			}
			else
			{
				_activePropertyCollection = _categorizedPropertyCollection;
				AddChildProperties(propEnum.CategorizedPropertyEnumerator);
				_activePropertyCollection = _flatPropertyCollection;
				_displayModeChangeInProgress = true;
				PropertyEnumerator newSelectedEnum = null;
				CloneHierarchyUnder(propEnum.CategorizedPropertyEnumerator, propEnum, SelectedPropertyEnumerator.Property, ref newSelectedEnum);
				_displayModeChangeInProgress = false;
			}
		}
		property.ExpandedInternal = expand;
		_parentCtrl.OnPropertyExpanded(new PropertyExpandedEventArgs(propEnum, expand));
		if (propEnum.Children.Count > 0)
		{
			InvalidateVisibleItemCount();
			if (base.IsHandleCreated)
			{
				Rectangle clientRectangle = base.ClientRectangle;
				OnSizeChanged(clientRectangle.Width, clientRectangle.Height);
				Invalidate();
			}
		}
	}

	public void ExpandAllProperties(bool expand)
	{
		ExpandAllProperties(FirstProperty, expand);
	}

	public void ExpandAllProperties(PropertyEnumerator propEnum, bool expand)
	{
		_parentCtrl.BeginUpdate();
		int depth = propEnum.Depth;
		while (propEnum != propEnum.RightBound)
		{
			if (propEnum.Depth - depth < _expandAllMaxLevels)
			{
				ExpandProperty(propEnum, expand);
			}
			propEnum.MoveNext();
		}
		_parentCtrl.EndUpdate();
	}

	private void InvalidateVisibleItemCount()
	{
		_visibleItemCountValid = false;
	}

	private int GetVisibleItemCount()
	{
		if (_visibleItemCountValid)
		{
			return _visibleItemCount;
		}
		_visibleItemCountValid = true;
		_visibleItemCount = 0;
		_visibleLineCount = 0;
		PropertyVisibleDeepEnumerator visibleDeepEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator();
		visibleDeepEnumerator.MoveFirst();
		while (visibleDeepEnumerator != visibleDeepEnumerator.RightBound)
		{
			_visibleLineCount += visibleDeepEnumerator.Property.HeightMultiplier;
			_visibleItemCount++;
			visibleDeepEnumerator.MoveNext();
		}
		return _visibleItemCount;
	}

	private int GetVisibleLineCount()
	{
		if (!_visibleItemCountValid)
		{
			GetVisibleItemCount();
		}
		return _visibleLineCount;
	}

	private bool TryOnForbidClickThrough(Point pt)
	{
		MethodInfo method = _currentInPlaceControl.GetType().GetMethod("OnForbidClickThrough");
		if (method != null)
		{
			return (bool)method.Invoke(_currentInPlaceControl, new object[1] { pt });
		}
		return false;
	}

	internal void CloseEmptySpace()
	{
		PropertyVisibleDeepEnumerator visibleDeepEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator();
		visibleDeepEnumerator.MoveLast();
		if (visibleDeepEnumerator.Property == null)
		{
			return;
		}
		Rectangle clientRectangle = base.ClientRectangle;
		Rectangle absoluteItemRect = GetAbsoluteItemRect(visibleDeepEnumerator);
		if (!IsScrollbarVisible())
		{
			while (clientRectangle.Bottom - absoluteItemRect.Bottom >= _basicPropertyHeight && (FirstDisplayedProperty.MovePrev() != LeftBound || _firstDisplayedLine > 1))
			{
				if (_firstDisplayedLine > 1)
				{
					_firstDisplayedLine--;
				}
				else
				{
					_firstDisplayedPropertyEnumerator.MovePrev();
					_firstDisplayedLine = FirstDisplayedProperty.Property.HeightMultiplier;
				}
				CheckScrollbar();
				absoluteItemRect = GetAbsoluteItemRect(visibleDeepEnumerator);
			}
			return;
		}
		bool flag = false;
		while (clientRectangle.Bottom - absoluteItemRect.Bottom >= _basicPropertyHeight && Win32Calls.GetScrollPos(base.Handle, 1) > 0)
		{
			ScrollLineUp();
			absoluteItemRect = GetAbsoluteItemRect(visibleDeepEnumerator);
			flag = true;
		}
		if (flag)
		{
			EndScroll();
		}
	}

	internal void BeginVerticalFreeze()
	{
		if (_beginVerticalFreezeCounter == 0)
		{
			_scrollPosBeforeVerticalFreeze = Win32Calls.GetScrollPos(base.Handle, 1);
		}
		_beginVerticalFreezeCounter++;
	}

	internal void EndVerticalFreeze()
	{
		_beginVerticalFreezeCounter--;
		if (_beginVerticalFreezeCounter == 0)
		{
			Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(4, _scrollPosBeforeVerticalFreeze), IntPtr.Zero);
			CloseEmptySpace();
			CheckScrollbar();
		}
	}

	private void OnSizeChanged(int width, int height)
	{
		if (!IsVerticallyFrozen)
		{
			CloseEmptySpace();
			CheckScrollbar();
		}
	}

	internal void HeightMultiplierChanged()
	{
		InvalidateVisibleItemCount();
		CheckScrollbar();
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		if (base.Size.Height != 0)
		{
			OnSizeChanged(base.Size.Width, base.Size.Height);
		}
	}

	internal void CheckScrollbar()
	{
		if (IsVerticallyFrozen || _displayModeChangeInProgress || !base.IsHandleCreated)
		{
			return;
		}
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Height == 0)
		{
			return;
		}
		Win32Calls.SCROLLINFO lpsi = default(Win32Calls.SCROLLINFO);
		lpsi.cbSize = Marshal.SizeOf(lpsi);
		int visibleLineCount = GetVisibleLineCount();
		if (visibleLineCount > 1 && visibleLineCount * _basicPropertyHeight > clientRectangle.Height)
		{
			if (!IsScrollbarVisible())
			{
				if (!_initializing)
				{
					int labelColumnWidth = LabelColumnWidth;
					Win32Calls.ShowScrollBar(base.Handle, 1, bShow: true);
					LabelColumnWidthRatio = (double)labelColumnWidth / (double)(base.ClientSize.Width - LeftColumnWidth);
				}
				Invalidate(new Rectangle(LeftColumnWidth + LabelColumnWidth, 0, clientRectangle.Width - (LeftColumnWidth + LabelColumnWidth), clientRectangle.Height), invalidateChildren: false);
				Invalidate(invalidateChildren: false);
				if (_currentInPlaceControl != null)
				{
					_currentInPlaceControl.Refresh();
				}
			}
			lpsi.nMin = 0;
			lpsi.nMax = Math.Max(1, visibleLineCount - 1);
			lpsi.nPage = Math.Max(1, clientRectangle.Height / _basicPropertyHeight);
			lpsi.fMask = 3;
			Win32Calls.SetScrollInfo(base.Handle, 1, ref lpsi, fRedraw: true);
			Win32Calls.EnableScrollBar(base.Handle, 1, 0);
			Win32Calls.GetScrollInfo(base.Handle, 1, ref lpsi);
			if (Win32Calls.GetScrollPos(base.Handle, 1) == lpsi.nMax - (lpsi.nPage - 1))
			{
				Win32Calls.EnableScrollBar(base.Handle, 1, 2);
			}
			else if (Win32Calls.GetScrollPos(base.Handle, 1) == 0)
			{
				Win32Calls.EnableScrollBar(base.Handle, 1, 1);
			}
			PropertyVisibleDeepEnumerator visibleDeepEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator();
			visibleDeepEnumerator.MoveFirst();
			int num = 0;
			while (visibleDeepEnumerator != _firstDisplayedPropertyEnumerator)
			{
				num += visibleDeepEnumerator.Property.HeightMultiplier;
				visibleDeepEnumerator.MoveNext();
			}
			if (_firstDisplayedPropertyEnumerator != _firstDisplayedPropertyEnumerator.RightBound)
			{
				num += _firstDisplayedLine - 1;
			}
			if (num != Win32Calls.GetScrollPos(base.Handle, 1))
			{
				Win32Calls.SetScrollPos(base.Handle, 1, num, bRedraw: true);
			}
		}
		else if (IsScrollbarVisible())
		{
			if (!_initializing)
			{
				int labelColumnWidth2 = LabelColumnWidth;
				Win32Calls.ShowScrollBar(base.Handle, 1, bShow: false);
				LabelColumnWidthRatio = (double)labelColumnWidth2 / (double)(base.ClientSize.Width - LeftColumnWidth);
			}
			Win32Calls.SetScrollPos(base.Handle, 1, 0, bRedraw: true);
			lpsi.nMin = 0;
			lpsi.nMax = 0;
			lpsi.nPage = 0;
			lpsi.fMask = 3;
			Win32Calls.SetScrollInfo(base.Handle, 1, ref lpsi, fRedraw: true);
			Invalidate(new Rectangle(LeftColumnWidth + LabelColumnWidth, 0, clientRectangle.Width - (LeftColumnWidth + LabelColumnWidth), clientRectangle.Height), invalidateChildren: false);
			Invalidate(invalidateChildren: false);
			if (_currentInPlaceControl != null)
			{
				_currentInPlaceControl.Refresh();
			}
		}
		Property property = SelectedPropertyEnumerator.Property;
		if (property == null || !property.Enabled)
		{
			return;
		}
		Rectangle itemRect = GetItemRect(SelectedPropertyEnumerator);
		itemRect = property.GetValueRect(itemRect);
		PropertyFeel feel = property.Feel;
		if (feel != null && feel != PropertyFeel.Empty)
		{
			if (itemRect.Bottom <= clientRectangle.Top || itemRect.Top >= clientRectangle.Bottom)
			{
				HideInPlaceControl();
				return;
			}
			if (_currentInPlaceControl == null)
			{
				ShowInPlaceControl(SelectedPropertyEnumerator);
				return;
			}
			feel.MoveControl(itemRect, SelectedPropertyEnumerator);
			_currentInPlaceControl.Refresh();
		}
	}

	private void ResetScrollPosToFirstDisplayedProperty()
	{
		if (_displayModeChangeInProgress || !IsScrollbarVisible())
		{
			return;
		}
		PropertyEnumerator propertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst();
		int num = 0;
		int num2 = 1;
		while (propertyEnumerator != FirstDisplayedProperty || num2 != _firstDisplayedLine)
		{
			if (num2 == propertyEnumerator.Property.HeightMultiplier)
			{
				propertyEnumerator.MoveNext();
				num2 = 1;
			}
			else
			{
				num2++;
			}
			num++;
		}
		int scrollPos = Win32Calls.GetScrollPos(base.Handle, 1);
		Win32Calls.SetScrollPos(base.Handle, 1, num, bRedraw: true);
		ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.ThumbPosition, scrollPos, num, ScrollOrientation.VerticalScroll);
		_parentCtrl.NotifyScroll(e);
	}

	public bool CheckPropertyVisibility(PropertyEnumerator propEnum)
	{
		PropertyEnumerator propertyEnumerator = new PropertyDeepEnumerator(propEnum.Node);
		do
		{
			if (!propertyEnumerator.Property.Visible)
			{
				return false;
			}
			if (propertyEnumerator.HasParent)
			{
				propertyEnumerator = propertyEnumerator.Parent;
				if (!propertyEnumerator.Property.Expanded)
				{
					return false;
				}
			}
		}
		while (propertyEnumerator.HasParent);
		return true;
	}

	public bool EnsureVisible(PropertyEnumerator propEnum)
	{
		if (propEnum == propEnum.RightBound)
		{
			return false;
		}
		if (DisplayMode == PropertyGrid.DisplayModes.Categorized)
		{
			PropertyEnumerator propertyEnumerator = propEnum;
			while (propertyEnumerator.HasParent)
			{
				propertyEnumerator = propertyEnumerator.Parent;
				if (!propertyEnumerator.Property.Expanded)
				{
					ShowProperty(propertyEnumerator, show: true);
					ExpandProperty(propertyEnumerator, expand: true);
				}
			}
		}
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Height > 0)
		{
			Rectangle absoluteItemRect = GetAbsoluteItemRect(propEnum);
			if (absoluteItemRect.Top < clientRectangle.Top)
			{
				_firstDisplayedLine = 1;
				FirstDisplayedProperty = propEnum.GetVisibleDeepEnumerator();
			}
			else
			{
				bool flag = false;
				while (absoluteItemRect.Bottom > clientRectangle.Bottom)
				{
					flag = true;
					Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(1, 0), IntPtr.Zero);
					absoluteItemRect = GetAbsoluteItemRect(propEnum);
				}
				if (flag)
				{
					Win32Calls.SendMessage(base.Handle, 277, (IntPtr)Win32Calls.MakeLong(8, 0), IntPtr.Zero);
				}
			}
			OnSizeChanged(clientRectangle.Width, clientRectangle.Height);
			Refresh();
		}
		return true;
	}

	public void ShowProperty(PropertyEnumerator propEnum, bool show)
	{
		if (propEnum == propEnum.RightBound || propEnum.Property.Visible == show)
		{
			return;
		}
		propEnum.Property.VisibleInternal = show;
		InvalidateVisibleItemCount();
		if (!show)
		{
			if (SelectedPropertyEnumerator == propEnum)
			{
				PropertyEnumerator visibleDeepEnumerator = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
				PropertyEnumerator propertyEnumerator = SelectedPropertyEnumerator.GetVisibleDeepEnumerator().MovePrev();
				if (visibleDeepEnumerator != visibleDeepEnumerator.RightBound)
				{
					SelectProperty(visibleDeepEnumerator);
				}
				else if (propertyEnumerator != propertyEnumerator.LeftBound)
				{
					SelectProperty(propertyEnumerator);
				}
				else
				{
					SelectProperty(visibleDeepEnumerator);
				}
			}
			if (_firstDisplayedPropertyEnumerator == propEnum || _firstDisplayedPropertyEnumerator.IsDescendantOf(propEnum))
			{
				PropertyEnumerator visibleDeepEnumerator2 = _firstDisplayedPropertyEnumerator.GetVisibleDeepEnumerator();
				PropertyEnumerator propertyEnumerator2 = _firstDisplayedPropertyEnumerator.GetVisibleDeepEnumerator().MovePrev();
				if (visibleDeepEnumerator2 != visibleDeepEnumerator2.RightBound)
				{
					_firstDisplayedPropertyEnumerator = visibleDeepEnumerator2 as PropertyVisibleDeepEnumerator;
				}
				else if (propertyEnumerator2 != propertyEnumerator2.LeftBound)
				{
					_firstDisplayedPropertyEnumerator = propertyEnumerator2 as PropertyVisibleDeepEnumerator;
				}
				else
				{
					_firstDisplayedPropertyEnumerator = visibleDeepEnumerator2 as PropertyVisibleDeepEnumerator;
				}
				_firstDisplayedLine = 1;
				ResetScrollPosToFirstDisplayedProperty();
			}
		}
		if (!_lockRefresh && base.IsHandleCreated)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			OnSizeChanged(clientRectangle.Width, clientRectangle.Height);
			Refresh();
		}
	}

	public void Clear()
	{
		InvalidateVisibleItemCount();
		HideInPlaceControl();
		_categorizedPropertyCollection.Clear();
		_flatPropertyCollection.Clear();
		_firstDisplayedLine = 1;
		_firstDisplayedPropertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator().RightBound.GetVisibleDeepEnumerator();
		_selectedPropertyEnum = _activePropertyCollection.GetVisibleDeepEnumerator().RightBound.GetVisibleDeepEnumerator();
		Invalidate();
		if (IsScrollbarVisible())
		{
			int scrollPos = Win32Calls.GetScrollPos(base.Handle, 1);
			Win32Calls.SetScrollPos(base.Handle, 1, 0, bRedraw: true);
			CheckScrollbar();
			ScrollEventArgs e = new ScrollEventArgs(ScrollEventType.ThumbPosition, scrollPos, 0, ScrollOrientation.VerticalScroll);
			_parentCtrl.NotifyScroll(e);
		}
	}

	public void ClearUnderProperty(PropertyEnumerator propEnum)
	{
		PropertyEnumerator propertyEnumerator = propEnum.Children;
		while (propertyEnumerator != propertyEnumerator.RightBound)
		{
			PropertyEnumerator propertyEnumerator2 = ((PropertyEnumerator)propertyEnumerator.Clone()).MoveNext();
			DeleteProperty(propertyEnumerator);
			propertyEnumerator = propertyEnumerator2;
		}
	}

	public PropertyEnumerator FindProperty(int propertyId)
	{
		return _categorizedPropertyCollection.Find(propertyId);
	}

	public PropertyEnumerator FindPropertyAfter(PropertyEnumerator afterEnumerator, int propertyId)
	{
		return _categorizedPropertyCollection.FindAfter(afterEnumerator, propertyId);
	}

	private int MeasureWidestLabel()
	{
		int num = 0;
		PropertyEnumerator propertyEnumerator = _activePropertyCollection.GetVisibleDeepEnumerator().MoveFirst();
		while (propertyEnumerator != propertyEnumerator.RightBound)
		{
			Property property = propertyEnumerator.Property;
			if ((!(property is RootCategory) || _parentCtrl.DrawManager.ShowCategoryVerticalLine) && !(property is PropertyHyperLink))
			{
				Graphics graphics = CreateGraphics();
				Size textExtent = Win32Calls.GetTextExtent(graphics, property.DisplayName, Font);
				graphics.Dispose();
				int num2 = property.GetLabelTextRect(GetItemRect(propertyEnumerator), propertyEnumerator).Left + textExtent.Width;
				if (num2 > num)
				{
					num = num2;
				}
			}
			propertyEnumerator.MoveNext();
		}
		if (num == 0)
		{
			return 0;
		}
		return num - LeftColumnWidth + _parentCtrl.GlobalTextMargin;
	}

	public void AdjustLabelColumn()
	{
		int num = MeasureWidestLabel();
		if (num > 0)
		{
			SetLabelColumnWidthRatio((double)num / (double)(base.ClientRectangle.Width - LeftColumnWidth));
		}
	}

	[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
	protected override bool ProcessDialogKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Return:
			if (_currentInPlaceControl == null)
			{
				break;
			}
			if (Focused)
			{
				SelectAllText();
				_parentCtrl.ActiveControl = _currentInPlaceControl;
			}
			else if (_currentInPlaceControl.ContainsFocus)
			{
				if (_parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys)
				{
					Focus();
					_currentInPlaceControl.Invalidate();
				}
				else if (_parentCtrl.TabToNextProperty())
				{
					return true;
				}
			}
			return true;
		case Keys.Escape:
			if (_currentInPlaceControl != null && _currentInPlaceControl.ContainsFocus)
			{
				if (_parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.ArrowKeys)
				{
					Focus();
					_currentInPlaceControl.Invalidate();
				}
				else
				{
					_parentCtrl.TabToNextProperty();
				}
			}
			return true;
		case Keys.Tab:
			if (Focused)
			{
				SelectAllText();
			}
			break;
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		Keys keys = keyData & Keys.KeyCode;
		if ((keyData & Keys.Control) != Keys.None && (keyData & Keys.Shift) == 0 && (keyData & Keys.Alt) == 0)
		{
			switch (keys)
			{
			case Keys.A:
				if (_currentInPlaceControl != null && ((_currentInPlaceControl as IInPlaceControl).TextBox != null || _currentInPlaceControl is TextBoxBase))
				{
					_currentInPlaceControl.Focus();
					SelectAllText();
				}
				return true;
			case Keys.C:
				DoCopy();
				return true;
			case Keys.V:
				DoPaste();
				return true;
			}
			_ = 88;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	public void DoCut()
	{
		if (_currentInPlaceControl == null)
		{
			return;
		}
		TextBoxBase textBoxBase = (_currentInPlaceControl as IInPlaceControl).TextBox;
		if (textBoxBase == null)
		{
			textBoxBase = _currentInPlaceControl as TextBoxBase;
		}
		if (textBoxBase != null)
		{
			_currentInPlaceControl.Focus();
			SelectAllText();
			textBoxBase.Cut();
			return;
		}
		string text = _currentInPlaceControl.Text;
		if (text != null && text.Length > 0)
		{
			Clipboard.SetDataObject(text);
		}
	}

	public void DoCopy()
	{
		if (_currentInPlaceControl == null)
		{
			return;
		}
		TextBoxBase textBoxBase = (_currentInPlaceControl as IInPlaceControl).TextBox;
		if (textBoxBase == null)
		{
			textBoxBase = _currentInPlaceControl as TextBoxBase;
		}
		if (textBoxBase != null)
		{
			_currentInPlaceControl.Focus();
			SelectAllText();
			textBoxBase.Copy();
			return;
		}
		string text = _currentInPlaceControl.Text;
		if (text != null && text.Length > 0)
		{
			Clipboard.SetDataObject(text);
		}
	}

	public void DoPaste()
	{
		if (_currentInPlaceControl == null)
		{
			return;
		}
		TextBoxBase textBoxBase = (_currentInPlaceControl as IInPlaceControl).TextBox;
		if (textBoxBase == null)
		{
			textBoxBase = _currentInPlaceControl as TextBoxBase;
		}
		if (textBoxBase != null)
		{
			if (!textBoxBase.ReadOnly)
			{
				if (textBoxBase.Focused)
				{
					textBoxBase.Paste();
					return;
				}
				_currentInPlaceControl.Focus();
				SelectAllText();
				textBoxBase.Paste();
			}
		}
		else
		{
			if (SelectedPropertyEnumerator.Property.Value.ReadOnly)
			{
				return;
			}
			string[] displayedValues = SelectedPropertyEnumerator.Property.Value.GetDisplayedValues();
			if (displayedValues.Length == 0)
			{
				return;
			}
			IDataObject dataObject = Clipboard.GetDataObject();
			if (dataObject == null)
			{
				return;
			}
			string text = (string)dataObject.GetData(typeof(string));
			if (text == null)
			{
				return;
			}
			string[] array = displayedValues;
			foreach (string value in array)
			{
				if (text.Equals(value))
				{
					_currentInPlaceControl.Text = text;
					break;
				}
			}
		}
	}

	private void OnChildGotFocus(object sender, EventArgs e)
	{
		_dontShowInPlaceCtrlInGotFocus = false;
	}

	private void OnChildLostFocusInternal()
	{
		if (Focused)
		{
			_dontShowInPlaceCtrlInGotFocus = true;
			if (_focusedControlAfterKillFocus == null)
			{
				(base.Parent as ContainerControl).ActiveControl = this;
				HideInPlaceControl();
			}
		}
		else if (_currentInPlaceControl != null)
		{
			if (!_currentInPlaceControl.ContainsFocus)
			{
				if (!PropertyGrid.IsInsideNotifyValueValidation)
				{
					Control activeControl = Utilities.GetTopLevelContainer(this).ActiveControl;
					if ((activeControl == _parentCtrl || (activeControl != null && activeControl.Contains(_parentCtrl))) && _focusedControlAfterKillFocus == null)
					{
						_validationWhileAppLoosingFocus = true;
						PropertyValue.ValueValidationResult valueValidationResult = (_currentInPlaceControl as IInPlaceControl).CommitChanges(final: true);
						if ((valueValidationResult & PropertyValue.ValueValidationResult.ErrorCode) != 0 && _parentCtrl.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
						{
							InPlaceControl.Refresh();
						}
						else
						{
							_validationWhileAppLoosingFocus = false;
							_parentCtrl.ActiveControl = this;
							HideInPlaceControl();
						}
					}
					else if (_selectedPropertyEnum.Property.InPlaceCtrlInAction == null)
					{
						HideInPlaceControl();
					}
				}
			}
			else
			{
				ContainerControl topLevelContainer = Utilities.GetTopLevelContainer(this);
				if (topLevelContainer.ActiveControl == _parentCtrl && _lastLeaveKey != Keys.None)
				{
					_parentCtrl.ActiveControl = this;
				}
			}
		}
		_lastLeaveKey = Keys.None;
		Invalidate();
	}

	public void DeleteProperty(PropertyEnumerator propEnum)
	{
		DeletePropertyInternal(propEnum);
		if (base.IsHandleCreated)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			OnSizeChanged(clientRectangle.Width, clientRectangle.Height);
			Invalidate();
		}
	}

	internal void DeletePropertyInternal(PropertyEnumerator propEnum)
	{
		if (_multiSelectMode != PropertyGrid.MultiSelectModes.None)
		{
			_multiSelectedItems.Clear();
		}
		bool flag = _selectedPropertyEnum.IsDescendantOf(propEnum) || propEnum == _selectedPropertyEnum;
		bool flag2 = _firstDisplayedPropertyEnumerator.IsDescendantOf(propEnum) || propEnum == _firstDisplayedPropertyEnumerator;
		if (flag)
		{
			SelectProperty(_categorizedPropertyCollection.GetVisibleDeepEnumerator().RightBound.GetVisibleDeepEnumerator());
		}
		PropertyEnumerator propertyEnumerator = _categorizedPropertyCollection.Delete(propEnum);
		if (!IsVerticallyFrozen)
		{
			if (flag2)
			{
				_firstDisplayedPropertyEnumerator = propertyEnumerator.GetVisibleDeepEnumerator();
			}
			if (flag)
			{
				SelectProperty(propertyEnumerator.GetVisibleDeepEnumerator());
			}
		}
		else
		{
			if (flag2)
			{
				_firstDisplayedPropertyEnumerator = RightBound.GetVisibleDeepEnumerator();
			}
			if (flag)
			{
				SelectProperty(RightBound);
			}
		}
		InvalidateVisibleItemCount();
	}

	private void AssignValidator(PropertyEnumerator propEnum)
	{
		PropertyValue value = propEnum.Property.Value;
		PropertyValidatorBase propertyValidatorBase = null;
		ArrayList attributes = value.GetAttributes(typeof(PropertyValidatorAttribute));
		foreach (PropertyValidatorAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0)
			{
				propertyValidatorBase = item.Validator;
				break;
			}
		}
		if (propertyValidatorBase == null && propEnum.Parent.Property != null && propEnum.Parent.Property.Value != null)
		{
			attributes = propEnum.Parent.Property.Value.GetAttributes(typeof(PropertyValidatorAttribute));
			foreach (PropertyValidatorAttribute item2 in attributes)
			{
				if (item2.ChildPropertyName == propEnum.Property.DisplayName)
				{
					propertyValidatorBase = item2.Validator;
					break;
				}
			}
		}
		if (propertyValidatorBase != null)
		{
			propertyValidatorBase.Enumerator = propEnum;
			value.Validator = propertyValidatorBase;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (ToolTip != null)
		{
			ToolTip.SetText("", null);
		}
		base.OnMouseLeave(e);
	}

	public bool OnMouseHooked(IntPtr hWnd, uint message, Point pt, uint hitTest)
	{
		if (!PropertyGrid.IsInsideNotifyValueValidation && (message == 513 || message == 516 || message == 519 || message == 515 || message == 161))
		{
			Control control = Control.FromHandle(hWnd);
			if (SelectedPropertyEnumerator.Property.InPlaceCtrlInAction == _currentInPlaceControl && control == _currentInPlaceControl && TryOnForbidClickThrough(_currentInPlaceControl.PointToClient(pt)) && control is IMouseHookListener)
			{
				(control as IMouseHookListener).OnListenMouseHook(control, message, pt, hitTest);
			}
			if (_validationWhileAppLoosingFocus)
			{
				_messageHook.HookMessages = false;
				if ((_currentInPlaceControl as IInPlaceControl).TextBox == null)
				{
					_currentInPlaceControl.Focus();
				}
				else
				{
					(_currentInPlaceControl as IInPlaceControl).TextBox.Focus();
				}
				_validationWhileAppLoosingFocus = false;
				PropertyValue.ValueValidationResult valueValidationResult = (_currentInPlaceControl as IInPlaceControl).CommitChanges(final: true);
				if ((valueValidationResult & PropertyValue.ValueValidationResult.ErrorCode) != 0 && _parentCtrl.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
				{
					SelectAllText();
				}
				_messageHook.HookMessages = true;
				return true;
			}
			if (_currentInPlaceControl != null && (control == _currentInPlaceControl || _currentInPlaceControl.Contains(control)))
			{
				if ((int)((DateTime.Now.Ticks - _lastTimeClickedFromParent) / 10000) < SystemInformation.DoubleClickTime)
				{
					bool result = false;
					if (Math.Abs(pt.X - _lastPosClickedInParent.X) < SystemInformation.DoubleClickSize.Width && Math.Abs(pt.Y - _lastPosClickedInParent.Y) < SystemInformation.DoubleClickSize.Height)
					{
						pt = _currentInPlaceControl.PointToClient(pt);
						Win32Calls.PostMessage(control.Handle, 515, IntPtr.Zero, (IntPtr)Win32Calls.MakeLong(pt.X, pt.Y));
						result = true;
					}
					_lastPosClickedInParent = Point.Empty;
					_lastTimeClickedFromParent = 0L;
					return result;
				}
				if ((_currentInPlaceControl as IInPlaceControl).RealtimeChange)
				{
					TextBox textBox = (_currentInPlaceControl as IInPlaceControl).TextBox;
					if (textBox != null && textBox.Focused && control != textBox && !(_currentInPlaceControl as IInPlaceControl).OwnerPropertyEnumerator.Property.Value.DisplayString.Equals(textBox.Text))
					{
						PropertyValue.ValueValidationResult valueValidationResult2 = (_currentInPlaceControl as IInPlaceControl).CommitChanges(final: false);
						if ((valueValidationResult2 & PropertyValue.ValueValidationResult.ErrorCode) != 0 && _parentCtrl.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
						{
							SelectAllText();
							return true;
						}
					}
					if (_selectedPropertyEnum.Property.InPlaceCtrlInAction != null && _currentInPlaceControl.Contains(control))
					{
						_gridOrCurrentInPlaceCtrlClickedDuringDropDown = true;
					}
				}
			}
			else if (_currentInPlaceControl != null && !Win32Calls.IsChild(_currentInPlaceControl.Handle, hWnd) && Win32Calls.GetWindowLongPtr(hWnd, -8) != _currentInPlaceControl.Handle && (_currentInPlaceControl.ContainsFocus || ((IInPlaceControl)_currentInPlaceControl).OwnerPropertyEnumerator.Property.InPlaceCtrlInAction != null) && (control == null || control.ProductName != "Skybound VisualTips"))
			{
				IntPtr intPtr = hWnd;
				Control control2 = null;
				do
				{
					intPtr = Win32Calls.GetWindowLongPtr(intPtr, -8);
					control2 = Control.FromHandle(intPtr);
					if (control2 != null)
					{
						Control dropDownContainer = (_currentInPlaceControl as IInPlaceControl).GetDropDownContainer();
						if (dropDownContainer == control2)
						{
							break;
						}
					}
				}
				while (intPtr != IntPtr.Zero);
				bool flag = false;
				if (hWnd != IntPtr.Zero)
				{
					IntPtr intPtr2 = Win32Calls.GetWindowLongPtr(hWnd, -8);
					if (intPtr2 == IntPtr.Zero)
					{
						intPtr2 = hWnd;
					}
					StringBuilder stringBuilder = new StringBuilder(25);
					if (Win32Calls.GetClassName(intPtr2, stringBuilder, stringBuilder.Capacity) > 0 && stringBuilder.ToString() == "Auto-Suggest Dropdown")
					{
						flag = true;
					}
				}
				if (control2 == null && !flag)
				{
					PropertyValue.ValueValidationResult valueValidationResult3 = (_currentInPlaceControl as IInPlaceControl).CommitChanges(final: true);
					if ((valueValidationResult3 & PropertyValue.ValueValidationResult.ErrorCode) != 0 && _parentCtrl.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
					{
						SelectAllText();
						return true;
					}
				}
				if (_selectedPropertyEnum.Property.InPlaceCtrlInAction != null && (control == this || _currentInPlaceControl.Contains(control)))
				{
					_gridOrCurrentInPlaceCtrlClickedDuringDropDown = true;
				}
			}
		}
		return false;
	}

	public bool OnMessageHooked(Control control, uint message, IntPtr wParam, IntPtr lParam)
	{
		bool flag = false;
		Control dropDownTopWindow = _dropDownTopWindow;
		if (dropDownTopWindow != null && dropDownTopWindow.Contains(control))
		{
			flag = true;
		}
		if (_dropDownTopWindow != null && _dropDownTopWindow is IMessageHookListener)
		{
			(_dropDownTopWindow as IMessageHookListener).OnListenMessageHook(control, message, wParam, lParam);
		}
		else if (_currentInPlaceControl != null && _currentInPlaceControl is IMessageHookListener)
		{
			(_currentInPlaceControl as IMessageHookListener).OnListenMessageHook(control, message, wParam, lParam);
		}
		switch (message)
		{
		case 6u:
			if (_validationWhileAppLoosingFocus)
			{
				_validationWhileAppLoosingFocus = false;
				PropertyValue.ValueValidationResult valueValidationResult = (_currentInPlaceControl as IInPlaceControl).CommitChanges(final: true);
				if ((valueValidationResult & PropertyValue.ValueValidationResult.ErrorCode) != 0 && _parentCtrl.ValueNotValidBehaviorMode == PropertyGrid.ValueNotValidBehaviorModes.KeepFocus)
				{
					SelectAllText();
				}
			}
			break;
		case 8u:
			_focusedControlAfterKillFocus = Control.FromHandle(wParam);
			if ((_currentInPlaceControl == control || (_currentInPlaceControl != null && _currentInPlaceControl.Contains(control)) || (flag && wParam == IntPtr.Zero)) && _currentInPlaceControl != null)
			{
				_currentInPlaceControl.Invalidate();
				if (_currentInPlaceControl != _focusedControlAfterKillFocus && !_currentInPlaceControl.Contains(_focusedControlAfterKillFocus))
				{
					OnChildLostFocusInternal();
				}
			}
			break;
		}
		return false;
	}

	public void OnInPlaceCtrlFinishedEdition()
	{
		if (!_gridOrCurrentInPlaceCtrlClickedDuringDropDown && _parentCtrl.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey)
		{
			_parentCtrl.TabToNextProperty();
		}
		_gridOrCurrentInPlaceCtrlClickedDuringDropDown = false;
	}

	public void SetDropDownTopWindow(Control control)
	{
		_dropDownTopWindow = control;
	}

	public bool OnKeyboardHooked(int message, KeyEventArgs args)
	{
		if (message == 256 && _currentInPlaceControl != null && _currentInPlaceControl.ContainsFocus)
		{
			if (args.KeyCode == Keys.Return || args.KeyCode == Keys.Escape)
			{
				_lastLeaveKey = args.KeyCode;
			}
			else
			{
				_lastLeaveKey = Keys.None;
			}
		}
		return false;
	}

	private void ProcessDisable()
	{
		if (!_enabled)
		{
			base.TabStop = true;
			if (_disableMode == PropertyGrid.DisableModes.Full)
			{
				SelectProperty(RightBound);
			}
			else if (_disableMode == PropertyGrid.DisableModes.NoPropertySelection)
			{
				SelectProperty(RightBound);
				base.TabStop = false;
			}
			else if (_disableMode == PropertyGrid.DisableModes.NoValueSelection)
			{
				HideInPlaceControl();
			}
			else if (_disableMode == PropertyGrid.DisableModes.Simple && ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.Disabled)
			{
				HideInPlaceControl();
			}
		}
	}

	public int GetPropertyVisibleIndex(PropertyEnumerator propEnum)
	{
		int num = -1;
		Rectangle absoluteItemRect = GetAbsoluteItemRect(propEnum);
		Rectangle clientRectangle = base.ClientRectangle;
		if (absoluteItemRect.Top >= clientRectangle.Top && absoluteItemRect.Top < clientRectangle.Bottom)
		{
			PropertyEnumerator firstDisplayedProperty = FirstDisplayedProperty;
			num = 0;
			while (firstDisplayedProperty != propEnum)
			{
				num = ((num != 0) ? (num + firstDisplayedProperty.Property.HeightMultiplier) : (num + (firstDisplayedProperty.Property.HeightMultiplier - _firstDisplayedLine + 1)));
				firstDisplayedProperty.MoveNext();
			}
		}
		return num;
	}

	public object SavePropertiesStates(PropertyEnumerator propEnum, PropertyGrid.PropertyStateFlags flags)
	{
		PropertiesStates propertiesStates = new PropertiesStates(flags);
		propEnum = (PropertyEnumerator)propEnum.Clone();
		while (propEnum != propEnum.RightBound)
		{
			int num = _parentCtrl.GetPropertyHashCode(propEnum);
			if (propEnum == FirstDisplayedProperty)
			{
				propertiesStates._firstDisplayedPropertyHashcode = num;
				propertiesStates._firstDisplayedLine = _firstDisplayedLine;
			}
			Property property = propEnum.Property;
			PropertyValue value = propEnum.Property.Value;
			if (property.Selected)
			{
				propertiesStates._selectedPropertyVisibleIndex = GetPropertyVisibleIndex(propEnum);
				propertiesStates._inPlaceCtrlInAction = property.InPlaceCtrlInAction;
			}
			PropertiesStates.PropertyState propertyState = new PropertiesStates.PropertyState(property.Selected, property.Visible, property.Expanded, property.HeightMultiplier);
			int num2 = 1;
			while (propertiesStates._states.Contains(num))
			{
				num ^= num2++;
			}
			if ((flags & PropertyGrid.PropertyStateFlags.Colors) != PropertyGrid.PropertyStateFlags.None)
			{
				if (property.IsSetBackColor)
				{
					propertyState.backColor = property.BackColor;
				}
				if (property.IsSetForeColor)
				{
					propertyState.foreColor = property.ForeColor;
				}
				if (property.IsSetDisabledForeColor)
				{
					propertyState.disabledForeColor = property.DisabledForeColor;
				}
				if (property.IsSetReadOnlyForeColor)
				{
					propertyState.readOnlyForeColor = property.ReadOnlyForeColor;
				}
				if (value != null)
				{
					if (value.IsSetBackColor)
					{
						propertyState.valueBackColor = value.BackColor;
					}
					if (value.IsSetForeColor)
					{
						propertyState.valueForeColor = value.ForeColor;
					}
					if (value.IsSetDisabledForeColor)
					{
						propertyState.valueDisabledForeColor = value.DisabledForeColor;
					}
					if (value.IsSetReadOnlyForeColor)
					{
						propertyState.valueReadOnlyForeColor = value.ReadOnlyForeColor;
					}
				}
			}
			if ((flags & PropertyGrid.PropertyStateFlags.Fonts) != PropertyGrid.PropertyStateFlags.None)
			{
				if (property.IsSetFont)
				{
					propertyState.font = property.Font;
				}
				if (value != null && value.IsSetFont)
				{
					propertyState.valueFont = value.Font;
				}
			}
			if ((flags & PropertyGrid.PropertyStateFlags.Icons) != PropertyGrid.PropertyStateFlags.None && value != null)
			{
				propertyState.imageSource = value.ImageSource;
				propertyState.imageList = value.ImageList;
				propertyState.imageSize = value.ImageSize;
				propertyState.imageIndex = value.ImageIndex;
			}
			propertiesStates._states[num] = propertyState;
			propEnum.MoveNext();
		}
		return propertiesStates;
	}

	public virtual void RestorePropertiesStates(PropertyEnumerator propEnum, object propertiesStates)
	{
		PropertyEnumerator propertyEnumerator = null;
		int selectedPropertyVisibleIndex = (propertiesStates as PropertiesStates)._selectedPropertyVisibleIndex;
		propEnum = (PropertyEnumerator)propEnum.Clone();
		PropertiesStates propertiesStates2 = propertiesStates as PropertiesStates;
		while (propEnum != propEnum.RightBound)
		{
			int num = _parentCtrl.GetPropertyHashCode(propEnum);
			if (num == propertiesStates2._firstDisplayedPropertyHashcode)
			{
				propertyEnumerator = (PropertyEnumerator)propEnum.Clone();
			}
			object obj = propertiesStates2._states[num];
			if (obj == null)
			{
				int num2 = 1;
				while (propertiesStates2._states.Contains(num) && (obj = propertiesStates2._states[num]) == null)
				{
					num ^= num2++;
				}
			}
			if (obj != null)
			{
				propertiesStates2._states[num] = null;
				PropertiesStates.PropertyState propertyState = (PropertiesStates.PropertyState)obj;
				Property property = propEnum.Property;
				PropertyValue value = property.Value;
				property.HeightMultiplier = propertyState.heightMultiplier;
				if ((propertiesStates2._flags & PropertyGrid.PropertyStateFlags.Colors) != PropertyGrid.PropertyStateFlags.None)
				{
					property.BackColor = propertyState.backColor;
					property.ForeColor = propertyState.foreColor;
					property.DisabledForeColor = propertyState.disabledForeColor;
					property.ReadOnlyForeColor = propertyState.readOnlyForeColor;
					if (value != null)
					{
						value.BackColor = propertyState.valueBackColor;
						value.ForeColor = propertyState.valueForeColor;
						value.DisabledForeColor = propertyState.valueDisabledForeColor;
						value.ReadOnlyForeColor = propertyState.valueReadOnlyForeColor;
					}
				}
				if ((propertiesStates2._flags & PropertyGrid.PropertyStateFlags.Fonts) != PropertyGrid.PropertyStateFlags.None)
				{
					property.Font = propertyState.font;
					if (value != null && !_parentCtrl.ShowDefaultValues)
					{
						value.Font = propertyState.valueFont;
					}
				}
				if ((propertiesStates2._flags & PropertyGrid.PropertyStateFlags.Icons) != PropertyGrid.PropertyStateFlags.None && value != null)
				{
					value.ImageSource = propertyState.imageSource;
					value.ImageList = propertyState.imageList;
					value.ImageIndex = propertyState.imageIndex;
					value.ImageSize = propertyState.imageSize;
				}
				if (!propertyState.visible)
				{
					ShowProperty(propEnum, show: false);
				}
				else
				{
					ExpandProperty(propEnum, propertyState.expanded);
					if (propertyState.selected)
					{
						SelectProperty(propEnum);
						if (InPlaceControl != null && (InPlaceControl as IInPlaceControl).OwnerPropertyEnumerator != propEnum)
						{
							(InPlaceControl as IInPlaceControl).OwnerPropertyEnumerator = propEnum;
							propEnum.Property.InPlaceCtrlInAction = propertiesStates2._inPlaceCtrlInAction;
						}
					}
				}
			}
			_parentCtrl.OnPropertyRestored(new PropertyRestoredEventArgs(propEnum));
			propEnum.MoveNext();
		}
		if (selectedPropertyVisibleIndex != -1)
		{
			MovePropertyToVisibleIndex(SelectedPropertyEnumerator, selectedPropertyVisibleIndex, finalize: false);
		}
		if (selectedPropertyVisibleIndex == -1 && propertyEnumerator != null)
		{
			_firstDisplayedLine = (propertiesStates as PropertiesStates)._firstDisplayedLine;
			FirstDisplayedProperty = propertyEnumerator.GetVisibleDeepEnumerator();
		}
		CloseEmptySpace();
		CheckScrollbar();
	}

	public void MovePropertyToVisibleIndex(PropertyEnumerator propEnum, int index, bool finalize)
	{
		int i = GetPropertyVisibleIndex(propEnum);
		if (i == -1)
		{
			EnsureVisible(propEnum);
			i = GetPropertyVisibleIndex(propEnum);
		}
		while (i > index)
		{
			ScrollLineDown();
			i--;
		}
		for (; i < index; i++)
		{
			ScrollLineUp();
		}
		EndScroll();
		if (finalize)
		{
			CloseEmptySpace();
			CheckScrollbar();
		}
	}

	protected override void OnRightToLeftChanged(EventArgs e)
	{
		_tooltipCtrl = null;
		base.OnRightToLeftChanged(e);
	}

	public void HandleDefaultValue(PropertyEnumerator propEnum)
	{
		if (!_parentCtrl.ShowDefaultValues || !(propEnum != null) || propEnum.Property.Value == null)
		{
			return;
		}
		PropertyValue value = propEnum.Property.Value;
		if (value != null)
		{
			if (value.IsDefault)
			{
				propEnum.Property.Value.Font = new Font(propEnum.Property.Value.Font, FontStyle.Regular);
			}
			else
			{
				propEnum.Property.Value.Font = new Font(propEnum.Property.Value.Font, FontStyle.Bold);
			}
		}
	}

	public void BeginInit()
	{
		_initializing = true;
	}

	public void EndInit()
	{
		_initializing = false;
	}
}
