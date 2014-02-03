using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public class DropDownContentListBox : ListBox, IDropDownContent
{
	private Control _wmFocusMissingArg;

	private PropertyEnumerator _ownerPropertyEnumerator;

	private PropertyValue _propValue;

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

	protected override CreateParams CreateParams
	{
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.Style &= -8388609;
			createParams.ExStyle &= -513;
			return createParams;
		}
	}

	public DropDownContentListBox()
	{
		base.IntegralHeight = false;
		DrawMode = DrawMode.OwnerDrawFixed;
		base.DrawItem += OnDrawItem;
		base.DisplayMember = "Name";
	}

	private void OnDrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index != -1 && e.Index < base.Items.Count)
		{
			e.DrawBackground();
			if ((e.State & DrawItemState.Focus) != DrawItemState.None)
			{
				e.DrawFocusRectangle();
			}
			Color textColor = (((e.State & DrawItemState.Selected) != DrawItemState.None) ? SystemColors.HighlightText : ForeColor);
			int imageIndex = _propValue.ImageIndex;
			_propValue.ImageIndexNoRedraw = e.Index;
			_propValue.DrawValue(e.Graphics, e.Bounds, textColor, Font, _ownerPropertyEnumerator, base.Items[e.Index] as string, multipleTexts: false, PropertyValue.DrawValueIn.DropDown);
			_propValue.ImageIndexNoRedraw = imageIndex;
		}
	}

	public int GetSelectedIndex()
	{
		return SelectedIndex;
	}

	public void InitializeContent(PropertyEnumerator propEnum, string currentValue, object valueKey)
	{
		Dock = DockStyle.None;
		_ownerPropertyEnumerator = propEnum;
		int num = 0;
		_propValue = ((valueKey == null) ? propEnum.Property.Value : propEnum.Property.GetValue(valueKey));
		Graphics graphics = CreateGraphics();
		if (_propValue.TriggerDisplayedValuesNeededEventAtDropDown)
		{
			_propValue.ResetDisplayedValues(PropertyGrid.ResetDisplayedValuesTriggerMode.TriggerOnceNow | PropertyGrid.ResetDisplayedValuesTriggerMode.TriggerAtDropDown);
			base.Items.Clear();
		}
		if (base.Items.Count == 0)
		{
			string[] displayedValues = _propValue.GetDisplayedValues();
			string[] array = displayedValues;
			foreach (string item in array)
			{
				base.Items.Add(item);
				Size textExtent = Win32Calls.GetTextExtent(graphics, item, Font);
				if (num < textExtent.Width)
				{
					num = textExtent.Width;
				}
			}
		}
		Win32Calls.TEXTMETRIC tm = default(Win32Calls.TEXTMETRIC);
		Win32Calls.GetTextMetrics(graphics, Font, ref tm);
		graphics.Dispose();
		ItemHeight = tm.tmHeight + propEnum.Property.ParentGrid.PropertyVerticalMargin - 2;
		base.Height = Math.Max(ItemHeight, Math.Min(200, ItemHeight * base.Items.Count));
		int globalTextMargin = propEnum.Property.ParentGrid.GlobalTextMargin;
		base.Width = num + 2 * globalTextMargin;
		if (_propValue.ImageList != null)
		{
			base.Width += ItemHeight + globalTextMargin;
		}
		DrawMode = DrawMode.OwnerDrawFixed;
		if (base.Items.Contains(currentValue))
		{
			base.SelectedItem = currentValue;
		}
		else
		{
			SelectedIndex = -1;
		}
	}

	public bool HasToBeClosedOnClick()
	{
		return true;
	}

	public object GetValue()
	{
		return base.SelectedItem;
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
		base.WndProc(ref m);
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		PropertyValue value = _ownerPropertyEnumerator.Property.Value;
		if (value.UseAutoCompletion)
		{
			int currentIndex = SelectedIndex;
			value.IncrementalFind(e.KeyChar, ref currentIndex);
			if (currentIndex != -1)
			{
				SelectedIndex = currentIndex;
			}
			e.Handled = true;
		}
		base.OnKeyPress(e);
	}
}
