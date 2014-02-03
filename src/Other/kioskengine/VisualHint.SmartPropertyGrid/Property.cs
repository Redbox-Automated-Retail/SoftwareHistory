using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[DebuggerDisplay("Id={_id}, Name={_displayName}, Value={_value.DisplayString}")]
public class Property
{
	internal enum LazyLoadingStates
	{
		None = 0,
		HasChildren = 1,
		HasNoChildren = 2,
		ContentLoaded = 4
	}

	protected const int TreeGlyphWidth = 8;

	private LazyLoadingStates _lazyLoadingState;

	private int _displayedAtPosition;

	private string _format = "";

	private object _tag;

	private int _sortedIndex = int.MaxValue;

	private bool _enabled;

	private bool _visible;

	private string _displayName;

	private string _comment = "";

	private int _id;

	private PropertyGrid _grid;

	private bool _canBeDisabledManually;

	private PropertyValue _enabledVariable;

	private bool _selected;

	private Control _inPlaceCtrlInAction;

	private PropertyValue _value;

	private Hashtable _additionalValues = new Hashtable();

	private Color _foreColor;

	private Color _disabledForeColor;

	private Color _readOnlyForeColor;

	private Color _backColor;

	private Font _font;

	private int _heightMultiplier = 1;

	private bool _expanded;

	private int _imageIndex = -1;

	internal LazyLoadingStates LazyLoadingState
	{
		get
		{
			return _lazyLoadingState;
		}
		set
		{
			_lazyLoadingState = value;
		}
	}

	public int DisplayedAtPosition
	{
		get
		{
			return _displayedAtPosition;
		}
		set
		{
			_displayedAtPosition = value;
		}
	}

	public string HyperLinkFormat
	{
		get
		{
			return _format;
		}
		set
		{
			_format = value;
			if (_value != null)
			{
				if (_format.Length > 0)
				{
					_value.ForeColor = Color.Blue;
				}
				else
				{
					_value.ForeColor = Color.Empty;
				}
			}
		}
	}

	public string HyperLink
	{
		get
		{
			if (_format.Length == 0)
			{
				return "";
			}
			return string.Format(HyperLinkFormat, Value.DisplayString);
		}
	}

	public object Tag
	{
		get
		{
			return _tag;
		}
		set
		{
			_tag = value;
		}
	}

	public int SortedIndex => _sortedIndex;

	internal int SortedIndexInternal
	{
		set
		{
			_sortedIndex = value;
		}
	}

	public bool Enabled => _enabled;

	public bool Visible => _visible;

	internal bool VisibleInternal
	{
		set
		{
			_visible = value;
		}
	}

	public string DisplayName
	{
		get
		{
			if (_grid == null)
			{
				return _displayName;
			}
			return _grid.DrawManager.GetDisplayName(this, _displayName);
		}
		set
		{
			_displayName = value;
			if (_grid != null)
			{
				_grid.Grid.Invalidate();
			}
		}
	}

	public string Name
	{
		get
		{
			if (_value == null)
			{
				return "";
			}
			return _value.Name;
		}
	}

	public string Comment
	{
		get
		{
			return _comment;
		}
		set
		{
			_comment = ((value == null) ? "" : value);
			if (_grid != null)
			{
				_grid.Invalidate();
			}
		}
	}

	public int Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	public PropertyFeel Feel
	{
		get
		{
			if (Value != null)
			{
				return Value.Feel;
			}
			return null;
		}
		set
		{
			if (Value != null)
			{
				Value.Feel = value;
			}
		}
	}

	public PropertyLook Look
	{
		get
		{
			if (Value != null)
			{
				return Value.Look;
			}
			return null;
		}
		set
		{
			if (Value != null)
			{
				Value.Look = value;
			}
		}
	}

	public PropertyGrid ParentGrid => _grid;

	internal PropertyGrid ParentGridInternal
	{
		set
		{
			_grid = value;
		}
	}

	public bool CanBeDisabledManually => _canBeDisabledManually;

	public bool Selected => _selected;

	internal bool SelectedInternal
	{
		set
		{
			_selected = value;
		}
	}

	public Control InPlaceCtrlInAction
	{
		get
		{
			return _inPlaceCtrlInAction;
		}
		set
		{
			_inPlaceCtrlInAction = value;
		}
	}

	public PropertyValue Value => _value;

	internal PropertyValue ValueInternal
	{
		set
		{
			_value = value;
		}
	}

