using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.SyntaxBox;

public class GotoLineForm : Form
{
	private readonly EditViewControl mOwner;

	private Button btnCancel;

	private Button btnOK;

	private Container components;

	private Label lblLines;

	private TextBox txtRow;

	public GotoLineForm()
	{
		InitializeComponent();
	}

	public GotoLineForm(EditViewControl Owner, int RowCount)
	{
		InitializeComponent();
		lblLines.Text = "Line number (1-" + RowCount.ToString(CultureInfo.InvariantCulture) + "):";
		mOwner = Owner;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void btnOK_Click(object sender, EventArgs e)
	{
		try
		{
			int rowIndex = int.Parse(txtRow.Text) - 1;
			mOwner.GotoLine(rowIndex);
		}
		catch
		{
		}
		Close();
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void GotoLine_Closing(object sender, CancelEventArgs e)
	{
	}

	private void GotoLine_Activated(object sender, EventArgs e)
	{
		txtRow.Focus();
	}

	private void InitializeComponent()
	{
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnOK = new System.Windows.Forms.Button();
		this.txtRow = new System.Windows.Forms.TextBox();
		this.lblLines = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(160, 48);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(72, 24);
		this.btnCancel.TabIndex = 0;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.btnOK.Location = new System.Drawing.Point(80, 48);
		this.btnOK.Name = "btnOK";
		this.btnOK.Size = new System.Drawing.Size(72, 24);
		this.btnOK.TabIndex = 1;
		this.btnOK.Text = "OK";
		this.btnOK.Click += new System.EventHandler(btnOK_Click);
		this.txtRow.Location = new System.Drawing.Point(8, 24);
		this.txtRow.Name = "txtRow";
		this.txtRow.Size = new System.Drawing.Size(224, 20);
		this.txtRow.TabIndex = 2;
		this.txtRow.Text = "";
		this.lblLines.Location = new System.Drawing.Point(8, 8);
		this.lblLines.Name = "lblLines";
		this.lblLines.Size = new System.Drawing.Size(128, 16);
		this.lblLines.TabIndex = 3;
		this.lblLines.Text = "-";
		base.AcceptButton = this.btnOK;
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		base.CancelButton = this.btnCancel;
		base.ClientSize = new System.Drawing.Size(242, 82);
		base.Controls.AddRange(new System.Windows.Forms.Control[4] { this.lblLines, this.txtRow, this.btnOK, this.btnCancel });
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
		base.Name = "GotoLineForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Go To Line";
		base.Closing += new System.ComponentModel.CancelEventHandler(GotoLine_Closing);
		base.Activated += new System.EventHandler(GotoLine_Activated);
		base.ResumeLayout(false);
	}
}
