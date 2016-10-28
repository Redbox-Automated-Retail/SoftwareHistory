using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Alsing.Drawing.GDI;
using Alsing.SourceCode;
using Alsing.Windows.Forms.CoreLib;
using Alsing.Windows.Forms.SyntaxBox;

namespace Alsing.Windows.Forms;

[Designer(typeof(SyntaxBoxDesigner), typeof(IDesigner))]
[ComVisible(true)]
public class SyntaxBoxControl : SplitViewParentControl
{
	protected internal bool DisableAutoList;

	protected internal bool DisableFindForm;

	protected internal bool DisableInfoTip;

	protected internal bool DisableIntelliMouse;

	private bool _AllowBreakPoints = true;

	private Color _BackColor = Color.White;

	private Color _BracketBackColor = Color.LightSteelBlue;

	private Color _BracketBorderColor = Color.DarkBlue;

	private Color _BracketForeColor = Color.Black;

	private bool _BracketMatching = true;

	private Color _BreakPointBackColor = Color.DarkRed;

	private Color _BreakPointForeColor = Color.White;

	private SyntaxDocument _Document;

	private string _FontName = "Courier new";

	private float _FontSize = 10f;

	private Color _GutterMarginBorderColor = SystemColors.ControlDark;

	private Color _GutterMarginColor = SystemColors.Control;

	private int _GutterMarginWidth = 19;

	private bool _HighLightActiveLine;

	private Color _HighLightedLineColor = Color.LightYellow;

	private Color _InactiveSelectionBackColor = SystemColors.ControlDark;

	private Color _InactiveSelectionForeColor = SystemColors.ControlLight;

	private IndentStyle _Indent = IndentStyle.LastRow;

	private KeyboardActionList _KeyboardActions = new KeyboardActionList();

	private Color _LineNumberBackColor = SystemColors.Window;

	private Color _LineNumberBorderColor = Color.Teal;

	private Color _LineNumberForeColor = Color.Teal;

	private Color _OutlineColor = SystemColors.ControlDark;

	private bool _ParseOnPaste;

	private Color _ScopeBackColor = Color.Transparent;

	private Color _ScopeIndicatorColor = Color.Transparent;

	private Color _SelectionBackColor = SystemColors.Highlight;

	private Color _SelectionForeColor = SystemColors.HighlightText;

	private Color _SeparatorColor = SystemColors.Control;

	private bool _ShowGutterMargin = true;

	private bool _ShowLineNumbers = true;

	private bool _ShowTabGuides;

	private bool _ShowWhitespace;

	private int _SmoothScrollSpeed = 2;

	private Color _TabGuideColor = ControlPaint.Light(SystemColors.ControlLight);

	private int _TabSize = 4;

	private int _TooltipDelay = 240;

	private bool _VirtualWhitespace;

	private Color _WhitespaceColor = SystemColors.ControlDark;

	private IContainer components;

	private ImageList _AutoListIcons;

	private ImageList _GutterIcons;

	private Timer ParseTimer;

	private bool _ShowEOLMarker;

	private Color _EOLMarkerColor = Color.Red;

	private bool _AutoListAutoSelect = true;

	private bool _CollapsedBlockTooltipsEnabled = true;

	private bool _ShowScopeIndicator;

	[DefaultValue(false)]
	[Category("Appearance")]
	[Description("Determines if a ¶ should be displayed at the end of a line")]
	public bool ShowEOLMarker
	{
		get
		{
			return _ShowEOLMarker;
		}
		set
		{
			_ShowEOLMarker = value;
			Redraw();
		}
	}

	[Description("The color of the EOL marker")]
	[Category("Appearance")]
	[DefaultValue(typeof(Color), "Red")]
	public Color EOLMarkerColor
	{
		get
		{
			return _EOLMarkerColor;
		}
		set
		{
			_EOLMarkerColor = value;
			Redraw();
		}
	}

	[DefaultValue(true)]
	public bool AutoListAutoSelect
	{
		get
		{
			return _AutoListAutoSelect;
		}
		set
		{
			_AutoListAutoSelect = value;
		}
	}

	[Category("Behavior - Clipboard")]
	[DefaultValue(typeof(Color), "false")]
	[Description("determines if the copy actions should be stored as RTF")]
	public bool CopyAsRTF { get; set; }

	[DefaultValue(typeof(Color), "Transparent")]
	[Category("Appearance - Scopes")]
	[Description("The color of the active scope")]
	public Color ScopeBackColor
	{
		get
		{
			return _ScopeBackColor;
		}
		set
		{
			_ScopeBackColor = value;
			Redraw();
		}
	}

	[Category("Appearance - Scopes")]
	[DefaultValue(typeof(Color), "Transparent")]
	[Description("The color of the scope indicator")]
	public Color ScopeIndicatorColor
	{
		get
		{
			return _ScopeIndicatorColor;
		}
		set
		{
			_ScopeIndicatorColor = value;
			Redraw();
		}
	}

	[Browsable(false)]
	[Category("Behavior")]
	public TextPoint AutoListPosition
	{
		get
		{
			return ((EditViewControl)_ActiveView).AutoListPosition;
		}
		set
		{
			if (_ActiveView != null)
			{
				((EditViewControl)_ActiveView).AutoListPosition = value;
			}
		}
	}