	public Color ForeColor
	{
		get
		{
			if (_foreColor.IsEmpty)
			{
				return _grid.GridForeColor;
			}
			return _foreColor;
		}
		set
		{
			_foreColor = value;
			if (IsSetForeColor && Selected && ParentGrid.InPlaceControl != null && !Value.ReadOnly)
			{
				ParentGrid.InPlaceControl.ForeColor = _foreColor;
			}
			if (_grid != null)
			{
				_grid.Grid.Invalidate();
			}
		}
	}

	public bool IsSetForeColor => !_foreColor.IsEmpty;

	public Color DisabledForeColor
	{
		get
		{
			if (_disabledForeColor.IsEmpty)
			{
				return _grid.DisabledForeColor;
			}
			return _disabledForeColor;
		}
		set
		{
			_disabledForeColor = value;
			if (IsSetDisabledForeColor && Selected && ParentGrid.InPlaceControl != null && Value.ReadOnly && !ParentGrid.Enabled && ParentGrid.DisableMode != PropertyGrid.DisableModes.None && ParentGrid.DisableModeGrayedOut)
			{
				ParentGrid.InPlaceControl.ForeColor = _disabledForeColor;
			}
			if (_grid != null)
			{
				_grid.Grid.Invalidate();
			}
		}
	}

	public bool IsSetDisabledForeColor => !_disabledForeColor.IsEmpty;

	public Color ReadOnlyForeColor
	{
		get
		{
			if (_readOnlyForeColor.IsEmpty)
			{
				return _grid.ReadOnlyForeColor;
			}
			return _readOnlyForeColor;
		}
		set
		{
			_readOnlyForeColor = value;
			if (IsSetReadOnlyForeColor && Selected && ParentGrid.InPlaceControl != null && Value.ReadOnly && (ParentGrid.Enabled || ParentGrid.DisableMode == PropertyGrid.DisableModes.None || !ParentGrid.DisableModeGrayedOut))
			{
				ParentGrid.InPlaceControl.ForeColor = _readOnlyForeColor;
			}
			if (_grid != null)
			{
				_grid.Grid.Invalidate();
			}
		}
	}

	public bool IsSetReadOnlyForeColor => !_readOnlyForeColor.IsEmpty;

	public Color BackColor
	{
		get
		{
			if (_backColor.IsEmpty)
			{
				return _grid.PropertyLabelBackColor;
			}
			return _backColor;
		}
		set
		{
			_backColor = value;
			if (IsSetBackColor && Selected && ParentGrid.InPlaceControl != null)
			{
				ParentGrid.InPlaceControl.BackColor = _backColor;
			}
			if (_grid != null)
			{
				_grid.Grid.Invalidate();
			}
		}
	}

	public bool IsSetBackColor => !_backColor.IsEmpty;

	public Font Font
	{
		get
		{
			if (_font != null)
			{
				return _font;
			}
			return _grid.Font;
		}
		set
		{
			_font = value;
			if (Value != null)
			{
				Value.FontSetOnProperty();
			}
			if (_grid != null)
			{
				_grid.Grid.Invalidate();
			}
		}
	}

	public bool IsSetFont => _font != null;

	public int HeightMultiplier
	{
		get
		{
			return _heightMultiplier;
		}
		set
		{
			if (value != _heightMultiplier)
			{
				_heightMultiplier = value;
				_grid.Grid.HeightMultiplierChanged();
				_grid.Grid.Refresh();
			}
		}
	}

	public bool Expanded => _expanded;

	internal bool ExpandedInternal
	{
		set
		{
			_expanded = value;
		}
	}

	public int ImageIndex
	{
		get
		{
			return _imageIndex;
		}
		set
		{
			_imageIndex = value;
			ParentGrid.Refresh();
		}
	}

	internal void SetManuallyDisabled(PropertyEnumerator propEnum, bool disabled)
	{
		_canBeDisabledManually = disabled;
		if (_canBeDisabledManually)
		{
			_enabledVariable = new PropertyValueManaged(_grid, 0, typeof(bool), _enabled, null);
			_enabledVariable.Grid = ParentGrid;
		}
		else
		{
			_enabledVariable = null;
		}
	}

	internal void SetManuallyDisabled(PropertyEnumerator propEnum, object containerInstance, string memberName)
	{
		if (containerInstance != null)
		{
			_canBeDisabledManually = true;
			_enabledVariable = new PropertyValueIndirect(_grid, null, containerInstance, TypeDescriptor.GetProperties(containerInstance)[memberName], null);
			_enabledVariable.Grid = ParentGrid;
			ParentGrid.EnableProperty(propEnum, GetManuallyDisabledVariable() && !ParentGrid.Grid.IsAncestorDisabled(propEnum));
		}
	}

