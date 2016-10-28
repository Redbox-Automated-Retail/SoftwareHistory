using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Alsing.SourceCode;

[ComVisible(true)]
public class TextStyleDesignerDialog : Form
{
	private readonly TextStyle _Style;

	private readonly TextStyle _TmpStyle;

	private Button btnCancel;

	private Button btnOK;

	private Container components;

	private Label lblCaption;

	private Label lblPreview;

	private Panel panel1;

	private Panel panel2;

	private Panel panel3;

	private PropertyGrid pgStyles;

	public static event EventHandler Change;

	public TextStyleDesignerDialog(TextStyle Style)
	{
		_Style = Style;
		_TmpStyle = (TextStyle)Style.Clone();
		InitializeComponent();
		pgStyles.SelectedObject = _TmpStyle;
		lblCaption.Text = _Style.ToString();
		PreviewStyle();
	}

	protected static void OnChange()
	{
		if (Change != null)
		{
			Change(null, EventArgs.Empty);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void pgStyles_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
	{
		PreviewStyle();
	}

	private void PreviewStyle()
	{
		TextStyle tmpStyle = _TmpStyle;
		lblPreview.ForeColor = tmpStyle.ForeColor;
		if (tmpStyle.BackColor != Color.Transparent)
		{
			lblPreview.BackColor = tmpStyle.BackColor;
		}
		else
		{
			lblPreview.BackColor = Color.White;
		}
		FontStyle fontStyle = FontStyle.Regular;
		if (tmpStyle.Bold)
		{
			fontStyle |= FontStyle.Bold;
		}
		if (tmpStyle.Italic)
		{
			fontStyle |= FontStyle.Italic;
		}
		if (tmpStyle.Underline)
		{
			fontStyle |= FontStyle.Underline;
		}
		lblPreview.Font = new Font("Courier New", 11f, fontStyle);
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		_Style.BackColor = _TmpStyle.BackColor;
		_Style.ForeColor = _TmpStyle.ForeColor;
		_Style.Bold = _TmpStyle.Bold;
		_Style.Italic = _TmpStyle.Italic;
		_Style.Underline = _TmpStyle.Underline;
		OnChange();
		base.DialogResult = DialogResult.OK;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
	}

	private void TextStyleDesignerDialog_Load(object sender, EventArgs e)
	{
	}

	private void InitializeComponent()
	{
		this.panel2 = new System.Windows.Forms.Panel();
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		this.pgStyles = new System.Windows.Forms.PropertyGrid();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel3 = new System.Windows.Forms.Panel();
		this.lblCaption = new System.Windows.Forms.Label();
		this.lblPreview = new System.Windows.Forms.Label();
		this.panel2.SuspendLayout();
		this.panel3.SuspendLayout();
		base.SuspendLayout();
		this.panel2.Controls.AddRange(new System.Windows.Forms.Control[3] { this.lblPreview, this.btnCancel, this.btnOK });
		this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel2.Location = new System.Drawing.Point(4, 255);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(354, 80);
		this.panel2.TabIndex = 8;
		this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(279, 48);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
		this.btnOK.Location = new System.Drawing.Point(200, 48);
		this.btnOK.Name = "btnOK";
		this.btnOK.TabIndex = 3;
		this.btnOK.Text = "OK";
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.pgStyles.CommandsVisibleIfAvailable = true;
		this.pgStyles.Dock = System.Windows.Forms.DockStyle.Fill;
		this.pgStyles.LargeButtons = false;
		this.pgStyles.LineColor = System.Drawing.SystemColors.ScrollBar;
		this.pgStyles.Location = new System.Drawing.Point(4, 26);
		this.pgStyles.Name = "pgStyles";
		this.pgStyles.Size = new System.Drawing.Size(354, 221);
		this.pgStyles.TabIndex = 6;
		this.pgStyles.Text = "propertyGrid1";
		this.pgStyles.ToolbarVisible = false;
		this.pgStyles.ViewBackColor = System.Drawing.SystemColors.Window;
		this.pgStyles.ViewForeColor = System.Drawing.SystemColors.WindowText;
		this.pgStyles.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(pgStyles_PropertyValueChanged);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.panel1.Location = new System.Drawing.Point(4, 247);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(354, 8);
		this.panel1.TabIndex = 9;
		this.panel3.BackColor = System.Drawing.SystemColors.ControlDark;
		this.panel3.Controls.AddRange(new System.Windows.Forms.Control[1] { this.lblCaption });
		this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel3.Location = new System.Drawing.Point(4, 2);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(354, 24);
		this.panel3.TabIndex = 10;
		this.lblCaption.Dock = System.Windows.Forms.DockStyle.Fill;
		this.lblCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.lblCaption.ForeColor = System.Drawing.SystemColors.Window;
		this.lblCaption.Name = "lblCaption";
		this.lblCaption.Size = new System.Drawing.Size(354, 24);
		this.lblCaption.TabIndex = 0;
		this.lblCaption.Text = "-";
		this.lblCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.lblPreview.BackColor = System.Drawing.SystemColors.Window;
		this.lblPreview.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.lblPreview.Dock = System.Windows.Forms.DockStyle.Top;
		this.lblPreview.Font = new System.Drawing.Font("Courier New", 10f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.lblPreview.Name = "lblPreview";
		this.lblPreview.Size = new System.Drawing.Size(354, 40);
		this.lblPreview.TabIndex = 8;
		this.lblPreview.Text = "The quick brown fox jumped over the lazy dog.        ";
		this.lblPreview.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		base.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(362, 335);
		base.ControlBox = false;
		base.Controls.AddRange(new System.Windows.Forms.Control[4] { this.pgStyles, this.panel3, this.panel1, this.panel2 });
		base.DockPadding.Left = 4;
		base.DockPadding.Right = 4;
		base.DockPadding.Top = 2;
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.Name = "TextStyleDesignerDialog";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Style Designer";
		base.Load += new System.EventHandler(TextStyleDesignerDialog_Load);
		this.panel2.ResumeLayout(false);
		this.panel3.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
