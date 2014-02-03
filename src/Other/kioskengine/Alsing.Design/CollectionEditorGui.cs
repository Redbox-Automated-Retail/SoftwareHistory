using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Alsing.Design;

[ToolboxItem(false)]
public class CollectionEditorGui : UserControl
{
	public Button btnAdd;

	public Button btnCancel;

	public Button btnDown;

	public Button btnDropdown;

	public Button btnOK;

	public Button btnRemove;

	public Button btnUp;

	private Container components;

	private GroupBox groupBox1;

	public Label lblMembers;

	public Label lblProperties;

	public ListBox lstMembers;

	public Panel pnlMain;

	public Panel pnlMembers;

	public PropertyGrid pygProperties;

	public ComponaCollectionEditor Editor { get; set; }

	public IWindowsFormsEditorService EditorService { get; set; }

	public object EditValue { get; set; }

	public CollectionEditorGui()
	{
		InitializeComponent();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	public void Bind()
	{
		if (!(EditValue is ICollection collection))
		{
			MessageBox.Show("EditValue is null");
			return;
		}
		lstMembers.Items.Clear();
		foreach (object item in collection)
		{
			lstMembers.Items.Add(item);
		}
		EnableRemove();
		SelectObject();
	}

	private void lstMembers_DrawItem(object sender, DrawItemEventArgs e)
	{
		try
		{
			if (e.Index == -1)
			{
				return;
			}
			int count = lstMembers.Items.Count;
			int num = (int)e.Graphics.MeasureString(count.ToString(), lstMembers.Font).Width;
			if (num < 18)
			{
				num = 18;
			}
			Rectangle rectangle = new Rectangle(0, e.Bounds.Top, num, lstMembers.ItemHeight);
			int num2 = (Editor.GetPaintValueSupported() ? 20 : 0);
			Rectangle rectangle2 = new Rectangle(rectangle.Right + num2, rectangle.Top, e.Bounds.Width - rectangle.Right - num2, lstMembers.ItemHeight);
			ControlPaint.DrawBorder3D(e.Graphics, rectangle, Border3DStyle.Raised, Border3DSide.All);
			StringFormat genericDefault = StringFormat.GenericDefault;
			genericDefault.Alignment = StringAlignment.Far;
			rectangle.Inflate(-1, -1);
			e.Graphics.DrawString(e.Index.ToString(), lstMembers.Font, Brushes.Black, rectangle, genericDefault);
			bool flag = (e.State & DrawItemState.Selected) != 0;
			using SolidBrush brush = GetBgBrush(flag);
			using SolidBrush brush2 = GetFgBrush(flag);
			e.Graphics.FillRectangle(brush, rectangle2);
			if (flag && e.Index != -1 && (e.State & DrawItemState.Focus) != DrawItemState.None)
			{
				ControlPaint.DrawFocusRectangle(e.Graphics, rectangle2);
			}
			if (e.Index >= 0)
			{
				object item = lstMembers.Items[e.Index];
				string displayText = GetDisplayText(item);
				e.Graphics.DrawString(displayText, lstMembers.Font, brush2, rectangle2);
			}
		}
		catch
		{
		}
	}

	private SolidBrush GetFgBrush(bool Selected)
	{
		return Selected ? new SolidBrush(SystemColors.HighlightText) : new SolidBrush(lstMembers.ForeColor);
	}

	private SolidBrush GetBgBrush(bool Selected)
	{
		return Selected ? new SolidBrush(SystemColors.Highlight) : new SolidBrush(lstMembers.BackColor);
	}

	private void lstMembers_SelectedIndexChanged(object sender, EventArgs e)
	{
		EnableRemove();
		SelectObject();
	}

	private void SelectObject()
	{
		pygProperties.SelectedObject = ((lstMembers.SelectedIndex >= 0) ? lstMembers.SelectedItem : null);
	}

	private void EnableRemove()
	{
		btnRemove.Enabled = lstMembers.SelectedIndices.Count > 0;
	}

	private static string GetDisplayText(object Item)
	{
		string text = null;
		if (Item == null)
		{
			return string.Empty;
		}
		PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(Item)["Name"];
		if (propertyDescriptor != null)
		{
			text = (string)propertyDescriptor.GetValue(Item);
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = Item.GetType().Name;
		}
		return text;
	}

	public void AddObject(object o)
	{
		Editor.AddObject(o);
	}

	public void RemoveObject(object o)
	{
		Editor.RemoveObject(o);
	}

	private void btnDown_Click(object sender, EventArgs e)
	{
	}

	private void btnUp_Click(object sender, EventArgs e)
	{
	}

	private void InitializeComponent()
	{
		System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(typeof(Alsing.Design.CollectionEditorGui));
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.pygProperties = new System.Windows.Forms.PropertyGrid();
		this.lstMembers = new System.Windows.Forms.ListBox();
		this.btnUp = new System.Windows.Forms.Button();
		this.btnDown = new System.Windows.Forms.Button();
		this.pnlMembers = new System.Windows.Forms.Panel();
		this.btnRemove = new System.Windows.Forms.Button();
		this.lblMembers = new System.Windows.Forms.Label();
		this.btnDropdown = new System.Windows.Forms.Button();
		this.btnAdd = new System.Windows.Forms.Button();
		this.pnlMain = new System.Windows.Forms.Panel();
		this.lblProperties = new System.Windows.Forms.Label();
		this.pnlMembers.SuspendLayout();
		this.pnlMain.SuspendLayout();
		base.SuspendLayout();
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.Location = new System.Drawing.Point(456, 312);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.TabIndex = 0;
		this.btnCancel.Text = "Cancel";
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.btnOK.Location = new System.Drawing.Point(376, 312);
		this.btnOK.Name = "btnOK";
		this.btnOK.TabIndex = 1;
		this.btnOK.Text = "OK";
		this.groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.groupBox1.Location = new System.Drawing.Point(8, 296);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(520, 8);
		this.groupBox1.TabIndex = 2;
		this.groupBox1.TabStop = false;
		this.pygProperties.CommandsVisibleIfAvailable = true;
		this.pygProperties.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pygProperties.HelpVisible = false;
		this.pygProperties.LargeButtons = false;
		this.pygProperties.LineColor = System.Drawing.SystemColors.ScrollBar;
		this.pygProperties.Location = new System.Drawing.Point(240, 16);
		this.pygProperties.Name = "pygProperties";
		this.pygProperties.Size = new System.Drawing.Size(280, 280);
		this.pygProperties.TabIndex = 3;
		this.pygProperties.Text = "propertyGrid1";
		this.pygProperties.ToolbarVisible = false;
		this.pygProperties.ViewBackColor = System.Drawing.SystemColors.Window;
		this.pygProperties.ViewForeColor = System.Drawing.SystemColors.WindowText;
		this.lstMembers.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.lstMembers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.lstMembers.IntegralHeight = false;
		this.lstMembers.ItemHeight = 16;
		this.lstMembers.Location = new System.Drawing.Point(0, 16);
		this.lstMembers.Name = "lstMembers";
		this.lstMembers.Size = new System.Drawing.Size(208, 240);
		this.lstMembers.TabIndex = 4;
		this.lstMembers.DrawItem += new System.Windows.Forms.DrawItemEventHandler(lstMembers_DrawItem);
		this.lstMembers.SelectedIndexChanged += new System.EventHandler(lstMembers_SelectedIndexChanged);
		this.btnUp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnUp.Image = (System.Drawing.Bitmap)resourceManager.GetObject("btnUp.Image");
		this.btnUp.Location = new System.Drawing.Point(212, 16);
		this.btnUp.Name = "btnUp";
		this.btnUp.Size = new System.Drawing.Size(22, 28);
		this.btnUp.TabIndex = 5;
		this.btnUp.Click += new System.EventHandler(btnUp_Click);
		this.btnDown.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnDown.Image = (System.Drawing.Bitmap)resourceManager.GetObject("btnDown.Image");
		this.btnDown.Location = new System.Drawing.Point(212, 48);
		this.btnDown.Name = "btnDown";
		this.btnDown.Size = new System.Drawing.Size(22, 28);
		this.btnDown.TabIndex = 6;
		this.btnDown.Click += new System.EventHandler(btnDown_Click);
		this.pnlMembers.Controls.AddRange(new System.Windows.Forms.Control[7] { this.btnRemove, this.lstMembers, this.lblMembers, this.btnDown, this.btnUp, this.btnDropdown, this.btnAdd });
		this.pnlMembers.Dock = System.Windows.Forms.DockStyle.Left;
		this.pnlMembers.Name = "pnlMembers";
		this.pnlMembers.Size = new System.Drawing.Size(240, 296);
		this.pnlMembers.TabIndex = 7;
		this.btnRemove.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnRemove.Location = new System.Drawing.Point(136, 264);
		this.btnRemove.Name = "btnRemove";
		this.btnRemove.Size = new System.Drawing.Size(72, 24);
		this.btnRemove.TabIndex = 11;
		this.btnRemove.Text = "Remove";
		this.lblMembers.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblMembers.Name = "lblMembers";
		this.lblMembers.Size = new System.Drawing.Size(240, 16);
		this.lblMembers.TabIndex = 10;
		this.lblMembers.Text = "Members:";
		this.btnDropdown.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnDropdown.Image = (System.Drawing.Bitmap)resourceManager.GetObject("btnDropdown.Image");
		this.btnDropdown.Location = new System.Drawing.Point(95, 264);
		this.btnDropdown.Name = "btnDropdown";
		this.btnDropdown.Size = new System.Drawing.Size(24, 24);
		this.btnDropdown.TabIndex = 9;
		this.btnDropdown.Visible = false;
		this.btnAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.btnAdd.Location = new System.Drawing.Point(8, 264);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(88, 24);
		this.btnAdd.TabIndex = 12;
		this.btnAdd.Text = "Add";
		this.pnlMain.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		this.pnlMain.Controls.AddRange(new System.Windows.Forms.Control[3] { this.pygProperties, this.lblProperties, this.pnlMembers });
		this.pnlMain.Location = new System.Drawing.Point(8, 0);
		this.pnlMain.Name = "pnlMain";
		this.pnlMain.Size = new System.Drawing.Size(520, 296);
		this.pnlMain.TabIndex = 8;
		this.lblProperties.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblProperties.Location = new System.Drawing.Point(240, 0);
		this.lblProperties.Name = "lblProperties";
		this.lblProperties.Size = new System.Drawing.Size(280, 16);
		this.lblProperties.TabIndex = 9;
		this.lblProperties.Text = "Properties:";
		base.Controls.AddRange(new System.Windows.Forms.Control[4] { this.groupBox1, this.btnOK, this.btnCancel, this.pnlMain });
		base.Name = "CollectionEditorGui";
		base.Size = new System.Drawing.Size(536, 352);
		this.pnlMembers.ResumeLayout(false);
		this.pnlMain.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