	public bool GetManuallyDisabledVariable()
	{
		if (_canBeDisabledManually)
		{
			return (bool)_enabledVariable.GetValue();
		}
		return Enabled;
	}

	internal Property(int propertyId, string name)
	{
		Id = propertyId;
		_selected = false;
		_enabled = true;
		_visible = true;
		_expanded = false;
		DisplayName = name;
		_foreColor = Color.Empty;
		_backColor = Color.Empty;
		_disabledForeColor = Color.Empty;
	}

	public override string ToString()
	{
		return "Property id = " + Id;
	}

	public virtual void Draw(Graphics graphics, Rectangle itemRect, PropertyVisibleDeepEnumerator enumSelf)
	{
		Color textColor;
		Color textColor2;
		if (Selected)
		{
			textColor = ((!ParentGrid.Enabled && ParentGrid.DisableMode != PropertyGrid.DisableModes.None && ParentGrid.DisableModeGrayedOut) ? _grid.DisabledForeColor : ((!Enabled) ? Value.DisabledForeColor : ((!Value.ReadOnly) ? Value.ForeColor : Value.ReadOnlyForeColor)));
			textColor2 = ((!ParentGrid.GridContainsFocus && InPlaceCtrlInAction == null) ? ForeColor : ParentGrid.HighlightedTextColor);
		}
		else if (!ParentGrid.Enabled && ParentGrid.DisableMode != PropertyGrid.DisableModes.None && ParentGrid.DisableModeGrayedOut)
		{
			textColor2 = _grid.DisabledForeColor;
			textColor = _grid.DisabledForeColor;
		}
		else if (!Enabled)
		{
			textColor = Value.DisabledForeColor;
			textColor2 = DisabledForeColor;
		}
		else if (Value.ReadOnly)
		{
			textColor = Value.ReadOnlyForeColor;
			textColor2 = ReadOnlyForeColor;
		}
		else
		{
			textColor2 = ForeColor;
			textColor = Value.ForeColor;
		}
		Rectangle labelRect = itemRect;
		labelRect.X += ParentGrid.LeftColumnWidth + 1;
		labelRect.Width = ParentGrid.LabelColumnWidth;
		ParentGrid.DrawManager.DrawPropertyLabelBackground(graphics, labelRect, enumSelf);
		if (ParentGrid.HasOneVisibleChild(enumSelf))
		{
			ParentGrid.DrawManager.DrawPlusMinusSign(graphics, ParentGrid.Grid, itemRect, enumSelf);
		}
		if (CanBeDisabledManually)
		{
			Rectangle manuallyDisableRect = GetManuallyDisableRect(itemRect, enumSelf);
			ParentGrid.DrawManager.DrawCheckBox(graphics, ParentGrid.Grid, manuallyDisableRect, GetManuallyDisabledVariable(), !ParentGrid.Grid.IsAncestorDisabled(enumSelf), PropertyValue.DrawValueIn.Grid);
		}
		Rectangle valueRect = GetValueRect(itemRect);
		ParentGrid.DrawManager.DrawPropertyValueBackground(graphics, valueRect, enumSelf, PropertyValue.DrawValueIn.Grid);
		Rectangle labelColumnRect = GetLabelColumnRect(itemRect, enumSelf);
		ParentGrid.DrawManager.DrawSeparationLines(graphics, itemRect, labelColumnRect, enumSelf);
		Rectangle imageRect = GetImageRect(itemRect, enumSelf);
		if (imageRect != Rectangle.Empty)
		{
			Win32Calls.SetClippingRect(graphics, ParentGrid.TransformRectIfRTL(labelColumnRect));
			ParentGrid.DrawImage(ImageIndex, graphics, imageRect.Left, imageRect.Top);
			Win32Calls.ResetClippingRect(graphics);
		}
		Rectangle labelTextRect = GetLabelTextRect(itemRect, enumSelf);
		ParentGrid.DrawManager.DrawPropertyLabelText(graphics, labelColumnRect, labelTextRect, textColor2, enumSelf);
		valueRect.Height--;
		Value.HasMultipleTextsMustBeCached = true;
		Value.DrawValue(graphics, valueRect, textColor, enumSelf, null, Value.HasMultipleTexts(), PropertyValue.DrawValueIn.Grid);
		Value.HasMultipleTextsMustBeCached = false;
	}

	public Rectangle GetValueRect(Rectangle itemRect)
	{
		Rectangle result = itemRect;
		result.X += ParentGrid.LeftColumnWidth + 1 + ParentGrid.LabelColumnWidth + 1;
		result.Width -= ParentGrid.LeftColumnWidth + 1 + ParentGrid.LabelColumnWidth + 1;
		return result;
	}

