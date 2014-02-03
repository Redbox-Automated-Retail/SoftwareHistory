using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using Alsing.Globalization;
using Alsing.SourceCode;
using Alsing.Windows.Forms.CoreLib;
using Alsing.Windows.Forms.IntelliMouse;
using Alsing.Windows.Forms.SyntaxBox.Painter;
using Properties;

namespace Alsing.Windows.Forms.SyntaxBox;

[ToolboxItem(false)]
public class EditViewControl : SplitViewChildControl
{
	private readonly Caret _Caret;

	private readonly Selection _Selection;

	private bool _AutoListVisible;

	private bool _InfoTipVisible;

	private double _IntelliScrollPos;

	private bool _KeyDownHandled;

	private bool _OverWrite;

	public TextPoint AutoListStartPos;

	public TextPoint InfoTipStartPos;

	private int MouseX;

	private int MouseY;

	public IPainter Painter;

	public ViewPoint View;

	private WeakReference _Control;

	private Timer CaretTimer;

	private IContainer components;

	private PictureBox Filler;

	private IntelliMouseControl IntelliMouse;

	private ToolTip tooltip;

	private AutoListForm _AutoList;

	private InfoTipForm _InfoTip;

	private FindReplaceForm _FindReplaceDialog;

	private int MaxCharWidth = 8;

	private int OldWidth;

	public AutoListForm AutoList
	{
		get
		{
			CreateAutoList();
			return _AutoList;
		}
		set
		{
			_AutoList = value;
		}
	}

	public InfoTipForm InfoTip
	{
		get
		{
			CreateInfoTip();
			return _InfoTip;
		}
		set
		{
			_InfoTip = value;
		}
	}

	public IMEWindow IMEWindow { get; set; }

	public FindReplaceForm FindReplaceDialog
	{
		get
		{
			CreateFindForm();
			return _FindReplaceDialog;
		}
		set
		{
			_FindReplaceDialog = value;
		}
	}

	public bool HasAutoList => _AutoList != null;

	public bool HasInfoTip => _InfoTip != null;

	public SyntaxBoxControl _SyntaxBox
	{
		get
		{
			try
			{
				if (_Control != null && _Control.IsAlive)
				{
					return (SyntaxBoxControl)_Control.Target;
				}
				return null;
			}
			catch
			{
				return null;
			}
		}
		set
		{
			_Control = new WeakReference(value);
		}
	}

	public int PixelTabSize => _SyntaxBox.TabSize * View.CharWidth;

	[Browsable(false)]
	public bool OverWrite => _OverWrite;

	[Browsable(false)]
	public bool CanCopy => Selection.IsValid;

	[Browsable(false)]
	public bool CanPaste
	{
		get
		{
			string text = "";
			try
			{
				IDataObject dataObject = Clipboard.GetDataObject();
				if (dataObject != null && dataObject.GetDataPresent(DataFormats.Text))
				{
					text = (string)dataObject.GetData(DataFormats.Text);
				}
				if (text != null)
				{
					return true;
				}
			}
			catch
			{
			}
			return false;
		}
	}

	[Browsable(false)]
	public bool CanUndo => Document.UndoStep > 0;

	[Browsable(false)]
	public bool CanRedo => Document.UndoStep < Document.UndoBuffer.Count - 1;

	public float FontSize => _SyntaxBox.FontSize;

	public IndentStyle Indent => _SyntaxBox.Indent;

	[Category("Content")]
	[Description("The SyntaxDocument that is attatched to the contro")]
	public SyntaxDocument Document => _SyntaxBox.Document;

	public int TooltipDelay => _SyntaxBox.TooltipDelay;

	public bool CollapsedBlockTooltipsEnabled => _SyntaxBox.CollapsedBlockTooltipsEnabled;

	public bool ReadOnly => _SyntaxBox.ReadOnly;

	public string FontName => _SyntaxBox.FontName;

	public bool BracketMatching => _SyntaxBox.BracketMatching;

	public bool VirtualWhitespace => _SyntaxBox.VirtualWhitespace;

	public Color SeparatorColor => _SyntaxBox.SeparatorColor;

	public Color BracketForeColor => _SyntaxBox.BracketForeColor;

	public Color BracketBackColor => _SyntaxBox.BracketBackColor;

	public Color SelectionBackColor => _SyntaxBox.SelectionBackColor;

	public Color SelectionForeColor => _SyntaxBox.SelectionForeColor;

	public Color InactiveSelectionBackColor => _SyntaxBox.InactiveSelectionBackColor;

	public Color InactiveSelectionForeColor => _SyntaxBox.InactiveSelectionForeColor;

	public Color GutterMarginBorderColor => _SyntaxBox.GutterMarginBorderColor;

	public Color LineNumberBorderColor => _SyntaxBox.LineNumberBorderColor;

	public Color BreakPointForeColor => _SyntaxBox.BreakPointForeColor;

	public Color BreakPointBackColor => _SyntaxBox.BreakPointBackColor;

	public Color LineNumberForeColor => _SyntaxBox.LineNumberForeColor;

	public Color LineNumberBackColor => _SyntaxBox.LineNumberBackColor;

	public Color GutterMarginColor => _SyntaxBox.GutterMarginColor;

	[Description("The background color of the client area")]
	[Category("Appearance")]
	public new Color BackColor
	{
		get
		{
			return _SyntaxBox.BackColor;
		}
		set
		{
			_SyntaxBox.BackColor = value;
		}
	}

	public Color HighLightedLineColor => _SyntaxBox.HighLightedLineColor;

	public bool HighLightActiveLine => _SyntaxBox.HighLightActiveLine;

	public bool ShowWhitespace => _SyntaxBox.ShowWhitespace;

	public bool ShowLineNumbers => _SyntaxBox.ShowLineNumbers;

	public bool ShowGutterMargin => _SyntaxBox.ShowGutterMargin;

	public int GutterMarginWidth => _SyntaxBox.GutterMarginWidth;

	public int TabSize => _SyntaxBox.TabSize;

	public bool TabsToSpaces => _SyntaxBox.TabsToSpaces;

	public bool ShowTabGuides => _SyntaxBox.ShowTabGuides;

	public Color WhitespaceColor => _SyntaxBox.WhitespaceColor;

	public Color TabGuideColor => _SyntaxBox.TabGuideColor;

	public Color BracketBorderColor => _SyntaxBox.BracketBorderColor;

	public Color OutlineColor => _SyntaxBox.OutlineColor;

	[Category("Behavior")]
	public TextPoint AutoListPosition
	{
		get
		{
			return AutoListStartPos;
		}
		set
		{
			AutoListStartPos = value;
		}
	}

	[Category("Behavior")]
	public TextPoint InfoTipPosition
	{
		get
		{
			return InfoTipStartPos;
		}
		set
		{
			InfoTipStartPos = value;
		}
	}

	[Category("Behavior")]
	public bool AutoListVisible
	{
		get
		{
			return _AutoListVisible;
		}
		set
		{
			CreateAutoList();
			if (AutoList == null)
			{
				return;
			}
			if (value)
			{
				AutoList.TopLevel = true;
				AutoList.BringToFront();
				AutoList.Location = new Point(-16000, -16000);
				AutoList.Show();
				InfoTip.BringToFront();
				if (base.TopLevelControl is Form)
				{
					AutoList.Owner = (Form)base.TopLevelControl;
				}
			}
			else
			{
				AutoList.Hide();
			}
			_AutoListVisible = value;
			InfoTip.BringToFront();
			Redraw();
		}
	}

	[Category("Behavior")]
	public bool InfoTipVisible
	{
		get
		{
			return _InfoTipVisible;
		}
		set
		{
			CreateInfoTip();
			if (InfoTip == null)
			{
				return;
			}
			if (value)
			{
				InfoTip.TopLevel = true;
				AutoList.BringToFront();
				if (base.TopLevelControl is Form)
				{
					InfoTip.Owner = (Form)base.TopLevelControl;
				}
			}
			InfoTip.BringToFront();
			_InfoTipVisible = value;
			if (InfoTip != null && value)
			{
				InfoTip.Init();
			}
			if (_InfoTip != null && !value)
			{
				_InfoTip.Visible = false;
			}
		}
	}

	public bool SmoothScroll => _SyntaxBox.SmoothScroll;

	public int SmoothScrollSpeed => _SyntaxBox.SmoothScrollSpeed;

	public bool ParseOnPaste => _SyntaxBox.ParseOnPaste;

	public Caret Caret => _Caret;

	public Selection Selection => _Selection;

	public event EventHandler CaretChange;

	public event EventHandler SelectionChange;

	public event RowMouseHandler RowMouseDown;

	public event RowMouseHandler RowMouseMove;

	public event RowMouseHandler RowMouseUp;

	public event RowMouseHandler RowClick;

	public event RowMouseHandler RowDoubleClick;

	public event CopyHandler ClipboardUpdated;

	private void CreateAutoList()
	{
		if (_SyntaxBox != null && !_SyntaxBox.DisableAutoList && _AutoList == null)
		{
			AutoList = new AutoListForm();
			NativeMethods.SetWindowLong(AutoList.Handle, -16, 1073741824);
			AutoList.SendToBack();
			AutoList.Visible = false;
			AutoList.DoubleClick += AutoListDoubleClick;
			AutoList.Images = _SyntaxBox.AutoListIcons;
			AutoList.Add("a123", "a123", "Some tooltip for this item 1", 1);
			AutoList.Add("b456", "b456", "Some tooltip for this item 2", 2);
			AutoList.Add("c789", "c789", "Some tooltip for this item 3", 2);
			AutoList.Add("d012", "d012", "Some tooltip for this item 4", 3);
			AutoList.Add("e345", "e345", "Some tooltip for this item 5", 4);
		}
	}

	private void CreateFindForm()
	{
		if (!_SyntaxBox.DisableFindForm && _FindReplaceDialog == null)
		{
			FindReplaceDialog = new FindReplaceForm(this);
		}
	}

