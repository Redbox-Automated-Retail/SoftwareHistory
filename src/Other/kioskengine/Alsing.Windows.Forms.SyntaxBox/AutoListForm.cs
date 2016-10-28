using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.SyntaxBox;

[ToolboxItem(false)]
[ComVisible(true)]
public class AutoListForm : Form
{
	private IContainer components;

	public ImageList Images;

	private TabListBox LB;

	private ToolTip tooltip;

	public string SelectedText
	{
		get
		{
			if (LB.SelectedItem == null)
			{
				return "";
			}
			ListItem listItem = (ListItem)LB.SelectedItem;
			return listItem.InsertText;
		}
	}

	public AutoListForm()
	{
		InitializeComponent();
		SetStyle(ControlStyles.Selectable, value: true);
	}

	[DllImport("User32.dll")]
	private static extern int SendMessage(IntPtr hWnd, int message, int _data, int _id);

	public void SendKey(int KeyCode)
	{
		SendMessage(LB.Handle, 256, KeyCode, 0);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		e.Graphics.Clear(SystemColors.Control);
		ControlPaint.DrawBorder3D(e.Graphics, 0, 0, base.Width, base.Height, Border3DStyle.Raised);
	}

	public void SelectItem(string text)
	{
		text = text.ToLowerInvariant();
		for (int i = 0; i < LB.Items.Count; i++)
		{
			ListItem listItem = (ListItem)LB.Items[i];
			string text2 = listItem.Text.ToLowerInvariant();
			if (text2.StartsWith(text))
			{
				LB.SelectedIndex = i;
				break;
			}
		}
	}

	private void LB_KeyDown(object sender, KeyEventArgs e)
	{
		OnKeyDown(e);
	}

	private void LB_KeyPress(object sender, KeyPressEventArgs e)
	{
		OnKeyPress(e);
	}

	private void LB_KeyUp(object sender, KeyEventArgs e)
	{
		OnKeyUp(e);
	}

	protected override bool IsInputKey(Keys keyData)
	{
		return true;
	}

	protected override bool IsInputChar(char charCode)
	{
		return true;
	}

	public ListItem Add(string text, int ImageIndex)
	{
		return Add(text, text, ImageIndex);
	}

	public ListItem Add(string text, string InsertText, int ImageIndex)
	{
		ListItem listItem = new ListItem(text, ImageIndex, "", InsertText);
		LB.Items.Add(listItem);
		return listItem;
	}

	public ListItem Add(string text, string InsertText, string ToolTip, int ImageIndex)
	{
		ListItem listItem = new ListItem(text, ImageIndex, "", InsertText);
		LB.Items.Add(listItem);
		listItem.ToolTip = ToolTip;
		return listItem;
	}

	public void Clear()
	{
		LB.Items.Clear();
	}