	internal void Enable(PropertyEnumerator enumSelf, bool enable, bool ancestorDisabled, bool direct)
	{
		if (!enable || !ancestorDisabled)
		{
			if (!CanBeDisabledManually || direct)
			{
				_enabled = enable;
			}
			else if (enable)
			{
				_enabled = GetManuallyDisabledVariable();
			}
			else
			{
				_enabled = false;
			}
		}
		if (CanBeDisabledManually && direct)
		{
			_enabledVariable.SetValue(enable);
		}
		if (_enabled && Feel != null && Feel.InPlaceCtrl != null)
		{
			(Feel.InPlaceCtrl as IInPlaceControl).RepositionChildren();
		}
		_grid.OnPropertyEnabled(new PropertyEnabledEventArgs(enumSelf, direct));
	}

	internal bool OnSetCursor(Point point, Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		return false;
	}

	internal virtual void OnPropertyClicked(PropertyEnumerator enumSelf, Point point, Rectangle itemRect)
	{
		if (Control.ModifierKeys == Keys.Control && HyperLinkFormat.Length > 0)
		{
			Rectangle valueRect = GetValueRect(itemRect);
			Graphics graphics = ParentGrid.CreateGraphics();
			Rectangle stringValueRect = Value.GetStringValueRect(graphics, valueRect, point);
			graphics.Dispose();
			if (stringValueRect.Contains(ParentGrid.TransformPointIfRTL(point)))
			{
				ParentGrid.OnHyperLinkPropertyClicked(new PropertyHyperLinkClickedEventArgs(enumSelf, HyperLink));
			}
		}
	}

	public Type GetValueType()
	{
		return _value.GetType();
	}