	[Category("Behavior")]
	[Browsable(false)]
	public TextPoint InfoTipPosition
	{
		get
		{
			return ((EditViewControl)_ActiveView).InfoTipPosition;
		}
		set
		{
			if (_ActiveView != null)
			{
				((EditViewControl)_ActiveView).InfoTipPosition = value;
			}
		}
	}

	[Browsable(false)]
	[Description("Prevents the control from changing the cursor.")]
	[Category("Appearance")]
	public bool LockCursorUpdate { get; set; }

	[Category("Appearance")]
	[DefaultValue(0)]
	[Description("The number of pixels to add between rows")]
	public int RowPadding { get; set; }

	[Browsable(false)]
	[Category("Appearance - Infotip")]
	[Description("The currently active selection in the infotip")]
	public int InfoTipSelectedIndex
	{
		get
		{
			return ((EditViewControl)_ActiveView).InfoTip.SelectedIndex;
		}
		set
		{
			if (_ActiveView != null && ((EditViewControl)_ActiveView).InfoTip != null)
			{
				((EditViewControl)_ActiveView).InfoTip.SelectedIndex = value;
			}
		}
	}

	[Category("Appearance - InfoTip")]
	[DefaultValue(null)]
	[Description("An image to show in the infotip")]
	public Image InfoTipImage
	{
		get
		{
			return ((EditViewControl)_ActiveView).InfoTip.Image;
		}
		set
		{
			if (_ActiveView != null && ((EditViewControl)_ActiveView).InfoTip != null)
			{
				((EditViewControl)_ActiveView).InfoTip.Image = value;
			}
		}
	}

	[Browsable(false)]
	[Category("Appearance")]
	[Description("Get or Sets the number of choices that could be made in the infotip")]
	public int InfoTipCount
	{
		get
		{
			return ((EditViewControl)_ActiveView).InfoTip.Count;
		}
		set
		{
			if (_ActiveView != null && ((EditViewControl)_ActiveView).InfoTip != null)
			{
				((EditViewControl)_ActiveView).InfoTip.Count = value;
				((EditViewControl)_ActiveView).InfoTip.Init();
			}
		}
	}

	[DefaultValue("")]
	[Description("The infotip text")]
	[Category("Appearance - InfoTip")]
	public string InfoTipText
	{
		get
		{
			return ((EditViewControl)_ActiveView).InfoTip.Data;
		}
		set
		{
			if (_ActiveView != null && ((EditViewControl)_ActiveView).InfoTip != null)
			{
				((EditViewControl)_ActiveView).InfoTip.Data = value;
			}
		}
	}

