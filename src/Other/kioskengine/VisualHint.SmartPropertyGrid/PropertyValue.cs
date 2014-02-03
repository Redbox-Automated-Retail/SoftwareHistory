using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

public class PropertyValue
{
	[Flags]
	public enum ValueValidationResult
	{
		Validated = 1,
		PreviousValueRestored = 2,
		TypeConverterFailed = 4,
		ValidatorFailed = 8,
		ExceptionByClient = 0x10,
		ErrorCode = TypeConverterFailed | ValidatorFailed | ExceptionByClient
	}

	public enum ValueUpdateSource
	{
		FromChild,
		FromParent,
		FromOutside
	}

	public enum DrawValueIn
	{
		None,
		Grid,
		InPlaceCtrl,
		DropDown
	}

	private object _feelCache;

	private static int UITypeEditorCustomDrawingWidth = 20;

	protected Type mUnderlyingType;

	private bool _useAutoCompletion = true;

	private bool _triggerDisplayedValuesNeededEventAtDropDown;

	private bool _hasMultipleTextsMustBeCached;

	private bool _cachedHasMultipleText;

	private string _cachedMultipleText;

	protected object mUnderlyingValue;

	private bool _noLinkWithChildren = true;

	private bool _childValueCreatedBySPG;

	private CultureInfo _cultureInfo;

	private int _id;

	private object _additionalValueKey;

	private bool _previousValueValid = true;

	private object _previousValue;

	private int _editboxLeftMargin = -1;

	private object _tag;

	private Font _font;

	private Color _foreColor;

	private Color _disabledForeColor;

	private Color _readOnlyForeColor;

	private Color _backColor;

	private Size _imageSize = Size.Empty;

	private PropertyGrid.ImageSources _imageSource = PropertyGrid.ImageSources.Value;

	private ImageList _imageList;

	private int _imageIndex = -1;

	private Image _image;

	private PropertyLook _look;

	protected PropertyFeel mFeel;

	protected PropertyGrid mPropCtrl;

	private bool _drawingDoneByEditor;

	private UITypeEditor _uiTypeEditor;

	protected PropertyEnumerator mOwnerEnumerator;

	private Hashtable _customAttributes = new Hashtable();

	private PropertyValue _parentValue;

	private Attribute[] _attributesToSet;

	private PropertyValidatorBase _validator;

	private bool _preventRefreshProperties;

	private bool _setValueFromInPlaceCtrl;

	internal static bool DontConvertStringValues = false;

	private Exception _lastExceptionByClient;

	protected TypeConverter mConverter;

	protected string[] cachedDisplayedStrings;

	private object[] _standardValues;

	protected Hashtable mActualObjectToDisplayedStr;

	protected Hashtable mDisplayedStrToActualObject;

	protected string mDisplayedStrForNullActualObject;

	protected bool mDisplayedStrForNullActualObjectSet;

	private string _autoCompleteBuffer = "";

	private Timer _timer;

	public object FeelCache
	{
		get
		{
			return _feelCache;
		}
		set
		{
			if (value == null && _feelCache is IDisposable)
			{
				(_feelCache as IDisposable).Dispose();
			}
			_feelCache = value;
		}
	}

	public virtual Type UnderlyingType
	{
		get
		{
			if (mUnderlyingType == null)
			{
				mUnderlyingType = UnderlyingValue?.GetType();
			}
			return mUnderlyingType;
		}
	}

	public bool UseAutoCompletion
	{
		get
		{
			return _useAutoCompletion;
		}
		set
		{
			_useAutoCompletion = value;
		}
	}

	public bool TriggerDisplayedValuesNeededEventAtDropDown => _triggerDisplayedValuesNeededEventAtDropDown;

	public virtual bool HasMultipleValues => false;

	public bool HasMultipleTextsMustBeCached
	{
		get
		{
			return _hasMultipleTextsMustBeCached;
		}
		set
		{
			_hasMultipleTextsMustBeCached = value;
			_cachedMultipleText = null;
		}
	}

	public bool CachedHasMultipleText
	{
		get
		{
			return _cachedHasMultipleText;
		}
		set
		{
			_cachedHasMultipleText = value;
		}
	}

	public string CachedMultipleText
	{
		get
		{
			return _cachedMultipleText;
		}
		set
		{
			_cachedMultipleText = value;
		}
	}

	internal virtual object UnderlyingValue
	{
		get
		{
			return mUnderlyingValue;
		}
		set
		{
			mUnderlyingValue = value;
		}
	}

	public bool NoLinkWithChildren
	{
		get
		{
			return _noLinkWithChildren;
		}
		set
		{
			_noLinkWithChildren = value;
		}
	}

	internal bool ChildValueCreatedBySPG
	{
		get
		{
			return _childValueCreatedBySPG;
		}
		set
		{
			_childValueCreatedBySPG = value;
		}
	}

	public CultureInfo CultureInfo
	{
		get
		{
			if (_cultureInfo == null)
			{
				_cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
			}
			return _cultureInfo;
		}
		set
		{
			_cultureInfo = value;
		}
	}

	internal int Id
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

	public object AdditionalValueKey
	{
		get
		{
			return _additionalValueKey;
		}
		set
		{
			_additionalValueKey = value;
		}
	}

	public bool PreviousValueValid
	{
		get
		{
			return _previousValueValid;
		}
		set
		{
			_previousValueValid = value;
		}
	}

	public object PreviousValue
	{
		get
		{
			if (!_previousValueValid)
			{
				throw new MultipleValuesException();
			}
			return _previousValue;
		}
		set
		{
			_previousValue = value;
			_previousValueValid = !HasMultipleValues;
		}
	}

	public int EditboxLeftMargin => _editboxLeftMargin;

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

	public Font Font
	{
		get
		{
			if (_font != null)
			{
				return _font;
			}
			return mOwnerEnumerator.Property.Font;
		}
		set
		{
			_font = value;
			FontSetOnProperty();
			if (mOwnerEnumerator != null && mOwnerEnumerator.Property != null)
			{
				Grid.Grid.Invalidate();
			}
		}
	}

	public bool IsSetFont => _font != null;