	public virtual Rectangle GetSignRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		if (!ParentGrid.ShowAdditionalIndentation && !ParentGrid.HasOneVisibleChild(enumSelf))
		{
			return Rectangle.Empty;
		}
		Rectangle result = itemRect;
		result.Y += result.Height / HeightMultiplier / 2 - 4;
		result.Height = 8;
		result.X = ParentGrid.LeftColumnWidth + 1 + (enumSelf.Depth - 1) * (_grid.GlobalTextMargin + 8) + _grid.GlobalTextMargin;
		result.Width = 8;
		return result;
	}

	public virtual Rectangle GetManuallyDisableRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		if (!CanBeDisabledManually)
		{
			return Rectangle.Empty;
		}
		Rectangle result = itemRect;
		Rectangle signRect = GetSignRect(itemRect, enumSelf);
		if (signRect != Rectangle.Empty)
		{
			result.X = _grid.GlobalTextMargin + signRect.Right;
		}
		else
		{
			result.X = _grid.GlobalTextMargin + ParentGrid.LeftColumnWidth + 1 + (enumSelf.Depth - 1) * (_grid.GlobalTextMargin + 8);
		}
		result.Y += 2;
		result.Height = ParentGrid.BasicPropertyHeight - 4;
		result.Width = result.Height;
		return result;
	}

	public virtual Rectangle GetImageRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		if (ImageIndex == -1)
		{
			return Rectangle.Empty;
		}
		Rectangle result = itemRect;
		Rectangle manuallyDisableRect = GetManuallyDisableRect(itemRect, enumSelf);
		if (manuallyDisableRect != Rectangle.Empty)
		{
			result.X = manuallyDisableRect.Right + ParentGrid.GlobalTextMargin;
		}
		else
		{
			Rectangle signRect = GetSignRect(itemRect, enumSelf);
			if (signRect != Rectangle.Empty)
			{
				result.X = signRect.Right + ParentGrid.GlobalTextMargin;
			}
			else if (enumSelf.Depth == 0)
			{
				result.X = ParentGrid.LeftColumnWidth + 1 + _grid.GlobalTextMargin;
			}
			else
			{
				result.X = ParentGrid.LeftColumnWidth + 1 + _grid.GlobalTextMargin + (enumSelf.Depth - 1) * (_grid.GlobalTextMargin + 8);
			}
		}
		try
		{
			Image image = ParentGrid.ImageList.Images[ImageIndex];
			result.Width = image.Width;
			result.Height = image.Height;
		}
		catch (Exception)
		{
			return Rectangle.Empty;
		}
		result.Y = itemRect.Top + (ParentGrid.BasicPropertyHeight - result.Height) / 2;
		return result;
	}

	public virtual Rectangle GetLabelTextRect(Rectangle itemRect, PropertyEnumerator enumSelf)
	{
		Rectangle result = itemRect;
		Rectangle imageRect = GetImageRect(itemRect, enumSelf);
		if (imageRect != Rectangle.Empty)
		{
			result.X = imageRect.Right;
		}
		else
		{
			Rectangle manuallyDisableRect = GetManuallyDisableRect(itemRect, enumSelf);
			if (manuallyDisableRect != Rectangle.Empty)
			{
				result.X = manuallyDisableRect.Right;
			}
			else
			{
				Rectangle signRect = GetSignRect(itemRect, enumSelf);
				if (signRect != Rectangle.Empty)
				{
					result.X = signRect.Right;
				}
				else
				{
					result.X = ParentGrid.LeftColumnWidth + 1 + (enumSelf.Depth - 1) * (_grid.GlobalTextMargin + 8);
				}
			}
		}
		result.X += _grid.GlobalTextMargin;
		Graphics graphics = ParentGrid.CreateGraphics();
		result.Width = Win32Calls.GetTextExtent(graphics, DisplayName, _grid.DrawManager.GetPropertyLabelFont(this)).Width;
		graphics.Dispose();
		int num = ParentGrid.LeftColumnWidth + 1 + ParentGrid.LabelColumnWidth;
		if ((ParentGrid.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnLabels) != PropertyGrid.EllipsisModes.None)
		{
			num -= _grid.GlobalTextMargin;
		}
		if (result.Right > num)
		{
			result.Width = num - result.Left;
		}
		return result;
	}

	public virtual Rectangle GetLabelColumnRect(Rectangle itemRect, PropertyEnumerator enumerator)
	{
		Rectangle result = itemRect;
		result.X = ParentGrid.LeftColumnWidth + 1;
		result.Width = ParentGrid.LabelColumnWidth;
		return result;
	}

	public virtual bool OverrideCursor(Point mouseLocation, Rectangle itemRect, PropertyEnumerator propEnum)
	{
		if (HyperLinkFormat.Length != 0 && Control.ModifierKeys == Keys.Control && Value != null)
		{
			Rectangle valueRect = GetValueRect(itemRect);
			Graphics graphics = ParentGrid.CreateGraphics();
			Rectangle stringValueRect = Value.GetStringValueRect(graphics, valueRect, mouseLocation);
			graphics.Dispose();
			if (stringValueRect.Contains(ParentGrid.TransformPointIfRTL(mouseLocation)) && Enabled)
			{
				ParentGrid.Grid.Cursor = Cursors.Hand;
				return true;
			}
		}
		return false;
	}

	public void AddValue(object key, object container, string memberName, params Attribute[] attributes)
	{
		PropertyValue propertyValue;
		if ((propertyValue = (PropertyValue)_additionalValues[key]) == null)
		{
			propertyValue = new PropertyValueIndirect(_grid, null, container, TypeDescriptor.GetProperties(container)[memberName], attributes);
			_additionalValues.Add(key, propertyValue);
			propertyValue.OwnerEnumerator = Value.OwnerEnumerator;
			propertyValue.AdditionalValueKey = key;
		}
		else
		{
			(propertyValue as PropertyValueIndirect).AddTargetInstance(container, TypeDescriptor.GetProperties(container)[memberName]);
		}
	}

	public void AddValue(object key, object container, PropertyDescriptor propertyDescriptor, params Attribute[] attributes)
	{
		PropertyValue propertyValue;
		if ((propertyValue = (PropertyValue)_additionalValues[key]) == null)
		{
			propertyValue = new PropertyValueIndirect(_grid, null, container, propertyDescriptor, attributes);
			_additionalValues.Add(key, propertyValue);
			propertyValue.OwnerEnumerator = Value.OwnerEnumerator;
			propertyValue.AdditionalValueKey = key;
		}
		else
		{
			(propertyValue as PropertyValueIndirect).AddTargetInstance(container, propertyDescriptor);
		}
	}

	public void AddManagedValue(object key, Type valueType, object defaultValue, params Attribute[] attributes)
	{
		PropertyValue propertyValue = new PropertyValueManaged(_grid, 0, valueType, defaultValue, attributes);
		_additionalValues.Add(key, propertyValue);
		propertyValue.OwnerEnumerator = Value.OwnerEnumerator;
		propertyValue.AdditionalValueKey = key;
	}

	public PropertyValue GetValue(object key)
	{
		PropertyValue result;
		if ((result = (PropertyValue)_additionalValues[key]) != null)
		{
			return result;
		}
		return null;
	}

	public object[] GetValueKeys()
	{
		object[] array = new object[_additionalValues.Keys.Count];
		_additionalValues.Keys.CopyTo(array, 0);
		return array;
	}
}