	private void CreateInfoTip()
	{
		if (_SyntaxBox != null && !_SyntaxBox.DisableInfoTip && _InfoTip == null)
		{
			InfoTip = new InfoTipForm(this);
			NativeMethods.SetWindowLong(InfoTip.Handle, -16, 1073741824);
			InfoTip.SendToBack();
			InfoTip.Visible = false;
		}
	}

	private void IntelliMouse_BeginScroll(object sender, EventArgs e)
	{
		_IntelliScrollPos = 0.0;
		View.YOffset = 0;
	}

	private void IntelliMouse_EndScroll(object sender, EventArgs e)
	{
		View.YOffset = 0;
		Redraw();
	}

	private void IntelliMouse_Scroll(object sender, Alsing.Windows.Forms.IntelliMouse.ScrollEventArgs e)
	{
		if (e.DeltaY < 0 && vScroll.Value == 0)
		{
			View.YOffset = 0;
			Redraw();
			return;
		}
		if (e.DeltaY > 0 && vScroll.Value >= vScroll.Maximum - View.VisibleRowCount + 1)
		{
			View.YOffset = 0;
			Redraw();
			return;
		}
		_IntelliScrollPos += (double)e.DeltaY / 8.0;
		int num = (int)_IntelliScrollPos / View.RowHeight;
		if (num != 0)
		{
			_IntelliScrollPos -= num * View.RowHeight;
		}
		View.YOffset = -(int)_IntelliScrollPos;
		ScrollScreen(num);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 2)
		{
			try
			{
				if (FindReplaceDialog != null)
				{
					FindReplaceDialog.Close();
				}
				if (AutoList != null)
				{
					AutoList.Close();
				}
				if (InfoTip != null)
				{
					InfoTip.Close();
				}
			}
			catch
			{
			}
		}
		base.WndProc(ref m);
	}

	protected void CopyAsRTF()
	{
		TextStyle[] styles = Document.Parser.SyntaxDefinition.Styles;
		Document.ParseAll(ParseKeywords: true);
		int firstRow = Selection.LogicalBounds.FirstRow;
		int lastRow = Selection.LogicalBounds.LastRow;
		int firstColumn = Selection.LogicalBounds.FirstColumn;
		int lastColumn = Selection.LogicalBounds.LastColumn;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang1053{\\fonttbl{\\f0\\fmodern\\fprq1\\fcharset0 " + FontName + ";}}");
		stringBuilder.Append("{\\colortbl ;");
		TextStyle[] array = styles;
		foreach (TextStyle textStyle in array)
		{
			stringBuilder.AppendFormat("\\red{0}\\green{1}\\blue{2};", textStyle.ForeColor.R, textStyle.ForeColor.G, textStyle.ForeColor.B);
			stringBuilder.AppendFormat("\\red{0}\\green{1}\\blue{2};", textStyle.BackColor.R, textStyle.BackColor.G, textStyle.BackColor.B);
		}
		stringBuilder.Append(";}");
		stringBuilder.Append("\\viewkind4\\uc1\\pard\\f0\\fs20");
		bool flag = false;
		for (int j = firstRow; j <= lastRow; j++)
		{
			Row row = Document[j];
			foreach (Word item in row)
			{
				if (j == firstRow && item.Column + item.Text.Length < firstColumn)
				{
					continue;
				}
				bool flag2 = j == firstRow && item.Column <= firstColumn && item.Column + item.Text.Length > firstColumn;
				bool flag3 = j == lastRow && item.Column < lastColumn && item.Column + item.Text.Length > lastColumn;
				if (item.Type == WordType.Word && item.Style != null)
				{
					int num = Array.IndexOf(styles, item.Style);
					num *= 2;
					num++;
					stringBuilder.Append("{\\cf" + num.ToString(CultureInfo.InvariantCulture));
					if (!item.Style.Transparent)
					{
						stringBuilder.Append("\\highlight" + (num + 1).ToString(CultureInfo.InvariantCulture));
					}
					stringBuilder.Append(" ");
				}
				if (item.Style != null)
				{
					if (item.Style.Bold)
					{
						stringBuilder.Append("\\b ");
					}
					if (item.Style.Underline)
					{
						stringBuilder.Append("\\ul ");
					}
					if (item.Style.Italic)
					{
						stringBuilder.Append("\\i ");
					}
				}
				string text = item.Text;
				if (flag3)
				{
					text = text.Substring(0, lastColumn - item.Column);
				}
				if (flag2)
				{
					text = text.Substring(firstColumn - item.Column);
				}
				text = text.Replace("\\", " \\ \\ ").Replace("\r\n        }\r\n        ", " \\\r\n      }\r\n      ").Replace("\r\n      {\r\n        ", " \\\r\n        {\r\n          ");
				stringBuilder.Append(text);
				if (item.Style != null)
				{
					if (item.Style.Bold)
					{
						stringBuilder.Append("\\b0 ");
					}
					if (item.Style.Underline)
					{
						stringBuilder.Append("\\ul0 ");
					}
					if (item.Style.Italic)
					{
						stringBuilder.Append("\\i0 ");
					}
				}
				if (item.Type == WordType.Word && item.Style != null)
				{
					stringBuilder.Append("}");
				}
				if (flag3)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			stringBuilder.Append("\\par");
		}
		DataObject dataObject = new DataObject();
		dataObject.SetData(DataFormats.Rtf, stringBuilder.ToString());
		string data = Selection.Text;
		dataObject.SetData(DataFormats.Text, data);
		Clipboard.SetDataObject(dataObject);
		CopyEventArgs e = new CopyEventArgs();
		e.Text = data;
		CopyEventArgs e2 = e;
		OnClipboardUpdated(e2);
	}

	public EditViewControl(SyntaxBoxControl Parent)
	{
		_SyntaxBox = Parent;
		Painter = new NativePainter(this);
		_Selection = new Selection(this);
		_Caret = new Caret(this);
		_Caret.Change += CaretChanged;
		_Selection.Change += SelectionChanged;
		InitializeComponent();
		CreateAutoList();
		CreateInfoTip();
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: false);
		SetStyle(ControlStyles.DoubleBuffer, value: false);
		SetStyle(ControlStyles.Selectable, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.Opaque, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		UpdateStyles();
	}

	protected override void Dispose(bool disposing)
	{
		RemoveFocus();
		if (disposing)
		{
			if (components != null)
			{
				components.Dispose();
			}
			try
			{
				if (Painter != null)
				{
					Painter.Dispose();
				}
			}
			catch
			{
			}
		}
		base.Dispose(disposing);
	}

	private void DoResize()
	{
		if (!base.Visible || base.Width <= 0 || base.Height <= 0 || !base.IsHandleCreated)
		{
			return;
		}
		try
		{
			if (Filler == null)
			{
				return;
			}
			TopThumb.Width = SystemInformation.VerticalScrollBarWidth;
			LeftThumb.Height = SystemInformation.HorizontalScrollBarHeight;
			vScroll.Width = SystemInformation.VerticalScrollBarWidth;
			hScroll.Height = SystemInformation.HorizontalScrollBarHeight;
			if (base.TopThumbVisible)
			{
				vScroll.Top = TopThumb.Height;
				if (hScroll.Visible)
				{
					vScroll.Height = base.ClientHeight - hScroll.Height - TopThumb.Height;
				}
				else
				{
					vScroll.Height = base.ClientHeight - TopThumb.Height;
				}
			}
			else
			{
				if (hScroll.Visible)
				{
					vScroll.Height = base.ClientHeight - hScroll.Height;
				}
				else
				{
					vScroll.Height = base.ClientHeight;
				}
				vScroll.Top = 0;
			}
			if (base.LeftThumbVisible)
			{
				hScroll.Left = LeftThumb.Width;
				if (vScroll.Visible)
				{
					hScroll.Width = base.ClientWidth - vScroll.Width - LeftThumb.Width;
				}
				else
				{
					hScroll.Width = base.ClientWidth - LeftThumb.Width;
				}
			}
			else
			{
				if (vScroll.Visible)
				{
					hScroll.Width = base.ClientWidth - vScroll.Width;
				}
				else
				{
					hScroll.Width = base.ClientWidth;
				}
				hScroll.Left = 0;
			}
			if (base.Width != OldWidth && base.Width > 0)
			{
				OldWidth = base.Width;
				if (Painter != null)
				{
					Painter.Resize();
				}
			}
			vScroll.Left = base.ClientWidth - vScroll.Width;
			hScroll.Top = base.ClientHeight - hScroll.Height;
			LeftThumb.Left = 0;
			LeftThumb.Top = hScroll.Top;
			TopThumb.Left = vScroll.Left;
			TopThumb.Top = 0;
			Filler.Left = vScroll.Left;
			Filler.Top = hScroll.Top;
			Filler.Width = vScroll.Width;
			Filler.Height = hScroll.Height;
		}
		catch
		{
		}
	}

	private void InsertText(string text)
	{
		Caret.CropPosition();
		if (Selection.IsValid)
		{
			Selection.DeleteSelection();
			InsertText(text);
			return;
		}
		if (!_OverWrite || text.Length > 1)
		{
			TextPoint pos = Document.InsertText(text, Caret.Position.X, Caret.Position.Y);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			if (text.Length == 1)
			{
				Caret.SetPos(pos);
				Caret.CaretMoved(Select: false);
			}
			else
			{
				Document.ResetVisibleRows();
				Caret.SetPos(pos);
				Caret.CaretMoved(Select: false);
			}
			return;
		}
		TextRange textRange = new TextRange();
		textRange.FirstColumn = Caret.Position.X;
		textRange.FirstRow = Caret.Position.Y;
		textRange.LastColumn = Caret.Position.X + 1;
		textRange.LastRow = Caret.Position.Y;
		TextRange range = textRange;
		UndoBlockCollection undoBlockCollection = new UndoBlockCollection();
		UndoBlock undoBlock = new UndoBlock();
		undoBlock.Action = UndoAction.DeleteRange;
		undoBlock.Text = Document.GetRange(range);
		undoBlock.Position = Caret.Position;
		UndoBlock item = undoBlock;
		undoBlockCollection.Add(item);
		Document.DeleteRange(range, StoreUndo: false);
		UndoBlock undoBlock2 = new UndoBlock();
		undoBlock2.Action = UndoAction.InsertRange;
		item = undoBlock2;
		string text2 = (item.Text = text);
		item.Position = Caret.Position;
		undoBlockCollection.Add(item);
		Document.AddToUndoList(undoBlockCollection);
		Document.InsertText(text2, Caret.Position.X, Caret.Position.Y, StoreUndo: false);
		Caret.CurrentRow.Parse(ParseKeywords: true);
		Caret.MoveRight(Select: false);
	}

	private void InsertEnter()
	{
		Caret.CropPosition();
		if (Selection.IsValid)
		{
			Selection.DeleteSelection();
			InsertEnter();
			return;
		}
		switch (Indent)
		{
		case IndentStyle.None:
			Document.InsertText("\n", Caret.Position.X, Caret.Position.Y);
			Caret.CurrentRow.Parse();
			Caret.MoveDown(Select: false);
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Caret.Position.X = 0;
			Caret.SetPos(Caret.Position);
			break;
		case IndentStyle.LastRow:
		{
			Row currentRow2 = Caret.CurrentRow;
			string leadingWhitespace = currentRow2.GetLeadingWhitespace();
			int length = Math.Min(leadingWhitespace.Length, Caret.Position.X);
			string text4 = "\n" + leadingWhitespace.Substring(0, length);
			Document.InsertText(text4, Caret.Position.X, Caret.Position.Y);
			Document.ResetVisibleRows();
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Caret.MoveDown(Select: false);
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Caret.Position.X = leadingWhitespace.Length;
			Caret.SetPos(Caret.Position);
			currentRow2.Parse(ParseKeywords: false);
			currentRow2.Parse(ParseKeywords: true);
			currentRow2.NextRow.Parse(ParseKeywords: false);
			currentRow2.NextRow.Parse(ParseKeywords: true);
			break;
		}
		case IndentStyle.Scope:
		{
			Row currentRow3 = Caret.CurrentRow;
			currentRow3.Parse(ParseKeywords: true);
			if (currentRow3.ShouldOutdent)
			{
				OutdentEndRow();
			}
			Document.InsertText("\n", Caret.Position.X, Caret.Position.Y);
			Caret.CurrentRow.Parse();
			Caret.MoveDown(Select: false);
			Caret.CurrentRow.Parse(ParseKeywords: false);
			string text5 = new string('\t', Caret.CurrentRow.Depth);
			Document.InsertText(text5, 0, Caret.Position.Y);
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Caret.Position.X = text5.Length;
			Caret.SetPos(Caret.Position);
			Caret.CropPosition();
			Selection.ClearSelection();
			currentRow3.Parse(ParseKeywords: false);
			currentRow3.Parse(ParseKeywords: true);
			currentRow3.NextRow.Parse(ParseKeywords: false);
			currentRow3.NextRow.Parse(ParseKeywords: true);
			break;
		}
		case IndentStyle.Smart:
		{
			Row currentRow = Caret.CurrentRow;
			if (currentRow.ShouldOutdent)
			{
				OutdentEndRow();
			}
			Document.InsertText("\n", Caret.Position.X, Caret.Position.Y);
			Caret.MoveDown(Select: false);
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Caret.CurrentRow.startSpan.StartRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.startSpan.StartRow.Parse(ParseKeywords: true);
			string text = "\t" + Caret.CurrentRow.startSpan.StartRow.GetVirtualLeadingWhitespace();
			string text2 = Caret.CurrentRow.PrevRow.GetLeadingWhitespace();
			if (text2.Length < text.Length)
			{
				text2 = text;
			}
			string text3 = "\t" + new string(' ', TabSize);
			while (text2.IndexOf(text3) >= 0)
			{
				text2 = text2.Replace(text3, "\t\t");
			}
			Document.InsertText(text2, 0, Caret.Position.Y);
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Caret.Position.X = text2.Length;
			Caret.SetPos(Caret.Position);
			Caret.CropPosition();
			Selection.ClearSelection();
			currentRow.Parse(ParseKeywords: false);
			currentRow.Parse(ParseKeywords: true);
			currentRow.NextRow.Parse(ParseKeywords: false);
			currentRow.NextRow.Parse(ParseKeywords: true);
			break;
		}
		}
		ScrollIntoView();
	}

	private void OutdentEndRow()
	{
		try
		{
			if (Indent == IndentStyle.Scope)
			{
				Row currentRow = Caret.CurrentRow;
				string text = new string('\t', Caret.CurrentRow.Depth);
				TextRange textRange = new TextRange();
				textRange.FirstColumn = 0;
				textRange.LastColumn = currentRow.GetLeadingWhitespace().Length;
				textRange.FirstRow = currentRow.Index;
				textRange.LastRow = currentRow.Index;
				TextRange textRange2 = textRange;
				Document.DeleteRange(textRange2);
				Document.InsertText(text, 0, currentRow.Index, StoreUndo: true);
				int num = text.Length - textRange2.LastColumn;
				Caret.Position.X += num;
				Caret.SetPos(Caret.Position);
				Caret.CropPosition();
				Selection.ClearSelection();
				Caret.CurrentRow.Parse(ParseKeywords: false);
				Caret.CurrentRow.Parse(ParseKeywords: true);
			}
			else
			{
				if (Indent != IndentStyle.Smart)
				{
					return;
				}
				Row currentRow2 = Caret.CurrentRow;
				if (currentRow2.FirstNonWsWord == currentRow2.expansion_EndSpan.EndWord)
				{
					string text2 = currentRow2.startSpan.StartWord.Row.GetVirtualLeadingWhitespace();
					TextRange textRange3 = new TextRange();
					textRange3.FirstColumn = 0;
					textRange3.LastColumn = currentRow2.GetLeadingWhitespace().Length;
					textRange3.FirstRow = currentRow2.Index;
					textRange3.LastRow = currentRow2.Index;
					TextRange textRange4 = textRange3;
					Document.DeleteRange(textRange4);
					string text3 = "\t" + new string(' ', TabSize);
					while (text2.IndexOf(text3) >= 0)
					{
						text2 = text2.Replace(text3, "\t\t");
					}
					Document.InsertText(text2, 0, currentRow2.Index, StoreUndo: true);
					int num2 = text2.Length - textRange4.LastColumn;
					Caret.Position.X += num2;
					Caret.SetPos(Caret.Position);
					Caret.CropPosition();
					Selection.ClearSelection();
					Caret.CurrentRow.Parse(ParseKeywords: false);
					Caret.CurrentRow.Parse(ParseKeywords: true);
				}
			}
		}
		catch
		{
		}
	}

	private void DeleteForward()
	{
		Caret.CropPosition();
		if (Selection.IsValid)
		{
			Selection.DeleteSelection();
			return;
		}
		Row currentRow = Caret.CurrentRow;
		if (Caret.Position.X == currentRow.Text.Length)
		{
			if (Caret.Position.Y <= Document.Count - 2)
			{
				TextRange textRange = new TextRange();
				textRange.FirstColumn = Caret.Position.X;
				textRange.FirstRow = Caret.Position.Y;
				TextRange textRange2 = textRange;
				textRange2.LastRow = textRange2.FirstRow + 1;
				textRange2.LastColumn = 0;
				Document.DeleteRange(textRange2);
				Document.ResetVisibleRows();
			}
		}
		else
		{
			TextRange textRange3 = new TextRange();
			textRange3.FirstColumn = Caret.Position.X;
			textRange3.FirstRow = Caret.Position.Y;
			TextRange textRange4 = textRange3;
			textRange4.LastRow = textRange4.FirstRow;
			textRange4.LastColumn = textRange4.FirstColumn + 1;
			Document.DeleteRange(textRange4);
			Document.ResetVisibleRows();
			Caret.CurrentRow.Parse(ParseKeywords: false);
			Caret.CurrentRow.Parse(ParseKeywords: true);
		}
	}

	private void DeleteBackwards()
	{
		Caret.CropPosition();
		if (Selection.IsValid)
		{
			Selection.DeleteSelection();
			return;
		}
		Row currentRow = Caret.CurrentRow;
		if (Caret.Position.X == 0)
		{
			if (Caret.Position.Y > 0)
			{
				Caret.Position.Y--;
				Caret.MoveEnd(Select: false);
				DeleteForward();
				Document.ResetVisibleRows();
			}
		}
		else if (Caret.Position.X >= currentRow.Text.Length)
		{
			TextRange textRange = new TextRange();
			textRange.FirstColumn = Caret.Position.X - 1;
			textRange.FirstRow = Caret.Position.Y;
			TextRange textRange2 = textRange;
			textRange2.LastRow = textRange2.FirstRow;
			textRange2.LastColumn = textRange2.FirstColumn + 1;
			Document.DeleteRange(textRange2);
			Document.ResetVisibleRows();
			Caret.MoveEnd(Select: false);
			Caret.CurrentRow.Parse();
		}
		else
		{
			TextRange textRange3 = new TextRange();
			textRange3.FirstColumn = Caret.Position.X - 1;
			textRange3.FirstRow = Caret.Position.Y;
			TextRange textRange4 = textRange3;
			textRange4.LastRow = textRange4.FirstRow;
			textRange4.LastColumn = textRange4.FirstColumn + 1;
			Document.DeleteRange(textRange4);
			Document.ResetVisibleRows();
			Caret.MoveLeft(Select: false);
			Caret.CurrentRow.Parse();
		}
	}

	private void ScrollScreen(int Amount)
	{
		try
		{
			tooltip.RemoveAll();
			int val = vScroll.Value + Amount;
			val = Math.Max(val, vScroll.Minimum);
			val = Math.Min(val, vScroll.Maximum);
			if (val >= vScroll.Maximum - View.VisibleRowCount + 1)
			{
				val = vScroll.Maximum - View.VisibleRowCount + 1;
			}
			vScroll.Value = val;
			Redraw();
		}
		catch
		{
		}
	}

	private void PasteText()
	{
		try
		{
			IDataObject dataObject = Clipboard.GetDataObject();
			if (dataObject == null)
			{
				return;
			}
			if (dataObject.GetDataPresent(DataFormats.UnicodeText))
			{
				string text = (string)dataObject.GetData(DataFormats.UnicodeText);
				InsertText(text);
				if (ParseOnPaste)
				{
					Document.ParseAll(ParseKeywords: true);
				}
			}
			else if (dataObject.GetDataPresent(DataFormats.Text))
			{
				string text2 = (string)dataObject.GetData(DataFormats.Text);
				InsertText(text2);
				if (ParseOnPaste)
				{
					Document.ParseAll(ParseKeywords: true);
				}
			}
		}
		catch
		{
		}
	}

	private void BeginDragDrop()
	{
		DoDragDrop(Selection.Text, DragDropEffects.All);
	}

	private void Redraw()
	{
		Invalidate();
	}

	private void RedrawCaret()
	{
		Graphics graphics = CreateGraphics();
		Painter.RenderCaret(graphics);
		graphics.Dispose();
	}

	private void SetMouseCursor(int x, int y)
	{
		if (_SyntaxBox.LockCursorUpdate)
		{
			Cursor = _SyntaxBox.Cursor;
		}
		else if (View.Action == EditAction.DragText)
		{
			Cursor = Cursors.Hand;
		}
		else if (x < View.TotalMarginWidth)
		{
			if (x < View.GutterMarginWidth)
			{
				Cursor = Cursors.Arrow;
				return;
			}
			MemoryStream stream = new MemoryStream(Resources.FlippedCursor);
			Cursor = new Cursor(stream);
		}
		else if (x > View.TextMargin - 8)
		{
			if (IsOverSelection(x, y))
			{
				Cursor = Cursors.Arrow;
				return;
			}
			TextPoint p = Painter.CharFromPixel(x, y);
			Word wordFromPos = Document.GetWordFromPos(p);
			if (wordFromPos != null && wordFromPos.Pattern != null && wordFromPos.Pattern.Category != null)
			{
				WordMouseEventArgs e = new WordMouseEventArgs();
				e.Pattern = wordFromPos.Pattern;
				e.Button = MouseButtons.None;
				e.Cursor = Cursors.Hand;
				e.Word = wordFromPos;
				WordMouseEventArgs e2 = e;
				_SyntaxBox.OnWordMouseHover(ref e2);
				Cursor = e2.Cursor;
			}
			else
			{
				Cursor = Cursors.IBeam;
			}
		}
		else
		{
			Cursor = Cursors.Arrow;
		}
	}

	private void CopyText()
	{
		if (!Selection.IsValid)
		{
			return;
		}
		if (_SyntaxBox.CopyAsRTF)
		{
			CopyAsRTF();
			return;
		}
		try
		{
			string data = Selection.Text;
			Clipboard.SetDataObject(data, copy: true);
			CopyEventArgs e = new CopyEventArgs();
			e.Text = data;
			CopyEventArgs e2 = e;
			OnClipboardUpdated(e2);
		}
		catch
		{
			try
			{
				string data2 = Selection.Text;
				Clipboard.SetDataObject(data2, copy: true);
				CopyEventArgs e3 = new CopyEventArgs();
				e3.Text = data2;
				CopyEventArgs e4 = e3;
				OnClipboardUpdated(e4);
			}
			catch
			{
			}
		}
	}

	protected override bool IsInputKey(Keys key)
	{
		switch (key)
		{
		case Keys.Tab:
		case Keys.Return:
		case Keys.Prior:
		case Keys.Next:
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
			return true;
		default:
			return true;
		}
	}

	protected override bool IsInputChar(char c)
	{
		return true;
	}

	public void RemoveFocus()
	{
		if (InfoTip != null && AutoList != null)
		{
			if (!base.ContainsFocus && !InfoTip.ContainsFocus && !AutoList.ContainsFocus)
			{
				CaretTimer.Enabled = false;
				Caret.Blink = false;
				_AutoListVisible = false;
				_InfoTipVisible = false;
			}
			Redraw();
		}
	}

	private void SelectCurrentWord()
	{
		Row currentRow = Caret.CurrentRow;
		if (!(currentRow.Text == "") && Caret.Position.X < currentRow.Text.Length)
		{
			string s = currentRow.Text.Substring(Caret.Position.X, 1);
			int num = CharType(s);
			int num2 = Caret.Position.X;
			int i = Caret.Position.X;
			while (num2 >= 0 && CharType(currentRow.Text.Substring(num2, 1)) == num)
			{
				num2--;
			}
			for (; i <= currentRow.Text.Length - 1 && CharType(currentRow.Text.Substring(i, 1)) == num; i++)
			{
			}
			TextRange bounds = Selection.Bounds;
			int firstRow = (Selection.Bounds.LastRow = currentRow.Index);
			bounds.FirstRow = firstRow;
			Selection.Bounds.FirstColumn = num2 + 1;
			Selection.Bounds.LastColumn = i;
			Caret.Position.X = i;
			Caret.SetPos(Caret.Position);
			Redraw();
		}
	}

	private static int CharType(string s)
	{
		if (" \t".IndexOf(s) >= 0)
		{
			return 1;
		}
		if (".,-+'?\u00b4=)(/&%¤#!\"\\<>[]$£@*:;{}".IndexOf(s) >= 0)
		{
			return 2;
		}
		return 3;
	}

	private void SelectPattern(int RowIndex, int Column, int Length)
	{
		Selection.Bounds.FirstColumn = Column;
		Selection.Bounds.FirstRow = RowIndex;
		Selection.Bounds.LastColumn = Column + Length;
		Selection.Bounds.LastRow = RowIndex;
		Caret.Position.X = Column + Length;
		Caret.Position.Y = RowIndex;
		Caret.CurrentRow.EnsureVisible();
		ScrollIntoView();
		Redraw();
	}

	public void InitVars()
	{
		if (View.RowHeight == 0)
		{
			View.RowHeight = 48;
		}
		if (View.CharWidth == 0)
		{
			View.CharWidth = 16;
		}
		View.FirstVisibleColumn = hScroll.Value;
		View.FirstVisibleRow = vScroll.Value;
		View.VisibleRowCount = 0;
		if (hScroll.Visible)
		{
			View.VisibleRowCount = (base.Height - hScroll.Height) / View.RowHeight + 1;
		}
		else
		{
			View.VisibleRowCount = (base.Height - hScroll.Height) / View.RowHeight + 2;
		}
		View.GutterMarginWidth = (ShowGutterMargin ? GutterMarginWidth : 0);
		if (ShowLineNumbers)
		{
			int length = Document.Count.ToString(CultureInfo.InvariantCulture).Length;
			string str = new string('9', length);
			View.LineNumberMarginWidth = 10 + Painter.MeasureString(str).Width;
		}
		else
		{
			View.LineNumberMarginWidth = 0;
		}
		View.TotalMarginWidth = View.GutterMarginWidth + View.LineNumberMarginWidth;
		if (Document.Folding)
		{
			View.TextMargin = View.TotalMarginWidth + 20;
		}
		else
		{
			View.TextMargin = View.TotalMarginWidth + 7;
		}
		View.ClientAreaWidth = base.Width - vScroll.Width - View.TextMargin;
		View.ClientAreaStart = View.FirstVisibleColumn * View.CharWidth;
	}

	public void CalcMaxCharWidth()
	{
		MaxCharWidth = Painter.GetMaxCharWidth();
	}

	public void SetMaxHorizontalScroll()
	{
		CalcMaxCharWidth();
		int num = View.CharWidth;
		if (num == 0)
		{
			num = 1;
		}
		if (View.ClientAreaWidth / num < 0)
		{
			hScroll.Maximum = 1000;
			return;
		}
		hScroll.LargeChange = View.ClientAreaWidth / num;
		try
		{
			int num2 = 0;
			for (int i = View.FirstVisibleRow; i < Document.VisibleRows.Count && i < View.VisibleRowCount + View.FirstVisibleRow; i++)
			{
				string text = (Document.VisibleRows[i].IsCollapsed ? Document.VisibleRows[i].VirtualCollapsedRow.Text : Document.VisibleRows[i].Text);
				text = text.Replace("\t", new string(' ', TabSize));
				if (text.Length > num2)
				{
					num2 = text.Length;
				}
			}
			int num3 = num2 * MaxCharWidth;
			int num4 = num3 / num;
			if (hScroll.Value <= num4)
			{
				hScroll.Maximum = num4;
			}
		}
		catch
		{
			hScroll.Maximum = 1000;
		}
	}

	public void InitScrollbars()
	{
		if (Document.VisibleRows.Count > 0)
		{
			vScroll.Maximum = Document.VisibleRows.Count + 1;
			vScroll.LargeChange = View.VisibleRowCount;
			SetMaxHorizontalScroll();
		}
		else
		{
			vScroll.Maximum = 1;
		}
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Alsing.Windows.Forms.SyntaxBox.EditViewControl));
		this.Filler = new System.Windows.Forms.PictureBox();
		this.CaretTimer = new System.Windows.Forms.Timer(this.components);
		this.tooltip = new System.Windows.Forms.ToolTip(this.components);
		base.SuspendLayout();
		if (!this._SyntaxBox.DisableIntelliMouse)
		{
			this.IntelliMouse = new Alsing.Windows.Forms.CoreLib.IntelliMouseControl
			{
				BackgroundImage = (System.Drawing.Bitmap)resourceManager.GetObject("IntelliMouse.BackgroundImage"),
				Image = (System.Drawing.Bitmap)resourceManager.GetObject("IntelliMouse.Image"),
				Location = new System.Drawing.Point(197, 157),
				Name = "IntelliMouse",
				Size = new System.Drawing.Size(28, 28),
				TabIndex = 4,
				TransparencyKey = System.Drawing.Color.FromArgb(255, 0, 255),
				Visible = false
			};
			this.IntelliMouse.EndScroll += new System.EventHandler(IntelliMouse_EndScroll);
			this.IntelliMouse.BeginScroll += new System.EventHandler(IntelliMouse_BeginScroll);
			this.IntelliMouse.Scroll += new Alsing.Windows.Forms.IntelliMouse.ScrollEventHandler(IntelliMouse_Scroll);
		}
		base.hScroll.Cursor = System.Windows.Forms.Cursors.Default;
		base.hScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(hScroll_Scroll);
		base.vScroll.Cursor = System.Windows.Forms.Cursors.Default;
		base.vScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(vScroll_Scroll);
		this.CaretTimer.Enabled = true;
		this.CaretTimer.Interval = 500;
		this.CaretTimer.Tick += new System.EventHandler(CaretTimer_Tick);
		this.tooltip.AutoPopDelay = 50000;
		this.tooltip.InitialDelay = 0;
		this.tooltip.ReshowDelay = 1000;
		this.tooltip.ShowAlways = true;
		base.TopThumb.BackColor = System.Drawing.SystemColors.Control;
		base.TopThumb.Cursor = System.Windows.Forms.Cursors.HSplit;
		base.TopThumb.Location = new System.Drawing.Point(101, 17);
		base.TopThumb.Name = "TopThumb";
		base.TopThumb.Size = new System.Drawing.Size(16, 8);
		base.TopThumb.TabIndex = 3;
		base.TopThumb.Visible = false;
		base.LeftThumb.BackColor = System.Drawing.SystemColors.Control;
		base.LeftThumb.Cursor = System.Windows.Forms.Cursors.VSplit;
		base.LeftThumb.Location = new System.Drawing.Point(423, 17);
		base.LeftThumb.Name = "LeftThumb";
		base.LeftThumb.Size = new System.Drawing.Size(8, 16);
		base.LeftThumb.TabIndex = 3;
		base.LeftThumb.Visible = false;
		try
		{
			this.AllowDrop = true;
		}
		catch
		{
		}
		base.Controls.AddRange(new System.Windows.Forms.Control[1] { this.IntelliMouse });
		base.Size = new System.Drawing.Size(0, 0);
		base.LostFocus += new System.EventHandler(EditViewControl_Leave);
		base.GotFocus += new System.EventHandler(EditViewControl_Enter);
		base.ResumeLayout(false);
	}

	public void InsertAutolistText()
	{
		TextRange textRange = new TextRange();
		textRange.FirstRow = Caret.Position.Y;
		textRange.LastRow = Caret.Position.Y;
		textRange.FirstColumn = AutoListStartPos.X;
		textRange.LastColumn = Caret.Position.X;
		TextRange range = textRange;
		Document.DeleteRange(range, StoreUndo: true);
		Caret.Position.X = AutoListStartPos.X;
		InsertText(AutoList.SelectedText);
		SetFocus();
	}

	private void MoveCaretToNextWord(bool Select)
	{
		int i = Caret.Position.X;
		int j = Caret.Position.Y;
		bool flag = false;
		int num;
		if (i == Caret.CurrentRow.Text.Length)
		{
			num = 1;
		}
		else
		{
			string s = Document[j].Text.Substring(Caret.Position.X, 1);
			num = CharType(s);
		}
		for (; j < Document.Count; j++)
		{
			for (; i < Document[j].Text.Length; i++)
			{
				string s2 = Document[j].Text.Substring(i, 1);
				int num2 = CharType(s2);
				if (num2 != num)
				{
					if (num2 != 1)
					{
						flag = true;
						break;
					}
					num = 1;
				}
			}
			if (flag)
			{
				break;
			}
			i = 0;
		}
		if (j >= Document.Count - 1)
		{
			j = Document.Count - 1;
			if (i >= Document[j].Text.Length)
			{
				i = Document[j].Text.Length - 1;
			}
			if (i == -1)
			{
				i = 0;
			}
		}
		Caret.SetPos(new TextPoint(i, j));
		if (!Select)
		{
			Selection.ClearSelection();
		}
		if (Select)
		{
			Selection.MakeSelection();
		}
		ScrollIntoView();
	}

	public void InitGraphics()
	{
		Painter.InitGraphics();
	}

	private void MoveCaretToPrevWord(bool Select)
	{
		int num = Caret.Position.X;
		int num2 = Caret.Position.Y;
		bool flag = false;
		int num3;
		if (num == Caret.CurrentRow.Text.Length)
		{
			num3 = 1;
			num = Caret.CurrentRow.Text.Length - 1;
		}
		else
		{
			string s = Document[num2].Text.Substring(Caret.Position.X, 1);
			num3 = CharType(s);
		}
		while (num2 >= 0)
		{
			while (num >= 0 && num < Document[num2].Text.Length)
			{
				string s2 = Document[num2].Text.Substring(num, 1);
				int num4 = CharType(s2);
				if (num4 != num3)
				{
					flag = true;
					while (num > 0)
					{
						string s3 = Document[num2].Text.Substring(num, 1);
						int num5 = CharType(s3);
						if (num5 != num4)
						{
							num++;
							break;
						}
						num--;
					}
					break;
				}
				num--;
			}
			if (flag)
			{
				break;
			}
			if (num2 == 0)
			{
				num = 0;
				break;
			}
			num2--;
			num = Document[num2].Text.Length - 1;
		}
		Caret.SetPos(new TextPoint(num, num2));
		if (!Select)
		{
			Selection.ClearSelection();
		}
		if (Select)
		{
			Selection.MakeSelection();
		}
		ScrollIntoView();
	}

	private void SetFocus()
	{
		Focus();
	}

	public void ShowGotoLine()
	{
		GotoLineForm gotoLineForm = new GotoLineForm(this, Document.Count);
		gotoLineForm.ShowDialog(base.TopLevelControl);
	}

	public void ShowSettings()
	{
	}

	public void GotoLine(int RowIndex)
	{
		if (RowIndex >= Document.Count)
		{
			RowIndex = Document.Count - 1;
		}
		if (RowIndex < 0)
		{
			RowIndex = 0;
		}
		Caret.Position.Y = RowIndex;
		Caret.Position.X = 0;
		Caret.CurrentRow.EnsureVisible();
		ClearSelection();
		ScrollIntoView();
		Redraw();
	}

	public void ClearSelection()
	{
		Selection.ClearSelection();
		Redraw();
	}

	public bool IsOverSelection(int x, int y)
	{
		TextPoint textPoint = Painter.CharFromPixel(x, y);
		if (textPoint.Y >= Selection.LogicalBounds.FirstRow && textPoint.Y <= Selection.LogicalBounds.LastRow && Selection.IsValid)
		{
			if (textPoint.Y > Selection.LogicalBounds.FirstRow && textPoint.Y < Selection.LogicalBounds.LastRow && Selection.IsValid)
			{
				return true;
			}
			if (textPoint.Y == Selection.LogicalBounds.FirstRow && Selection.LogicalBounds.FirstRow == Selection.LogicalBounds.LastRow)
			{
				if (textPoint.X >= Selection.LogicalBounds.FirstColumn && textPoint.X <= Selection.LogicalBounds.LastColumn)
				{
					return true;
				}
				return false;
			}
			if (textPoint.X >= Selection.LogicalBounds.FirstColumn && textPoint.Y == Selection.LogicalBounds.FirstRow)
			{
				return true;
			}
			if (textPoint.X <= Selection.LogicalBounds.LastColumn && textPoint.Y == Selection.LogicalBounds.LastRow)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public void ScrollIntoView(TextPoint Pos)
	{
		TextPoint position = Caret.Position;
		Caret.Position = Pos;
		Caret.CurrentRow.EnsureVisible();
		ScrollIntoView();
		Caret.Position = position;
		Invalidate();
	}

	public void ScrollIntoView(int RowIndex)
	{
		Row row = Document[RowIndex];
		row.EnsureVisible();
		vScroll.Value = row.VisibleIndex;
		Invalidate();
	}

	public void ScrollIntoView()
	{
		InitScrollbars();
		Caret.CropPosition();
		try
		{
			Row currentRow = Caret.CurrentRow;
			if (currentRow.VisibleIndex >= View.FirstVisibleRow + View.VisibleRowCount - 2)
			{
				int num = Caret.CurrentRow.VisibleIndex - (View.FirstVisibleRow + View.VisibleRowCount - 2) + View.FirstVisibleRow;
				if (num > Document.VisibleRows.Count - 1)
				{
					num = Document.VisibleRows.Count - 1;
				}
				Row row = Document.VisibleRows[num];
				int visibleIndex = row.VisibleIndex;
				if (visibleIndex != -1)
				{
					vScroll.Value = visibleIndex;
				}
			}
		}
		catch
		{
		}
		if (Caret.CurrentRow.VisibleIndex < View.FirstVisibleRow)
		{
			Row currentRow2 = Caret.CurrentRow;
			int visibleIndex2 = currentRow2.VisibleIndex;
			if (visibleIndex2 != -1)
			{
				vScroll.Value = visibleIndex2;
			}
		}
		Row currentRow3 = Caret.CurrentRow;
		if (Caret.CurrentRow.IsCollapsedEndPart)
		{
			int num2 = Painter.MeasureRow(currentRow3, Caret.Position.X).Width + Caret.CurrentRow.Expansion_PixelStart;
			num2 -= Painter.MeasureRow(currentRow3, currentRow3.Expansion_StartChar).Width;
			if (num2 >= View.ClientAreaWidth + View.ClientAreaStart)
			{
				hScroll.Value = Math.Min(hScroll.Maximum, (num2 - View.ClientAreaWidth) / View.CharWidth + 15);
			}
			if (num2 < View.ClientAreaStart + 10)
			{
				hScroll.Value = Math.Max(hScroll.Minimum, num2 / View.CharWidth - 15);
			}
		}
		else
		{
			int num2 = Painter.MeasureRow(currentRow3, Caret.Position.X).Width;
			if (num2 >= View.ClientAreaWidth + View.ClientAreaStart)
			{
				hScroll.Value = Math.Min(hScroll.Maximum, (num2 - View.ClientAreaWidth) / View.CharWidth + 15);
			}
			if (num2 < View.ClientAreaStart)
			{
				hScroll.Value = Math.Max(hScroll.Minimum, num2 / View.CharWidth - 15);
			}
		}
	}

	public void GotoNextBookmark()
	{
		int nextBookmark = Document.GetNextBookmark(Caret.Position.Y);
		Caret.SetPos(new TextPoint(0, nextBookmark));
		ScrollIntoView();
		Redraw();
	}

	public void GotoPreviousBookmark()
	{
		int previousBookmark = Document.GetPreviousBookmark(Caret.Position.Y);
		Caret.SetPos(new TextPoint(0, previousBookmark));
		ScrollIntoView();
		Redraw();
	}

	public bool SelectNext(string Pattern, bool MatchCase, bool WholeWords, bool UseRegEx)
	{
		string text = Pattern;
		for (int i = Caret.Position.Y; i < Document.Count; i++)
		{
			Row row = Document[i];
			string text2 = row.Text;
			if (WholeWords)
			{
				string text3 = " " + row.Text + " ";
				text2 = "";
				text = " " + Pattern + " ";
				string text4 = text3;
				foreach (char c in text4)
				{
					text2 = ((".,+-*^\\/()[]{}@:;'?£$#%& \t=<>".IndexOf(c) < 0) ? (text2 + c) : (text2 + " "));
				}
			}
			if (!MatchCase)
			{
				text2 = text2.ToLowerInvariant();
				text = text.ToLowerInvariant();
			}
			int num = text2.IndexOf(text);
			int num2 = Caret.Position.X;
			int num3 = Caret.Position.Y;
			if (num >= num2 || (i > num3 && num >= 0))
			{
				SelectPattern(i, num, Pattern.Length);
				return true;
			}
		}
		return false;
	}

	public bool ReplaceSelection(string text)
	{
		if (!Selection.IsValid)
		{
			return false;
		}
		int firstColumn = Selection.LogicalBounds.FirstColumn;
		int firstRow = Selection.LogicalBounds.FirstRow;
		Selection.DeleteSelection();
		Caret.Position.X = firstColumn;
		Caret.Position.Y = firstRow;
		InsertText(text);
		Selection.Bounds.FirstRow = firstRow;
		Selection.Bounds.FirstColumn = firstColumn + text.Length;
		Selection.Bounds.LastRow = firstRow;
		Selection.Bounds.LastColumn = firstColumn + text.Length;
		Caret.Position.X = firstColumn + text.Length;
		Caret.Position.Y = firstRow;
		return true;
	}

	public void ToggleBookmark()
	{
		Document[Caret.Position.Y].Bookmarked = !Document[Caret.Position.Y].Bookmarked;
		Redraw();
	}

	public void Delete()
	{
		DeleteForward();
		Refresh();
	}

	public void SelectAll()
	{
		Selection.SelectAll();
		Redraw();
	}

	public void Paste()
	{
		PasteText();
		Refresh();
	}

	public void Copy()
	{
		CopyText();
	}

	public void Cut()
	{
		CopyText();
		Selection.DeleteSelection();
	}

	public void RemoveCurrentRow()
	{
		if (Caret.CurrentRow != null && Document.Count > 1)
		{
			Document.Remove(Caret.CurrentRow.Index, StoreUndo: true);
			Document.ResetVisibleRows();
			Caret.CropPosition();
			Caret.CurrentRow.Text = Caret.CurrentRow.Text;
			Caret.CurrentRow.Parse(ParseKeywords: true);
			Document.ResetVisibleRows();
			ScrollIntoView();
		}
	}

	public void CutClear()
	{
		if (Selection.IsValid)
		{
			Cut();
		}
		else
		{
			RemoveCurrentRow();
		}
	}

	public void Redo()
	{
		TextPoint textPoint = Document.Redo();
		if (textPoint.X != -1 && textPoint.Y != -1)
		{
			Caret.Position = textPoint;
			Selection.ClearSelection();
			ScrollIntoView();
		}
	}

	public void Undo()
	{
		TextPoint textPoint = Document.Undo();
		if (textPoint.X != -1 && textPoint.Y != -1)
		{
			Caret.Position = textPoint;
			Selection.ClearSelection();
			ScrollIntoView();
		}
	}

	public TextPoint CharFromPixel(int x, int y)
	{
		return Painter.CharFromPixel(x, y);
	}

	public void ShowFind()
	{
		if (FindReplaceDialog != null)
		{
			FindReplaceDialog.TopLevel = true;
			if (base.TopLevelControl is Form)
			{
				FindReplaceDialog.Owner = (Form)base.TopLevelControl;
			}
			FindReplaceDialog.ShowFind();
		}
	}

	public void ShowReplace()
	{
		if (FindReplaceDialog != null)
		{
			FindReplaceDialog.TopLevel = true;
			if (base.TopLevelControl is Form)
			{
				FindReplaceDialog.Owner = (Form)base.TopLevelControl;
			}
			FindReplaceDialog.ShowReplace();
		}
	}

	public void AutoListBeginLoad()
	{
		AutoList.BeginLoad();
	}

	public void AutoListEndLoad()
	{
		AutoList.EndLoad();
	}

	public void FindNext()
	{
		FindReplaceDialog.FindNext();
	}

	private void OnClipboardUpdated(CopyEventArgs e)
	{
		if (ClipboardUpdated != null)
		{
			ClipboardUpdated(this, e);
		}
	}

	private void OnRowMouseDown(RowMouseEventArgs e)
	{
		if (RowMouseDown != null)
		{
			RowMouseDown(this, e);
		}
	}

	private void OnRowMouseMove(RowMouseEventArgs e)
	{
		if (RowMouseMove != null)
		{
			RowMouseMove(this, e);
		}
	}

	private void OnRowMouseUp(RowMouseEventArgs e)
	{
		if (RowMouseUp != null)
		{
			RowMouseUp(this, e);
		}
	}

	private void OnRowClick(RowMouseEventArgs e)
	{
		if (RowClick != null)
		{
			RowClick(this, e);
		}
	}

	private void OnRowDoubleClick(RowMouseEventArgs e)
	{
		if (RowDoubleClick != null)
		{
			RowDoubleClick(this, e);
		}
	}

	protected override void OnLoad(EventArgs e)
	{
		DoResize();
		Refresh();
	}

	public void OnParse()
	{
		Redraw();
	}

	public void OnChange()
	{
		if (Caret.Position.Y > Document.Count - 1)
		{
			Caret.Position.Y = Document.Count - 1;
			ScrollIntoView();
		}
		try
		{
			if (!VirtualWhitespace && Caret.CurrentRow != null && Caret.Position.X > Caret.CurrentRow.Text.Length)
			{
				Caret.Position.X = Caret.CurrentRow.Text.Length;
				Redraw();
			}
		}
		catch
		{
		}
		if (!base.ContainsFocus)
		{
			Selection.ClearSelection();
		}
		if (Selection.LogicalBounds.FirstRow > Document.Count)
		{
			Selection.Bounds.FirstColumn = Caret.Position.X;
			Selection.Bounds.LastColumn = Caret.Position.X;
			Selection.Bounds.FirstRow = Caret.Position.Y;
			Selection.Bounds.LastRow = Caret.Position.Y;
		}
		if (Selection.LogicalBounds.LastRow > Document.Count)
		{
			Selection.Bounds.FirstColumn = Caret.Position.X;
			Selection.Bounds.LastColumn = Caret.Position.X;
			Selection.Bounds.FirstRow = Caret.Position.Y;
			Selection.Bounds.LastRow = Caret.Position.Y;
		}
		Redraw();
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		_KeyDownHandled = e.Handled;
		if (e.KeyCode == Keys.Escape && (InfoTipVisible || AutoListVisible))
		{
			InfoTipVisible = false;
			AutoListVisible = false;
			e.Handled = true;
			Redraw();
			return;
		}
		if (!e.Handled && InfoTipVisible && InfoTip.Count > 1)
		{
			if (e.KeyCode == Keys.Up)
			{
				_SyntaxBox.InfoTipSelectedIndex++;
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.Down)
			{
				_SyntaxBox.InfoTipSelectedIndex--;
				e.Handled = true;
				return;
			}
		}
		if (!e.Handled && AutoListVisible)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Prior || e.KeyCode == Keys.Next)
			{
				AutoList.SendKey((int)e.KeyCode);
				e.Handled = true;
				return;
			}
			if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Return || e.KeyCode == Keys.Tab)
			{
				string selectedText = AutoList.SelectedText;
				if (selectedText != "")
				{
					InsertAutolistText();
				}
				AutoListVisible = false;
				e.Handled = true;
				Redraw();
				return;
			}
		}
		if (e.Handled)
		{
			return;
		}
		foreach (KeyboardAction keyboardAction in _SyntaxBox.KeyboardActions)
		{
			if ((!ReadOnly || keyboardAction.AllowReadOnly) && keyboardAction.Key == e.KeyCode && keyboardAction.Alt == e.Alt && keyboardAction.Shift == e.Shift && keyboardAction.Control == e.Control)
			{
				keyboardAction.Action();
			}
		}
		switch (e.KeyCode)
		{
		case Keys.ShiftKey:
		case Keys.ControlKey:
		case Keys.Alt:
			return;
		case Keys.Down:
			if (e.Control)
			{
				ScrollScreen(1);
				break;
			}
			Caret.MoveDown(e.Shift);
			Redraw();
			break;
		case Keys.Up:
			if (e.Control)
			{
				ScrollScreen(-1);
			}
			else
			{
				Caret.MoveUp(e.Shift);
			}
			Redraw();
			break;
		case Keys.Left:
			if (e.Control)
			{
				MoveCaretToPrevWord(e.Shift);
			}
			else
			{
				Caret.MoveLeft(e.Shift);
			}
			Redraw();
			break;
		case Keys.Right:
			if (e.Control)
			{
				MoveCaretToNextWord(e.Shift);
			}
			else
			{
				Caret.MoveRight(e.Shift);
			}
			Redraw();
			break;
		case Keys.End:
			if (e.Control)
			{
				Caret.MoveAbsoluteEnd(e.Shift);
			}
			else
			{
				Caret.MoveEnd(e.Shift);
			}
			Redraw();
			break;
		case Keys.Home:
			if (e.Control)
			{
				Caret.MoveAbsoluteHome(e.Shift);
			}
			else
			{
				Caret.MoveHome(e.Shift);
			}
			Redraw();
			break;
		case Keys.Next:
			Caret.MoveDown(View.VisibleRowCount - 2, e.Shift);
			Redraw();
			break;
		case Keys.Prior:
			Caret.MoveUp(View.VisibleRowCount - 2, e.Shift);
			Redraw();
			break;
		}
		if (!ReadOnly)
		{
			switch (e.KeyCode)
			{
			case Keys.Return:
				if (e.Control)
				{
					if (Caret.CurrentRow.CanFold)
					{
						Caret.MoveHome(Select: false);
						Document.ToggleRow(Caret.CurrentRow);
						Redraw();
					}
				}
				else
				{
					InsertEnter();
				}
				break;
			case Keys.Back:
				if (!e.Control)
				{
					DeleteBackwards();
					break;
				}
				if (Selection.IsValid)
				{
					Selection.DeleteSelection();
				}
				else
				{
					Selection.ClearSelection();
					MoveCaretToPrevWord(Select: true);
					Selection.DeleteSelection();
				}
				Caret.CurrentRow.Parse(ParseKeywords: true);
				break;
			case Keys.Delete:
				if (!e.Control && !e.Alt && !e.Shift)
				{
					Delete();
				}
				else if (e.Control && !e.Alt && !e.Shift)
				{
					if (Selection.IsValid)
					{
						Cut();
					}
					else
					{
						Selection.ClearSelection();
						MoveCaretToNextWord(Select: true);
						Selection.DeleteSelection();
					}
					Caret.CurrentRow.Parse(ParseKeywords: true);
				}
				break;
			case Keys.Insert:
				if (!e.Control && !e.Alt && !e.Shift)
				{
					_OverWrite = !_OverWrite;
				}
				break;
			case Keys.Tab:
				if (!Selection.IsValid)
				{
					if (!TabsToSpaces)
					{
						InsertText("\t");
					}
					else
					{
						InsertText(new string(' ', TabSize));
					}
				}
				break;
			}
		}
		Caret.Blink = true;
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		base.OnKeyPress(e);
		if (!e.Handled && !_KeyDownHandled && e.KeyChar != '\u007f')
		{
			if (e.KeyChar < ' ')
			{
				return;
			}
			if (!ReadOnly)
			{
				_ = e.KeyChar;
				InsertText(e.KeyChar.ToString(CultureInfo.InvariantCulture));
				if ((Indent == IndentStyle.Scope || Indent == IndentStyle.Smart) && Caret.CurrentRow.ShouldOutdent)
				{
					OutdentEndRow();
				}
			}
		}
		if (!AutoListVisible || e.Handled || !_SyntaxBox.AutoListAutoSelect)
		{
			return;
		}
		string text = Caret.CurrentRow.Text;
		try
		{
			if (Caret.Position.X - AutoListStartPos.X >= 0)
			{
				text = text.Substring(AutoListStartPos.X, Caret.Position.X - AutoListStartPos.X);
				AutoList.SelectItem(text);
			}
		}
		catch
		{
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		MouseX = e.X;
		MouseY = e.Y;
		SetFocus();
		base.OnMouseDown(e);
		TextPoint textPoint = Painter.CharFromPixel(e.X, e.Y);
		Row row = null;
		if (textPoint.Y >= 0 && textPoint.Y < Document.Count)
		{
			row = Document[textPoint.Y];
		}
		RowMouseEventArgs e2 = new RowMouseEventArgs();
		e2.Row = row;
		e2.Button = e.Button;
		e2.MouseX = MouseX;
		e2.MouseY = MouseY;
		RowMouseEventArgs e3 = e2;
		if (e.X >= View.TextMargin - 7)
		{
			e3.Area = RowArea.TextArea;
		}
		else if (e.X < View.GutterMarginWidth)
		{
			e3.Area = RowArea.GutterArea;
		}
		else if (e.X < View.LineNumberMarginWidth + View.GutterMarginWidth)
		{
			e3.Area = RowArea.LineNumberArea;
		}
		else if (e.X < View.TextMargin - 7)
		{
			e3.Area = RowArea.FoldingArea;
		}
		OnRowMouseDown(e3);
		try
		{
			Row row2 = Document[textPoint.Y];
			if (row2 != null && e.X >= row2.Expansion_PixelEnd && row2.IsCollapsed && row2.expansion_StartSpan != null && row2.expansion_StartSpan.StartRow != null && row2.expansion_StartSpan.EndRow != null && !row2.expansion_StartSpan.Expanded && !IsOverSelection(e.X, e.Y))
			{
				Caret.Position.X = textPoint.X;
				Caret.Position.Y = textPoint.Y;
				Selection.ClearSelection();
				Row expansion_EndRow = row2.Expansion_EndRow;
				int expansion_StartChar = expansion_EndRow.Expansion_StartChar;
				Caret.Position.X = expansion_StartChar;
				Caret.Position.Y = expansion_EndRow.Index;
				Selection.MakeSelection();
				Redraw();
				View.Action = EditAction.SelectText;
				return;
			}
		}
		catch
		{
		}
		bool flag = NativeMethods.IsKeyPressed(Keys.ShiftKey);
		if (e.X > View.TotalMarginWidth)
		{
			if (e.X > View.TextMargin - 8)
			{
				if (!IsOverSelection(e.X, e.Y) && e.Button == MouseButtons.Left)
				{
					if (!flag)
					{
						TextPoint p = textPoint;
						Word wordFromPos = Document.GetWordFromPos(p);
						if (wordFromPos != null && wordFromPos.Pattern != null && wordFromPos.Pattern.Category != null)
						{
							WordMouseEventArgs e4 = new WordMouseEventArgs();
							e4.Pattern = wordFromPos.Pattern;
							e4.Button = e.Button;
							e4.Cursor = Cursors.Hand;
							e4.Word = wordFromPos;
							WordMouseEventArgs e5 = e4;
							_SyntaxBox.OnWordMouseDown(ref e5);
							Cursor = e5.Cursor;
							return;
						}
						View.Action = EditAction.SelectText;
						Caret.SetPos(textPoint);
						Selection.ClearSelection();
						Caret.Blink = true;
						Redraw();
					}
					else
					{
						View.Action = EditAction.SelectText;
						Caret.SetPos(textPoint);
						Selection.MakeSelection();
						Caret.Blink = true;
						Redraw();
					}
				}
			}
			else if (row != null && row.expansion_StartSpan != null)
			{
				Caret.SetPos(new TextPoint(0, textPoint.Y));
				Selection.ClearSelection();
				Document.ToggleRow(row);
				Redraw();
			}
		}
		else if (e.X < View.GutterMarginWidth)
		{
			if (_SyntaxBox.AllowBreakPoints)
			{
				Row row3 = Document[Painter.CharFromPixel(e.X, e.Y).Y];
				row3.Breakpoint = !row3.Breakpoint;
				Redraw();
			}
			else
			{
				Row row4 = Document[Painter.CharFromPixel(e.X, e.Y).Y];
				row4.Breakpoint = false;
				Redraw();
			}
		}
		else
		{
			View.Action = EditAction.SelectText;
			Caret.SetPos(Painter.CharFromPixel(0, e.Y));
			Selection.ClearSelection();
			Caret.MoveDown(Select: true);
			Redraw();
		}
		SetMouseCursor(e.X, e.Y);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		MouseX = e.X;
		MouseY = e.Y;
		TextPoint textPoint = Painter.CharFromPixel(e.X, e.Y);
		Row row = null;
		if (textPoint.Y >= 0 && textPoint.Y < Document.Count)
		{
			row = Document[textPoint.Y];
		}
		RowMouseEventArgs e2 = new RowMouseEventArgs();
		e2.Row = row;
		e2.Button = e.Button;
		e2.MouseX = MouseX;
		e2.MouseY = MouseY;
		RowMouseEventArgs e3 = e2;
		if (e.X >= View.TextMargin - 7)
		{
			e3.Area = RowArea.TextArea;
		}
		else if (e.X < View.GutterMarginWidth)
		{
			e3.Area = RowArea.GutterArea;
		}
		else if (e.X < View.LineNumberMarginWidth + View.GutterMarginWidth)
		{
			e3.Area = RowArea.LineNumberArea;
		}
		else if (e.X < View.TextMargin - 7)
		{
			e3.Area = RowArea.FoldingArea;
		}
		OnRowMouseMove(e3);
		try
		{
			if (Document == null)
			{
				return;
			}
			if (e.Button == MouseButtons.Left)
			{
				if (View.Action == EditAction.SelectText)
				{
					Caret.Blink = true;
					Caret.SetPos(textPoint);
					if (e.X <= View.TotalMarginWidth)
					{
						Caret.MoveDown(Select: true);
					}
					Caret.CropPosition();
					Selection.MakeSelection();
					ScrollIntoView();
					Redraw();
				}
				else if (View.Action == EditAction.None)
				{
					if (IsOverSelection(e.X, e.Y))
					{
						BeginDragDrop();
					}
				}
				else if (View.Action != EditAction.DragText)
				{
				}
			}
			else
			{
				TextPoint textPoint2 = textPoint;
				Row row2 = Document[textPoint2.Y];
				bool flag = false;
				if (row2 != null)
				{
					if (e.X >= row2.Expansion_PixelEnd && row2.IsCollapsed)
					{
						if (CollapsedBlockTooltipsEnabled && row2.expansion_StartSpan != null && row2.expansion_StartSpan.StartRow != null && row2.expansion_StartSpan.EndRow != null && !row2.expansion_StartSpan.Expanded)
						{
							string text = "";
							int num = 0;
							for (int i = row2.expansion_StartSpan.StartRow.Index; i <= row2.expansion_StartSpan.EndRow.Index; i++)
							{
								if (num > 0)
								{
									text += "\n";
								}
								Row row3 = Document[i];
								string text2 = row3.Text.Replace("\t", "    ");
								text += text2;
								if (num > 20)
								{
									text += "...";
									break;
								}
								num++;
							}
							tooltip.InitialDelay = TooltipDelay;
							if (tooltip.GetToolTip(this) != text)
							{
								tooltip.SetToolTip(this, text);
							}
							tooltip.Active = true;
							flag = true;
						}
					}
					else
					{
						Word formatWordFromPos = Document.GetFormatWordFromPos(textPoint2);
						if (formatWordFromPos != null && formatWordFromPos.InfoTip != null)
						{
							tooltip.InitialDelay = TooltipDelay;
							if (tooltip.GetToolTip(this) != formatWordFromPos.InfoTip)
							{
								tooltip.SetToolTip(this, formatWordFromPos.InfoTip);
							}
							tooltip.Active = true;
							flag = true;
						}
					}
				}
				if (tooltip != null && !flag)
				{
					tooltip.SetToolTip(this, "");
				}
			}
			SetMouseCursor(e.X, e.Y);
			base.OnMouseMove(e);
		}
		catch
		{
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		MouseX = e.X;
		MouseY = e.Y;
		TextPoint textPoint = Painter.CharFromPixel(e.X, e.Y);
		Row row = null;
		if (textPoint.Y >= 0 && textPoint.Y < Document.Count)
		{
			row = Document[textPoint.Y];
		}
		RowMouseEventArgs e2 = new RowMouseEventArgs();
		e2.Row = row;
		e2.Button = e.Button;
		e2.MouseX = MouseX;
		e2.MouseY = MouseY;
		RowMouseEventArgs e3 = e2;
		if (e.X >= View.TextMargin - 7)
		{
			e3.Area = RowArea.TextArea;
		}
		else if (e.X < View.GutterMarginWidth)
		{
			e3.Area = RowArea.GutterArea;
		}
		else if (e.X < View.LineNumberMarginWidth + View.GutterMarginWidth)
		{
			e3.Area = RowArea.LineNumberArea;
		}
		else if (e.X < View.TextMargin - 7)
		{
			e3.Area = RowArea.FoldingArea;
		}
		OnRowMouseUp(e3);
		if (View.Action == EditAction.None && e.X > View.TotalMarginWidth && IsOverSelection(e.X, e.Y) && e.Button == MouseButtons.Left)
		{
			View.Action = EditAction.SelectText;
			Caret.SetPos(Painter.CharFromPixel(e.X, e.Y));
			Selection.ClearSelection();
			Redraw();
		}
		View.Action = EditAction.None;
		base.OnMouseUp(e);
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		int mouseWheelScrollLines = SystemInformation.MouseWheelScrollLines;
		ScrollScreen(-(e.Delta / 120) * mouseWheelScrollLines);
		base.OnMouseWheel(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (Document != null && base.Width > 0 && base.Height > 0)
		{
			Painter.RenderAll(e.Graphics);
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		DoResize();
	}

	protected override void OnDragOver(DragEventArgs drgevent)
	{
		if (!ReadOnly)
		{
			if (Document != null)
			{
				View.Action = EditAction.DragText;
				Point point = PointToClient(new Point(drgevent.X, drgevent.Y));
				int num = point.X;
				int num2 = point.Y;
				drgevent.Effect = (((drgevent.KeyState & 8) == 8) ? DragDropEffects.Copy : DragDropEffects.Move);
				Caret.SetPos(Painter.CharFromPixel(num, num2));
				Redraw();
			}
		}
		else
		{
			drgevent.Effect = DragDropEffects.None;
		}
	}

	protected override void OnDragDrop(DragEventArgs drgevent)
	{
		if (!ReadOnly && Document != null)
		{
			View.Action = EditAction.None;
			int logicalSelStart = Selection.LogicalSelStart;
			int num = Document.PointToIntPos(Caret.Position);
			string text = drgevent.Data.GetData(typeof(string)).ToString();
			int length = text.Length;
			if (num >= logicalSelStart && num <= logicalSelStart + Math.Abs(Selection.SelLength))
			{
				num = logicalSelStart;
			}
			else if (num >= logicalSelStart + length)
			{
				num -= length;
			}
			Document.StartUndoCapture();
			if ((drgevent.KeyState & 8) == 0)
			{
				_SyntaxBox.Selection.DeleteSelection();
				Caret.Position = Document.IntPosToPoint(num);
			}
			Document.InsertText(text, Caret.Position.X, Caret.Position.Y);
			Document.EndUndoCapture();
			Selection.SelStart = Document.PointToIntPos(Caret.Position);
			Selection.SelLength = length;
			Document.ResetVisibleRows();
			ScrollIntoView();
			Redraw();
			drgevent.Effect = DragDropEffects.All;
			if (ParseOnPaste)
			{
				Document.ParseAll(ParseKeywords: true);
			}
			View.Action = EditAction.None;
		}
	}

	protected override void OnDragEnter(DragEventArgs drgevent)
	{
	}

	protected override void OnDragLeave(EventArgs e)
	{
		View.Action = EditAction.None;
	}

	protected override void OnDoubleClick(EventArgs e)
	{
		TextPoint textPoint = Painter.CharFromPixel(MouseX, MouseY);
		Row row = null;
		if (textPoint.Y >= 0 && textPoint.Y < Document.Count)
		{
			row = Document[textPoint.Y];
		}
		RowMouseEventArgs e2 = new RowMouseEventArgs();
		e2.Row = row;
		e2.Button = MouseButtons.None;
		e2.MouseX = MouseX;
		e2.MouseY = MouseY;
		RowMouseEventArgs e3 = e2;
		if (MouseX >= View.TextMargin - 7)
		{
			e3.Area = RowArea.TextArea;
		}
		else if (MouseX < View.GutterMarginWidth)
		{
			e3.Area = RowArea.GutterArea;
		}
		else if (MouseX < View.LineNumberMarginWidth + View.GutterMarginWidth)
		{
			e3.Area = RowArea.LineNumberArea;
		}
		else if (MouseX < View.TextMargin - 7)
		{
			e3.Area = RowArea.FoldingArea;
		}
		OnRowDoubleClick(e3);
		try
		{
			Row row2 = Document[textPoint.Y];
			if (row2 != null && MouseX >= row2.Expansion_PixelEnd && row2.IsCollapsed && row2.expansion_StartSpan != null && row2.expansion_StartSpan.StartRow != null && row2.expansion_StartSpan.EndRow != null && !row2.expansion_StartSpan.Expanded)
			{
				row2.Expanded = true;
				Document.ResetVisibleRows();
				Redraw();
				return;
			}
		}
		catch
		{
		}
		if (MouseX > View.TotalMarginWidth)
		{
			SelectCurrentWord();
		}
	}

	protected override void OnClick(EventArgs e)
	{
		TextPoint textPoint = Painter.CharFromPixel(MouseX, MouseY);
		Row row = null;
		if (textPoint.Y >= 0 && textPoint.Y < Document.Count)
		{
			row = Document[textPoint.Y];
		}
		RowMouseEventArgs e2 = new RowMouseEventArgs();
		e2.Row = row;
		e2.Button = MouseButtons.None;
		e2.MouseX = MouseX;
		e2.MouseY = MouseY;
		RowMouseEventArgs e3 = e2;
		if (MouseX >= View.TextMargin - 7)
		{
			e3.Area = RowArea.TextArea;
		}
		else if (MouseX < View.GutterMarginWidth)
		{
			e3.Area = RowArea.GutterArea;
		}
		else if (MouseX < View.LineNumberMarginWidth + View.GutterMarginWidth)
		{
			e3.Area = RowArea.LineNumberArea;
		}
		else if (MouseX < View.TextMargin - 7)
		{
			e3.Area = RowArea.FoldingArea;
		}
		OnRowClick(e3);
	}

	private void vScroll_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
	{
		SetMaxHorizontalScroll();
		_InfoTipVisible = false;
		_AutoListVisible = false;
		SetFocus();
		int num = e.NewValue - vScroll.Value;
		if ((num == -1 || num == 1) && (e.Type == ScrollEventType.SmallDecrement || e.Type == ScrollEventType.SmallIncrement))
		{
			ScrollScreen(num);
		}
		else
		{
			Invalidate();
		}
	}

	private void hScroll_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
	{
		_InfoTipVisible = false;
		_AutoListVisible = false;
		SetFocus();
		Invalidate();
	}

	private void CaretTimer_Tick(object sender, EventArgs e)
	{
		Caret.Blink = !Caret.Blink;
		RedrawCaret();
	}

	private void AutoListDoubleClick(object sender, EventArgs e)
	{
		string selectedText = AutoList.SelectedText;
		if (selectedText != "")
		{
			InsertAutolistText();
		}
		AutoListVisible = false;
		Redraw();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (tooltip != null)
		{
			tooltip.RemoveAll();
		}
	}

	private void CaretChanged(object s, EventArgs e)
	{
		OnCaretChange();
	}

	private void EditViewControl_Leave(object sender, EventArgs e)
	{
		RemoveFocus();
	}

	private void EditViewControl_Enter(object sender, EventArgs e)
	{
		CaretTimer.Enabled = true;
	}

	private void SelectionChanged(object s, EventArgs e)
	{
		OnSelectionChange();
	}

	private void OnCaretChange()
	{
		if (CaretChange != null)
		{
			CaretChange(this, null);
		}
	}

	private void OnSelectionChange()
	{
		if (SelectionChange != null)
		{
			SelectionChange(this, null);
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		if (!base.Visible)
		{
			RemoveFocus();
		}
		base.OnVisibleChanged(e);
		DoResize();
	}
}
