using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

public class AdvancedMode : Form
{
	private readonly ConfigPropertyList PropertyList;

	private readonly HardwareService Service;

	private IContainer components;

	private Panel panel1;

	private Button m_exitButton;

	private Button button2;

	private Button button1;

	public AdvancedMode(HardwareService service)
	{
		InitializeComponent();
		Service = service;
		PropertyList = new ConfigPropertyList(service);
		panel1.Controls.Add(PropertyList);
	}

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		PropertyList.Save();
		PropertyList.Dispose();
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		try
		{
			DeckConfigurationForm deckConfigurationForm = new DeckConfigurationForm(Service, allowInventoryUpdate: true);
			deckConfigurationForm.ShowDialog();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Unable to run Decks form.", e2);
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

	private void InitializeComponent()
	{
		this.panel1 = new System.Windows.Forms.Panel();
		this.m_exitButton = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.button1 = new System.Windows.Forms.Button();
		base.SuspendLayout();
		this.panel1.Location = new System.Drawing.Point(12, 12);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(371, 492);
		this.panel1.TabIndex = 0;
		this.m_exitButton.BackColor = System.Drawing.Color.GreenYellow;
		this.m_exitButton.Location = new System.Drawing.Point(583, 461);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(112, 42);
		this.m_exitButton.TabIndex = 1;
		this.m_exitButton.Text = "Exit";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		this.button2.Location = new System.Drawing.Point(437, 461);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(113, 43);
		this.button2.TabIndex = 2;
		this.button2.Text = "Cancel";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.button1.Location = new System.Drawing.Point(437, 12);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(166, 61);
		this.button1.TabIndex = 3;
		this.button1.Text = "Decks Configuration";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(748, 535);
		base.Controls.Add(this.button1);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.m_exitButton);
		base.Controls.Add(this.panel1);
		base.Name = "AdvancedMode";
		this.Text = "AdvancedMode";
		base.ResumeLayout(false);
	}
}
