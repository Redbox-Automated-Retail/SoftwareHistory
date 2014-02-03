using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class ViewSlotDataForm : Form
{
	private IContainer components;

	private TableLayoutPanel tableLayoutPanel1;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label label4;

	public ViewSlotDataForm()
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

	private void InitializeComponent()
	{
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.tableLayoutPanel1.ColumnCount = 6;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 91f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 102f));
		this.tableLayoutPanel1.Location = new System.Drawing.Point(110, 52);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 10;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(549, 208);
		this.tableLayoutPanel1.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(117, 26);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(60, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Quadrant 1";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(566, 31);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(60, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Quadrant 6";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(25, 52);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(42, 13);
		this.label3.TabIndex = 3;
		this.label3.Text = "Deck 1";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(25, 199);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(42, 13);
		this.label4.TabIndex = 4;
		this.label4.Text = "Deck 8";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(694, 460);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.tableLayoutPanel1);
		base.Name = "ViewSlotDataForm";
		this.Text = "ViewSlotDataForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
