using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class SimulatedPositionForm : Form
{
	private IContainer components;

	private Button button1;

	private Label label1;

	private Label label2;

	private TextBox m_xPos;

	private TextBox m_yPos;

	internal int StartX => ToInt(m_xPos.Text);

	internal int StartY => ToInt(m_yPos.Text);

	public SimulatedPositionForm()
	{
		InitializeComponent();
	}

	private int ToInt(string s)
	{
		if (int.TryParse(s, out var result))
		{
			return result;
		}
		return -1;
	}

	private void button1_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
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
		this.button1 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_xPos = new System.Windows.Forms.TextBox();
		this.m_yPos = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.button1.Location = new System.Drawing.Point(44, 204);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(117, 41);
		this.button1.TabIndex = 0;
		this.button1.Text = "Ok";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(36, 38);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(14, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "X";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(37, 67);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(14, 13);
		this.label2.TabIndex = 2;
		this.label2.Text = "Y";
		this.m_xPos.Location = new System.Drawing.Point(56, 31);
		this.m_xPos.Name = "m_xPos";
		this.m_xPos.Size = new System.Drawing.Size(105, 20);
		this.m_xPos.TabIndex = 3;
		this.m_yPos.Location = new System.Drawing.Point(56, 60);
		this.m_yPos.Name = "m_yPos";
		this.m_yPos.Size = new System.Drawing.Size(105, 20);
		this.m_yPos.TabIndex = 4;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(411, 265);
		base.Controls.Add(this.m_yPos);
		base.Controls.Add(this.m_xPos);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button1);
		base.Name = "SimulatedPositionForm";
		this.Text = "SimulatedPositionForm";
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
