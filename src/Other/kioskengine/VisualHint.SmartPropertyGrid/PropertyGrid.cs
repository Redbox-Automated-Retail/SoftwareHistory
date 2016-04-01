using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security.Permissions;
using System.Windows.Forms;
using Microsoft.Win32;
using Skybound.VisualTips.External;
using Skybound.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[Designer(typeof(PropertyGridDesigner))]
public class PropertyGrid : ContainerControl, ISupportInitialize
{
	public delegate void ValueValidationEventHandler(object sender, ValueValidationEventArgs e);

	public delegate void SelectedObjectChangedEventHandler(object sender, SelectedObjectChangedEventArgs e);

	public delegate void SelectedObjectChangingEventHandler(object sender, EventArgs e);

	public delegate void PropertySelectedEventHandler(object sender, PropertySelectedEventArgs e);

	public delegate void PropertyExpandedEventHandler(object sender, PropertyExpandedEventArgs e);

	public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);

	public delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);

	public delegate void PropertyCreatedEventHandler(object sender, PropertyCreatedEventArgs e);

	public delegate void PropertyRestoredEventHandler(object sender, PropertyRestoredEventArgs e);

	public delegate void PropertyValueAttributesNeededEventHandler(object sender, PropertyValueAttributesNeededEventArgs e);

	public delegate void PropertyUpDownEventHandler(object sender, PropertyUpDownEventArgs e);

	public delegate void InPlaceCtrlVisibleEventHandler(object sender, InPlaceCtrlVisibleEventArgs e);

	public delegate void InPlaceCtrlCreatedEventHandler(object sender, InPlaceCtrlCreatedEventArgs e);

	public delegate void InPlaceCtrlHiddenEventHandler(object sender, InPlaceCtrlVisibleEventArgs e);

	public delegate void PropertyButtonClickedEventHandler(object sender, PropertyButtonClickedEventArgs e);

	public delegate void DisplayedValuesNeededEventHandler(object sender, DisplayedValuesNeededEventArgs e);

	public delegate void DisplayModeChangedEventHandler(object sender, EventArgs e);

	public delegate void HyperLinkPropertyClickedEventHandler(object sender, PropertyHyperLinkClickedEventArgs e);

	public delegate void PropertyEnabledEventHandler(object sender, PropertyEnabledEventArgs e);

	public delegate void DrawingManagerChangedEventHandler(object sender, EventArgs e);

	public delegate void PropertyPostFilterOutEventHandler(object sender, PropertyPostFilterOutEventArgs e);

	public delegate void PropertyPreFilterOutEventHandler(object sender, PropertyPreFilterOutEventArgs e);

	public delegate void BeginCommitChangesEventHandler(object sender, CommitChangesEventArgs e);

	public delegate void EndCommitChangesEventHandler(object sender, CommitChangesEventArgs e);

	[Flags]
	public enum PropertyHashCodeMode
	{
		Depth = 1,
		Name = 2,
		Comment = 4,
		Type = 8,
		Parent = 0x10,
		All = Depth | Name | Comment | Type | Parent
	}

	[Flags]
	public enum ResetDisplayedValuesTriggerMode
	{
		None = 0,
		TriggerOnceNow = 1,
		TriggerAtDropDown = 2
	}

	public enum NavigationKeyModes
	{
		ArrowKeys,
		TabKey
	}

	[Flags]
	public enum TabKeyNavigationModes
	{
		NoSubMode = 0,
		TabKeyWithAutoFocus = 1,
		TabKeyLeavesAtExtremes = 2,
		TabKeyInSubControls = 4,
		TabKeyInChildProperties = 8,
		TabKeySkipReadonlyProperties = 0x10
	}

	[Obsolete("Multi selection is not supported by the VisualHint team. Use at your own risks.")]
	public enum MultiSelectModes
	{
		None,
		SameLevel,
		Global
	}

	[Flags]
	public enum ValueNotValidBehaviorModes
	{
		IgnoreAndRestore = 0,
		KeepFocus = 1
	}

	public enum DisplayModes
	{
		Categorized,
		Flat,
		FlatSorted
	}

	public enum ImageSources
	{
		None,
		Value,
		Grid,
		Image
	}

	public enum HitTests
	{
		Nowhere,
		CommentSplit,
		CommentArea,
		ColumnSplit,
		TreeGlyph,
		ManualDisable,
		Value,
		Label,
		LeftColumn,
		Client
	}

	public enum DrawManagers
	{
		DefaultDrawManager,
		DotnetDrawManager,
		LightColorDrawManager,
		[Browsable(false)]
		CustomDrawManager
	}

	public enum PropertyStateFlags
	{
		None = 0,
		Colors = 1,
		Icons = 2,
		Fonts = 4,
		All = 7
	}

	[Flags]
	public enum ToolTipModes
	{
		None = 0,
		ToolTipsOnLabels = 1,
		ToolTipsOnValues = 2,
		ToolTipsOnValuesAndLabels = ToolTipsOnLabels | ToolTipsOnValues
	}

	[Flags]
	public enum EllipsisModes
	{
		None = 0,
		EllipsisOnLabels = 1,
		EllipsisOnValues = 2,
		EllipsisOnValuesAndLabels = EllipsisOnLabels | EllipsisOnValues
	}

	public enum ReadOnlyVisuals
	{
		ReadOnlyFeel = 1,
		Disabled
	}

	public enum DisableModes
	{
		None,
		Simple,
		NoValueSelection,
		NoPropertySelection,
		Full
	}

	private class SelectedObjectConverter : ReferenceConverter
	{
		public SelectedObjectConverter()
			: base(typeof(IComponent))
		{
		}
	}

	private class VisualTipExtender : IVisualTipExtender
	{
		public object GetChildAtPoint(Control control, int x, int y)
		{
			Rectangle itemRect;
			return (control as InternalGrid).PropertyItemFromPoint(new Point(x, y), out itemRect);
		}

		public object GetParent(object component)
		{
			if (component is PropertyEnumerator)
			{
				return (component as PropertyEnumerator).Property.ParentGrid;
			}
			return null;
		}

		public Type[] GetChildTypes()
		{
			return new Type[1] { typeof(PropertyEnumerator) };
		}
	}

	public const string FeelNone = "none";

	public const string FeelEdit = "edit";

	public const string FeelMaskedEdit = "maskededit";

	public const string FeelUpDown = "updown";

	public const string FeelEditUpDown = "editupdown";

	public const string FeelEditPassword = "editpwd";

	public const string FeelList = "list";

	public const string FeelEditList = "editlist";

	public const string FeelButton = "button";

	public const string FeelFontButton = "fontbutton";

	public const string FeelEditButton = "editbutton";

	public const string FeelDateTime = "datetime";

	public const string FeelCheckbox = "checkbox";

	public const string FeelRadioButton = "radiobutton";

	public const string FeelTrackbar = "trackbar";

	public const string FeelTrackbarEdit = "trackbaredit";

	public const string FeelMultilineEdit = "multilineedit";

	public const string FeelEditUnit = "editunit";

	internal static string MiscCategoryName;

	internal Hashtable _registeredFeels = new Hashtable();

	internal Hashtable _registeredFeelAttachments = new Hashtable();

	internal Hashtable _registeredLookAttachments = new Hashtable();

	internal Hashtable _registeredDropDownContent = new Hashtable();

	private object[] _selectedObjects;

	private bool _clickThroughInProgress;

	private int _beginUpdateCounter;

	private PropertyFeel _defaultFeel;

	internal int _globalTextMargin;

	private int MinCommentsAreaHeight;

	private bool _resizingCommentsInProgress;

	private Point _resizingCommentsPoint;

	private ToolStrip _toolbar;

	private bool _showComments;

	private int _commentsHeight;

	private DisplayModes _reportedDisplayMode;

	private InternalGrid _grid;

	private ValueNotValidBehaviorModes _valueNotValidBehaviorMode = ValueNotValidBehaviorModes.KeepFocus;

	private Container components;

	private BorderStyle _borderStyle;

	private EllipsisModes _ellipsisMode = EllipsisModes.EllipsisOnValuesAndLabels;

	private int _propertyId;

	private int _categoryId;

	private Hashtable _categoryMap = new Hashtable();

	private IComparer _propertyComparer;

	private Color _commentsBackColor;

	private Color _commentsForeColor;

	private bool _commentsLock;

	private bool _insideValueRecreate;

	private NavigationKeyModes _navigationKeyMode;

	private TabKeyNavigationModes _tabKeyNavigationMode;

	private bool _lazyLoading;

	private bool _showDefaultValues;

	private bool _showAdditionalIndentation;

	private static FieldInfo _pdcComparerField;

	private static IComparer _noSortComparer;

	private bool _compareUnderCategories;

	private static int _autoCompletionTimerDuration;

	private static bool _handleIdleDuringDropDown;

	private byte[] byCategoriesBmp = new byte[822]
	{
		66, 77, 54, 3, 0, 0, 0, 0, 0, 0,
		54, 0, 0, 0, 40, 0, 0, 0, 16, 0,
		0, 0, 16, 0, 0, 0, 1, 0, 24, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 18, 11,
		0, 0, 18, 11, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 255, 0, 255, 181, 152, 120,
		181, 152, 120, 181, 152, 120, 181, 152, 120, 181,
		152, 120, 255, 0, 255, 255, 0, 255, 199, 177,
		165, 191, 166, 153, 180, 153, 139, 169, 142, 126,
		255, 0, 255, 191, 166, 153, 180, 153, 139, 169,
		142, 126, 181, 152, 120, 191, 204, 210, 174, 190,
		198, 168, 184, 194, 167, 184, 193, 167, 184, 193,
		181, 152, 120, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		181, 152, 120, 217, 225, 228, 207, 216, 220, 0,
		0, 0, 201, 211, 216, 198, 209, 214, 181, 152,
		120, 255, 0, 255, 118, 118, 118, 100, 99, 99,
		81, 82, 81, 64, 65, 64, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 181, 152,
		120, 238, 242, 242, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 227, 233, 234, 181, 152, 120, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 181, 152, 120, 241,
		245, 245, 241, 245, 245, 0, 0, 0, 241, 245,
		245, 241, 245, 245, 181, 152, 120, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 181, 152, 120, 245, 247, 247,
		245, 247, 247, 244, 247, 247, 244, 247, 247, 244,
		246, 246, 181, 152, 120, 255, 0, 255, 198, 177,
		165, 190, 166, 152, 180, 153, 138, 170, 142, 127,
		255, 0, 255, 190, 166, 152, 180, 153, 138, 170,
		142, 127, 255, 0, 255, 181, 152, 120, 181, 152,
		120, 181, 152, 120, 181, 152, 120, 181, 152, 120,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 199, 178, 165, 191, 166, 153,
		179, 153, 139, 169, 142, 126, 255, 0, 255, 191,
		166, 153, 179, 153, 139, 169, 142, 126, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 181,
		152, 120, 181, 152, 120, 181, 152, 120, 181, 152,
		120, 181, 152, 120, 255, 0, 255, 255, 0, 255,
		199, 177, 166, 190, 166, 153, 179, 153, 138, 170,
		142, 126, 255, 0, 255, 190, 166, 153, 179, 153,
		138, 170, 142, 126, 181, 152, 120, 191, 204, 210,
		174, 190, 198, 168, 184, 194, 167, 184, 193, 167,
		184, 193, 181, 152, 120, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 181, 152, 120, 217, 225, 228, 207, 216,
		220, 0, 0, 0, 201, 211, 216, 198, 209, 214,
		181, 152, 120, 255, 0, 255, 199, 177, 165, 191,
		166, 153, 180, 153, 139, 169, 142, 126, 255, 0,
		255, 191, 166, 153, 180, 153, 139, 169, 142, 126,
		181, 152, 120, 238, 242, 242, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 227, 233, 234, 181, 152,
		120, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 181, 152,
		120, 241, 245, 245, 241, 245, 245, 0, 0, 0,
		241, 245, 245, 241, 245, 245, 181, 152, 120, 255,
		0, 255, 118, 118, 118, 100, 99, 99, 81, 82,
		81, 64, 65, 64, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 181, 152, 120, 245,
		247, 247, 245, 247, 247, 244, 247, 247, 244, 247,
		247, 244, 246, 246, 181, 152, 120, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 181, 152, 120,
		181, 152, 120, 181, 152, 120, 181, 152, 120, 181,
		152, 120, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255
	};

	private byte[] bySortedNamesBmp = new byte[822]
	{
		66, 77, 54, 3, 0, 0, 0, 0, 0, 0,
		54, 0, 0, 0, 40, 0, 0, 0, 16, 0,
		0, 0, 16, 0, 0, 0, 1, 0, 24, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 18, 11,
		0, 0, 18, 11, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 255, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 50, 60, 141, 43, 52,
		124, 123, 128, 176, 112, 119, 173, 54, 62, 132,
		120, 126, 174, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 255, 255, 132, 132,
		132, 255, 255, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 173, 162, 198, 62, 63, 143, 170,
		174, 208, 255, 0, 255, 255, 0, 255, 116, 123,
		179, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 226, 226, 226, 0, 0, 0, 218,
		218, 218, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 131, 111, 162, 60, 70, 149,
		216, 211, 229, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 255,
		255, 22, 22, 22, 0, 0, 0, 10, 10, 10,
		255, 255, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 216, 211, 229, 77, 71, 146, 110, 116,
		169, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 224, 224, 224, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 224, 224,
		224, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 159, 151, 194, 37, 47, 126, 182,
		185, 212, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 255, 255, 126, 126, 126, 180, 180, 180,
		0, 0, 0, 186, 186, 186, 117, 117, 117, 255,
		255, 255, 255, 0, 255, 131, 125, 178, 255, 0,
		255, 255, 0, 255, 92, 89, 159, 65, 73, 142,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		255, 255, 255, 0, 255, 255, 255, 255, 0, 0,
		0, 255, 255, 255, 255, 0, 255, 255, 255, 255,
		255, 0, 255, 157, 142, 183, 146, 132, 178, 168,
		154, 191, 151, 135, 178, 109, 104, 165, 171, 175,
		210, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 255, 255, 0, 0, 0, 255,
		255, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 255, 255, 0, 0, 0, 255, 255, 255,
		255, 0, 255, 255, 0, 255, 198, 139, 110, 148,
		90, 60, 213, 189, 177, 255, 0, 255, 144, 90,
		64, 136, 79, 51, 157, 111, 88, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		255, 255, 0, 0, 0, 255, 255, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 187, 114, 79,
		255, 0, 255, 255, 0, 255, 224, 205, 195, 136,
		79, 51, 238, 228, 222, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 255, 255,
		0, 0, 0, 255, 255, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 198, 137, 107, 186, 123,
		93, 191, 156, 137, 144, 89, 61, 173, 127, 104,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 255, 255, 0, 0,
		0, 255, 255, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 239, 218, 207, 185, 112, 75, 255,
		0, 255, 137, 80, 51, 223, 203, 193, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 255, 255, 0, 0, 0, 255,
		255, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 194, 128, 94, 184, 120, 89,
		150, 98, 69, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 255, 255, 0, 0, 0, 255, 255, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		0, 255, 214, 161, 134, 183, 107, 69, 207, 178,
		165, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 255, 0, 255, 255, 0, 255, 255,
		255, 255, 0, 0, 0, 255, 255, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 0, 255,
		255, 0, 255, 194, 118, 79, 255, 0, 255, 255,
		0, 255, 255, 0, 255, 255, 0, 255, 255, 0,
		255, 255, 0, 255, 255, 0, 255, 255, 255, 255,
		0, 0, 0, 255, 255, 255, 255, 0, 255, 255,
		0, 255
	};

	private bool _insideSelectedObjects;

	private bool _initializing;

	private bool _insideRefreshProperties;

	private bool _preventPropertySelectedEvent;

	private static bool _insideNotifyValueValidation;

	[Description("The maximum number of levels that will be expanded under a single property when calling ExpandAllProperties.")]
	[Category("Behavior")]
	[DefaultValue(10)]
	public int ExpandAllMaxLevels
	{
		get
		{
			return _grid.ExpandAllMaxLevels;
		}
		set
		{
			_grid.ExpandAllMaxLevels = value;
		}
	}

	public static bool HandleIdleDuringDropDown
	{
		get
		{
			return _handleIdleDuringDropDown;
		}
		set
		{
			_handleIdleDuringDropDown = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(1000)]
	public int AutoCompletionTimerDuration
	{
		get
		{
			return _autoCompletionTimerDuration;
		}
		set
		{
			_autoCompletionTimerDuration = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ShowAdditionalIndentation
	{
		get
		{
			return _showAdditionalIndentation;
		}
		set
		{
			_showAdditionalIndentation = value;
			Refresh();
		}
	}

	[Category("Appearance")]
	public override RightToLeft RightToLeft
	{
		get
		{
			return base.RightToLeft;
		}
		set
		{
			if (value != RightToLeft)
			{
				base.RightToLeft = value;
				Refresh();
				Grid.CheckScrollbar();
			}
		}
	}

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			switch (_borderStyle)
			{
			case BorderStyle.FixedSingle:
				createParams.Style |= 8388608;
				break;
			case BorderStyle.Fixed3D:
				createParams.ExStyle |= 131072;
				break;
			}
			return createParams;
		}
	}

	internal bool InsideValueRecreate
	{
		set
		{
			_insideValueRecreate = value;
		}
	}

	public override bool AllowDrop
	{
		get
		{
			return base.AllowDrop;
		}
		set
		{
			_grid.AllowDrop = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IComparer PropertyComparer
	{
		get
		{
			return _propertyComparer;
		}
		set
		{
			_propertyComparer = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Image BackgroundImage
	{
		get
		{
			return base.BackgroundImage;
		}
		set
		{
			base.BackgroundImage = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	public override ImageLayout BackgroundImageLayout
	{
		get
		{
			return base.BackgroundImageLayout;
		}
		set
		{
			base.BackgroundImageLayout = value;
		}
	}

	[DefaultValue(16)]
	[Category("Appearance")]
	[Browsable(false)]
	public int LeftColumnWidth
	{
		get
		{
			return _grid.LeftColumnWidth;
		}
		set
		{
			_grid.LeftColumnWidth = value;
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
			if (_toolbar != null)
			{
				_toolbar.BackColor = value;
				_toolbar.Invalidate(invalidateChildren: true);
			}
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "ActiveBorder")]
	public Color GridColor
	{
		get
		{
			return _grid.GridColor;
		}
		set
		{
			_grid.GridColor = value;
			Invalidate();
		}
	}

	[DefaultValue(typeof(Color), "Window")]
	[Category("Appearance")]
	public Color GridBackColor
	{
		get
		{
			return _grid.BackColor;
		}
		set
		{
			_grid.BackColor = value;
		}
	}

	[DefaultValue(typeof(Color), "")]
	[Category("Appearance")]
	public Color PropertyLabelBackColor
	{
		get
		{
			return _grid.PropertyLabelBackColor;
		}
		set
		{
			_grid.PropertyLabelBackColor = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "ControlText")]
	public Color GridForeColor
	{
		get
		{
			return _grid.ForeColor;
		}
		set
		{
			_grid.ForeColor = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "")]
	public Color PropertyValueBackColor
	{
		get
		{
			return _grid.PropertyValueBackColor;
		}
		set
		{
			_grid.PropertyValueBackColor = value;
		}
	}

	[DefaultValue(typeof(Color), "GrayText")]
	[Category("Appearance")]
	public Color DisabledForeColor
	{
		get
		{
			return _grid.DisabledForeColor;
		}
		set
		{
			_grid.DisabledForeColor = value;
		}
	}

	[DefaultValue(typeof(Color), "GrayText")]
	[Category("Appearance")]
	public Color ReadOnlyForeColor
	{
		get
		{
			return _grid.ReadOnlyForeColor;
		}
		set
		{
			_grid.ReadOnlyForeColor = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "Control")]
	public Color CommentsBackColor
	{
		get
		{
			return _commentsBackColor;
		}
		set
		{
			_commentsBackColor = value;
			Invalidate();
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "ControlText")]
	public Color CommentsForeColor
	{
		get
		{
			return _commentsForeColor;
		}
		set
		{
			_commentsForeColor = value;
			Invalidate();
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "Highlight")]
	public Color SelectedBackColor
	{
		get
		{
			return _grid.SelectedBackColor;
		}
		set
		{
			_grid.SelectedBackColor = value;
		}
	}

	[DefaultValue(typeof(Color), "ActiveBorder")]
	[Category("Appearance")]
	public Color SelectedNotFocusedBackColor
	{
		get
		{
			return _grid.SelectedNotFocusedBackColor;
		}
		set
		{
			_grid.SelectedNotFocusedBackColor = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(typeof(Color), "HighlightText")]
	public Color HighlightedTextColor
	{
		get
		{
			return _grid.HighlightedTextColor;
		}
		set
		{
			_grid.HighlightedTextColor = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(EllipsisModes.EllipsisOnValuesAndLabels)]
	public EllipsisModes EllipsisMode
	{
		get
		{
			return _ellipsisMode;
		}
		set
		{
			_ellipsisMode = value;
			Refresh();
		}
	}

	[DefaultValue(ToolTipModes.None)]
	[Category("Appearance")]
	public ToolTipModes ToolTipMode
	{
		get
		{
			return _grid.ToolTipMode;
		}
		set
		{
			_grid.ToolTipMode = value;
		}
	}

	[DefaultValue(ReadOnlyVisuals.Disabled)]
	[Category("Appearance")]
	public ReadOnlyVisuals ReadOnlyVisual
	{
		get
		{
			return _grid.ReadOnlyVisual;
		}
		set
		{
			_grid.ReadOnlyVisual = value;
		}
	}

	[Category("Appearance")]
	[Description("Border style that can be 3D (2 pixels), flat (1 pixel) or none.")]
	public BorderStyle BorderStyle
	{
		get
		{
			return _borderStyle;
		}
		set
		{
			BeginUpdate();
			_borderStyle = value;
			RecreateHandle();
			Grid.CheckScrollbar();
			EndUpdate();
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
			_grid.Font = value;
		}
	}

	[Category("Appearance")]
	[DefaultValue(false)]
	public bool ShowDefaultValues
	{
		get
		{
			return _showDefaultValues;
		}
		set
		{
			if (value != _showDefaultValues)
			{
				_showDefaultValues = value;
				HandleDefaultValues();
			}
		}
	}

	[DefaultValue(0.5)]
	[Category("Appearance")]
	public double LabelColumnWidthRatio
	{
		get
		{
			return _grid.LabelColumnWidthRatio;
		}
		set
		{
			_grid.LabelColumnWidthRatio = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int LabelColumnWidth => _grid.LabelColumnWidth;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PropertyFeel DefaultFeel
	{
		get
		{
			return _defaultFeel;
		}
		set
		{
			_defaultFeel = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool ClickThroughInProgress
	{
		get
		{
			return _clickThroughInProgress;
		}
		set
		{
			_clickThroughInProgress = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int BasicPropertyHeight => _grid.BasicPropertyHeight;

	[DefaultValue(DisplayModes.Categorized)]
	[Category("Appearance")]
	public DisplayModes DisplayMode
	{
		get
		{
			return _reportedDisplayMode;
		}
		set
		{
			_reportedDisplayMode = value;
			_grid.DisplayMode = value;
			if (_toolbar != null)
			{
				ToolStripButton toolStripButton = (ToolStripButton)_toolbar.Items["spg_sortByCategory"];
				if (toolStripButton != null)
				{
					toolStripButton.Checked = value == DisplayModes.Categorized;
				}
				toolStripButton = (ToolStripButton)_toolbar.Items["spg_sortAlpha"];
				if (toolStripButton != null)
				{
					toolStripButton.Checked = value == DisplayModes.FlatSorted;
				}
			}
		}
	}

	[Category("Behavior")]
	[DefaultValue(MultiSelectModes.None)]
	[Obsolete("Multi selection is not supported by the VisualHint team. Use at your own risks.")]
	public MultiSelectModes MultiSelectMode
	{
		get
		{
			return _grid.MultiSelectMode;
		}
		set
		{
			_grid.MultiSelectMode = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(ValueNotValidBehaviorModes.KeepFocus)]
	public ValueNotValidBehaviorModes ValueNotValidBehaviorMode
	{
		get
		{
			return _valueNotValidBehaviorMode;
		}
		set
		{
			_valueNotValidBehaviorMode = value;
		}
	}

	[DefaultValue(4)]
	[Category("Appearance")]
	public int PropertyVerticalMargin
	{
		get
		{
			return _grid.PropertyVerticalMargin;
		}
		set
		{
			_grid.PropertyVerticalMargin = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int GlobalTextMargin => _globalTextMargin;

	[DefaultValue(false)]
	[Category("Appearance")]
	public bool ToolbarVisibility
	{
		get
		{
			return _toolbar != null;
		}
		set
		{
			BeginUpdate();
			if (value && _toolbar == null)
			{
				_toolbar = new ToolStrip();
				_toolbar.ShowItemToolTips = true;
				_toolbar.TabStop = true;
				_toolbar.Text = "PropertyGridToolBar";
				_toolbar.AutoSize = false;
				_toolbar.TabIndex = 1;
				_toolbar.CanOverflow = false;
				_toolbar.GripStyle = ToolStripGripStyle.Hidden;
				_toolbar.Cursor = Cursors.Arrow;
				Padding padding = _toolbar.Padding;
				padding.Left = 2;
				_toolbar.Padding = padding;
				_toolbar.Renderer = new ToolStripSystemRenderer();
				_toolbar.BackColor = BackColor;
				base.Controls.Add(_toolbar);
				ToolStripButton toolStripButton = new ToolStripButton();
				MemoryStream stream = new MemoryStream(byCategoriesBmp);
				toolStripButton.Image = Image.FromStream(stream);
				toolStripButton.Name = "spg_sortByCategory";
				ResourceManager resourceManager = new ResourceManager("System.Windows.Forms", Assembly.GetAssembly(typeof(Form)));
				toolStripButton.ImageTransparentColor = Color.Magenta;
				toolStripButton.ToolTipText = resourceManager.GetString("PBRSToolTipCategorized");
				toolStripButton.Click += OnSortByCategoy;
				toolStripButton.Checked = DisplayMode == DisplayModes.Categorized;
				_toolbar.Items.Add(toolStripButton);
				toolStripButton = new ToolStripButton();
				stream = new MemoryStream(bySortedNamesBmp);
				toolStripButton.Image = Image.FromStream(stream);
				toolStripButton.Name = "spg_sortAlpha";
				toolStripButton.ImageTransparentColor = Color.Magenta;
				toolStripButton.ToolTipText = resourceManager.GetString("PBRSToolTipAlphabetic");
				toolStripButton.Click += OnSortAlphabetical;
				toolStripButton.Checked = DisplayMode == DisplayModes.FlatSorted;
				_toolbar.Items.Add(toolStripButton);
				ToolStripSeparator value2 = new ToolStripSeparator();
				_toolbar.Items.Add(value2);
				RepositionInternalGrid();
				_toolbar.Resize += OnToolbarResized;
			}
			else if (!value && _toolbar != null)
			{
				base.Controls.Remove(_toolbar);
				_toolbar.Resize -= OnToolbarResized;
				_toolbar.Dispose();
				_toolbar = null;
				RepositionInternalGrid();
			}
			EndUpdate();
			Refresh();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ToolStrip Toolbar => _toolbar;

	[Category("Appearance")]
	[DefaultValue(false)]
	public bool CommentsVisibility
	{
		get
		{
			return _showComments;
		}
		set
		{
			_showComments = value;
			Invalidate();
			RepositionInternalGrid();
		}
	}

	[Category("Appearance")]
	[DefaultValue(40)]
	public int CommentsHeight
	{
		get
		{
			return _commentsHeight;
		}
		set
		{
			if (_initializing)
			{
				_commentsHeight = value;
				return;
			}
			if (value < MinCommentsAreaHeight)
			{
				value = MinCommentsAreaHeight;
			}
			if (base.ClientRectangle.Height == 0 || value <= base.ClientRectangle.Height - 2 * BasicPropertyHeight - ToolbarHeight)
			{
				_commentsHeight = value;
				Invalidate();
				RepositionInternalGrid();
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Rectangle CommentsRect
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			clientRectangle.Y = clientRectangle.Y + clientRectangle.Height - CommentsHeight;
			clientRectangle.Height = CommentsHeight;
			return clientRectangle;
		}
	}

	[Category("Behavior")]
	[DefaultValue(false)]
	public bool CommentsLock
	{
		get
		{
			return _commentsLock;
		}
		set
		{
			_commentsLock = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	private int CurrentCommentsHeight
	{
		get
		{
			if (!CommentsVisibility)
			{
				return 0;
			}
			return _commentsHeight;
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	public bool ColumnsLock
	{
		get
		{
			return _grid.ColumnsLock;
		}
		set
		{
			_grid.ColumnsLock = value;
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	public bool HideRemainingCategory
	{
		get
		{
			return _grid.HideRemainingCategory;
		}
		set
		{
			_grid.HideRemainingCategory = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool GridContainsFocus => Grid.ContainsFocus;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public CustomDrawManager DrawManager
	{
		get
		{
			return _grid.DrawManager;
		}
		set
		{
			BeginUpdate();
			_grid.DrawManager = value;
			OnDrawingManagerChanged(EventArgs.Empty);
			EndUpdate();
		}
	}

	[DefaultValue(DrawManagers.DotnetDrawManager)]
	[Category("Appearance")]
	public DrawManagers DrawingManager
	{
		get
		{
			if (DrawManager.GetType() == typeof(DotnetDrawManager))
			{
				return DrawManagers.DotnetDrawManager;
			}
			if (DrawManager.GetType() == typeof(LightColorDrawManager))
			{
				return DrawManagers.LightColorDrawManager;
			}
			if (DrawManager.GetType() == typeof(DefaultDrawManager))
			{
				return DrawManagers.DefaultDrawManager;
			}
			return DrawManagers.CustomDrawManager;
		}
		set
		{
			switch (value)
			{
			case DrawManagers.CustomDrawManager:
				break;
			case DrawManagers.DotnetDrawManager:
				DrawManager = new DotnetDrawManager();
				break;
			case DrawManagers.LightColorDrawManager:
				DrawManager = new LightColorDrawManager();
				break;
			default:
				DrawManager = new DefaultDrawManager();
				break;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Control InternalGrid => _grid;

	internal InternalGrid Grid => _grid;

	[Browsable(false)]
	public Control InPlaceControl => _grid.InPlaceControl;

	[Category("Behavior")]
	[DefaultValue(NavigationKeyModes.ArrowKeys)]
	public NavigationKeyModes NavigationKeyMode
	{
		get
		{
			return _navigationKeyMode;
		}
		set
		{
			_navigationKeyMode = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(TabKeyNavigationModes.NoSubMode)]
	public TabKeyNavigationModes TabKeyNavigationMode
	{
		get
		{
			return _tabKeyNavigationMode;
		}
		set
		{
			_tabKeyNavigationMode = value;
		}
	}

	private int ToolbarHeight
	{
		get
		{
			if (_toolbar == null)
			{
				return 0;
			}
			return _toolbar.Height;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PropertyVisibleDeepEnumerator SelectedPropertyEnumerator => _grid.SelectedPropertyEnumerator;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ImageList ImageList
	{
		get
		{
			return _grid.ImageList;
		}
		set
		{
			_grid.ImageList = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public PropertyEnumerator RightBound => _grid.RightBound;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PropertyEnumerator LeftBound => _grid.LeftBound;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public PropertyEnumerator FirstProperty => _grid.FirstProperty;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public PropertyEnumerator LastProperty => _grid.LastProperty;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public PropertyEnumerator FirstDisplayedProperty
	{
		get
		{
			return _grid.FirstDisplayedProperty;
		}
		set
		{
			_grid.FirstDisplayedProperty = value.GetVisibleDeepEnumerator();
			_grid.CloseEmptySpace();
			_grid.CheckScrollbar();
			_grid.Refresh();
		}
	}

	[TypeConverter(typeof(SelectedObjectConverter))]
	[DefaultValue(null)]
	[Category("Behavior")]
	public virtual object SelectedObject
	{
		get
		{
			if (_selectedObjects != null && _selectedObjects.Length != 0)
			{
				return _selectedObjects[0];
			}
			return null;
		}
		set
		{
			if (value == null)
			{
				SelectedObjects = new object[0];
				return;
			}
			SelectedObjects = new object[1] { value };
		}
	}

	internal bool IsInsideSelectedObjects => _insideSelectedObjects;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual object[] SelectedObjects
	{
		get
		{
			if (_selectedObjects == null)
			{
				return new object[0];
			}
			return (object[])_selectedObjects.Clone();
		}
		set
		{
			if (!_insideRefreshProperties)
			{
				OnSelectedObjectChanging(EventArgs.Empty);
			}
			_insideSelectedObjects = true;
			BeginUpdate();
			object[] selectedObjects = _selectedObjects;
			Clear();
			_categoryMap.Clear();
			_categoryId = 1;
			_propertyId = 1;
			if (value == null)
			{
				_selectedObjects = new object[0];
			}
			else
			{
				_selectedObjects = (object[])value.Clone();
			}
			DisplayModes displayMode = DisplayMode;
			if (displayMode != DisplayModes.Categorized)
			{
				_grid.DisplayMode = DisplayModes.Categorized;
			}
			if (_selectedObjects != null && _selectedObjects.Length != 0)
			{
				ArrayList arrayList;
				if (_selectedObjects.Length == 1)
				{
					PropertyDescriptorCollection propertyDescriptors = GetPropertyDescriptors(null, _selectedObjects[0], null, null);
					arrayList = new ArrayList();
					foreach (PropertyDescriptor item in propertyDescriptors)
					{
						ArrayList arrayList2 = new ArrayList(1);
						arrayList2.Add(item);
						arrayList.Add(arrayList2);
					}
				}
				else
				{
					SortedList sortedList = new SortedList(PropertyComparer);
					bool flag = true;
					object[] selectedObjects2 = _selectedObjects;
					foreach (object targetInstance in selectedObjects2)
					{
						PropertyDescriptorCollection propertyDescriptors2 = GetPropertyDescriptors(null, targetInstance, null, null);
						if (PropertyComparer.GetType() == typeof(PropertyComparerNoSort))
						{
							(PropertyComparer as PropertyComparerNoSort).OriginalCollection = propertyDescriptors2;
						}
						foreach (PropertyDescriptor item2 in propertyDescriptors2)
						{
							if (item2.Attributes[typeof(MergablePropertyAttribute)] is MergablePropertyAttribute { AllowMerge: false })
							{
								continue;
							}
							if (flag)
							{
								ArrayList arrayList3 = new ArrayList();
								arrayList3.Add(item2);
								sortedList.Add(item2, arrayList3);
								continue;
							}
							int num = sortedList.IndexOfKey(item2);
							if (num >= 0)
							{
								ArrayList arrayList4 = (ArrayList)sortedList.GetByIndex(num);
								if (MatchPropertyDescriptors((PropertyDescriptor)arrayList4[0], item2))
								{
									arrayList4.Add(item2);
								}
								else
								{
									sortedList.RemoveAt(num);
								}
							}
						}
						flag = false;
					}
					arrayList = new ArrayList();
					foreach (ArrayList value2 in sortedList.Values)
					{
						if (value2.Count == _selectedObjects.Length)
						{
							arrayList.Add(value2);
						}
					}
				}
				Hashtable hashtable = new Hashtable();
				foreach (ArrayList item3 in arrayList)
				{
					PropertyDescriptor propertyDescriptor2 = (PropertyDescriptor)item3[0];
					if (propertyDescriptor2.Attributes[typeof(ValueAddedToPropertyAttribute)] is ValueAddedToPropertyAttribute valueAddedToPropertyAttribute)
					{
						for (int j = 0; j < item3.Count; j++)
						{
							propertyDescriptor2 = (PropertyDescriptor)item3[j];
							ArrayList arrayList7 = (ArrayList)hashtable[valueAddedToPropertyAttribute.PropertyName];
							if (arrayList7 == null)
							{
								arrayList7 = new ArrayList();
								hashtable.Add(valueAddedToPropertyAttribute.PropertyName, arrayList7);
							}
							arrayList7.Add(new KeyValuePair(valueAddedToPropertyAttribute.ValueKey, propertyDescriptor2));
						}
						continue;
					}
					PropertyDescriptor propertyDescriptor3 = (PropertyDescriptor)item3[0];
					string text = propertyDescriptor3.Category;
					PropertyEnumerator propertyEnumerator;
					if (propertyDescriptor3.Attributes[typeof(SortedCategoryAttribute)] is SortedCategoryAttribute sortedCategoryAttribute)
					{
						propertyEnumerator = (PropertyEnumerator)_categoryMap[text];
						if (propertyEnumerator == null)
						{
							PropertyEnumerator siblingEnumerator = FirstProperty.GetSiblingEnumerator();
							while (siblingEnumerator != siblingEnumerator.RightBound)
							{
								int sortedIndex = siblingEnumerator.Property.SortedIndex;
								if (sortedIndex == int.MaxValue || sortedIndex > sortedCategoryAttribute.SortedIndex)
								{
									break;
								}
								siblingEnumerator.MoveNext();
							}
							propertyEnumerator = InsertRootCategory(siblingEnumerator, -(_categoryId++), text);
							propertyEnumerator.Property.SortedIndexInternal = sortedCategoryAttribute.SortedIndex;
							_categoryMap[text] = propertyEnumerator;
						}
					}
					else
					{
						if (text == null)
						{
							text = MiscCategoryName;
						}
						propertyEnumerator = (PropertyEnumerator)_categoryMap[text];
						if (propertyEnumerator == null)
						{
							propertyEnumerator = AppendRootCategory(-(_categoryId++), text);
							_categoryMap[text] = propertyEnumerator;
						}
					}
					if (propertyDescriptor3.Attributes[typeof(PropertyIdAttribute)] is PropertyIdAttribute propertyIdAttribute)
					{
						_propertyId = propertyIdAttribute.Id;
					}
					PropertyEnumerator propertyEnumerator2;
					if (!(propertyDescriptor3.Attributes[typeof(SortedPropertyAttribute)] is SortedPropertyAttribute sortedPropertyAttribute))
					{
						PropertyDescriptor[] array = new PropertyDescriptor[item3.Count];
						Array.Copy(item3.ToArray(), array, item3.Count);
						propertyEnumerator2 = _grid.AppendProperty(propertyEnumerator, _propertyId, propertyDescriptor3.DisplayName, _selectedObjects, (array.Length == 1) ? array[0] : new MultiplePropertyDescriptor(array), propertyDescriptor3.Description, null);
					}
					else
					{
						PropertyEnumerator children = propertyEnumerator.Children;
						while (children != children.RightBound)
						{
							int sortedIndex2 = children.Property.SortedIndex;
							if (sortedIndex2 == int.MaxValue || sortedIndex2 > sortedPropertyAttribute.SortedIndex)
							{
								break;
							}
							children.MoveNext();
						}
						PropertyDescriptor[] array2 = new PropertyDescriptor[item3.Count];
						Array.Copy(item3.ToArray(), array2, item3.Count);
						children = _grid.InsertProperty(children, _propertyId, propertyDescriptor3.DisplayName, _selectedObjects, (array2.Length == 1) ? array2[0] : new MultiplePropertyDescriptor(array2), propertyDescriptor3.Description, null);
						children.Property.SortedIndexInternal = sortedPropertyAttribute.SortedIndex;
						propertyEnumerator2 = children;
					}
					_propertyId++;
					PropertyPostFilterOutEventArgs e = new PropertyPostFilterOutEventArgs(propertyEnumerator2);
					OnPropertyPostFilterOut(e);
					if (e.GetFilterOutInternal())
					{
						if (propertyEnumerator2.Count == 1)
						{
							_categoryMap.Remove(propertyEnumerator2.Parent.Property.DisplayName);
							DeleteProperty(propertyEnumerator2.Parent);
						}
						else
						{
							DeleteProperty(propertyEnumerator2);
						}
					}
				}
				if (hashtable.Count > 0)
				{
					PropertyEnumerator firstProperty = FirstProperty;
					while (firstProperty != RightBound)
					{
						string name = firstProperty.Property.Name;
						if (name.Length > 0)
						{
							ArrayList arrayList8 = (ArrayList)hashtable[name];
							if (arrayList8 != null)
							{
								int num2 = 0;
								foreach (KeyValuePair item4 in arrayList8)
								{
									firstProperty.Property.AddValue(item4.Key, _selectedObjects[num2++], (PropertyDescriptor)item4.Value, (Attribute[])null);
								}
							}
						}
						firstProperty.MoveNext();
					}
				}
				_grid.FirstDisplayedProperty = FirstProperty.GetVisibleDeepEnumerator();
			}
			if (!_insideRefreshProperties)
			{
				OnSelectedObjectChanged(new SelectedObjectChangedEventArgs(selectedObjects));
			}
			if (displayMode != DisplayModes.Categorized)
			{
				_grid.DisplayMode = displayMode;
			}
			EndUpdate();
			_insideSelectedObjects = false;
		}
	}

	[Category("Behavior")]
	[DefaultValue(DisableModes.Simple)]
	public DisableModes DisableMode
	{
		get
		{
			return _grid.DisableMode;
		}
		set
		{
			if (_grid.DisableMode != value)
			{
				_grid.DisableMode = value;
				base.Enabled = Enabled || value != DisableModes.Full;
			}
		}
	}

	[DefaultValue(true)]
	[Category("Behavior")]
	public bool DisableModeGrayedOut
	{
		get
		{
			return _grid.DisableModeGrayedOut;
		}
		set
		{
			_grid.DisableModeGrayedOut = value;
		}
	}

	[DefaultValue(true)]
	public new bool Enabled
	{
		get
		{
			return _grid.Enabled;
		}
		set
		{
			if (_grid.Enabled != value)
			{
				_grid.Enabled = value;
				if (DisableMode == DisableModes.Full)
				{
					base.Enabled = value;
				}
			}
		}
	}

	protected override Size DefaultSize => new Size(150, 200);

	[DefaultValue(false)]
	[Category("Behavior")]
	public bool LazyLoading
	{
		get
		{
			return _lazyLoading;
		}
		set
		{
			_lazyLoading = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	public bool IsInsideRefreshProperties => _insideRefreshProperties;

	internal bool PreventPropertySelectedEvent
	{
		set
		{
			if (_preventPropertySelectedEvent != value)
			{
				_preventPropertySelectedEvent = value;
				if (!_preventPropertySelectedEvent)
				{
					PropertyEnumerator selectedPropertyEnumerator = SelectedPropertyEnumerator;
					OnPropertySelected(new PropertySelectedEventArgs(selectedPropertyEnumerator, selectedPropertyEnumerator));
				}
			}
		}
	}

	internal static bool IsInsideNotifyValueValidation => _insideNotifyValueValidation;

	public bool CompareUnderCategories => _compareUnderCategories;

	public event ValueValidationEventHandler ValueValidation;

	public event SelectedObjectChangedEventHandler SelectedObjectChanged;

	public event SelectedObjectChangingEventHandler SelectedObjectChanging;

	public event PropertySelectedEventHandler PropertySelected;

	public event PropertyExpandedEventHandler PropertyExpanded;

	public event PropertyChangedEventHandler PropertyChanged;

	public event PropertyChangingEventHandler PropertyChanging;

	public event PropertyCreatedEventHandler PropertyCreated;

	public event PropertyRestoredEventHandler PropertyRestored;

	public event PropertyValueAttributesNeededEventHandler PropertyValueAttributesNeeded;

	public event PropertyUpDownEventHandler PropertyUpDown;

	public event InPlaceCtrlVisibleEventHandler InPlaceCtrlVisible;

	public event InPlaceCtrlCreatedEventHandler InPlaceCtrlCreated;

	public event InPlaceCtrlHiddenEventHandler InPlaceCtrlHidden;

	public event PropertyButtonClickedEventHandler PropertyButtonClicked;

	public event DisplayedValuesNeededEventHandler DisplayedValuesNeeded;

	public event DisplayModeChangedEventHandler DisplayModeChanged;

	public event HyperLinkPropertyClickedEventHandler HyperLinkPropertyClicked;

	public event PropertyEnabledEventHandler PropertyEnabled;

	public event DrawingManagerChangedEventHandler DrawingManagerChanged;

	public event PropertyPostFilterOutEventHandler PropertyPostFilterOut;

	public event PropertyPreFilterOutEventHandler PropertyPreFilterOut;

	public event BeginCommitChangesEventHandler BeginCommitChanges;

	public event EndCommitChangesEventHandler EndCommitChanges;

	protected virtual void OnValueValidation(ValueValidationEventArgs e)
	{
		if (ValueValidation != null)
		{
			ValueValidation(this, e);
		}
	}

	protected internal virtual void OnSelectedObjectChanged(SelectedObjectChangedEventArgs e)
	{
		if (SelectedObjectChanged != null)
		{
			SelectedObjectChanged(this, e);
		}
	}

	protected internal virtual void OnSelectedObjectChanging(EventArgs e)
	{
		if (SelectedObjectChanging != null)
		{
			SelectedObjectChanging(this, e);
		}
	}

	protected internal virtual void OnPropertySelected(PropertySelectedEventArgs e)
	{
		if (PropertySelected != null && !_preventPropertySelectedEvent)
		{
			PropertySelected(this, e);
		}
	}

	protected internal virtual void OnPropertyExpanded(PropertyExpandedEventArgs e)
	{
		if (PropertyExpanded != null)
		{
			PropertyExpanded(this, e);
		}
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		_grid.HandleDefaultValue(e.PropertyEnum);
		if (PropertyChanged != null)
		{
			PropertyChanged(this, e);
		}
	}

	protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
	{
		if (PropertyChanging != null)
		{
			PropertyChanging(this, e);
		}
	}

	protected internal virtual void OnPropertyCreated(PropertyCreatedEventArgs e)
	{
		_grid.HandleDefaultValue(e.PropertyEnum);
		if (PropertyCreated != null)
		{
			PropertyCreated(this, e);
		}
	}

	protected internal virtual void OnPropertyRestored(PropertyRestoredEventArgs e)
	{
		if (PropertyRestored != null)
		{
			PropertyRestored(this, e);
		}
	}

	protected internal virtual void OnPropertyValueAttributesNeeded(PropertyValueAttributesNeededEventArgs e)
	{
		if (PropertyValueAttributesNeeded != null)
		{
			PropertyValueAttributesNeeded(this, e);
		}
	}

	protected virtual void OnPropertyUpDown(PropertyUpDownEventArgs e)
	{
		if (PropertyUpDown != null)
		{
			PropertyUpDown(this, e);
		}
	}

	protected internal virtual void OnInPlaceCtrlVisible(InPlaceCtrlVisibleEventArgs e)
	{
		if (InPlaceCtrlVisible != null)
		{
			InPlaceCtrlVisible(this, e);
		}
	}

	protected internal virtual void OnInPlaceCtrlCreated(InPlaceCtrlCreatedEventArgs e)
	{
		if (InPlaceCtrlCreated != null)
		{
			InPlaceCtrlCreated(this, e);
		}
	}

	protected internal virtual void OnInPlaceCtrlHidden(InPlaceCtrlVisibleEventArgs e)
	{
		if (InPlaceCtrlHidden != null)
		{
			InPlaceCtrlHidden(this, e);
		}
	}

	protected internal virtual void OnPropertyButtonClicked(PropertyButtonClickedEventArgs e)
	{
		if (PropertyButtonClicked != null)
		{
			PropertyButtonClicked(this, e);
		}
	}

	protected internal virtual void OnDisplayedValuesNeeded(DisplayedValuesNeededEventArgs e)
	{
		if (DisplayedValuesNeeded != null)
		{
			DisplayedValuesNeeded(this, e);
		}
	}

	protected internal virtual void OnDisplayModeChanged(EventArgs e)
	{
		if (!_insideValueRecreate && !_insideSelectedObjects && DisplayModeChanged != null)
		{
			DisplayModeChanged(this, e);
		}
	}

	protected internal virtual void OnHyperLinkPropertyClicked(PropertyHyperLinkClickedEventArgs e)
	{
		if (HyperLinkPropertyClicked != null)
		{
			HyperLinkPropertyClicked(this, e);
		}
	}

	protected internal virtual void OnPropertyEnabled(PropertyEnabledEventArgs e)
	{
		if (PropertyEnabled != null)
		{
			PropertyEnabled(this, e);
		}
	}

	protected internal virtual void OnDrawingManagerChanged(EventArgs e)
	{
		if (DrawingManagerChanged != null)
		{
			DrawingManagerChanged(this, e);
		}
	}

	protected internal virtual void OnPropertyPostFilterOut(PropertyPostFilterOutEventArgs e)
	{
		if (PropertyPostFilterOut != null)
		{
			PropertyPostFilterOut(this, e);
		}
	}

	protected internal virtual void OnPropertyPreFilterOut(PropertyPreFilterOutEventArgs e)
	{
		if (PropertyPreFilterOut != null)
		{
			PropertyPreFilterOut(this, e);
		}
	}

	protected internal virtual void OnBeginCommitChanges(CommitChangesEventArgs e)
	{
		if (BeginCommitChanges != null)
		{
			BeginCommitChanges(this, e);
		}
	}

	protected internal virtual void OnEndCommitChanges(CommitChangesEventArgs e)
	{
		if (EndCommitChanges != null)
		{
			EndCommitChanges(this, e);
		}
	}

	internal void SetGlobalTextMargin(int value)
	{
		_globalTextMargin = value;
	}

	private void OnToolbarResized(object sender, EventArgs e)
	{
		RepositionInternalGrid();
	}

	public virtual int GetPropertyHashCode(PropertyEnumerator propEnum)
	{
		return GetPropertyHashCode(propEnum, PropertyHashCodeMode.All);
	}

	public virtual int GetPropertyHashCode(PropertyEnumerator propEnum, PropertyHashCodeMode mode)
	{
		propEnum = propEnum.GetDeepEnumerator();
		propEnum.RestrictedToThisLevelAndUnder = false;
		int num = 0;
		if ((mode & PropertyHashCodeMode.Depth) != 0)
		{
			num ^= propEnum.Depth.GetHashCode();
		}
		if ((mode & PropertyHashCodeMode.Name) != 0)
		{
			num = ((!propEnum.Property.Name.Equals(propEnum.Property.DisplayName)) ? (num ^ (propEnum.Property.Name.GetHashCode() ^ propEnum.Property.DisplayName.GetHashCode())) : (num ^ propEnum.Property.Name.GetHashCode()));
		}
		if ((mode & PropertyHashCodeMode.Comment) != 0)
		{
			num ^= propEnum.Property.Comment.GetHashCode();
		}
		if (propEnum.Property.Value != null && (mode & PropertyHashCodeMode.Type) != 0)
		{
			num ^= propEnum.Property.Value.PropertyDescriptor.PropertyType.GetHashCode();
		}
		if (propEnum.Parent.Property != null && (mode & PropertyHashCodeMode.Parent) != 0)
		{
			num ^= GetPropertyHashCode(propEnum.Parent, mode);
		}
		return num;
	}

	public object SavePropertiesStates(PropertyStateFlags flags)
	{
		return SavePropertiesStates(FirstProperty, flags);
	}

	public object SavePropertiesStates(PropertyEnumerator propEnum, PropertyStateFlags flags)
	{
		return _grid.SavePropertiesStates(propEnum, flags);
	}

	public void RestorePropertiesStates(object states)
	{
		RestorePropertiesStates(FirstProperty, states);
	}

	public void RestorePropertiesStates(PropertyEnumerator propEnum, object propertiesStates)
	{
		if (!(propertiesStates is PropertiesStates))
		{
			throw new ArgumentException("The parameter must have been returned by SavePropertiesStates.");
		}
		_grid.RestorePropertiesStates(propEnum, propertiesStates);
	}

	public PropertyEnumerator RecreateProperty(PropertyEnumerator propEnum)
	{
		PropertyValue value = propEnum.Property.Value;
		if (value == null || value is PropertyValueSimulated)
		{
			return propEnum;
		}
		Type type = propEnum.GetType();
		PreventPropertySelectedEvent = true;
		PropertyEnumerator deepEnumerator = propEnum.GetDeepEnumerator();
		deepEnumerator.RestrictedToThisPropertyAndUnder = true;
		object propertiesStates = SavePropertiesStates(deepEnumerator, PropertyStateFlags.All);
		BeginUpdate();
		DisplayModes displayMode = DisplayMode;
		if (displayMode != DisplayModes.Categorized)
		{
			_grid.PropEnumToConvertDuringDisplayMode = propEnum;
			_grid.DisplayMode = DisplayModes.Categorized;
			propEnum = _grid.PropEnumToConvertDuringDisplayMode;
		}
		PropertyEnumerator propertyEnumerator = RightBound;
		if (_selectedObjects != null && _selectedObjects.Length != 0)
		{
			bool flag = true;
			int num = 0;
			object[] targetInstances = value.TargetInstances;
			foreach (object targetInstance in targetInstances)
			{
				PropertyDescriptor propertyDescriptor = ((value.TargetInstances.Length == 1) ? value.PropertyDescriptor : (value.PropertyDescriptor as MultiplePropertyDescriptor)[num++]);
				PropertyPreFilterOutEventArgs e = new PropertyPreFilterOutEventArgs(propertyDescriptor, targetInstance);
				OnPropertyPreFilterOut(e);
				switch (e.GetFilterOutInternal())
				{
				case PropertyPreFilterOutEventArgs.FilterModes.FilterOut:
					flag = false;
					break;
				case PropertyPreFilterOutEventArgs.FilterModes.FilterDefault:
					if (propertyDescriptor.Attributes[typeof(BrowsableAttribute)] is BrowsableAttribute { Browsable: false })
					{
						flag = false;
					}
					break;
				}
				if (!flag)
				{
					DeleteProperty(propEnum);
					break;
				}
			}
			if (flag)
			{
				propertyEnumerator = InsertProperty(propEnum, propEnum.Property.Id, propEnum.Property.DisplayName, value.TargetInstances, value.PropertyDescriptor, propEnum.Property.Comment, value.GetCustomAttributes());
				propertyEnumerator.Property.Tag = propEnum.Property.Tag;
				propertyEnumerator.Property.Value.Tag = propEnum.Property.Value.Tag;
				if (propEnum.Property.Selected)
				{
					Grid.SelectProperty(propertyEnumerator);
				}
				object[] valueKeys = propEnum.Property.GetValueKeys();
				foreach (object key in valueKeys)
				{
					PropertyValue value2 = propEnum.Property.GetValue(key);
					_ = propertyEnumerator.Property.Value;
					if (value2 is PropertyValueIndirect)
					{
						if (value2.TargetInstances.Length == 1)
						{
							propertyEnumerator.Property.AddValue(key, value2.TargetInstance, value2.PropertyDescriptor, value2.GetCustomAttributes());
							continue;
						}
						MultiplePropertyDescriptor multiplePropertyDescriptor = (MultiplePropertyDescriptor)value2.PropertyDescriptor;
						int index = 0;
						object[] targetInstances2 = value2.TargetInstances;
						foreach (object container in targetInstances2)
						{
							propertyEnumerator.Property.AddValue(key, container, multiplePropertyDescriptor[index], value2.GetCustomAttributes());
						}
					}
					else
					{
						propertyEnumerator.Property.AddManagedValue(key, value2.UnderlyingType, value2.GetValue(), value2.GetCustomAttributes());
					}
				}
				DeleteProperty(propEnum);
			}
		}
		else
		{
			propertyEnumerator = ((!(value is PropertyValueIndirect)) ? InsertManagedProperty(propEnum, propEnum.Property.Id, propEnum.Property.DisplayName, value.UnderlyingType, value.GetValue(), propEnum.Property.Comment, value.GetCustomAttributes()) : InsertProperty(propEnum, propEnum.Property.Id, propEnum.Property.DisplayName, value.TargetInstances, value.PropertyDescriptor, propEnum.Property.Comment, value.GetCustomAttributes()));
			propertyEnumerator.Property.Tag = propEnum.Property.Tag;
			propertyEnumerator.Property.Value.Tag = propEnum.Property.Value.Tag;
			propertyEnumerator.Property.Feel = propEnum.Property.Feel;
			propertyEnumerator.Property.Look = propEnum.Property.Look;
			if (propEnum.Property.Selected)
			{
				Grid.SelectProperty(propertyEnumerator);
			}
			object[] valueKeys2 = propEnum.Property.GetValueKeys();
			foreach (object key2 in valueKeys2)
			{
				PropertyValue value3 = propEnum.Property.GetValue(key2);
				_ = propertyEnumerator.Property.Value;
				if (value3 is PropertyValueIndirect)
				{
					if (value3.TargetInstances.Length == 1)
					{
						propertyEnumerator.Property.AddValue(key2, value3.TargetInstance, value3.PropertyDescriptor, value3.GetCustomAttributes());
						continue;
					}
					MultiplePropertyDescriptor multiplePropertyDescriptor2 = (MultiplePropertyDescriptor)value3.PropertyDescriptor;
					int index2 = 0;
					object[] targetInstances3 = value3.TargetInstances;
					foreach (object container2 in targetInstances3)
					{
						propertyEnumerator.Property.AddValue(key2, container2, multiplePropertyDescriptor2[index2], value3.GetCustomAttributes());
					}
				}
				else
				{
					propertyEnumerator.Property.AddManagedValue(key2, value3.UnderlyingType, value3.GetValue(), value3.GetCustomAttributes());
				}
			}
			propertyEnumerator.Property.Value.Validator = propEnum.Property.Value.Validator;
			DeleteProperty(propEnum);
		}
		if (displayMode != DisplayModes.Categorized)
		{
			_grid.PropEnumToConvertDuringDisplayMode = propertyEnumerator;
			_grid.DisplayMode = displayMode;
			propertyEnumerator = _grid.PropEnumToConvertDuringDisplayMode;
		}
		PropertyEnumerator deepEnumerator2 = propertyEnumerator.GetDeepEnumerator();
		deepEnumerator2.RestrictedToThisPropertyAndUnder = true;
		RestorePropertiesStates(deepEnumerator2, propertiesStates);
		PreventPropertySelectedEvent = false;
		EndUpdate();
		if (type.Equals(typeof(PropertySiblingEnumerator)))
		{
			propertyEnumerator = propertyEnumerator.GetSiblingEnumerator();
		}
		else if (type.Equals(typeof(PropertyVisibleSiblingEnumerator)))
		{
			propertyEnumerator = propertyEnumerator.GetVisibleSiblingEnumerator();
		}
		else if (type.Equals(typeof(PropertyVisibleDeepEnumerator)))
		{
			propertyEnumerator = propertyEnumerator.GetVisibleDeepEnumerator();
		}
		return propertyEnumerator;
	}

	static PropertyGrid()
	{
		MiscCategoryName = "Misc";
		_pdcComparerField = typeof(PropertyDescriptorCollection).GetField("comparer", BindingFlags.Instance | BindingFlags.NonPublic);
		_noSortComparer = new PropertyComparerNoSort();
		_autoCompletionTimerDuration = 1000;
		_handleIdleDuringDropDown = false;
		_insideNotifyValueValidation = false;
		VisualTipService.SetExtender(typeof(InternalGrid), new VisualTipExtender());
		ResourceManager resourceManager = new ResourceManager("System", Assembly.GetAssembly(typeof(CategoryAttribute)));
		MiscCategoryName = (string)resourceManager.GetObject("PropertyCategoryDefault", CultureInfo.CurrentUICulture);
	}

	public PropertyGrid()
	{
		base.AutoScaleMode = AutoScaleMode.None;
		_grid = new InternalGrid(this);
		_grid.TabIndex = 2;
		_grid.ForeColor = ForeColor;
		_commentsBackColor = BackColor;
		_commentsForeColor = ForeColor;
		MinCommentsAreaHeight = 35;
		CommentsHeight = 40;
		DrawingManager = DrawManagers.DotnetDrawManager;
		SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | Utilities.DoubleBufferStyle | ControlStyles.Opaque, value: true);
		UpdateStyles();
		_propertyComparer = new PropertyComparerDefaultSort(this);
		base.Controls.Add(_grid);
		_grid.Font = Font;
		RegisterFeel("none", PropertyFeel.Empty);
		RegisterFeel("edit", new PropertyEditFeel(this, password: false));
		RegisterFeel("maskededit", new PropertyMaskedEditFeel(this));
		RegisterFeel("editpwd", new PropertyEditFeel(this, password: true));
		RegisterFeel("multilineedit", new PropertyMultilineEditFeel(this));
		RegisterFeel("updown", new PropertyUpDownFeel(this, editable: false));
		RegisterFeel("editupdown", new PropertyUpDownFeel(this, editable: true));
		RegisterFeel("trackbar", new PropertyTrackBarFeel(this, editable: false));
		RegisterFeel("trackbaredit", new PropertyTrackBarFeel(this, editable: true));
		RegisterFeel("button", new PropertyButtonFeel(this, editable: false));
		RegisterFeel("fontbutton", new PropertyFontFeel(this));
		RegisterFeel("editbutton", new PropertyButtonFeel(this, editable: true));
		RegisterFeel("list", new PropertyListFeel(this, editable: false));
		RegisterFeel("editlist", new PropertyListFeel(this, editable: true));
		RegisterFeel("radiobutton", new PropertyRadioButtonFeel(this));
		RegisterFeel("checkbox", new PropertyCheckboxFeel(this));
		RegisterFeel("editunit", new PropertyUnitFeel(this));
		RegisterFeel("datetime", new PropertyDateTimeFeel(this));
		RegisterFeelAttachment(typeof(Font), "fontbutton");
		RegisterFeelAttachment(typeof(Pen), "none");
		RegisterLookAttachment(typeof(Pen), typeof(PropertyPenLook));
		RegisterDropDownContent(typeof(Enum), typeof(DropDownContentListBox));
		RegisterDropDownContent(typeof(bool), typeof(DropDownContentListBox));
		RegisterDropDownContent(typeof(string), typeof(DropDownContentListBox));
		DefaultFeel = GetRegisteredFeel("edit");
		_grid.MouseDown += OnGridMouseDown;
		_grid.MouseMove += OnGridMouseMove;
		_grid.MouseUp += OnGridMouseUp;
		_grid.MouseEnter += OnGridMouseEnter;
		_grid.MouseHover += OnGridMouseHover;
		_grid.MouseLeave += OnGridMouseLeave;
		_grid.MouseClick += OnGridMouseClick;
		_grid.MouseDoubleClick += OnGridMouseDoubleClick;
		_grid.Click += OnGridClick;
		_grid.KeyDown += OnGridKeyDown;
		_grid.KeyUp += OnGridKeyUp;
		_grid.KeyPress += OnGridKeyPress;
		_grid.DragOver += InternalGrid_DragOver;
		_grid.DragEnter += InternalGrid_DragEnter;
		_grid.DragDrop += InternalGrid_DragDrop;
		_grid.DragLeave += InternalGrid_DragLeave;
		_grid.QueryContinueDrag += InternalGrid_QueryContinueDrag;
		_grid.GiveFeedback += InternalGrid_GiveFeedback;
		SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
	}

	public void BeginInit()
	{
		_initializing = true;
		_grid.BeginInit();
	}

	public void EndInit()
	{
		_grid.EndInit();
		_initializing = false;
		int commentsHeight = _commentsHeight;
		if (_commentsHeight < MinCommentsAreaHeight)
		{
			_commentsHeight = MinCommentsAreaHeight;
		}
		else if (_commentsHeight > base.ClientRectangle.Height - 2 * BasicPropertyHeight - ToolbarHeight)
		{
			_commentsHeight = base.ClientRectangle.Height - 2 * BasicPropertyHeight - ToolbarHeight;
		}
		if (commentsHeight != _commentsHeight)
		{
			Invalidate();
			RepositionInternalGrid();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
		}
		base.Dispose(disposing);
	}

	public void SetDropDownTopWindow(Control control)
	{
		_grid.SetDropDownTopWindow(control);
	}

	public void RegisterFeel(string feelName, PropertyFeel feel)
	{
		if (feel != null)
		{
			feel.Name = feelName;
		}
		_registeredFeels[feelName] = feel;
	}

	public PropertyFeel GetRegisteredFeel(string feelName)
	{
		PropertyFeel result = null;
		if (feelName != null && feelName.Length > 0)
		{
			result = (PropertyFeel)_registeredFeels[feelName];
		}
		return result;
	}

	public void RegisterFeelAttachment(Type valueType, string feelName)
	{
		_registeredFeelAttachments[valueType] = feelName;
	}

	public PropertyFeel GetRegisteredFeelAttachment(Type valueType)
	{
		string text = (string)_registeredFeelAttachments[valueType];
		if (text == null && valueType.IsEnum)
		{
			text = (string)_registeredFeelAttachments[typeof(Enum)];
		}
		if (text == null || text.Length == 0)
		{
			return null;
		}
		return GetRegisteredFeel(text);
	}

	public void RegisterLookAttachment(Type valueType, Type lookType)
	{
		_registeredLookAttachments[valueType] = lookType;
	}

	public PropertyLook GetRegisteredLookAttachment(Type valueType)
	{
		Type type = (Type)_registeredLookAttachments[valueType];
		if (type != null)
		{
			return (PropertyLook)Activator.CreateInstance(type);
		}
		return null;
	}

	public void RegisterDropDownContent(Type valueType, Type listBoxType)
	{
		_registeredDropDownContent[valueType] = listBoxType;
	}

	internal Control GetDropDownContent(PropertyEnumerator propEnum, object valueKey)
	{
		PropertyValue propertyValue = ((valueKey != null) ? propEnum.Property.GetValue(valueKey) : propEnum.Property.Value);
		Type underlyingType = propertyValue.UnderlyingType;
		ArrayList attributes = propertyValue.GetAttributes(typeof(PropertyDropDownContentAttribute));
		PropertyDropDownContentAttribute propertyDropDownContentAttribute = null;
		foreach (PropertyDropDownContentAttribute item in attributes)
		{
			if (item.ChildPropertyName.Length == 0)
			{
				propertyDropDownContentAttribute = item;
				break;
			}
		}
		if (propertyDropDownContentAttribute == null && propEnum.Parent.Property != null)
		{
			PropertyValue value = propEnum.Parent.Property.Value;
			if (value != null)
			{
				attributes = value.GetAttributes(typeof(PropertyDropDownContentAttribute));
				foreach (PropertyDropDownContentAttribute item2 in attributes)
				{
					if (item2.ChildPropertyName.CompareTo(propertyValue.DisplayName) == 0)
					{
						propertyDropDownContentAttribute = item2;
						break;
					}
				}
			}
		}
		if (propertyDropDownContentAttribute != null && propertyDropDownContentAttribute.ControlType != null)
		{
			Control control;
			try
			{
				control = (Control)Activator.CreateInstance(propertyDropDownContentAttribute.ControlType, propertyDropDownContentAttribute.Parameters);
			}
			catch
			{
				control = (Control)Activator.CreateInstance(propertyDropDownContentAttribute.ControlType, nonPublic: false);
			}
			if (control != null)
			{
				return control;
			}
		}
		Type type = ((!underlyingType.IsEnum) ? ((Type)_registeredDropDownContent[underlyingType]) : ((Type)_registeredDropDownContent[typeof(Enum)]));
		if (type == null)
		{
			type = typeof(DropDownContentListBox);
		}
		return (Control)Activator.CreateInstance(type, nonPublic: false);
	}

	private Rectangle GetToolbarRect(Rectangle clientRect)
	{
		if (_toolbar == null)
		{
			clientRect.Height = 0;
		}
		else
		{
			clientRect.Height = _toolbar.Height;
		}
		return clientRect;
	}

	private void DrawCommentsArea(Graphics graphics, Rectangle commentRect)
	{
		Rectangle rectangle = commentRect;
		rectangle.Height = DrawManager.CommentsGapHeight;
		DrawManager.DrawCommentsGap(graphics, rectangle);
		rectangle.Y = rectangle.Bottom;
		rectangle.Height = commentRect.Height - DrawManager.CommentsGapHeight;
		DrawManager.DrawCommentsBackground(graphics, rectangle);
		PropertyEnumerator selectedPropertyEnumerator = SelectedPropertyEnumerator;
		if (selectedPropertyEnumerator != selectedPropertyEnumerator.RightBound)
		{
			rectangle.Inflate(-GlobalTextMargin - 1, -GlobalTextMargin - 1);
			DrawManager.DrawCommentText(graphics, rectangle, SelectedPropertyEnumerator);
		}
	}

	public Rectangle TransformRectIfRTL(Rectangle rect)
	{
		return TransformRectIfRTL(rect.Left, rect.Top, rect.Width, rect.Height, Grid.ClientSize.Width);
	}

	public Rectangle TransformRectIfRTL(Rectangle rect, int containerWidth)
	{
		return TransformRectIfRTL(rect.Left, rect.Top, rect.Width, rect.Height, containerWidth);
	}

	public Rectangle TransformRectIfRTL(int x, int y, int width, int height)
	{
		return TransformRectIfRTL(x, y, width, height, Grid.ClientSize.Width);
	}

	public Rectangle TransformRectIfRTL(int x, int y, int width, int height, int containerWidth)
	{
		if (RightToLeft == RightToLeft.No)
		{
			return new Rectangle(x, y, width, height);
		}
		return new Rectangle(containerWidth - (x + width), y, width, height);
	}

	public Point TransformPointIfRTL(Point pt)
	{
		return TransformPointIfRTL(pt.X, pt.Y, Grid.ClientSize.Width);
	}

	public Point TransformPointIfRTL(Point pt, int containerWidth)
	{
		return TransformPointIfRTL(pt.X, pt.Y, containerWidth);
	}

	public Point TransformPointIfRTL(int x, int y)
	{
		return TransformPointIfRTL(x, y, Grid.ClientSize.Width);
	}

	public Point TransformPointIfRTL(int x, int y, int containerWidth)
	{
		if (RightToLeft == RightToLeft.No)
		{
			return new Point(x, y);
		}
		return new Point(containerWidth - x - 1, y);
	}

	protected override void OnPaint(PaintEventArgs pe)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		if (ToolbarVisibility)
		{
			Rectangle toolbarRect = GetToolbarRect(clientRectangle);
			DrawManager.DrawButtonsBackground(pe.Graphics, toolbarRect);
			DrawManager.DrawButtonsGap(pe.Graphics, toolbarRect);
		}
		if (_showComments)
		{
			DrawCommentsArea(pe.Graphics, CommentsRect);
		}
		base.OnPaint(pe);
	}

	internal void RepositionInternalGrid()
	{
		_grid.Location = new Point(0, ToolbarHeight);
		_grid.Size = new Size(base.ClientRectangle.Width, base.ClientRectangle.Height - ToolbarHeight - CurrentCommentsHeight);
	}

	protected override void OnResize(EventArgs e)
	{
		int num = base.ClientRectangle.Height - ToolbarHeight - CurrentCommentsHeight;
		if (num < 2 * BasicPropertyHeight)
		{
			CommentsHeight = base.ClientRectangle.Height - ToolbarHeight - 2 * BasicPropertyHeight;
		}
		RepositionInternalGrid();
		Refresh();
		base.OnResize(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (_grid.Bounds.Contains(new Point(e.X, e.Y)))
		{
			base.OnMouseDown(e);
			return;
		}
		if (!_grid.Focused && _grid.TabStop)
		{
			_grid.Focus();
		}
		Invalidate();
		HitTests hitTests = HitTest(new Point(e.X, e.Y));
		if (hitTests == HitTests.CommentSplit && !_commentsLock)
		{
			_resizingCommentsInProgress = true;
			_resizingCommentsPoint = new Point(e.X, e.Y);
			base.Capture = true;
		}
		base.OnMouseDown(e);
	}

	public HitTests HitTest(Point point)
	{
		if (!base.ClientRectangle.Contains(point))
		{
			return HitTests.Nowhere;
		}
		if (CommentsVisibility)
		{
			Rectangle commentsRect = CommentsRect;
			if (!_commentsLock)
			{
				commentsRect.Height = DrawManager.CommentsGapHeight;
				if (commentsRect.Contains(point))
				{
					return HitTests.CommentSplit;
				}
			}
			commentsRect = CommentsRect;
			commentsRect.Y += DrawManager.CommentsGapHeight;
			commentsRect.Height -= DrawManager.CommentsGapHeight;
			if (commentsRect.Contains(point))
			{
				return HitTests.CommentArea;
			}
		}
		if (_grid.Bounds.Contains(point))
		{
			Point point2 = PointToInternalGrid(point);
			PropertyEnumerator propEnum;
			Rectangle itemRect;
			return _grid.HitTest(point2, out propEnum, out itemRect);
		}
		return HitTests.Client;
	}

	public PropertyEnumerator PropertyItemFromPoint(Point point, out Rectangle itemRect)
	{
		Point point2 = PointToInternalGrid(point);
		return _grid.PropertyItemFromPoint(point2, out itemRect);
	}

	public Rectangle GetItemRect(PropertyEnumerator propEnum)
	{
		return _grid.GetItemRect(propEnum);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (_grid.Bounds.Contains(new Point(e.X, e.Y)) && !base.Capture)
		{
			return;
		}
		HitTests hitTests = HitTest(new Point(e.X, e.Y));
		if (hitTests == HitTests.CommentSplit || _resizingCommentsInProgress)
		{
			Cursor = Cursors.HSplit;
		}
		else
		{
			Cursor = Cursors.Arrow;
		}
		if (_resizingCommentsInProgress)
		{
			Rectangle clientRectangle = base.ClientRectangle;
			int commentsHeight = CommentsHeight;
			int num = CommentsHeight + (_resizingCommentsPoint.Y - e.Y);
			if (num < MinCommentsAreaHeight)
			{
				_resizingCommentsPoint = new Point(e.X, e.Y);
				_resizingCommentsPoint.Y += num - MinCommentsAreaHeight;
				num = MinCommentsAreaHeight;
			}
			else if (num > clientRectangle.Height - 2 * BasicPropertyHeight - ToolbarHeight)
			{
				_resizingCommentsPoint = new Point(e.X, e.Y);
				_resizingCommentsPoint.Y += num - (clientRectangle.Height - 2 * BasicPropertyHeight - ToolbarHeight);
				num = clientRectangle.Height - 2 * BasicPropertyHeight - ToolbarHeight;
			}
			else
			{
				_resizingCommentsPoint = new Point(e.X, e.Y);
			}
			if (num != commentsHeight)
			{
				CommentsHeight = num;
			}
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (_resizingCommentsInProgress)
		{
			_resizingCommentsInProgress = false;
			base.Capture = false;
		}
		base.OnMouseUp(e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		if ((NavigationKeyMode == NavigationKeyModes.ArrowKeys || (TabKeyNavigationMode & TabKeyNavigationModes.TabKeyWithAutoFocus) == 0) && base.ActiveControl == null)
		{
			base.ActiveControl = _grid;
		}
		base.OnGotFocus(e);
	}

	public bool HasOneVisibleChild(PropertyEnumerator propEnum)
	{
		return _grid.HasOneVisibleChild(propEnum);
	}

	public PropertyEnumerator AppendRootCategory(int id, string catName)
	{
		return _grid.AppendRootCategory(id, catName);
	}

	public PropertyEnumerator InsertRootCategory(PropertyEnumerator beforeCategory, int id, string catName)
	{
		return _grid.InsertRootCategory(beforeCategory, id, catName);
	}

	public PropertyEnumerator AppendSubCategory(PropertyEnumerator underCategory, int id, string catName)
	{
		return _grid.AppendSubCategory(underCategory, id, catName);
	}

	public PropertyEnumerator InsertSubCategory(PropertyEnumerator beforeProperty, int id, string catName)
	{
		return _grid.InsertSubCategory(beforeProperty, id, catName);
	}

	public PropertyEnumerator AppendProperty(PropertyEnumerator underCategory, int id, string propName, object container, string memberName, string comment)
	{
		return AppendProperty(underCategory, id, propName, container, memberName, comment, new Attribute[0]);
	}

	public PropertyEnumerator AppendProperty(PropertyEnumerator underCategory, int id, string propName, object container, string memberName, string comment, params Attribute[] attributes)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(container, noCustomTypeDesc: true);
		PropertyDescriptor propertyDescriptor = ((!converter.GetPropertiesSupported()) ? TypeDescriptor.GetProperties(container)[memberName] : converter.GetProperties(container)[memberName]);
		if (propertyDescriptor == null)
		{
			throw new MemberAccessException("The property " + memberName + " doesn't exist in type " + container.GetType());
		}
		return AppendProperty(underCategory, id, propName, container, propertyDescriptor, comment, attributes);
	}

	public PropertyEnumerator AppendVariable(PropertyEnumerator underCategory, int id, string propName, object container, string memberName, string comment)
	{
		return AppendVariable(underCategory, id, propName, container, memberName, comment, new Attribute[0]);
	}

	public PropertyEnumerator AppendVariable(PropertyEnumerator underCategory, int id, string propName, object container, string memberName, string comment, params Attribute[] attributes)
	{
		FieldInfo field = container.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
		FieldPropertyDescriptor fieldPropertyDescriptor = new FieldPropertyDescriptor(field);
		if (fieldPropertyDescriptor == null)
		{
			throw new MemberAccessException("The field " + memberName + " doesn't exist in type " + container.GetType());
		}
		return AppendProperty(underCategory, id, propName, container, fieldPropertyDescriptor, comment, attributes);
	}

	private PropertyEnumerator AppendProperty(PropertyEnumerator underCategory, int id, string propName, object container, PropertyDescriptor propertyDescriptor, string comment, params Attribute[] attributes)
	{
		if (attributes == null)
		{
			throw new ArgumentNullException("attributes", "The attributes of the property can't be null.");
		}
		if (propName == null)
		{
			foreach (Attribute attribute in attributes)
			{
				if (attribute is DisplayNameAttribute)
				{
					propName = (attribute as DisplayNameAttribute).DisplayName;
					break;
				}
			}
			if (propName == null)
			{
				propName = propertyDescriptor.DisplayName;
			}
		}
		if (propName == null)
		{
			throw new ArgumentNullException("propName", "The display name of the property can't be null.");
		}
		if (comment == null)
		{
			foreach (Attribute attribute2 in attributes)
			{
				if (attribute2 is DescriptionAttribute)
				{
					comment = (attribute2 as DescriptionAttribute).Description;
					break;
				}
			}
			if (comment == null)
			{
				comment = propertyDescriptor.Description;
			}
		}
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The description of the property can't be null.");
		}
		return _grid.AppendProperty(underCategory, id, propName, container, propertyDescriptor, comment, attributes);
	}

	public PropertyEnumerator InsertVariable(PropertyEnumerator beforeProperty, int id, string propName, object container, string memberName, string comment)
	{
		return InsertVariable(beforeProperty, id, propName, container, memberName, comment, new Attribute[0]);
	}

	public PropertyEnumerator InsertVariable(PropertyEnumerator beforeProperty, int id, string propName, object container, string memberName, string comment, params Attribute[] attributes)
	{
		FieldInfo field = container.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public);
		FieldPropertyDescriptor fieldPropertyDescriptor = new FieldPropertyDescriptor(field);
		if (fieldPropertyDescriptor == null)
		{
			throw new MemberAccessException("The field " + memberName + " doesn't exist in type " + container.GetType());
		}
		return InsertProperty(beforeProperty, id, propName, container, fieldPropertyDescriptor, comment, attributes);
	}

	public PropertyEnumerator InsertProperty(PropertyEnumerator beforeProperty, int id, string propName, object container, string memberName, string comment)
	{
		return InsertProperty(beforeProperty, id, propName, container, memberName, comment, new Attribute[0]);
	}

	public PropertyEnumerator InsertProperty(PropertyEnumerator beforeProperty, int id, string propName, object container, string memberName, string comment, params Attribute[] attributes)
	{
		TypeConverter converter = TypeDescriptor.GetConverter(container, noCustomTypeDesc: true);
		PropertyDescriptor propertyDescriptor = ((!converter.GetPropertiesSupported()) ? TypeDescriptor.GetProperties(container)[memberName] : converter.GetProperties(container)[memberName]);
		if (propertyDescriptor == null)
		{
			throw new MemberAccessException("The property " + memberName + " doesn't exist in type " + container.GetType());
		}
		return InsertProperty(beforeProperty, id, propName, container, propertyDescriptor, comment, attributes);
	}

	private PropertyEnumerator InsertProperty(PropertyEnumerator beforeProperty, int id, string propName, object container, PropertyDescriptor propertyDescriptor, string comment, params Attribute[] attributes)
	{
		if (attributes == null)
		{
			throw new ArgumentNullException("attributes", "The attributes of the property can't be null.");
		}
		if (propName == null)
		{
			foreach (Attribute attribute in attributes)
			{
				if (attribute is DisplayNameAttribute)
				{
					propName = (attribute as DisplayNameAttribute).DisplayName;
					break;
				}
			}
			if (propName == null)
			{
				propName = propertyDescriptor.DisplayName;
			}
		}
		if (propName == null)
		{
			throw new ArgumentNullException("propName", "The display name of the property can't be null.");
		}
		if (comment == null)
		{
			foreach (Attribute attribute2 in attributes)
			{
				if (attribute2 is DescriptionAttribute)
				{
					comment = (attribute2 as DescriptionAttribute).Description;
					break;
				}
			}
			if (comment == null)
			{
				comment = propertyDescriptor.Description;
			}
		}
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The description of the property can't be null.");
		}
		return _grid.InsertProperty(beforeProperty, id, propName, container, propertyDescriptor, comment, attributes);
	}

	public PropertyEnumerator AppendHyperLinkProperty(PropertyEnumerator underCategory, int id, string propName, string comment)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The comment can't be null. Use an empty string instead.");
		}
		return _grid.AppendHyperLinkProperty(underCategory, id, propName, comment);
	}

	public PropertyEnumerator InsertHyperLinkProperty(PropertyEnumerator beforeProperty, int id, string propName, string comment)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The comment can't be null. Use an empty string instead.");
		}
		return _grid.InsertHyperLinkProperty(beforeProperty, id, propName, comment);
	}

	public PropertyEnumerator AppendManagedProperty(PropertyEnumerator underCategory, int id, string propName, Type valueType, object initialValue, string comment)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The comment can't be null. Use an empty string instead.");
		}
		return AppendManagedProperty(underCategory, id, propName, valueType, initialValue, comment, (Attribute[])null);
	}

	public PropertyEnumerator AppendManagedProperty(PropertyEnumerator underCategory, int id, string propName, Type valueType, object initialValue, string comment, params Attribute[] attributes)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The comment can't be null. Use an empty string instead.");
		}
		return _grid.AppendManagedProperty(underCategory, id, propName, valueType, initialValue, comment, attributes);
	}

	public PropertyEnumerator InsertManagedProperty(PropertyEnumerator beforeProperty, int id, string propName, Type valueType, object initialValue, string comment)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The comment can't be null. Use an empty string instead.");
		}
		return InsertManagedProperty(beforeProperty, id, propName, valueType, initialValue, comment, (Attribute[])null);
	}

	public PropertyEnumerator InsertManagedProperty(PropertyEnumerator beforeProperty, int id, string propName, Type valueType, object initialValue, string comment, params Attribute[] attributes)
	{
		if (comment == null)
		{
			throw new ArgumentNullException("comment", "The comment can't be null. Use an empty string instead.");
		}
		return _grid.InsertManagedProperty(beforeProperty, id, propName, valueType, initialValue, comment, attributes);
	}

	public void Clear()
	{
		_grid.Clear();
		_selectedObjects = null;
	}

	public void ClearUnderProperty(PropertyEnumerator propEnum)
	{
		BeginUpdate();
		_grid.ClearUnderProperty(propEnum);
		EndUpdate();
	}

	internal bool DrawImage(int imageIndex, Graphics graphics, int x, int y)
	{
		return _grid.DrawImage(imageIndex, graphics, x, y);
	}

	public void EnableProperty(PropertyEnumerator propEnum, bool enable)
	{
		_grid.EnableProperty(propEnum, enable);
	}

	public void SetManuallyDisabled(PropertyEnumerator propEnum, bool disabled)
	{
		propEnum.Property.SetManuallyDisabled(propEnum, disabled);
		_grid.Invalidate();
	}

	public void SetManuallyDisabled(PropertyEnumerator propEnum, object containerInstance, string memberName)
	{
		propEnum.Property.SetManuallyDisabled(propEnum, containerInstance, memberName);
		_grid.Invalidate();
	}

	public void ExpandProperty(PropertyEnumerator propEnum, bool expand)
	{
		_grid.ExpandProperty(propEnum, expand);
	}

	public void ExpandAllProperties(bool expand)
	{
		_grid.ExpandAllProperties(expand);
	}

	public void ExpandAllProperties(PropertyEnumerator propEnum, bool expand)
	{
		_grid.ExpandAllProperties(propEnum, expand);
	}

	public void SelectProperty(PropertyEnumerator enumerator)
	{
		_grid.SelectProperty(enumerator);
	}

	public void SelectAndFocusProperty(PropertyEnumerator enumerator, bool selectAllText)
	{
		_grid.SelectAndFocusProperty(enumerator, selectAllText);
	}

	public void NotifyPropertyChanged(PropertyChangedEventArgs e)
	{
		OnPropertyChanged(e);
	}

	internal void NotifyPropertyChanging(PropertyChangingEventArgs e)
	{
		OnPropertyChanging(e);
	}

	public void NotifyBeginCommitChanges(CommitChangesEventArgs e)
	{
		OnBeginCommitChanges(e);
	}

	public void NotifyEndCommitChanges(CommitChangesEventArgs e)
	{
		OnEndCommitChanges(e);
	}

	public void NotifyPropertyUpDown(PropertyUpDownEventArgs e)
	{
		OnPropertyUpDown(e);
	}

	internal void NotifyScroll(ScrollEventArgs e)
	{
		OnScroll(e);
	}

	public void RefreshInPlaceCtrl()
	{
		if (InPlaceControl is IInPlaceControl inPlaceControl)
		{
			inPlaceControl.OwnerPropertyEnumerator = inPlaceControl.OwnerPropertyEnumerator;
		}
	}

	public void RefreshProperties()
	{
		_insideRefreshProperties = true;
		try
		{
			BeginUpdate();
			PreventPropertySelectedEvent = true;
			object states = SavePropertiesStates(PropertyStateFlags.All);
			SelectedObjects = SelectedObjects;
			RestorePropertiesStates(states);
			PreventPropertySelectedEvent = false;
		}
		catch (Exception)
		{
		}
		finally
		{
			EndUpdate();
			_insideRefreshProperties = false;
		}
	}

	public void NotifyValueValidation(ValueValidationEventArgs e)
	{
		if (_insideNotifyValueValidation)
		{
			return;
		}
		try
		{
			_insideNotifyValueValidation = true;
			if (!_grid.ValidationWhileAppLoosingFocus || (e.ValueValidationResult & PropertyValue.ValueValidationResult.ErrorCode) == 0)
			{
				OnValueValidation(e);
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			_insideNotifyValueValidation = false;
		}
	}

	public void OnInPlaceCtrlFinishedEdition()
	{
		_grid.OnInPlaceCtrlFinishedEdition();
	}

	public void NotifyInPlaceControlCreated(InPlaceCtrlCreatedEventArgs e)
	{
		OnInPlaceCtrlCreated(e);
	}

	[Obsolete]
	protected internal virtual bool OnValidateMultiSelection(PropertyVisibleDeepEnumerator mainSelectedEnum, PropertyVisibleDeepEnumerator selectEnum)
	{
		return true;
	}

	public PropertyEnumerator FindProperty(int propertyId)
	{
		return _grid.FindProperty(propertyId);
	}

	public PropertyEnumerator FindPropertyAfter(PropertyEnumerator afterEnumerator, int propertyId)
	{
		return _grid.FindPropertyAfter(afterEnumerator, propertyId);
	}

	public bool IsPropertyDisplayed(PropertyEnumerator propEnum)
	{
		return _grid.IsPropertyDisplayed(propEnum);
	}

	public void DeleteProperty(PropertyEnumerator propEnum)
	{
		_grid.DeleteProperty(propEnum);
	}

	public bool EnsureVisible(PropertyEnumerator propEnum)
	{
		return _grid.EnsureVisible(propEnum);
	}

	public void ShowProperty(PropertyEnumerator propEnum, bool show)
	{
		_grid.ShowProperty(propEnum, show);
	}

	public void UpdateInPlaceControlFromValue()
	{
		if (InPlaceControl != null)
		{
			SelectedPropertyEnumerator.Property.Feel.UpdateInPlaceControlFromValue(SelectedPropertyEnumerator);
		}
	}

	private void OnSortByCategoy(object sender, EventArgs e)
	{
		DisplayMode = DisplayModes.Categorized;
		_grid.Focus();
	}

	private void OnSortAlphabetical(object sender, EventArgs e)
	{
		DisplayMode = DisplayModes.FlatSorted;
		_grid.Focus();
	}

	public void BeginUpdate()
	{
		if (base.Visible)
		{
			if (_beginUpdateCounter == 0)
			{
				Win32Calls.SendMessage(base.Handle, 11, IntPtr.Zero, IntPtr.Zero);
			}
			_beginUpdateCounter++;
		}
	}

	public void EndUpdate()
	{
		if (base.Visible && _beginUpdateCounter > 0)
		{
			_beginUpdateCounter--;
			if (_beginUpdateCounter == 0)
			{
				Win32Calls.SendMessage(base.Handle, 11, (IntPtr)1, IntPtr.Zero);
				Refresh();
			}
		}
	}

	public void BeginVerticalFreeze()
	{
		BeginUpdate();
		_grid.BeginVerticalFreeze();
	}

	public void EndVerticalFreeze()
	{
		_grid.EndVerticalFreeze();
		EndUpdate();
		Refresh();
	}

	public void AdjustLabelColumn()
	{
		_grid.AdjustLabelColumn();
	}

	public void AdjustComments(bool ensureVisibleSelectedProperty)
	{
		Property property = SelectedPropertyEnumerator.Property;
		if (property == null)
		{
			return;
		}
		Rectangle commentsRect = CommentsRect;
		commentsRect.Width -= 2 * (GlobalTextMargin + 1);
		int num = 0;
		Rectangle rect = commentsRect;
		using (Graphics graphics = CreateGraphics())
		{
			using (Font font = new Font(Font, FontStyle.Bold))
			{
				Win32Calls.DrawText(graphics, property.DisplayName, ref rect, font, CommentsForeColor, 0xC20 | ((RightToLeft == RightToLeft.Yes) ? 131074 : 0));
			}
			num += rect.Height + 3;
			rect = commentsRect;
			Win32Calls.DrawText(graphics, property.Comment, ref rect, Font, CommentsForeColor, 0xC10 | ((RightToLeft == RightToLeft.Yes) ? 131074 : 0));
		}
		num += rect.Height;
		num += DrawManager.CommentsGapHeight + 2 * (GlobalTextMargin + 1);
		CommentsHeight = num;
		if (ensureVisibleSelectedProperty)
		{
			EnsureVisible(SelectedPropertyEnumerator);
		}
	}

	private Point PointFromInternalGrid(Point pt)
	{
		Win32Calls.POINT lpPoints = new Win32Calls.POINT
		{
			x = pt.X,
			y = pt.Y
		};
		Win32Calls.MapWindowPoints(Grid.Handle, base.Handle, ref lpPoints, 1);
		pt.X = lpPoints.x;
		pt.Y = lpPoints.y;
		return pt;
	}

	private Point PointToInternalGrid(Point pt)
	{
		Win32Calls.POINT lpPoints = new Win32Calls.POINT
		{
			x = pt.X,
			y = pt.Y
		};
		Win32Calls.MapWindowPoints(base.Handle, Grid.Handle, ref lpPoints, 1);
		pt.X = lpPoints.x;
		pt.Y = lpPoints.y;
		return pt;
	}

	private void OnGridMouseMove(object sender, MouseEventArgs e)
	{
		Point point2 = PointFromInternalGrid(new Point(e.X, e.Y));
		OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, point2.X, point2.Y, e.Delta));
	}

	private void OnGridMouseDown(object sender, MouseEventArgs e)
	{
		Point point2 = PointFromInternalGrid(new Point(e.X, e.Y));
		OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, point2.X, point2.Y, e.Delta));
	}

	private void OnGridMouseUp(object sender, MouseEventArgs e)
	{
		Point point2 = PointFromInternalGrid(new Point(e.X, e.Y));
		OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, point2.X, point2.Y, e.Delta));
	}

	private void OnGridMouseClick(object sender, MouseEventArgs e)
	{
		Point point2 = PointFromInternalGrid(new Point(e.X, e.Y));
		OnMouseClick(new MouseEventArgs(e.Button, e.Clicks, point2.X, point2.Y, e.Delta));
	}

	private void OnGridMouseDoubleClick(object sender, MouseEventArgs e)
	{
		Point point2 = PointFromInternalGrid(new Point(e.X, e.Y));
		OnMouseDoubleClick(new MouseEventArgs(e.Button, e.Clicks, point2.X, point2.Y, e.Delta));
	}

	private void OnGridMouseEnter(object sender, EventArgs e)
	{
		OnMouseEnter(e);
	}

	private void OnGridMouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void OnGridMouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void OnGridClick(object sender, EventArgs e)
	{
		OnClick(EventArgs.Empty);
	}

	private void OnGridKeyPress(object sender, KeyPressEventArgs e)
	{
		OnKeyPress(e);
	}

	private void OnGridKeyDown(object sender, KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	private void OnGridKeyUp(object sender, KeyEventArgs e)
	{
		OnKeyUp(e);
	}

	public new DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
	{
		return _grid.DoDragDrop(data, allowedEffects);
	}

	private void InternalGrid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
	{
		OnGiveFeedback(e);
	}

	private void InternalGrid_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
	{
		OnQueryContinueDrag(e);
	}

	private void InternalGrid_DragLeave(object sender, EventArgs e)
	{
		OnDragLeave(e);
	}

	private void InternalGrid_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void InternalGrid_DragEnter(object sender, DragEventArgs e)
	{
		OnDragEnter(e);
	}

	private void InternalGrid_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	public void AddTargetInstance(PropertyEnumerator propEnum, object targetInstance, string memberName)
	{
		_grid.AddTargetInstance(propEnum, targetInstance, memberName);
	}

	protected override bool ProcessTabKey(bool forward)
	{
		if (_grid.ContainsFocus && NavigationKeyMode == NavigationKeyModes.TabKey)
		{
			if (forward)
			{
				if (_grid.InPlaceControl != null && _grid.InPlaceControl.ContainsFocus)
				{
					if (((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyInSubControls) == 0 || (_grid.InPlaceControl as IInPlaceControl).GoToNextPropertyOnTab(forward: true)) && TabToNextProperty())
					{
						return true;
					}
				}
				else if (SelectedPropertyEnumerator.Property != null && (SelectedPropertyEnumerator.Property is RootCategory || SelectedPropertyEnumerator.Property.Feel == null || SelectedPropertyEnumerator.Property.Feel == PropertyFeel.Empty || !SelectedPropertyEnumerator.Property.Enabled) && TabToNextProperty())
				{
					return true;
				}
			}
			else if (_grid.InPlaceControl != null && _grid.InPlaceControl.ContainsFocus)
			{
				if ((_grid.InPlaceControl as IInPlaceControl).GoToNextPropertyOnTab(forward: false))
				{
					if (TabToPrevProperty())
					{
						return true;
					}
					base.ActiveControl = _grid;
				}
			}
			else if (TabToPrevProperty())
			{
				return true;
			}
		}
		return base.ProcessTabKey(forward);
	}

	private bool CanTakeTabKey(PropertyEnumerator propEnum, PropertyEnumerator startEnum)
	{
		Property property = propEnum.Property;
		if (property.Feel == null || property.Feel == PropertyFeel.Empty)
		{
			return false;
		}
		if (property.Feel.DontShowInPlaceCtrl(propEnum))
		{
			return false;
		}
		if (property is RootCategory)
		{
			return false;
		}
		if (!property.Enabled)
		{
			return false;
		}
		if ((TabKeyNavigationMode & TabKeyNavigationModes.TabKeySkipReadonlyProperties) != TabKeyNavigationModes.NoSubMode && property.Value != null && property.Value.ReadOnly)
		{
			return false;
		}
		return true;
	}

	internal bool TabToNextProperty()
	{
		PropertyEnumerator propertyEnumerator;
		PropertyEnumerator propertyEnumerator2;
		if ((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyInChildProperties) != TabKeyNavigationModes.NoSubMode)
		{
			propertyEnumerator = SelectedPropertyEnumerator.GetDeepEnumerator();
			propertyEnumerator2 = SelectedPropertyEnumerator.GetDeepEnumerator();
		}
		else
		{
			propertyEnumerator = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
			propertyEnumerator2 = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
		}
		bool flag = false;
		do
		{
			propertyEnumerator.MoveNext();
			if (propertyEnumerator == RightBound)
			{
				if ((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyLeavesAtExtremes) != TabKeyNavigationModes.NoSubMode)
				{
					flag = true;
					break;
				}
				propertyEnumerator = (((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyInChildProperties) == 0) ? FirstProperty.GetVisibleDeepEnumerator() : FirstProperty.GetDeepEnumerator());
			}
			if (propertyEnumerator == propertyEnumerator2)
			{
				flag = true;
				break;
			}
		}
		while (!CanTakeTabKey(propertyEnumerator, propertyEnumerator2));
		if (!flag)
		{
			_grid.Focus();
			SelectProperty(RightBound);
			EnsureVisible(propertyEnumerator);
			SelectProperty(propertyEnumerator);
			if (InPlaceControl != null)
			{
				Control nextControl = InPlaceControl.GetNextControl(InPlaceControl, forward: true);
				base.ActiveControl = ((nextControl == null) ? InPlaceControl : nextControl);
				_grid.SelectAllText();
			}
			return true;
		}
		return propertyEnumerator == propertyEnumerator2;
	}

	private bool TabToPrevProperty()
	{
		PropertyEnumerator propertyEnumerator;
		PropertyEnumerator propertyEnumerator2;
		if ((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyInChildProperties) != TabKeyNavigationModes.NoSubMode)
		{
			propertyEnumerator = SelectedPropertyEnumerator.GetDeepEnumerator();
			propertyEnumerator2 = SelectedPropertyEnumerator.GetDeepEnumerator();
		}
		else
		{
			propertyEnumerator = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
			propertyEnumerator2 = SelectedPropertyEnumerator.GetVisibleDeepEnumerator();
		}
		bool flag = false;
		do
		{
			propertyEnumerator.MovePrev();
			if (propertyEnumerator == LeftBound)
			{
				if ((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyLeavesAtExtremes) != TabKeyNavigationModes.NoSubMode)
				{
					flag = true;
					break;
				}
				propertyEnumerator = (((TabKeyNavigationMode & TabKeyNavigationModes.TabKeyInChildProperties) == 0) ? LastProperty.GetVisibleDeepEnumerator() : LastProperty.GetDeepEnumerator());
			}
			if (propertyEnumerator == propertyEnumerator2)
			{
				flag = true;
				break;
			}
		}
		while (!CanTakeTabKey(propertyEnumerator, propertyEnumerator2));
		if (!flag)
		{
			_grid.Focus();
			SelectProperty(RightBound);
			EnsureVisible(propertyEnumerator);
			SelectProperty(propertyEnumerator);
			if (InPlaceControl != null)
			{
				Control nextControl = InPlaceControl.GetNextControl(InPlaceControl, forward: true);
				base.ActiveControl = ((nextControl == null) ? InPlaceControl : nextControl);
				_grid.SelectAllText();
			}
			return true;
		}
		return propertyEnumerator == propertyEnumerator2;
	}

	private void FilterPropertyDescriptors(object targetInstance, ref PropertyDescriptorCollection collection)
	{
		new ArrayList();
		for (int i = 0; i < collection.Count; i++)
		{
			PropertyDescriptor propertyDescriptor = collection[i];
			PropertyPreFilterOutEventArgs e = new PropertyPreFilterOutEventArgs(propertyDescriptor, targetInstance);
			OnPropertyPreFilterOut(e);
			switch (e.GetFilterOutInternal())
			{
			case PropertyPreFilterOutEventArgs.FilterModes.FilterOut:
				collection.RemoveAt(i--);
				break;
			case PropertyPreFilterOutEventArgs.FilterModes.FilterDefault:
				if (propertyDescriptor.Attributes[typeof(BrowsableAttribute)] is BrowsableAttribute { Browsable: false })
				{
					collection.RemoveAt(i--);
				}
				break;
			}
		}
	}

	internal PropertyDescriptorCollection GetPropertyDescriptors(PropertyEnumerator parentEnum, object targetInstance, PropertyDescriptor propertyDescriptor, TypeConverter typeConverter)
	{
		if (targetInstance == null)
		{
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}
		_compareUnderCategories = parentEnum == null;
		if (typeConverter == null)
		{
			typeConverter = ((propertyDescriptor != null) ? propertyDescriptor.Converter : TypeDescriptor.GetConverter(targetInstance, noCustomTypeDesc: true));
		}
		object obj = null;
		obj = ((!(parentEnum != null)) ? targetInstance : ((parentEnum.Property.Value.TargetInstances.Length == 1) ? parentEnum.Property.Value.TargetInstance : parentEnum.Property.Value.TargetInstances));
		PropertyTypeDescriptorContext context = new PropertyTypeDescriptorContext(propertyDescriptor, obj, null, this);
		PropertyDescriptorCollection propertyDescriptorCollection = (typeConverter.GetPropertiesSupported(context) ? typeConverter.GetProperties(context, targetInstance, new Attribute[0]) : ((!(targetInstance is ICustomTypeDescriptor)) ? TypeDescriptor.GetProperties(targetInstance) : (targetInstance as ICustomTypeDescriptor).GetProperties(new Attribute[0])));
		if (propertyDescriptorCollection == null)
		{
			return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
		}
		IComparer comparer = (IComparer)_pdcComparerField.GetValue(propertyDescriptorCollection);
		propertyDescriptorCollection = propertyDescriptorCollection.Sort(_noSortComparer);
		FilterPropertyDescriptors(targetInstance, ref propertyDescriptorCollection);
		if (targetInstance is ICollection && !(targetInstance is ICustomTypeDescriptor))
		{
			return propertyDescriptorCollection.Sort(_noSortComparer);
		}
		if (comparer == null)
		{
			return propertyDescriptorCollection.Sort(_propertyComparer);
		}
		return propertyDescriptorCollection.Sort(comparer);
	}

	internal bool MatchPropertyDescriptors(PropertyDescriptor pd1, PropertyDescriptor pd2)
	{
		if (pd1.IsReadOnly != pd1.IsReadOnly || pd1.Category != pd2.Category || pd1.DisplayName != pd2.DisplayName)
		{
			return false;
		}
		foreach (Attribute attribute in pd1.Attributes)
		{
			if ((attribute.GetType() == typeof(PropertyFeelAttribute) || attribute.GetType() == typeof(PropertyLookAttribute) || attribute.GetType() == typeof(ValueAddedToPropertyAttribute) || attribute.GetType() == typeof(PropertyDisableAttribute) || attribute.GetType() == typeof(PropertyDropDownContentAttribute) || attribute.GetType() == typeof(PropertyHideAttribute) || attribute.GetType() == typeof(PropertyValidatorAttribute) || attribute.GetType() == typeof(PropertyValueDisplayedAsAttribute) || attribute.GetType() == typeof(ShowChildPropertiesAttribute)) && !pd2.Attributes.Matches(attribute))
			{
				return false;
			}
		}
		return true;
	}

	private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		if (e.Category == UserPreferenceCategory.VisualStyle)
		{
			ThemeRenderer.ResetInitialization();
		}
	}

	protected override void WndProc(ref Message m)
	{
		base.WndProc(ref m);
	}

	public void HandleDefaultValue(PropertyEnumerator propEnum)
	{
		if (_showDefaultValues)
		{
			_grid.HandleDefaultValue(propEnum);
		}
		_grid.Refresh();
	}

	public void HandleDefaultValues()
	{
		BeginUpdate();
		PropertyEnumerator firstProperty = FirstProperty;
		while (firstProperty != firstProperty.RightBound)
		{
			if (_showDefaultValues)
			{
				HandleDefaultValue(firstProperty);
			}
			else if (firstProperty.Property.Value != null)
			{
				firstProperty.Property.Value.Font = new Font(firstProperty.Property.Value.Font, FontStyle.Regular);
			}
			firstProperty.MoveNext();
		}
		EndUpdate();
	}

	public int GetPropertyVisibleIndex(PropertyEnumerator propEnum)
	{
		return _grid.GetPropertyVisibleIndex(propEnum);
	}

	public void MovePropertyToVisibleIndex(PropertyEnumerator propEnum, int index)
	{
		_grid.MovePropertyToVisibleIndex(propEnum, index, finalize: true);
	}

	public static void ResetRenderer()
	{
		ThemeRenderer.ResetInitialization();
	}
}