	private void LB_DrawItem(object sender, DrawItemEventArgs e)
	{
		bool flag = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
		if (e.Index != -1)
		{
			ListItem listItem = (ListItem)LB.Items[e.Index];
			string s = listItem.Text;
			Brush highlight;
			Brush brush;
			if (flag)
			{
				highlight = SystemBrushes.Highlight;
				brush = SystemBrushes.HighlightText;
			}
			else
			{
				highlight = SystemBrushes.Window;
				brush = SystemBrushes.WindowText;
			}
			if (!flag)
			{
				e.Graphics.FillRectangle(highlight, 0, e.Bounds.Top, e.Bounds.Width, LB.ItemHeight);
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, 24, e.Bounds.Top, e.Bounds.Width - 24, LB.ItemHeight);
				e.Graphics.FillRectangle(SystemBrushes.Highlight, new Rectangle(25, e.Bounds.Top + 1, e.Bounds.Width - 24 - 2, LB.ItemHeight - 2));
				ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle(24, e.Bounds.Top, e.Bounds.Width - 24, LB.ItemHeight));
			}
			e.Graphics.DrawString(s, e.Font, brush, 26f, e.Bounds.Top + 1);
			if (Images != null)
			{
				e.Graphics.DrawImage(Images.Images[listItem.Type], 6, e.Bounds.Top);
			}
		}
	}

	private void LB_DoubleClick(object sender, EventArgs e)
	{
		OnDoubleClick(e);
	}

	public void BeginLoad()
	{
		LB.Sorted = false;
		LB.DrawMode = DrawMode.Normal;
		LB.SuspendLayout();
	}

	public void EndLoad()
	{
		LB.ResumeLayout();
		LB.Sorted = true;
		LB.DrawMode = DrawMode.OwnerDrawFixed;
		base.Height = 0;
		if (LB.Items.Count > 10)
		{
			base.Height = LB.ItemHeight * 11 + 12;
		}
		else
		{
			base.Height = LB.ItemHeight * LB.Items.Count + 12;
		}
		int num = 0;
		Graphics graphics = LB.CreateGraphics();
		foreach (ListItem item in LB.Items)
		{
			int num2 = (int)graphics.MeasureString(item.Text, LB.Font).Width + 45;
			if (num2 > num)
			{
				num = num2;
			}
		}
		base.Width = num + SystemInformation.VerticalScrollBarWidth;
		Refresh();
		graphics.Dispose();
	}

	private void AutoListForm_Resize(object sender, EventArgs e)
	{
		LB.Size = new Size(base.Width - 8, base.Height - 8);
	}

	private void LB_SelectedIndexChanged(object sender, EventArgs e)
	{
		ListItem listItem = (ListItem)LB.SelectedItem;
		if (listItem.ToolTip != "")
		{
			tooltip.ShowAlways = true;
			tooltip.SetToolTip(LB, listItem.ToolTip);
			tooltip.InitialDelay = 2;
			tooltip.Active = true;
		}
	}

	private void LB_MouseDown(object sender, MouseEventArgs e)
	{
		SelectItem(e.X, e.Y);
	}

	private void SelectItem(int x, int y)
	{
		int num = new Point(x, y).Y / LB.ItemHeight + LB.TopIndex;
		if (num != LB.SelectedIndex && num < LB.Items.Count && num >= 0)
		{
			LB.SelectedIndex = num;
		}
	}

	private void LB_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.None)
		{
			SelectItem(e.X, e.Y);
		}
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.LB = new Alsing.Windows.Forms.SyntaxBox.TabListBox();
		this.tooltip = new System.Windows.Forms.ToolTip(this.components);
		base.SuspendLayout();
		this.LB.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.LB.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
		this.LB.Font = new System.Drawing.Font("Tahoma", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.LB.IntegralHeight = false;
		this.LB.ItemHeight = 16;
		this.LB.Location = new System.Drawing.Point(4, 4);
		this.LB.Name = "LB";
		this.LB.Size = new System.Drawing.Size(168, 184);
		this.LB.Sorted = true;
		this.LB.TabIndex = 0;
		this.LB.KeyDown += new System.Windows.Forms.KeyEventHandler(LB_KeyDown);
		this.LB.MouseDown += new System.Windows.Forms.MouseEventHandler(LB_MouseDown);
		this.LB.KeyPress += new System.Windows.Forms.KeyPressEventHandler(LB_KeyPress);
		this.LB.DoubleClick += new System.EventHandler(LB_DoubleClick);
		this.LB.KeyUp += new System.Windows.Forms.KeyEventHandler(LB_KeyUp);
		this.LB.SelectedIndexChanged += new System.EventHandler(LB_SelectedIndexChanged);
		this.LB.MouseMove += new System.Windows.Forms.MouseEventHandler(LB_MouseMove);
		this.LB.DrawItem += new System.Windows.Forms.DrawItemEventHandler(LB_DrawItem);
		this.tooltip.AutoPopDelay = 5000;
		this.tooltip.InitialDelay = 100;
		this.tooltip.ReshowDelay = 100;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.ClientSize = new System.Drawing.Size(168, 165);
		base.Controls.AddRange(new System.Windows.Forms.Control[1] { this.LB });
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Name = "AutoListForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		base.Resize += new System.EventHandler(AutoListForm_Resize);
		base.ResumeLayout(false);
	}
}