	[Browsable(false)]
	public Selection Selection
	{
		get
		{
			if (_ActiveView != null)
			{
				return ((EditViewControl)_ActiveView).Selection;
			}
			return null;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public KeyboardActionList KeyboardActions
	{
		get
		{
			return _KeyboardActions;
		}
		set
		{
			_KeyboardActions = value;
		}
	}

	[Category("Appearance")]
	[Description("Gets or Sets if the AutoList is visible in the active view.")]
	[Browsable(false)]
	public bool AutoListVisible
	{
		get
		{
			if (_ActiveView != null)
			{
				return ((EditViewControl)_ActiveView).AutoListVisible;
			}
			return false;
		}
		set
		{
			if (_ActiveView != null)
			{
				((EditViewControl)_ActiveView).AutoListVisible = value;
			}
		}
	}

	[Category("Appearance")]
	[Description("Gets or Sets if the InfoTip is visible in the active view.")]
	[Browsable(false)]
	public bool InfoTipVisible
	{
		get
		{
			if (_ActiveView != null)
			{
				return ((EditViewControl)_ActiveView).InfoTipVisible;
			}
			return false;
		}
		set
		{
			if (_ActiveView != null)
			{
				((EditViewControl)_ActiveView).InfoTipVisible = value;
			}
		}
	}

	[Browsable(false)]
	public bool CanCopy => ((EditViewControl)_ActiveView).CanCopy;

	[Browsable(false)]
	public bool CanPaste => ((EditViewControl)_ActiveView).CanPaste;

	[Browsable(false)]
	public bool CanRedo => ((EditViewControl)_ActiveView).CanRedo;

	[Browsable(false)]
	public bool CanUndo => ((EditViewControl)_ActiveView).CanUndo;

	[Description("Gets or Sets the imagelist to use in the gutter margin.")]
	[Category("Appearance - Gutter Margin")]
	public ImageList GutterIcons
	{
		get
		{
			return _GutterIcons;
		}
		set
		{
			_GutterIcons = value;
			Redraw();
		}
	}

	[Description("Gets or Sets the imagelist to use in the autolist.")]
	[Category("Appearance")]
	[DefaultValue(null)]
	public ImageList AutoListIcons
	{
		get
		{
			return _AutoListIcons;
		}
		set
		{
			_AutoListIcons = value;
			foreach (EditViewControl view in base.Views)
			{
				if (view != null && view.AutoList != null)
				{
					view.AutoList.Images = value;
				}
			}
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "Control")]
	[Description("Gets or Sets the color to use when rendering Tab guides.")]
	[Category("Appearance - Tabs")]
	public Color TabGuideColor
	{
		get
		{
			return _TabGuideColor;
		}
		set
		{
			_TabGuideColor = value;
			Redraw();
		}
	}

	[Description("Gets or Sets the color of the bracket match borders.")]
	[Category("Appearance - Bracket Match")]
	[DefaultValue(typeof(Color), "DarkBlue")]
	public Color BracketBorderColor
	{
		get
		{
			return _BracketBorderColor;
		}
		set
		{
			_BracketBorderColor = value;
			Redraw();
		}
	}

	[Description("Gets or Sets if the control should render Tab guides.")]
	[DefaultValue(false)]
	[Category("Appearance - Tabs")]
	public bool ShowTabGuides
	{
		get
		{
			return _ShowTabGuides;
		}
		set
		{
			_ShowTabGuides = value;
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "Control")]
	[Category("Appearance")]
	[Description("Gets or Sets the color to use when rendering whitespace characters.")]
	public Color WhitespaceColor
	{
		get
		{
			return _WhitespaceColor;
		}
		set
		{
			_WhitespaceColor = value;
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "ControlDark")]
	[Description("Gets or Sets the color of the code Outlining (both folding lines and collapsed blocks).")]
	[Category("Appearance")]
	public Color OutlineColor
	{
		get
		{
			return _OutlineColor;
		}
		set
		{
			_OutlineColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "False")]
	[Category("Behavior")]
	[Description("Determines if the control should use a smooth scroll when scrolling one row up or down.")]
	public bool SmoothScroll { get; set; }

	[DefaultValue(2)]
	[Category("Behavior")]
	[Description("Gets or Sets the speed of the vertical scroll when SmoothScroll is activated")]
	public int SmoothScrollSpeed
	{
		get
		{
			return _SmoothScrollSpeed;
		}
		set
		{
			if (value <= 0)
			{
				throw new Exception("Scroll speed may not be less than 1");
			}
			_SmoothScrollSpeed = value;
		}
	}

	[DefaultValue(true)]
	[Category("Behavior")]
	[Description("Gets or Sets if the control can display breakpoints or not.")]
	public bool AllowBreakPoints
	{
		get
		{
			return _AllowBreakPoints;
		}
		set
		{
			_AllowBreakPoints = value;
		}
	}

	[Category("Behavior - Clipboard")]
	[DefaultValue(false)]
	[Description("Gets or Sets if the control should perform a full parse of the document when content is drag dropped or pasted into the control")]
	public bool ParseOnPaste
	{
		get
		{
			return _ParseOnPaste;
		}
		set
		{
			_ParseOnPaste = value;
			Redraw();
		}
	}

	[DefaultValue(10f)]
	[Category("Appearance - Font")]
	[Description("The size of the font")]
	public float FontSize
	{
		get
		{
			return _FontSize;
		}
		set
		{
			_FontSize = value;
			InitGraphics();
			Redraw();
		}
	}

	[DefaultValue(IndentStyle.LastRow)]
	[Category("Behavior")]
	[Description("Determines how the the control indents a new line")]
	public IndentStyle Indent
	{
		get
		{
			return _Indent;
		}
		set
		{
			_Indent = value;
		}
	}

	[Category("Content")]
	[Description("The SyntaxDocument that is attatched to the contro")]
	public SyntaxDocument Document
	{
		get
		{
			return _Document;
		}
		set
		{
			AttachDocument(value);
		}
	}

	[Category("Behavior")]
	[Description("The delay before the tooltip is displayed over a collapsed block")]
	[DefaultValue(240)]
	public int TooltipDelay
	{
		get
		{
			return _TooltipDelay;
		}
		set
		{
			_TooltipDelay = value;
		}
	}

	[Category("Behavior")]
	[Description("The delay before the tooltip is displayed over a collapsed block")]
	[DefaultValue(true)]
	public bool CollapsedBlockTooltipsEnabled
	{
		get
		{
			return _CollapsedBlockTooltipsEnabled;
		}
		set
		{
			_CollapsedBlockTooltipsEnabled = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(false)]
	[Description("Determines if the control is readonly or not")]
	public bool ReadOnly { get; set; }

	[Editor(typeof(FontList), typeof(UITypeEditor))]
	[Category("Appearance - Font")]
	[Description("The name of the font that is used to render the control")]
	[DefaultValue("Courier New")]
	public string FontName
	{
		get
		{
			return _FontName;
		}
		set
		{
			if (base.Views == null)
			{
				return;
			}
			_FontName = value;
			InitGraphics();
			foreach (EditViewControl view in base.Views)
			{
				view.CalcMaxCharWidth();
			}
			Redraw();
		}
	}

	[Category("Appearance - Bracket Match")]
	[DefaultValue(true)]
	[Description("Determines if the control should highlight scope patterns")]
	public bool BracketMatching
	{
		get
		{
			return _BracketMatching;
		}
		set
		{
			_BracketMatching = value;
			Redraw();
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Determines if virtual Whitespace is active")]
	public bool VirtualWhitespace
	{
		get
		{
			return _VirtualWhitespace;
		}
		set
		{
			_VirtualWhitespace = value;
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "Control")]
	[Description("The separator color")]
	[Category("Appearance")]
	public Color SeparatorColor
	{
		get
		{
			return _SeparatorColor;
		}
		set
		{
			_SeparatorColor = value;
			Redraw();
		}
	}

	[Description("The foreground color to use when BracketMatching is activated")]
	[Category("Appearance - Bracket Match")]
	[DefaultValue(typeof(Color), "Black")]
	public Color BracketForeColor
	{
		get
		{
			return _BracketForeColor;
		}
		set
		{
			_BracketForeColor = value;
			Redraw();
		}
	}

	[Category("Appearance - Bracket Match")]
	[Description("The background color to use when BracketMatching is activated")]
	[DefaultValue(typeof(Color), "LightSteelBlue")]
	public Color BracketBackColor
	{
		get
		{
			return _BracketBackColor;
		}
		set
		{
			_BracketBackColor = value;
			Redraw();
		}
	}

	[Category("Appearance - Selection")]
	[DefaultValue(typeof(Color), "ControlDark")]
	[Description("The inactive selection background color.")]
	public Color InactiveSelectionBackColor
	{
		get
		{
			return _InactiveSelectionBackColor;
		}
		set
		{
			_InactiveSelectionBackColor = value;
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "ControlLight")]
	[Category("Appearance - Selection")]
	[Description("The inactive selection foreground color.")]
	public Color InactiveSelectionForeColor
	{
		get
		{
			return _InactiveSelectionForeColor;
		}
		set
		{
			_InactiveSelectionForeColor = value;
			Redraw();
		}
	}

	[Description("The selection background color.")]
	[Category("Appearance - Selection")]
	[DefaultValue(typeof(Color), "Highlight")]
	public Color SelectionBackColor
	{
		get
		{
			return _SelectionBackColor;
		}
		set
		{
			_SelectionBackColor = value;
			Redraw();
		}
	}

	[Category("Appearance - Selection")]
	[Description("The selection foreground color.")]
	[DefaultValue(typeof(Color), "HighlightText")]
	public Color SelectionForeColor
	{
		get
		{
			return _SelectionForeColor;
		}
		set
		{
			_SelectionForeColor = value;
			Redraw();
		}
	}

	[DefaultValue(typeof(Color), "ControlDark")]
	[Description("The border color of the gutter margin")]
	[Category("Appearance - Gutter Margin")]
	public Color GutterMarginBorderColor
	{
		get
		{
			return _GutterMarginBorderColor;
		}
		set
		{
			_GutterMarginBorderColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[Category("Appearance - Line Numbers")]
	[Description("The border color of the line number margin")]
	[DefaultValue(typeof(Color), "Teal")]
	public Color LineNumberBorderColor
	{
		get
		{
			return _LineNumberBorderColor;
		}
		set
		{
			_LineNumberBorderColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[Category("Appearance - BreakPoints")]
	[Description("The foreground color of a Breakpoint")]
	[DefaultValue(typeof(Color), "White")]
	public Color BreakPointForeColor
	{
		get
		{
			return _BreakPointForeColor;
		}
		set
		{
			_BreakPointForeColor = value;
			Redraw();
		}
	}

	[Category("Appearance - BreakPoints")]
	[Description("The background color to use when BracketMatching is activated")]
	[DefaultValue(typeof(Color), "DarkRed")]
	public Color BreakPointBackColor
	{
		get
		{
			return _BreakPointBackColor;
		}
		set
		{
			_BreakPointBackColor = value;
			Redraw();
		}
	}

	[Description("The foreground color of line numbers")]
	[Category("Appearance - Line Numbers")]
	[DefaultValue(typeof(Color), "Teal")]
	public Color LineNumberForeColor
	{
		get
		{
			return _LineNumberForeColor;
		}
		set
		{
			_LineNumberForeColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[Category("Appearance - Line Numbers")]
	[Description("The background color of line numbers")]
	[DefaultValue(typeof(Color), "Window")]
	public Color LineNumberBackColor
	{
		get
		{
			return _LineNumberBackColor;
		}
		set
		{
			_LineNumberBackColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[Description("The color of the gutter margin")]
	[Category("Appearance - Gutter Margin")]
	[DefaultValue(typeof(Color), "Control")]
	public Color GutterMarginColor
	{
		get
		{
			return _GutterMarginColor;
		}
		set
		{
			_GutterMarginColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[Description("The background color of the client area")]
	[Category("Appearance")]
	[DefaultValue(typeof(Color), "Window")]
	public new Color BackColor
	{
		get
		{
			return _BackColor;
		}
		set
		{
			_BackColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[Category("Appearance - Active Line")]
	[Description("The background color of the active line")]
	[DefaultValue(typeof(Color), "LightYellow")]
	public Color HighLightedLineColor
	{
		get
		{
			return _HighLightedLineColor;
		}
		set
		{
			_HighLightedLineColor = value;
			InitGraphics();
			Redraw();
		}
	}

	[DefaultValue(false)]
	[Description("Determines if the active line should be highlighted")]
	[Category("Appearance - Active Line")]
	public bool HighLightActiveLine
	{
		get
		{
			return _HighLightActiveLine;
		}
		set
		{
			_HighLightActiveLine = value;
			Redraw();
		}
	}

	[DefaultValue(false)]
	[Category("Appearance")]
	[Description("Determines if Whitespace should be rendered as symbols")]
	public bool ShowWhitespace
	{
		get
		{
			return _ShowWhitespace;
		}
		set
		{
			_ShowWhitespace = value;
			Redraw();
		}
	}

	[Category("Appearance - Line Numbers")]
	[DefaultValue(true)]
	[Description("Determines if the line number margin should be visible")]
	public bool ShowLineNumbers
	{
		get
		{
			return _ShowLineNumbers;
		}
		set
		{
			_ShowLineNumbers = value;
			Redraw();
		}
	}

	[DefaultValue(true)]
	[Category("Appearance - Gutter Margin")]
	[Description("Determines if the gutter margin should be visible")]
	public bool ShowGutterMargin
	{
		get
		{
			return _ShowGutterMargin;
		}
		set
		{
			_ShowGutterMargin = value;
			Redraw();
		}
	}

	[DefaultValue(19)]
	[Category("Appearance - Gutter Margin")]
	[Description("Determines the width of the gutter margin in pixels")]
	public int GutterMarginWidth
	{
		get
		{
			return _GutterMarginWidth;
		}
		set
		{
			_GutterMarginWidth = value;
			Redraw();
		}
	}

	[Category("Appearance - Tabs")]
	[Description("Determines whether or not the SyntaxBox converts tabs to spaces as you type.")]
	[DefaultValue(false)]
	public bool TabsToSpaces { get; set; }

	[DefaultValue(4)]
	[Category("Appearance - Tabs")]
	[Description("Determines the size of a TAB in number of SPACE chars")]
	public int TabSize
	{
		get
		{
			return _TabSize;
		}
		set
		{
			_TabSize = value;
			Redraw();
		}
	}

	[DefaultValue(true)]
	[Category("Appearance - Scopes")]
	[Description("Determines if the scope indicator should be shown")]
	public bool ShowScopeIndicator
	{
		get
		{
			return _ShowScopeIndicator;
		}
		set
		{
			_ShowScopeIndicator = value;
			Redraw();
		}
	}

	[Browsable(false)]
	public Caret Caret
	{
		get
		{
			if (_ActiveView != null)
			{
				return ((EditViewControl)_ActiveView).Caret;
			}
			return null;
		}
	}

	[Obsolete("Use .FontName and .FontSize", true)]
	[Browsable(false)]
	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
		}
	}

	[Obsolete("Apply a syntax instead", true)]
	[Browsable(false)]
	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
		}
	}

	[Browsable(false)]
	public string AutoListSelectedText
	{
		get
		{
			return ((EditViewControl)_ActiveView).AutoList.SelectedText;
		}
		set
		{
			if (_ActiveView != null && ((EditViewControl)_ActiveView).AutoList != null)
			{
				((EditViewControl)_ActiveView).AutoList.SelectItem(value);
			}
		}
	}

	public event WordMouseHandler WordMouseHover;

	public event WordMouseHandler WordMouseDown;

	public event CopyHandler ClipboardUpdated;

	public event EventHandler CaretChange;

	public event EventHandler SelectionChange;

	public event EventHandler InfoTipSelectedIndexChanged;

	public event RowPaintHandler RenderRow;

	public event RowMouseHandler RowMouseDown;

	public event RowMouseHandler RowMouseMove;

	public event RowMouseHandler RowMouseUp;

	public event RowMouseHandler RowClick;

	public event RowMouseHandler RowDoubleClick;

	public void ConvertTabsToSpaces()
	{
		if (_Document == null)
		{
			return;
		}
		_Document.StartUndoCapture();
		string newValue = new string(' ', _TabSize);
		for (int i = 0; i < _Document.Count; i++)
		{
			Row row = _Document[i];
			string text = row.Text;
			string text2 = text.Replace("\t", newValue);
			if (text2 != text)
			{
				_Document.DeleteRange(new TextRange(0, i, text.Length, i));
				_Document.InsertText(text2, 0, i, StoreUndo: true);
			}
		}
		_Document.EndUndoCapture();
	}

	public void ConvertSpacesToTabs()
	{
		if (_Document == null)
		{
			return;
		}
		_Document.StartUndoCapture();
		string oldValue = new string(' ', _TabSize);
		for (int i = 0; i < _Document.Count; i++)
		{
			Row row = _Document[i];
			string text = row.Text;
			string text2 = text.Replace(oldValue, "\t");
			if (text2 != text)
			{
				_Document.DeleteRange(new TextRange(0, i, text.Length - 1, i));
				_Document.InsertText(text2, 0, i, StoreUndo: true);
			}
		}
		_Document.EndUndoCapture();
	}

	public void ScrollIntoView(int RowIndex)
	{
		((EditViewControl)_ActiveView).ScrollIntoView(RowIndex);
	}

	public void AutoListBeginLoad()
	{
		((EditViewControl)_ActiveView).AutoListBeginLoad();
	}

	public void AutoListEndLoad()
	{
		((EditViewControl)_ActiveView).AutoListEndLoad();
	}

	public void AutoListClear()
	{
		((EditViewControl)_ActiveView).AutoList.Clear();
	}

	public void AutoListAdd(string text, int ImageIndex)
	{
		((EditViewControl)_ActiveView).AutoList.Add(text, ImageIndex);
	}

	public void AutoListAdd(string text, string InsertText, int ImageIndex)
	{
		((EditViewControl)_ActiveView).AutoList.Add(text, InsertText, ImageIndex);
	}

	public void AutoListAdd(string text, string InsertText, string ToolTip, int ImageIndex)
	{
		((EditViewControl)_ActiveView).AutoList.Add(text, InsertText, ToolTip, ImageIndex);
	}

	public TextPoint CharFromPixel(int x, int y)
	{
		return ((EditViewControl)_ActiveView).CharFromPixel(x, y);
	}

	public void ClearSelection()
	{
		((EditViewControl)_ActiveView).ClearSelection();
	}

	public void Copy()
	{
		((EditViewControl)_ActiveView).Copy();
	}

	public void Cut()
	{
		((EditViewControl)_ActiveView).Cut();
	}

	public void Delete()
	{
		((EditViewControl)_ActiveView).Delete();
	}

	public void GotoLine(int RowIndex)
	{
		((EditViewControl)_ActiveView).GotoLine(RowIndex);
	}

	public void GotoNextBookmark()
	{
		((EditViewControl)_ActiveView).GotoNextBookmark();
	}

	public void GotoPreviousBookmark()
	{
		((EditViewControl)_ActiveView).GotoPreviousBookmark();
	}

	public bool IsOverSelection(int x, int y)
	{
		return ((EditViewControl)_ActiveView).IsOverSelection(x, y);
	}

	public void Paste()
	{
		((EditViewControl)_ActiveView).Paste();
	}

	public void Redo()
	{
		((EditViewControl)_ActiveView).Redo();
	}

	public void ScrollIntoView()
	{
		((EditViewControl)_ActiveView).ScrollIntoView();
	}

	public void ScrollIntoView(TextPoint Pos)
	{
		((EditViewControl)_ActiveView).ScrollIntoView(Pos);
	}

	public void SelectAll()
	{
		((EditViewControl)_ActiveView).SelectAll();
	}

	public void FindNext(string Pattern, bool MatchCase, bool WholeWords, bool UseRegEx)
	{
		((EditViewControl)_ActiveView).SelectNext(Pattern, MatchCase, WholeWords, UseRegEx);
	}

	public void FindNext()
	{
		((EditViewControl)_ActiveView).FindNext();
	}

	public void ShowGotoLine()
	{
		((EditViewControl)_ActiveView).ShowGotoLine();
	}

	public void ShowSettings()
	{
		((EditViewControl)_ActiveView).ShowSettings();
	}

	public void ToggleBookmark()
	{
		((EditViewControl)_ActiveView).ToggleBookmark();
	}

	public void Undo()
	{
		((EditViewControl)_ActiveView).Undo();
	}

	public void ShowFind()
	{
		((EditViewControl)_ActiveView).ShowFind();
	}

	public void ShowReplace()
	{
		((EditViewControl)_ActiveView).ShowReplace();
	}

	public void Save(string filename)
	{
		string value = Document.Text;
		StreamWriter streamWriter = new StreamWriter(filename);
		streamWriter.Write(value);
		streamWriter.Flush();
		streamWriter.Close();
	}

	public void Open(string filename)
	{
		if (Document == null)
		{
			throw new NullReferenceException("CodeEditorControl.Document");
		}
		StreamReader streamReader = new StreamReader(filename);
		Document.Text = streamReader.ReadToEnd();
		streamReader.Close();
	}

	public void AttachDocument(SyntaxDocument document)
	{
		if (_Document != null)
		{
			_Document.ParsingCompleted -= OnParsingCompleted;
			_Document.Parsing -= OnParse;
			_Document.Change -= OnChange;
		}
		if (document == null)
		{
			document = new SyntaxDocument();
		}
		_Document = document;
		if (_Document != null)
		{
			_Document.ParsingCompleted += OnParsingCompleted;
			_Document.Parsing += OnParse;
			_Document.Change += OnChange;
		}
		Redraw();
	}

	protected virtual void OnParse(object Sender, EventArgs e)
	{
		foreach (EditViewControl view in base.Views)
		{
			view.OnParse();
		}
	}

	protected virtual void OnParsingCompleted(object Sender, EventArgs e)
	{
		foreach (EditViewControl view in base.Views)
		{
			view.Invalidate();
		}
	}

	protected virtual void OnChange(object Sender, EventArgs e)
	{
		if (base.Views == null)
		{
			return;
		}
		foreach (EditViewControl view in base.Views)
		{
			view.OnChange();
		}
		OnTextChanged(EventArgs.Empty);
	}

	public void RemoveCurrentRow()
	{
		((EditViewControl)_ActiveView).RemoveCurrentRow();
	}

	public void CutClear()
	{
		((EditViewControl)_ActiveView).CutClear();
	}

	public void AutoListInsertSelectedText()
	{
		((EditViewControl)_ActiveView).InsertAutolistText();
	}

	protected override SplitViewChildControl GetNewView()
	{
		return new EditViewControl(this);
	}

	protected override void OnImeModeChanged(EventArgs e)
	{
		base.OnImeModeChanged(e);
		foreach (EditViewControl view in base.Views)
		{
			view.ImeMode = base.ImeMode;
		}
	}

	public SyntaxBoxControl()
	{
		try
		{
			Document = new SyntaxDocument();
			CreateViews();
			InitializeComponent();
			SetStyle(ControlStyles.Selectable, value: true);
			KeyboardActions.Add(new KeyboardAction(Keys.Z, shift: false, control: true, alt: false, allowreadonly: false, Undo));
			KeyboardActions.Add(new KeyboardAction(Keys.Y, shift: false, control: true, alt: false, allowreadonly: false, Redo));
			KeyboardActions.Add(new KeyboardAction(Keys.F3, shift: false, control: false, alt: false, allowreadonly: true, FindNext));
			KeyboardActions.Add(new KeyboardAction(Keys.C, shift: false, control: true, alt: false, allowreadonly: true, Copy));
			KeyboardActions.Add(new KeyboardAction(Keys.X, shift: false, control: true, alt: false, allowreadonly: false, CutClear));
			KeyboardActions.Add(new KeyboardAction(Keys.V, shift: false, control: true, alt: false, allowreadonly: false, Paste));
			KeyboardActions.Add(new KeyboardAction(Keys.Insert, shift: false, control: true, alt: false, allowreadonly: true, Copy));
			KeyboardActions.Add(new KeyboardAction(Keys.Delete, shift: true, control: false, alt: false, allowreadonly: false, Cut));
			KeyboardActions.Add(new KeyboardAction(Keys.Insert, shift: true, control: false, alt: false, allowreadonly: false, Paste));
			KeyboardActions.Add(new KeyboardAction(Keys.A, shift: false, control: true, alt: false, allowreadonly: true, SelectAll));
			KeyboardActions.Add(new KeyboardAction(Keys.F, shift: false, control: true, alt: false, allowreadonly: false, ShowFind));
			KeyboardActions.Add(new KeyboardAction(Keys.H, shift: false, control: true, alt: false, allowreadonly: false, ShowReplace));
			KeyboardActions.Add(new KeyboardAction(Keys.G, shift: false, control: true, alt: false, allowreadonly: true, ShowGotoLine));
			KeyboardActions.Add(new KeyboardAction(Keys.T, shift: false, control: true, alt: false, allowreadonly: false, ShowSettings));
			KeyboardActions.Add(new KeyboardAction(Keys.F2, shift: false, control: true, alt: false, allowreadonly: true, ToggleBookmark));
			KeyboardActions.Add(new KeyboardAction(Keys.F2, shift: false, control: false, alt: false, allowreadonly: true, GotoNextBookmark));
			KeyboardActions.Add(new KeyboardAction(Keys.F2, shift: true, control: false, alt: false, allowreadonly: true, GotoPreviousBookmark));
			KeyboardActions.Add(new KeyboardAction(Keys.Escape, shift: false, control: false, alt: false, allowreadonly: true, ClearSelection));
			KeyboardActions.Add(new KeyboardAction(Keys.Tab, shift: false, control: false, alt: false, allowreadonly: false, Selection.Indent));
			KeyboardActions.Add(new KeyboardAction(Keys.Tab, shift: true, control: false, alt: false, allowreadonly: false, Selection.Outdent));
			AutoListIcons = _AutoListIcons;
		}
		catch
		{
		}
	}

	protected virtual void OnClipboardUpdated(CopyEventArgs e)
	{
		if (ClipboardUpdated != null)
		{
			ClipboardUpdated(this, e);
		}
	}

	protected virtual void OnRowMouseDown(RowMouseEventArgs e)
	{
		if (RowMouseDown != null)
		{
			RowMouseDown(this, e);
		}
	}

	protected virtual void OnRowMouseMove(RowMouseEventArgs e)
	{
		if (RowMouseMove != null)
		{
			RowMouseMove(this, e);
		}
	}

	protected virtual void OnRowMouseUp(RowMouseEventArgs e)
	{
		if (RowMouseUp != null)
		{
			RowMouseUp(this, e);
		}
	}

	protected virtual void OnRowClick(RowMouseEventArgs e)
	{
		if (RowClick != null)
		{
			RowClick(this, e);
		}
	}

	protected virtual void OnRowDoubleClick(RowMouseEventArgs e)
	{
		if (RowDoubleClick != null)
		{
			RowDoubleClick(this, e);
		}
	}

	private void ParseTimer_Tick(object sender, EventArgs e)
	{
		Document.ParseSome();
	}

	protected virtual void OnInfoTipSelectedIndexChanged()
	{
		if (InfoTipSelectedIndexChanged != null)
		{
			InfoTipSelectedIndexChanged(null, null);
		}
	}

	protected override void OnEnter(EventArgs e)
	{
		base.OnEnter(e);
		if (_ActiveView != null)
		{
			_ActiveView.Focus();
		}
	}

	private void View_RowClick(object sender, RowMouseEventArgs e)
	{
		OnRowClick(e);
	}

	private void View_RowDoubleClick(object sender, RowMouseEventArgs e)
	{
		OnRowDoubleClick(e);
	}

	private void View_RowMouseDown(object sender, RowMouseEventArgs e)
	{
		OnRowMouseDown(e);
	}

	private void View_RowMouseMove(object sender, RowMouseEventArgs e)
	{
		OnRowMouseMove(e);
	}

	private void View_RowMouseUp(object sender, RowMouseEventArgs e)
	{
		OnRowMouseUp(e);
	}

	private void View_ClipboardUpdated(object sender, CopyEventArgs e)
	{
		OnClipboardUpdated(e);
	}

	public void OnRenderRow(RowPaintEventArgs e)
	{
		if (RenderRow != null)
		{
			RenderRow(this, e);
		}
	}

	public void OnWordMouseHover(ref WordMouseEventArgs e)
	{
		if (WordMouseHover != null)
		{
			WordMouseHover(this, ref e);
		}
	}

	public void OnWordMouseDown(ref WordMouseEventArgs e)
	{
		if (WordMouseDown != null)
		{
			WordMouseDown(this, ref e);
		}
	}

	protected virtual void OnCaretChange(object sender)
	{
		if (CaretChange != null)
		{
			CaretChange(this, null);
		}
	}

	protected virtual void OnSelectionChange(object sender)
	{
		if (SelectionChange != null)
		{
			SelectionChange(this, null);
		}
	}

	private void View_CaretChanged(object s, EventArgs e)
	{
		OnCaretChange(s);
	}

	private void View_SelectionChanged(object s, EventArgs e)
	{
		OnSelectionChange(s);
	}

	private void View_DoubleClick(object sender, EventArgs e)
	{
		OnDoubleClick(e);
	}

	private void View_MouseUp(object sender, MouseEventArgs e)
	{
		EditViewControl editViewControl = (EditViewControl)sender;
		MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, e.X + editViewControl.Location.X + editViewControl.BorderWidth, e.Y + editViewControl.Location.Y + editViewControl.BorderWidth, e.Delta);
		OnMouseUp(e2);
	}

	private void View_MouseMove(object sender, MouseEventArgs e)
	{
		EditViewControl editViewControl = (EditViewControl)sender;
		MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, e.X + editViewControl.Location.X + editViewControl.BorderWidth, e.Y + editViewControl.Location.Y + editViewControl.BorderWidth, e.Delta);
		OnMouseMove(e2);
	}

	private void View_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void View_MouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void View_MouseEnter(object sender, EventArgs e)
	{
		OnMouseEnter(e);
	}

	private void View_MouseDown(object sender, MouseEventArgs e)
	{
		EditViewControl editViewControl = (EditViewControl)sender;
		MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, e.X + editViewControl.Location.X + editViewControl.BorderWidth, e.Y + editViewControl.Location.Y + editViewControl.BorderWidth, e.Delta);
		OnMouseDown(e2);
	}

	private void View_KeyUp(object sender, KeyEventArgs e)
	{
		OnKeyUp(e);
	}

	private void View_KeyPress(object sender, KeyPressEventArgs e)
	{
		OnKeyPress(e);
	}

	private void View_KeyDown(object sender, KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	private void View_Click(object sender, EventArgs e)
	{
		OnClick(e);
	}

	private void View_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void View_DragLeave(object sender, EventArgs e)
	{
		OnDragLeave(e);
	}

	private void View_DragEnter(object sender, DragEventArgs e)
	{
		OnDragEnter(e);
	}

	private void View_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void View_InfoTipSelectedIndexChanged(object sender, EventArgs e)
	{
		OnInfoTipSelectedIndexChanged();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Alsing.Windows.Forms.SyntaxBoxControl));
		this._GutterIcons = new System.Windows.Forms.ImageList(this.components);
		this._AutoListIcons = new System.Windows.Forms.ImageList(this.components);
		this.ParseTimer = new System.Windows.Forms.Timer(this.components);
		this._GutterIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
		this._GutterIcons.ImageSize = new System.Drawing.Size(17, 17);
		this._GutterIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resourceManager.GetObject("_GutterIcons.ImageStream");
		this._GutterIcons.TransparentColor = System.Drawing.Color.Transparent;
		this._AutoListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this._AutoListIcons.ImageSize = new System.Drawing.Size(16, 16);
		this._AutoListIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resourceManager.GetObject("_AutoListIcons.ImageStream");
		this._AutoListIcons.TransparentColor = System.Drawing.Color.Transparent;
		this.ParseTimer.Enabled = true;
		this.ParseTimer.Interval = 1;
		this.ParseTimer.Tick += new System.EventHandler(ParseTimer_Tick);
	}

	protected override void OnLoad(EventArgs e)
	{
		Refresh();
	}

	private void Redraw()
	{
		if (base.Views == null)
		{
			return;
		}
		foreach (EditViewControl view in base.Views)
		{
			view?.Refresh();
		}
	}

	private void InitGraphics()
	{
		if (base.Views == null || base.Parent == null)
		{
			return;
		}
		foreach (EditViewControl view in base.Views)
		{
			view.InitGraphics();
		}
	}

	protected override void CreateViews()
	{
		base.CreateViews();
		foreach (EditViewControl view in base.Views)
		{
			if (!DoOnce || view != LowerRight)
			{
				view.Enter += base.View_Enter;
				view.Leave += base.View_Leave;
				view.GotFocus += base.View_Enter;
				view.LostFocus += base.View_Leave;
				view.CaretChange += View_CaretChanged;
				view.SelectionChange += View_SelectionChanged;
				view.Click += View_Click;
				view.DoubleClick += View_DoubleClick;
				view.MouseDown += View_MouseDown;
				view.MouseEnter += View_MouseEnter;
				view.MouseHover += View_MouseHover;
				view.MouseLeave += View_MouseLeave;
				view.MouseMove += View_MouseMove;
				view.MouseUp += View_MouseUp;
				view.KeyDown += View_KeyDown;
				view.KeyPress += View_KeyPress;
				view.KeyUp += View_KeyUp;
				view.DragDrop += View_DragDrop;
				view.DragOver += View_DragOver;
				view.DragLeave += View_DragLeave;
				view.DragEnter += View_DragEnter;
				if (view.InfoTip != null)
				{
					view.InfoTip.Data = "";
					view.InfoTip.SelectedIndexChanged += View_InfoTipSelectedIndexChanged;
				}
				view.RowClick += View_RowClick;
				view.RowDoubleClick += View_RowDoubleClick;
				view.RowMouseDown += View_RowMouseDown;
				view.RowMouseMove += View_RowMouseMove;
				view.RowMouseUp += View_RowMouseUp;
				view.ClipboardUpdated += View_ClipboardUpdated;
			}
		}
		DoOnce = true;
		AutoListIcons = AutoListIcons;
		InfoTipImage = InfoTipImage;
		base.ChildBorderStyle = base.ChildBorderStyle;
		base.ChildBorderColor = base.ChildBorderColor;
		BackColor = BackColor;
		Document = Document;
		base.ImeMode = base.ImeMode;
		Redraw();
	}
}