	public Color ForeColor
	{
		get
		{
			if (_foreColor.IsEmpty)
			{
				return mOwnerEnumerator.Property.ForeColor;
			}
			return _foreColor;
		}
		set
		{
			_foreColor = value;
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && IsSetForeColor && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator && !ReadOnly)
			{
				inPlaceControl.ForeColor = _foreColor;
			}
			else
			{
				Grid.Refresh();
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
				return mOwnerEnumerator.Property.DisabledForeColor;
			}
			return _disabledForeColor;
		}
		set
		{
			_disabledForeColor = value;
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && IsSetDisabledForeColor && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator)
			{
				if (ReadOnly && !Grid.Enabled && Grid.DisableMode != PropertyGrid.DisableModes.None && Grid.DisableModeGrayedOut)
				{
					inPlaceControl.ForeColor = _disabledForeColor;
				}
			}
			else
			{
				Grid.Refresh();
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
				return mOwnerEnumerator.Property.ReadOnlyForeColor;
			}
			return _readOnlyForeColor;
		}
		set
		{
			_readOnlyForeColor = value;
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && IsSetReadOnlyForeColor && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator)
			{
				if (ReadOnly && (Grid.Enabled || Grid.DisableMode == PropertyGrid.DisableModes.None || !Grid.DisableModeGrayedOut))
				{
					inPlaceControl.ForeColor = _readOnlyForeColor;
				}
			}
			else
			{
				Grid.Refresh();
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
				if (!Grid.Grid.IsSetPropertyValueBackColor)
				{
					return mOwnerEnumerator.Property.BackColor;
				}
				return Grid.PropertyValueBackColor;
			}
			return _backColor;
		}
		set
		{
			_backColor = value;
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && IsSetBackColor && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator)
			{
				inPlaceControl.BackColor = _backColor;
			}
			else
			{
				Grid.Refresh();
			}
		}
	}

	public bool IsSetBackColor => !_backColor.IsEmpty;

	public Size ImageSize
	{
		get
		{
			return _imageSize;
		}
		set
		{
			_imageSize = value;
		}
	}

	public PropertyGrid.ImageSources ImageSource
	{
		get
		{
			return _imageSource;
		}
		set
		{
			_imageSource = value;
			Grid.Grid.Refresh();
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
			if (!HasMultipleValues)
			{
				if (ImageSource == PropertyGrid.ImageSources.Value)
				{
					ImageIndex = SelectedDisplayedValueIndex;
				}
			}
			else
			{
				ImageIndex = -1;
			}
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
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator)
			{
				Rectangle itemRect = Grid.Grid.GetItemRect(mOwnerEnumerator);
				Rectangle valueRect = mOwnerEnumerator.Property.GetValueRect(itemRect);
				Feel.MoveControl(valueRect, mOwnerEnumerator);
			}
			Grid.Grid.Refresh();
		}
	}

	public int ImageIndexNoRedraw
	{
		set
		{
			_imageIndex = value;
		}
	}

	public Image Image
	{
		get
		{
			return _image;
		}
		set
		{
			_image = value;
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator)
			{
				Rectangle itemRect = Grid.Grid.GetItemRect(mOwnerEnumerator);
				Rectangle valueRect = mOwnerEnumerator.Property.GetValueRect(itemRect);
				Feel.MoveControl(valueRect, mOwnerEnumerator);
			}
			Grid.Grid.Refresh();
		}
	}

	public int SelectedDisplayedValueIndex
	{
		get
		{
			if (HasMultipleValues)
			{
				return -1;
			}
			string[] displayedValues = GetDisplayedValues();
			if (displayedValues.Length == 0)
			{
				return -1;
			}
			string displayString = DisplayString;
			int num = 0;
			string[] array = displayedValues;
			foreach (string value in array)
			{
				if (displayString.Equals(value))
				{
					return num;
				}
				num++;
			}
			return -1;
		}
	}

	public PropertyLook Look
	{
		get
		{
			return _look;
		}
		set
		{
			_look = value;
			if (_look != null)
			{
				_look.Value = this;
				_look.AttachProperty(mOwnerEnumerator);
				_look.ControlHeightMultiplier();
				if (IsReadOnly(mOwnerEnumerator) && _look.NoFeelForReadOnly && mPropCtrl.ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.ReadOnlyFeel)
				{
					Feel = null;
				}
			}
		}
	}

	public PropertyFeel Feel
	{
		get
		{
			if (ReadOnly && Look != null && Look.NoFeelForReadOnly)
			{
				return null;
			}
			if (!Grid.Grid.Enabled && Grid.DisableMode == PropertyGrid.DisableModes.Simple && Grid.ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.Disabled)
			{
				return null;
			}
			return mFeel;
		}
		set
		{
			mFeel = value;
		}
	}

	public PropertyGrid Grid
	{
		get
		{
			return mPropCtrl;
		}
		set
		{
			mPropCtrl = value;
		}
	}

	public bool DrawingDoneByEditor => _drawingDoneByEditor;

	internal UITypeEditor UITypeEditor => _uiTypeEditor;

	public PropertyEnumerator OwnerEnumerator
	{
		get
		{
			return mOwnerEnumerator;
		}
		set
		{
			mOwnerEnumerator = (PropertyEnumerator)value.Clone();
			if (_attributesToSet != null && _attributesToSet.Length > 0)
			{
				Attribute[] attributesToSet = _attributesToSet;
				foreach (Attribute attribute in attributesToSet)
				{
					SetAttribute(attribute);
				}
			}
			ExtractDisplayedValues();
			if (_uiTypeEditor == null)
			{
				InitializeUITypeEditor();
			}
		}
	}

	public virtual string DisplayName => "";

	public virtual string Name => mOwnerEnumerator.Property.DisplayName;

	protected PropertyValue ParentValue => _parentValue;

	internal Attribute[] Attributes
	{
		set
		{
			_attributesToSet = value;
		}
	}

	public bool ReadOnly
	{
		get
		{
			return IsReadOnly(mOwnerEnumerator);
		}
		set
		{
			ReadOnlyAttribute readOnlyAttribute = (ReadOnlyAttribute)_GetAttribute(typeof(ReadOnlyAttribute));
			bool flag = false;
			if (readOnlyAttribute != null)
			{
				if (value && !readOnlyAttribute.IsReadOnly)
				{
					SetSingleAttribute(ReadOnlyAttribute.Yes);
					flag = true;
				}
				else if (!value && readOnlyAttribute.IsReadOnly)
				{
					SetSingleAttribute(ReadOnlyAttribute.No);
					flag = true;
				}
			}
			else if (value && !ReadOnly)
			{
				SetSingleAttribute(ReadOnlyAttribute.Yes);
				flag = true;
			}
			else if (!value && ReadOnly)
			{
				SetSingleAttribute(ReadOnlyAttribute.No);
				flag = true;
			}
			if (!flag)
			{
				return;
			}
			if (ReadOnly)
			{
				if (Grid.ReadOnlyVisual == PropertyGrid.ReadOnlyVisuals.Disabled)
				{
					Grid.EnableProperty(OwnerEnumerator, enable: false);
				}
				else
				{
					Grid.Grid.Invalidate();
				}
			}
			else
			{
				Grid.EnableProperty(OwnerEnumerator, enable: true);
				Grid.Grid.Invalidate();
			}
		}
	}

	public PropertyValidatorBase Validator
	{
		get
		{
			return _validator;
		}
		set
		{
			_validator = value;
			if (_validator != null && mOwnerEnumerator != null)
			{
				_validator.Enumerator = mOwnerEnumerator;
				_validator.Check(GetValue(), modify: true);
			}
		}
	}

	internal bool PreventRefreshProperties
	{
		get
		{
			return _preventRefreshProperties;
		}
		set
		{
			_preventRefreshProperties = value;
		}
	}

	public Exception LastExceptionByClient => _lastExceptionByClient;

	public string DisplayString
	{
		get
		{
			if (HasMultipleTexts(out var text))
			{
				return "";
			}
			if (_look != null)
			{
				return _look.DisplayString;
			}
			return text;
		}
	}

	public string DisplayStringWithoutLook
	{
		get
		{
			if (HasMultipleTexts(out var text))
			{
				return "";
			}
			return text;
		}
	}

	public virtual TypeConverter TypeConverter
	{
		get
		{
			if (mConverter == null)
			{
				object underlyingValue = UnderlyingValue;
				if (underlyingValue == null)
				{
					mConverter = TypeDescriptor.GetConverter(UnderlyingType);
				}
				else
				{
					mConverter = TypeDescriptor.GetConverter(underlyingValue);
				}
			}
			return mConverter;
		}
	}

	internal bool NoCustomAttributesSet
	{
		get
		{
			if (_customAttributes.Count == 0)
			{
				if (_attributesToSet != null)
				{
					return _attributesToSet.Length == 0;
				}
				return true;
			}
			return false;
		}
	}

	public virtual object TargetInstance => null;

	public virtual object[] TargetInstances => null;

	public virtual PropertyDescriptor PropertyDescriptor => null;

	public bool ReadOnlyByTypeConverter
	{
		get
		{
			if (TypeConverter != null)
			{
				PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(mOwnerEnumerator);
				if (!TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)))
				{
					return true;
				}
				if (TypeConverter.GetStandardValuesExclusive(typeDescriptorContext))
				{
					return true;
				}
			}
			return false;
		}
	}

	public virtual bool IsDefault => false;

	internal event EventHandler PropertyChangedEvent;

	public bool HasMultipleTexts()
	{
		string text;
		return HasMultipleTexts(out text);
	}

	public virtual bool HasMultipleTexts(out string text)
	{
		if (_hasMultipleTextsMustBeCached && _cachedMultipleText != null)
		{
			text = _cachedMultipleText;
			return _cachedHasMultipleText;
		}
		text = ConvertValueToDisplayedString(GetValue());
		_cachedMultipleText = text;
		_cachedHasMultipleText = false;
		return false;
	}

	internal void FontSetOnProperty()
	{
		if (!IsSetFont && !mOwnerEnumerator.Property.IsSetFont)
		{
			return;
		}
		_editboxLeftMargin = Grid.GlobalTextMargin - 3;
		if (mOwnerEnumerator.Property.Selected)
		{
			Control inPlaceControl = Grid.InPlaceControl;
			if (inPlaceControl != null && (inPlaceControl as IInPlaceControl).OwnerPropertyEnumerator == OwnerEnumerator)
			{
				inPlaceControl.Font = Font;
				Rectangle itemRect = Grid.Grid.GetItemRect(mOwnerEnumerator);
				Rectangle valueRect = mOwnerEnumerator.Property.GetValueRect(itemRect);
				Feel.MoveControl(valueRect, mOwnerEnumerator);
			}
		}
	}

	private void InitializeUITypeEditor()
	{
		EditorAttribute editorAttribute = (EditorAttribute)GetAttribute(typeof(EditorAttribute));
		if (editorAttribute == null)
		{
			object[] customAttributes = UnderlyingType.GetCustomAttributes(typeof(EditorAttribute), inherit: true);
			foreach (object obj in customAttributes)
			{
				if (obj is EditorAttribute)
				{
					editorAttribute = (EditorAttribute)obj;
					break;
				}
			}
		}
		if (editorAttribute != null)
		{
			Type[] array = new Type[1] { typeof(Type) };
			Type typeFromName = Utilities.GetTypeFromName(editorAttribute.EditorTypeName, UnderlyingType);
			if (typeFromName == null)
			{
				return;
			}
			if (typeFromName != null && typeFromName.IsSubclassOf(typeof(UITypeEditor)))
			{
				if (typeFromName.GetConstructor(array) != null)
				{
					_uiTypeEditor = (UITypeEditor)TypeDescriptor.CreateInstance(null, typeFromName, array, new object[1] { UnderlyingType });
				}
				else
				{
					_uiTypeEditor = (UITypeEditor)TypeDescriptor.CreateInstance(null, typeFromName, null, null);
				}
			}
		}
		if (_uiTypeEditor == null)
		{
			_uiTypeEditor = (UITypeEditor)PropertyDescriptor.GetEditor(typeof(UITypeEditor));
		}
		if (_uiTypeEditor == null && TypeConverter is NullableConverter nullableConverter)
		{
			_uiTypeEditor = (UITypeEditor)TypeDescriptor.GetEditor(nullableConverter.UnderlyingType, typeof(UITypeEditor));
		}
		if (_uiTypeEditor == null)
		{
			_uiTypeEditor = (UITypeEditor)TypeDescriptor.GetEditor(UnderlyingType, typeof(UITypeEditor));
		}
		PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(mOwnerEnumerator);
		if (_uiTypeEditor != null && !_uiTypeEditor.GetType().Equals(typeof(UITypeEditor)) && _uiTypeEditor.GetPaintValueSupported(typeDescriptorContext) && _uiTypeEditor.GetEditStyle(typeDescriptorContext) == UITypeEditorEditStyle.None && TypeConverter.GetStandardValuesSupported(typeDescriptorContext))
		{
			_drawingDoneByEditor = true;
		}
	}

	public void SetAttribute(Attribute attr)
	{
		AttributeUsageAttribute attributeUsageAttribute = (AttributeUsageAttribute)TypeDescriptor.GetAttributes(attr)[typeof(AttributeUsageAttribute)];
		if (!attributeUsageAttribute.AllowMultiple || attr.GetType() == typeof(EditorAttribute))
		{
			SetSingleAttribute(attr);
			if (attr.GetType() == typeof(EditorAttribute) && OwnerEnumerator != null)
			{
				InitializeUITypeEditor();
				Feel = Grid.Grid.FindFeel(OwnerEnumerator.Parent, OwnerEnumerator);
			}
			else if (attr.GetType() == typeof(TypeConverterAttribute))
			{
				mConverter = null;
			}
			else if (attr.GetType() == typeof(DefaultValueAttribute))
			{
				Grid.Grid.HandleDefaultValue(OwnerEnumerator);
			}
			return;
		}
		ArrayList arrayList;
		if (!_customAttributes.ContainsKey(attr.GetType()))
		{
			arrayList = new ArrayList();
			_customAttributes[attr.GetType()] = arrayList;
		}
		else
		{
			arrayList = (ArrayList)_customAttributes[attr.GetType()];
		}
		if (attr is PropertyAndChildrenAttribute)
		{
			PropertyAndChildrenAttribute propertyAndChildrenAttribute = attr as PropertyAndChildrenAttribute;
			for (int i = 0; i < arrayList.Count; i++)
			{
				PropertyAndChildrenAttribute propertyAndChildrenAttribute2 = (PropertyAndChildrenAttribute)arrayList[i];
				if (propertyAndChildrenAttribute2.ChildPropertyName.Equals(propertyAndChildrenAttribute.ChildPropertyName))
				{
					arrayList.RemoveAt(i);
					break;
				}
			}
		}
		arrayList.Add(attr);
		if (attr.GetType() == typeof(PropertyValueDisplayedAsAttribute))
		{
			ClearStandardValues();
			ExtractDisplayedValues(dontTriggerEvent: true);
		}
		else if (attr.GetType() == typeof(PropertyValidatorAttribute))
		{
			Validator = (attr as PropertyValidatorAttribute).Validator;
		}
	}

	public void DeleteAttribute(Attribute attr)
	{
		AttributeUsageAttribute attributeUsageAttribute = (AttributeUsageAttribute)TypeDescriptor.GetAttributes(attr)[typeof(AttributeUsageAttribute)];
		if (!attributeUsageAttribute.AllowMultiple || attr.GetType() == typeof(EditorAttribute))
		{
			DeleteSingleAttribute(attr);
			if (attr.GetType() == typeof(EditorAttribute))
			{
				InitializeUITypeEditor();
				Feel = Grid.Grid.FindFeel(OwnerEnumerator.Parent, OwnerEnumerator);
			}
			else if (attr.GetType() == typeof(TypeConverterAttribute))
			{
				mConverter = null;
			}
			else if (attr.GetType() == typeof(DefaultValueAttribute))
			{
				Grid.Grid.HandleDefaultValue(OwnerEnumerator);
			}
			return;
		}
		ArrayList arrayList = (ArrayList)_customAttributes[attr.GetType()];
		if (arrayList == null)
		{
			return;
		}
		for (int i = 0; i < arrayList.Count; i++)
		{
			Attribute attribute = (Attribute)arrayList[i];
			if (attribute.Equals(attr))
			{
				arrayList.RemoveAt(i);
				break;
			}
		}
	}

	internal void SetSingleAttribute(Attribute attr)
	{
		ArrayList arrayList;
		if ((arrayList = (ArrayList)_customAttributes[attr.GetType()]) == null)
		{
			arrayList = new ArrayList();
			_customAttributes[attr.GetType()] = arrayList;
		}
		else
		{
			arrayList.Clear();
		}
		arrayList.Add(attr);
	}

	internal void DeleteSingleAttribute(Attribute attr)
	{
		_customAttributes.Remove(attr.GetType());
	}

	public PropertyValue()
	{
		_foreColor = Color.Empty;
		_backColor = Color.Empty;
		_disabledForeColor = Color.Empty;
	}

	public PropertyValue(PropertyValue parentValue)
		: this()
	{
		_parentValue = parentValue;
	}

	public virtual bool ValidateValue(object value, out PropertyEnumerator invalidPropertyEnum)
	{
		invalidPropertyEnum = Grid.RightBound;
		return true;
	}

	public static bool IsErrorCode(ValueValidationResult result)
	{
		return (result & ValueValidationResult.ErrorCode) != 0;
	}

	public ValueValidationResult SetValue(object value)
	{
		PropertyEnumerator invalidPropEnum;
		return SetValue(value, out invalidPropEnum);
	}

	public ValueValidationResult SetValueFromInPlaceCtrl(object value, string valueKey)
	{
		PropertyEnumerator invalidPropEnum;
		return SetValueFromInPlaceCtrl(value, out invalidPropEnum, valueKey);
	}

	public ValueValidationResult SetValueFromInPlaceCtrl(object value, out PropertyEnumerator invalidPropEnum, string valueKey)
	{
		_setValueFromInPlaceCtrl = true;
		DontConvertStringValues = true;
		if (valueKey != null)
		{
			Grid.NotifyPropertyChanging(new PropertyChangingEventArgs(OwnerEnumerator, valueKey));
		}
		else
		{
			Grid.NotifyPropertyChanging(new PropertyChangingEventArgs(OwnerEnumerator));
		}
		ValueValidationResult result = SetValue(value, out invalidPropEnum);
		DontConvertStringValues = false;
		_setValueFromInPlaceCtrl = false;
		return result;
	}

	protected void CheckRecreateAfterValueChanged()
	{
		object[] selectedObjects = Grid.SelectedObjects;
		ShowChildPropertiesAttribute showChildPropertiesAttribute = (ShowChildPropertiesAttribute)GetAttribute(typeof(ShowChildPropertiesAttribute));
		if (selectedObjects != null && selectedObjects.Length > 0)
		{
			PropertyEnumerator propertyEnumerator = (PropertyEnumerator)mOwnerEnumerator.Clone();
			PropertyValue value = propertyEnumerator.Property.Value;
			PropertyValue propertyValue = null;
			PropertyTypeDescriptorContext typeDescriptorContext = value.GetTypeDescriptorContext(propertyEnumerator);
			if (value.TypeConverter.GetCreateInstanceSupported(typeDescriptorContext) || value.TypeConverter.GetPropertiesSupported(typeDescriptorContext) || (showChildPropertiesAttribute != null && showChildPropertiesAttribute.ShowChildProperties))
			{
				propertyValue = value;
			}
			RefreshPropertiesAttribute refreshPropertiesAttribute = (RefreshPropertiesAttribute)GetAttribute(typeof(RefreshPropertiesAttribute));
			bool flag = refreshPropertiesAttribute != null && refreshPropertiesAttribute.RefreshProperties == RefreshProperties.All;
			while (!flag && propertyEnumerator.HasParent)
			{
				propertyEnumerator.MoveParent();
				value = propertyEnumerator.Property.Value;
				if (value == null || !value.TypeConverter.GetCreateInstanceSupported(typeDescriptorContext))
				{
					break;
				}
				propertyValue = value;
				refreshPropertiesAttribute = (RefreshPropertiesAttribute)value.GetAttribute(typeof(RefreshPropertiesAttribute));
				flag = refreshPropertiesAttribute != null && refreshPropertiesAttribute.RefreshProperties == RefreshProperties.All;
			}
			if (propertyValue != null || flag)
			{
				if (_setValueFromInPlaceCtrl)
				{
					Grid.ActiveControl = Grid.Grid;
					Grid.Grid.Focus();
				}
				Grid.BeginUpdate();
				int propertyHashCode = Grid.GetPropertyHashCode(OwnerEnumerator);
				if (flag)
				{
					Grid.RefreshProperties();
				}
				else if (propertyValue != null)
				{
					Grid.RecreateProperty(propertyValue.OwnerEnumerator);
				}
				if (_setValueFromInPlaceCtrl && propertyValue != this && propertyHashCode == Grid.GetPropertyHashCode(Grid.SelectedPropertyEnumerator))
				{
					OwnerEnumerator = Grid.SelectedPropertyEnumerator;
				}
				if (Grid.InPlaceControl != null && Grid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey)
				{
					Grid.ActiveControl = Grid.InPlaceControl;
					Grid.ActiveControl.Focus();
				}
				Grid.EndUpdate();
			}
			return;
		}
		PropertyTypeDescriptorContext typeDescriptorContext2 = GetTypeDescriptorContext(mOwnerEnumerator);
		RefreshPropertiesAttribute refreshPropertiesAttribute2 = (RefreshPropertiesAttribute)GetAttribute(typeof(RefreshPropertiesAttribute));
		if (TypeConverter.GetCreateInstanceSupported(typeDescriptorContext2) || TypeConverter.GetPropertiesSupported(typeDescriptorContext2) || (showChildPropertiesAttribute != null && showChildPropertiesAttribute.ShowChildProperties) || (refreshPropertiesAttribute2 != null && refreshPropertiesAttribute2.RefreshProperties == RefreshProperties.All) || GetValue() is ICustomTypeDescriptor)
		{
			if (_setValueFromInPlaceCtrl)
			{
				Grid.ActiveControl = Grid.Grid;
				Grid.Grid.Focus();
			}
			Grid.RecreateProperty(OwnerEnumerator);
			if (Grid.InPlaceControl != null && Grid.NavigationKeyMode == PropertyGrid.NavigationKeyModes.TabKey)
			{
				Grid.ActiveControl = Grid.InPlaceControl;
				Grid.ActiveControl.Focus();
			}
		}
	}

	public ValueValidationResult SetValue(object value, out PropertyEnumerator invalidPropEnum)
	{
		invalidPropEnum = Grid.RightBound;
		object obj = value;
		if (value is string && !DontConvertStringValues)
		{
			string text = obj as string;
			if (_look != null)
			{
				text = _look.TransformDisplayString(text);
			}
			try
			{
				obj = ConvertDisplayedStringToValue(text);
			}
			catch (Exception e)
			{
				Grid.NotifyValueValidation(new ValueValidationEventArgs(OwnerEnumerator, OwnerEnumerator, value, ValueValidationResult.TypeConverterFailed, e));
				return ValueValidationResult.TypeConverterFailed;
			}
		}
		ValueValidationResult result = ValueValidationResult.ValidatorFailed;
		if (ValidateValue(obj, out invalidPropEnum))
		{
			bool flag;
			try
			{
				flag = SetValueInternal(obj, ValueUpdateSource.FromOutside);
			}
			catch (PropertyValueSimulatedCreateInstanceException lastExceptionByClient)
			{
				_lastExceptionByClient = lastExceptionByClient;
				return ValueValidationResult.TypeConverterFailed;
			}
			catch (Exception lastExceptionByClient2)
			{
				_lastExceptionByClient = lastExceptionByClient2;
				return ValueValidationResult.ExceptionByClient;
			}
			if (flag && mOwnerEnumerator != null)
			{
				Control inPlaceControl = Grid.InPlaceControl;
				if (inPlaceControl is IInPlaceControl inPlaceControl2 && !_setValueFromInPlaceCtrl && inPlaceControl2.OwnerPropertyEnumerator == mOwnerEnumerator)
				{
					inPlaceControl2.OwnerPropertyEnumerator = mOwnerEnumerator;
				}
				Grid.HandleDefaultValue(mOwnerEnumerator);
			}
			result = (flag ? ValueValidationResult.Validated : ValueValidationResult.TypeConverterFailed);
		}
		if (!_preventRefreshProperties && !IsErrorCode(result) && AdditionalValueKey == null && !(this is PropertyValueSimulated) && mOwnerEnumerator != null)
		{
			CheckRecreateAfterValueChanged();
		}
		return result;
	}

	public ValueValidationResult SetValues(object[] values, out PropertyEnumerator invalidPropEnum)
	{
		invalidPropEnum = Grid.RightBound;
		ValueValidationResult result = ValueValidationResult.ValidatorFailed;
		bool flag = true;
		foreach (object value in values)
		{
			if (!ValidateValue(value, out invalidPropEnum))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			bool flag2;
			try
			{
				flag2 = SetValuesInternal(values, ValueUpdateSource.FromOutside);
			}
			catch (Exception lastExceptionByClient)
			{
				_lastExceptionByClient = lastExceptionByClient;
				return ValueValidationResult.ExceptionByClient;
			}
			if (flag2 && mOwnerEnumerator != null)
			{
				Control inPlaceControl = Grid.InPlaceControl;
				if (inPlaceControl is IInPlaceControl inPlaceControl2 && !_setValueFromInPlaceCtrl && inPlaceControl2.OwnerPropertyEnumerator == mOwnerEnumerator)
				{
					inPlaceControl2.OwnerPropertyEnumerator = mOwnerEnumerator;
					Grid.HandleDefaultValue(mOwnerEnumerator);
				}
				else
				{
					Grid.Grid.Invalidate();
				}
			}
			result = (flag2 ? ValueValidationResult.Validated : ValueValidationResult.TypeConverterFailed);
		}
		if (!_preventRefreshProperties && !IsErrorCode(result) && mOwnerEnumerator != null)
		{
			CheckRecreateAfterValueChanged();
		}
		return result;
	}

	protected virtual bool SetValueInternal(object value, ValueUpdateSource updateSource)
	{
		return false;
	}

	protected virtual bool SetValuesInternal(object[] values, ValueUpdateSource updateSource)
	{
		return false;
	}

	protected internal PropertyValue[] GetChildValues(PropertyEnumerator parentEnum, object[] targetInstances, PropertyDescriptor propertyDescriptor)
	{
		PropertyDescriptor[] childPropertyDescriptors = GetChildPropertyDescriptors(parentEnum, targetInstances, propertyDescriptor);
		object instance = null;
		if (parentEnum != null)
		{
			instance = ((parentEnum.Property.Value.TargetInstances.Length == 1) ? parentEnum.Property.Value.TargetInstance : parentEnum.Property.Value.TargetInstances);
		}
		PropertyTypeDescriptorContext context = new PropertyTypeDescriptorContext(propertyDescriptor, instance, null, Grid);
		bool createInstanceSupported = TypeConverter.GetCreateInstanceSupported(context);
		PropertyValue[] array = new PropertyValue[childPropertyDescriptors.Length];
		int num = 0;
		PropertyDescriptor[] array2 = childPropertyDescriptors;
		foreach (PropertyDescriptor propertyDescriptor2 in array2)
		{
			PropertyValue propertyValue = ((!createInstanceSupported) ? new PropertyValueIndirect(Grid, this, targetInstances, propertyDescriptor2, null) : new PropertyValueSimulated(Grid, this, targetInstances, propertyDescriptor2));
			propertyValue.ChildValueCreatedBySPG = true;
			propertyValue.Id = (propertyDescriptor2.Attributes[typeof(PropertyIdAttribute)] as PropertyIdAttribute)?.Id ?? 0;
			array[num++] = propertyValue;
		}
		return array;
	}

	internal bool HasChildPropertyDescriptors(PropertyEnumerator parentEnum, object[] targetInstances, PropertyDescriptor propertyDescriptor)
	{
		return GetChildPropertyDescriptors(parentEnum, targetInstances, propertyDescriptor).Length > 0;
	}

	private PropertyDescriptor[] GetChildPropertyDescriptors(PropertyEnumerator parentEnum, object[] targetInstances, PropertyDescriptor propertyDescriptor)
	{
		Hashtable hashtable = new Hashtable();
		PropertyDescriptorCollection propertyDescriptorCollection = null;
		bool flag = true;
		for (int i = 0; i < targetInstances.Length; i++)
		{
			object targetInstance = targetInstances[i];
			PropertyDescriptor propertyDescriptor2 = ((propertyDescriptor is MultiplePropertyDescriptor) ? (propertyDescriptor as MultiplePropertyDescriptor)[i] : propertyDescriptor);
			PropertyDescriptorCollection propertyDescriptors = Grid.GetPropertyDescriptors(parentEnum, targetInstance, propertyDescriptor2, TypeConverter);
			if (propertyDescriptorCollection == null)
			{
				propertyDescriptorCollection = propertyDescriptors;
			}
			foreach (PropertyDescriptor item in propertyDescriptors)
			{
				if (item.Attributes[typeof(MergablePropertyAttribute)] is MergablePropertyAttribute { AllowMerge: false })
				{
					continue;
				}
				if (flag)
				{
					ArrayList arrayList = new ArrayList();
					arrayList.Add(item);
					hashtable.Add(item, arrayList);
					continue;
				}
				ArrayList arrayList2 = (ArrayList)hashtable[item];
				if (arrayList2 != null)
				{
					if (Grid.MatchPropertyDescriptors((PropertyDescriptor)arrayList2[0], item))
					{
						arrayList2.Add(item);
					}
					else
					{
						hashtable.Remove(item);
					}
				}
			}
			flag = false;
		}
		ArrayList arrayList3 = new ArrayList();
		if (targetInstances.Length > 1)
		{
			foreach (PropertyDescriptor item2 in propertyDescriptorCollection)
			{
				ArrayList arrayList4 = (ArrayList)hashtable[item2];
				if (arrayList4 != null && arrayList4.Count == targetInstances.Length)
				{
					PropertyDescriptor[] array = new PropertyDescriptor[arrayList4.Count];
					Array.Copy(arrayList4.ToArray(), array, arrayList4.Count);
					arrayList3.Add(new MultiplePropertyDescriptor(array));
				}
			}
		}
		else
		{
			foreach (PropertyDescriptor item3 in propertyDescriptorCollection)
			{
				arrayList3.Add(item3);
			}
		}
		PropertyDescriptor[] array2 = new PropertyDescriptor[arrayList3.Count];
		Array.Copy(arrayList3.ToArray(), array2, arrayList3.Count);
		return array2;
	}

	public string ConvertValueToDisplayedString(object value)
	{
		if (mActualObjectToDisplayedStr != null && mActualObjectToDisplayedStr.Count > 0)
		{
			string text = null;
			if (value != null)
			{
				text = (string)mActualObjectToDisplayedStr[value];
			}
			else if (mDisplayedStrForNullActualObjectSet)
			{
				return mDisplayedStrForNullActualObject;
			}
			if (text != null)
			{
				return text;
			}
		}
		PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(OwnerEnumerator);
		if (TypeConverter.CanConvertTo(typeDescriptorContext, typeof(string)))
		{
			string text2 = TypeConverter.ConvertToString(typeDescriptorContext, CultureInfo, value);
			if (text2 != null)
			{
				return text2;
			}
			return "";
		}
		if (value != null)
		{
			return value.ToString();
		}
		return "";
	}

	protected string ConvertValueToDisplayedString(object value, PropertyDescriptor pd, object target)
	{
		if (mActualObjectToDisplayedStr != null && mActualObjectToDisplayedStr.Count > 0 && value != null)
		{
			string text = null;
			if (value != null)
			{
				text = (string)mActualObjectToDisplayedStr[value];
			}
			if (text != null)
			{
				return text;
			}
		}
		PropertyTypeDescriptorContext context = new PropertyTypeDescriptorContext(pd, target, OwnerEnumerator, Grid);
		if (TypeConverter.CanConvertTo(context, typeof(string)))
		{
			string text2 = TypeConverter.ConvertToString(context, CultureInfo, value);
			if (text2 != null)
			{
				return text2;
			}
			return "";
		}
		if (value != null)
		{
			return value.ToString();
		}
		return "";
	}

	public object ConvertDisplayedStringToValue(string displayedStr)
	{
		if (displayedStr != null)
		{
			if (mDisplayedStrToActualObject != null)
			{
				object obj = mDisplayedStrToActualObject[displayedStr];
				if (obj != null)
				{
					return obj;
				}
			}
			if (mDisplayedStrForNullActualObject != null && mDisplayedStrForNullActualObject.Equals(displayedStr))
			{
				return null;
			}
			PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(OwnerEnumerator);
			if (TypeConverter.CanConvertFrom(typeDescriptorContext, typeof(string)))
			{
				return TypeConverter.ConvertFromString(typeDescriptorContext, CultureInfo, displayedStr);
			}
		}
		return GetValue();
	}

	public string GetActualString(string displayStr)
	{
		if (mDisplayedStrToActualObject != null)
		{
			string text = (string)mDisplayedStrToActualObject[displayStr];
			if (text != null)
			{
				return text;
			}
			return displayStr;
		}
		return displayStr;
	}

	public Rectangle GetStringValueRect(Graphics graphics, Rectangle valueRect, Point point)
	{
		return GetStringValueRect(graphics, valueRect, point, DrawValueIn.None);
	}

	public Rectangle GetStringValueRect(Graphics graphics, Rectangle valueRect, Point point, DrawValueIn drawValueIn)
	{
		if (_look != null)
		{
			return _look.GetDisplayStringRect(graphics, valueRect, point);
		}
		Rectangle result = valueRect;
		result.Width = Win32Calls.GetTextExtent(graphics, DisplayString, Font).Width;
		result.X += Grid.GlobalTextMargin;
		Rectangle rectangle = DrawIcon(null, valueRect, drawValueIn);
		if (rectangle.Width > 0)
		{
			rectangle.Width += Grid.GlobalTextMargin;
		}
		result.X += rectangle.Width;
		PropertyTypeDescriptorContext typeDescriptorContext = mOwnerEnumerator.Property.Value.GetTypeDescriptorContext(mOwnerEnumerator);
		if (DrawingDoneByEditor || (Feel is PropertyUITypeEditorFeel && _uiTypeEditor.GetPaintValueSupported(typeDescriptorContext)))
		{
			result.X += UITypeEditorCustomDrawingWidth + 1 + Grid.GlobalTextMargin;
		}
		return result;
	}

	public void DrawValue(Graphics graphics, Rectangle valueRect, Color textColor, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, DrawValueIn drawValueIn)
	{
		string hyperLink = mOwnerEnumerator.Property.HyperLink;
		Font font = ((hyperLink.Length == 0) ? Font : new Font(Font, FontStyle.Underline));
		DrawValue(graphics, valueRect, textColor, font, propEnum, drawAnotherString, multipleTexts, drawValueIn);
		if (hyperLink.Length != 0)
		{
			font.Dispose();
		}
	}

	public void DrawValue(Graphics graphics, Rectangle valueRect, Color textColor, Font font, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, DrawValueIn drawValueIn)
	{
		int num = 0;
		num = drawValueIn switch
		{
			DrawValueIn.Grid => Grid.Grid.ClientSize.Width, 
			DrawValueIn.InPlaceCtrl => Feel.InPlaceCtrl.ClientSize.Width, 
			_ => valueRect.Width, 
		};
		if (!Rectangle.Intersect(Grid.TransformRectIfRTL(valueRect, num), Rectangle.Round(graphics.ClipBounds)).IsEmpty)
		{
			if (_look != null)
			{
				_look.OnDraw(graphics, valueRect, textColor, propEnum, drawAnotherString, multipleTexts, drawValueIn);
			}
			else
			{
				OnDraw(graphics, valueRect, textColor, font, propEnum, drawAnotherString, multipleTexts, drawValueIn);
			}
		}
	}

	public void OnDraw(Graphics graphics, Rectangle valueRect, Color textColor, Font font, PropertyEnumerator propEnum, string drawAnotherString, bool multipleTexts, DrawValueIn drawValueIn)
	{
		string text = "";
		text = ((drawAnotherString == null) ? DisplayString : drawAnotherString);
		int num = 0;
		num = drawValueIn switch
		{
			DrawValueIn.Grid => Grid.Grid.ClientSize.Width, 
			DrawValueIn.InPlaceCtrl => Feel.InPlaceCtrl.ClientSize.Width, 
			_ => valueRect.Width, 
		};
		Win32Calls.SetClippingRect(graphics, Grid.TransformRectIfRTL(valueRect, num));
		Rectangle rectangle = DrawIcon(graphics, valueRect, drawValueIn);
		Win32Calls.ResetClippingRect(graphics);
		if (rectangle.Width > 0)
		{
			rectangle.Width += Grid.GlobalTextMargin;
		}
		PropertyTypeDescriptorContext typeDescriptorContext = propEnum.Property.Value.GetTypeDescriptorContext(propEnum);
		if ((Feel is PropertyUITypeEditorFeel && _uiTypeEditor.GetPaintValueSupported(typeDescriptorContext)) || DrawingDoneByEditor)
		{
			Rectangle rect = valueRect;
			rect.X += rectangle.Width + Grid.GlobalTextMargin;
			rect.Width = UITypeEditorCustomDrawingWidth;
			rect.Y++;
			rect.Height -= 2;
			if (drawAnotherString == null)
			{
				if (multipleTexts)
				{
					_uiTypeEditor.PaintValue(new PaintValueEventArgs(typeDescriptorContext, null, graphics, Grid.TransformRectIfRTL(rect, num)));
				}
				else
				{
					_uiTypeEditor.PaintValue(new PaintValueEventArgs(typeDescriptorContext, propEnum.Property.Value.GetValue(), graphics, Grid.TransformRectIfRTL(rect, num)));
				}
			}
			else if (multipleTexts)
			{
				_uiTypeEditor.PaintValue(new PaintValueEventArgs(typeDescriptorContext, null, graphics, Grid.TransformRectIfRTL(rect, num)));
			}
			else
			{
				object obj = null;
				try
				{
					obj = propEnum.Property.Value.ConvertDisplayedStringToValue(text);
				}
				catch (Exception)
				{
					obj = propEnum.Property.Value.GetValue();
				}
				_uiTypeEditor.PaintValue(new PaintValueEventArgs(typeDescriptorContext, obj, graphics, Grid.TransformRectIfRTL(rect, num)));
			}
			Rectangle rect2 = new Rectangle(rect.Left, rect.Top, UITypeEditorCustomDrawingWidth, rect.Height);
			rect2 = Grid.TransformRectIfRTL(rect2, num);
			rect2.Width--;
			rect2.Height--;
			graphics.DrawRectangle(SystemPens.WindowText, rect2);
			rect.Height++;
			rect.X = rect.Right + Grid.GlobalTextMargin + 1;
			rect.Width = valueRect.Right - rect.Left;
			rect = Grid.TransformRectIfRTL(rect, num);
			Win32Calls.DrawText(graphics, text, ref rect, font, textColor, 0x824 | (((mPropCtrl.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
		else if (text != null)
		{
			valueRect.Height++;
			Rectangle stringValueRect = GetStringValueRect(graphics, valueRect, Point.Empty, drawValueIn);
			stringValueRect.Width = valueRect.Right - stringValueRect.Left;
			stringValueRect = Grid.TransformRectIfRTL(stringValueRect, num);
			Win32Calls.DrawText(graphics, text, ref stringValueRect, font, textColor, 0x824 | (((mPropCtrl.EllipsisMode & PropertyGrid.EllipsisModes.EllipsisOnValues) != PropertyGrid.EllipsisModes.None) ? 32768 : 0) | ((Grid.RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
	}

	public Rectangle DrawIcon(Graphics graphics, Rectangle valueRect, DrawValueIn drawValueIn)
	{
		if (drawValueIn == DrawValueIn.DropDown && ImageSource != PropertyGrid.ImageSources.Value)
		{
			return Rectangle.Empty;
		}
		int num = 0;
		num = drawValueIn switch
		{
			DrawValueIn.Grid => Grid.Grid.ClientSize.Width, 
			DrawValueIn.InPlaceCtrl => Feel.InPlaceCtrl.ClientSize.Width, 
			_ => valueRect.Width, 
		};
		valueRect.Height++;
		ImageList imageList = null;
		if (ImageSource == PropertyGrid.ImageSources.Value)
		{
			if (_imageList == null)
			{
				return Rectangle.Empty;
			}
			imageList = _imageList;
		}
		else if (ImageSource == PropertyGrid.ImageSources.Grid)
		{
			if (Grid.ImageList == null)
			{
				return Rectangle.Empty;
			}
			imageList = Grid.ImageList;
		}
		if (imageList != null && (ImageIndex >= imageList.Images.Count || ImageIndex < 0))
		{
			return Rectangle.Empty;
		}
		if (ImageSource == PropertyGrid.ImageSources.Image && Image == null)
		{
			return Rectangle.Empty;
		}
		Rectangle rectangle = Rectangle.Empty;
		if (_imageSize != Size.Empty && _imageSize.Height <= valueRect.Height - 2)
		{
			int y = valueRect.Top + (valueRect.Height - 2 - _imageSize.Height) / 2 + 1;
			rectangle = new Rectangle(valueRect.Left + Grid.GlobalTextMargin, y, _imageSize.Width, _imageSize.Height);
		}
		else if (imageList != null)
		{
			int num2 = Math.Min(imageList.ImageSize.Height, (drawValueIn == DrawValueIn.DropDown) ? (valueRect.Height - 1) : Grid.BasicPropertyHeight);
			int y2 = valueRect.Top + (Grid.BasicPropertyHeight - imageList.ImageSize.Height) / 2;
			rectangle = new Rectangle(valueRect.Left + Grid.GlobalTextMargin, y2, num2, num2 * imageList.ImageSize.Width / imageList.ImageSize.Height);
		}
		else if (ImageSource == PropertyGrid.ImageSources.Image)
		{
			int num3 = Math.Min(Image.Height, (drawValueIn == DrawValueIn.DropDown) ? (valueRect.Height - 1) : Grid.BasicPropertyHeight);
			int y3 = valueRect.Top + (Grid.BasicPropertyHeight - Image.Height) / 2;
			rectangle = new Rectangle(valueRect.Left + Grid.GlobalTextMargin, y3, num3 - 1, (num3 - 1) * Image.Width / Image.Height);
		}
		if (graphics != null)
		{
			if (imageList != null && ImageIndex < imageList.Images.Count && ImageIndex >= 0)
			{
				rectangle = Grid.TransformRectIfRTL(rectangle, num);
				Win32Calls.ImageListDraw(imageList, ImageIndex, graphics, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
				rectangle = Grid.TransformRectIfRTL(rectangle, num);
			}
			else if (ImageSource == PropertyGrid.ImageSources.Image && Image != null)
			{
				rectangle = Grid.TransformRectIfRTL(rectangle, num);
				graphics.DrawImage(Image, rectangle);
				rectangle = Grid.TransformRectIfRTL(rectangle, num);
			}
		}
		return rectangle;
	}

	public string[] GetDisplayedValues()
	{
		if (cachedDisplayedStrings == null)
		{
			ExtractDisplayedValues();
		}
		return cachedDisplayedStrings;
	}

	private Attribute _GetAttribute(Type attributeType)
	{
		ArrayList arrayList = (ArrayList)_customAttributes[attributeType];
		if (arrayList != null)
		{
			return (Attribute)arrayList[0];
		}
		return null;
	}

	public virtual Attribute GetAttribute(Type attributeType)
	{
		return _GetAttribute(attributeType);
	}

	public virtual ArrayList GetAttributes(Type attributeType)
	{
		ArrayList arrayList = (ArrayList)_customAttributes[attributeType];
		if (arrayList != null)
		{
			return arrayList;
		}
		return new ArrayList();
	}

	internal Attribute[] GetCustomAttributes()
	{
		ArrayList arrayList = new ArrayList();
		foreach (ArrayList value in _customAttributes.Values)
		{
			arrayList.AddRange(value);
		}
		Attribute[] array = new Attribute[arrayList.Count];
		arrayList.CopyTo(array, 0);
		return array;
	}

	internal Attribute[] GetCustomAttributes(Type attributeType)
	{
		ArrayList arrayList = (ArrayList)_customAttributes[attributeType];
		Attribute[] array = new Attribute[arrayList?.Count ?? 0];
		arrayList?.CopyTo(array, 0);
		return array;
	}

	protected void ClearStandardValues()
	{
		_standardValues = null;
	}

	public object[] GetStandardValues()
	{
		if (_standardValues == null)
		{
			TypeConverter typeConverter = TypeConverter;
			PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(OwnerEnumerator);
			ICollection collection = null;
			if (typeConverter.GetStandardValuesSupported(typeDescriptorContext) && (collection = typeConverter.GetStandardValues(typeDescriptorContext)) != null)
			{
				_standardValues = new object[collection.Count];
				collection.CopyTo(_standardValues, 0);
			}
			else
			{
				_standardValues = new object[0];
			}
		}
		return _standardValues;
	}

	protected void ExtractDisplayedValues()
	{
		ExtractDisplayedValues(dontTriggerEvent: false);
	}

	protected void ExtractDisplayedValues(bool dontTriggerEvent)
	{
		if (mActualObjectToDisplayedStr != null)
		{
			mActualObjectToDisplayedStr.Clear();
			mDisplayedStrToActualObject.Clear();
			cachedDisplayedStrings = null;
		}
		ArrayList arrayList = new ArrayList();
		object[] array = new object[0];
		if (!dontTriggerEvent)
		{
			DisplayedValuesNeededEventArgs e = new DisplayedValuesNeededEventArgs(OwnerEnumerator);
			Grid.OnDisplayedValuesNeeded(e);
			if (e.DisplayedValues != null)
			{
				array = e.DisplayedValues;
			}
		}
		ArrayList attributes = GetAttributes(typeof(PropertyValueDisplayedAsAttribute));
		foreach (PropertyValueDisplayedAsAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0 && item.TriggerDisplayedValuesNeededEventAtDropDown)
			{
				_triggerDisplayedValuesNeededEventAtDropDown = true;
				break;
			}
			if (array.Length == 0 && item.DisplayedValues != null)
			{
				array = item.DisplayedValues;
				break;
			}
		}
		if (ParentValue != null)
		{
			attributes = ParentValue.GetAttributes(typeof(PropertyValueDisplayedAsAttribute));
			foreach (PropertyValueDisplayedAsAttribute item2 in attributes)
			{
				if (item2.ChildPropertyName == DisplayName && item2.TriggerDisplayedValuesNeededEventAtDropDown)
				{
					_triggerDisplayedValuesNeededEventAtDropDown = true;
					break;
				}
				if (array.Length == 0)
				{
					object[] displayedValues = item2.GetDisplayedValues(DisplayName);
					if (displayedValues != null)
					{
						array = displayedValues;
						break;
					}
				}
			}
		}
		object[] standardValues = GetStandardValues();
		PropertyTypeDescriptorContext typeDescriptorContext = GetTypeDescriptorContext(OwnerEnumerator);
		if (array.Length > 0 || standardValues.Length > 0)
		{
			if (mActualObjectToDisplayedStr == null)
			{
				mActualObjectToDisplayedStr = new Hashtable();
				mDisplayedStrToActualObject = new Hashtable();
			}
			if (UnderlyingType.IsEnum)
			{
				FieldInfo[] fields = UnderlyingType.GetFields(BindingFlags.Static | BindingFlags.Public);
				Hashtable hashtable = new Hashtable();
				FieldInfo[] array2 = fields;
				foreach (FieldInfo fieldInfo in array2)
				{
					object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(BrowsableAttribute), inherit: false);
					if (customAttributes.Length == 0 || ((BrowsableAttribute)customAttributes[0]).Browsable)
					{
						hashtable[fieldInfo.Name] = fieldInfo;
					}
				}
				int num = 0;
				object[] array3 = standardValues;
				foreach (object obj in array3)
				{
					FieldInfo fieldInfo2 = (FieldInfo)hashtable[obj.ToString()];
					if (fieldInfo2 == null)
					{
						continue;
					}
					object[] customAttributes2 = fieldInfo2.GetCustomAttributes(typeof(PropertyValueDisplayedAsAttribute), inherit: false);
					if (customAttributes2.Length > 0)
					{
						string text = (string)((PropertyValueDisplayedAsAttribute)customAttributes2[0]).DisplayedValues[0];
						if (!mDisplayedStrToActualObject.Contains(text))
						{
							arrayList.Add(text);
						}
						if (obj != null)
						{
							mActualObjectToDisplayedStr[obj] = text;
							mDisplayedStrToActualObject[text] = obj;
						}
					}
					else if (array.Length > 0)
					{
						object obj2 = null;
						if (array is KeyValuePair[])
						{
							bool flag = false;
							object[] array4 = array;
							for (int k = 0; k < array4.Length; k++)
							{
								KeyValuePair keyValuePair = (KeyValuePair)array4[k];
								if (keyValuePair.Key.Equals(obj))
								{
									obj2 = keyValuePair.Value;
									flag = true;
									break;
								}
							}
							if (!flag)
							{
								obj2 = TypeConverter.ConvertToString(typeDescriptorContext, CultureInfo, obj);
							}
							if (obj2 != null)
							{
								if (!mDisplayedStrToActualObject.Contains(obj2))
								{
									arrayList.Add(obj2);
								}
								if (obj != null)
								{
									mActualObjectToDisplayedStr[obj] = obj2;
									mDisplayedStrToActualObject[obj2] = obj;
								}
							}
						}
						else
						{
							obj2 = array[num++];
							if (!mDisplayedStrToActualObject.Contains(obj2))
							{
								arrayList.Add(obj2);
							}
							if (obj != null)
							{
								mActualObjectToDisplayedStr[obj] = obj2;
								mDisplayedStrToActualObject[obj2] = obj;
							}
						}
					}
					else
					{
						string text2 = TypeConverter.ConvertToString(typeDescriptorContext, CultureInfo, obj);
						if (text2 == null)
						{
							text2 = "";
						}
						if (!mDisplayedStrToActualObject.Contains(text2))
						{
							arrayList.Add(text2);
						}
						if (obj != null)
						{
							mActualObjectToDisplayedStr[obj] = text2;
							mDisplayedStrToActualObject[text2] = obj;
						}
					}
				}
			}
			else if (array.Length > 0)
			{
				ArrayList arrayList2 = new ArrayList();
				if (array is string[])
				{
					if (standardValues.Length > 0)
					{
						if (array.Length != standardValues.Length)
						{
							throw new ArgumentException("Displayed values count is not equal to the number of standard values.");
						}
						IEnumerator enumerator3 = standardValues.GetEnumerator();
						enumerator3.MoveNext();
						string[] array5 = array as string[];
						foreach (string text3 in array5)
						{
							if (text3 != null)
							{
								arrayList2.Add(new KeyValuePair(enumerator3.Current, text3));
							}
							enumerator3.MoveNext();
						}
					}
					else
					{
						string[] array6 = array as string[];
						foreach (string text4 in array6)
						{
							if (text4 != null)
							{
								arrayList2.Add(new KeyValuePair(text4, text4));
							}
						}
					}
				}
				else if (array is KeyValuePair[])
				{
					if (standardValues.Length > 0)
					{
						TypeConverter typeConverter = TypeConverter;
						Hashtable hashtable2 = new Hashtable(array.Length);
						object value = null;
						object[] array7 = array;
						for (int n = 0; n < array7.Length; n++)
						{
							KeyValuePair keyValuePair2 = (KeyValuePair)array7[n];
							if (keyValuePair2.Key == null)
							{
								value = keyValuePair2.Value;
							}
							else
							{
								hashtable2.Add(keyValuePair2.Key, keyValuePair2);
							}
						}
						object[] array8 = standardValues;
						foreach (object obj3 in array8)
						{
							if (obj3 == null)
							{
								arrayList2.Add(new KeyValuePair(null, value));
								continue;
							}
							KeyValuePair keyValuePair3 = (KeyValuePair)hashtable2[obj3];
							if (keyValuePair3 == null)
							{
								string text5 = typeConverter.ConvertToString(typeDescriptorContext, CultureInfo, obj3);
								if (text5 != null)
								{
									arrayList2.Add(new KeyValuePair(obj3, text5));
								}
							}
							else if (keyValuePair3.Value != null)
							{
								arrayList2.Add(keyValuePair3);
							}
						}
					}
					else
					{
						arrayList2.AddRange(array);
					}
				}
				else
				{
					TypeConverter typeConverter2 = TypeConverter;
					object[] array9 = array;
					foreach (object obj4 in array9)
					{
						string text6 = typeConverter2.ConvertToString(typeDescriptorContext, CultureInfo, obj4);
						if (text6 != null)
						{
							arrayList2.Add(new KeyValuePair(obj4, text6));
						}
					}
				}
				foreach (KeyValuePair item3 in arrayList2)
				{
					object key = item3.Key;
					string text7 = (string)item3.Value;
					if (key != null)
					{
						if (text7 != null)
						{
							mActualObjectToDisplayedStr[key] = text7;
							mDisplayedStrToActualObject[text7] = key;
							arrayList.Add(text7);
						}
					}
					else
					{
						mDisplayedStrForNullActualObject = text7;
						mDisplayedStrForNullActualObjectSet = true;
						if (text7 != null)
						{
							arrayList.Add(text7);
						}
					}
				}
			}
			else
			{
				TypeConverter typeConverter3 = TypeConverter;
				object[] array3 = standardValues;
				foreach (object value2 in array3)
				{
					string text8 = typeConverter3.ConvertToString(typeDescriptorContext, CultureInfo, value2);
					arrayList.Add((text8 == null) ? "" : text8);
				}
			}
		}
		cachedDisplayedStrings = new string[arrayList.Count];
		arrayList.CopyTo(cachedDisplayedStrings, 0);
		if (_look != null)
		{
			_look.ControlHeightMultiplier();
		}
	}

	public void ResetDisplayedValues(string[] displayedValues)
	{
		ArrayList attributes = GetAttributes(typeof(PropertyValueDisplayedAsAttribute));
		foreach (PropertyValueDisplayedAsAttribute item in attributes)
		{
			if (item.DisplayedValues != null)
			{
				DeleteAttribute(item);
				break;
			}
		}
		PropertyValueDisplayedAsAttribute attribute = new PropertyValueDisplayedAsAttribute(displayedValues);
		SetAttribute(attribute);
		ClearStandardValues();
		ExtractDisplayedValues(dontTriggerEvent: true);
	}

	public void ResetDisplayedValues(bool usePairs, object[] displayedValues)
	{
		ArrayList attributes = GetAttributes(typeof(PropertyValueDisplayedAsAttribute));
		foreach (PropertyValueDisplayedAsAttribute item in attributes)
		{
			if (item.DisplayedValues != null)
			{
				DeleteAttribute(item);
				break;
			}
		}
		PropertyValueDisplayedAsAttribute attribute = new PropertyValueDisplayedAsAttribute(usePairs, displayedValues);
		SetAttribute(attribute);
		ClearStandardValues();
		ExtractDisplayedValues(dontTriggerEvent: true);
	}

	[Obsolete("Use ResetDisplayedValues(PropertyGrid.ResetDisplayedValuesTriggerMode) instead.")]
	public void ResetDisplayedValues(bool acquireDisplayedValues)
	{
		ResetDisplayedValues(acquireDisplayedValues ? PropertyGrid.ResetDisplayedValuesTriggerMode.TriggerOnceNow : PropertyGrid.ResetDisplayedValuesTriggerMode.None);
	}

	public void ResetDisplayedValues(PropertyGrid.ResetDisplayedValuesTriggerMode triggerMode)
	{
		ArrayList attributes = GetAttributes(typeof(PropertyValueDisplayedAsAttribute));
		foreach (PropertyValueDisplayedAsAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0)
			{
				DeleteAttribute(item);
				break;
			}
		}
		ClearStandardValues();
		if ((triggerMode & PropertyGrid.ResetDisplayedValuesTriggerMode.TriggerOnceNow) != PropertyGrid.ResetDisplayedValuesTriggerMode.None)
		{
			ExtractDisplayedValues();
		}
		else
		{
			if (mActualObjectToDisplayedStr != null)
			{
				mActualObjectToDisplayedStr.Clear();
				mDisplayedStrToActualObject.Clear();
				cachedDisplayedStrings = null;
			}
			if (_look != null)
			{
				_look.ControlHeightMultiplier();
			}
		}
		_triggerDisplayedValuesNeededEventAtDropDown = (triggerMode & PropertyGrid.ResetDisplayedValuesTriggerMode.TriggerAtDropDown) != 0;
	}

	public object GetValue()
	{
		return UnderlyingValue;
	}

	public virtual object GetValue(int index)
	{
		return null;
	}

	public virtual bool IsReadOnly(PropertyEnumerator selfEnum)
	{
		return false;
	}

	public virtual string GetTooltipString(int line)
	{
		if (Look != null)
		{
			return Look.GetTooltipString(line);
		}
		return DisplayString;
	}

	public virtual Size GetTooltipStringSize(Graphics graphics, Rectangle valueRect, Point point, int line)
	{
		if (Look != null)
		{
			return Look.GetTooltipStringSize(graphics, valueRect, point, line);
		}
		return Win32Calls.GetTextExtent(graphics, DisplayString, Font);
	}

	public virtual PropertyTypeDescriptorContext GetTypeDescriptorContext(PropertyEnumerator propEnum)
	{
		return null;
	}

	protected internal virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (PropertyChangedEvent != null)
		{
			PropertyChangedEvent(this, e);
		}
	}

	public virtual void ResetToDefaultValue()
	{
	}

	public int FindString(string str, string[] strCollection, int startIndex)
	{
		if (str == null || strCollection == null || strCollection.Length == 0)
		{
			return -1;
		}
		if (startIndex < -1 || startIndex >= strCollection.Length)
		{
			return -1;
		}
		int length = str.Length;
		int num = 0;
		int num2 = startIndex % strCollection.Length;
		while (num < strCollection.Length)
		{
			num++;
			if (string.Compare(str, 0, strCollection[num2], 0, length, ignoreCase: true, CultureInfo) == 0)
			{
				return num2;
			}
			num2 = (num2 + 1) % strCollection.Length;
		}
		return -1;
	}

	public string IncrementalFind(char key, ref int currentIndex)
	{
		return IncrementalFind(key, GetDisplayedValues(), ref currentIndex);
	}

	public string IncrementalFind(char key, string[] displayedValues, ref int currentIndex)
	{
		if (currentIndex == -1)
		{
			currentIndex = 0;
		}
		if (_timer != null)
		{
			_timer.Stop();
			_timer.Start();
		}
		else if (Grid.AutoCompletionTimerDuration > 0)
		{
			_timer = new Timer();
			_timer.Tick += _timer_Tick;
			_timer.Interval = Grid.AutoCompletionTimerDuration;
			_timer.Start();
		}
		_autoCompleteBuffer += new string(key, 1);
		int num = FindString(_autoCompleteBuffer, displayedValues, currentIndex);
		if (num == -1)
		{
			_autoCompleteBuffer = new string(key, 1);
			num = FindString(_autoCompleteBuffer, displayedValues, currentIndex);
		}
		if (num != -1)
		{
			currentIndex = num;
			return displayedValues[currentIndex];
		}
		currentIndex = -1;
		return null;
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		_timer.Stop();
		_timer.Tick -= _timer_Tick;
		_timer = null;
		_autoCompleteBuffer = "";
	}
}
