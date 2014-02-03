using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy;

namespace Redbox.TicketServices.KernelExtension;

public class GeneralPrefPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private Label m_headerLabel;

	private TextBox m_ticketServicesTimeoutTextBox;

	private Label label4;

	private TextBox m_ticketServicesUrlTextBox;

	private Label label5;

	public GeneralPrefPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_ticketServicesUrlTextBox.Text = TicketServiceProxy.Instance.Url;
		m_ticketServicesTimeoutTextBox.Text = TicketServiceProxy.Instance.Timeout.ToString();
		m_isLoaded = true;
	}

	private void m_ticketServicesTimeoutTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && int.TryParse(m_ticketServicesTimeoutTextBox.Text, out var result))
		{
			TicketServiceProxy.Instance.Timeout = result;
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
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_ticketServicesTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_ticketServicesUrlTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkGoldenrod;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 43;
		this.m_headerLabel.Text = "  Ticket Services: General Settings";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_ticketServicesTimeoutTextBox.Location = new System.Drawing.Point(109, 94);
		this.m_ticketServicesTimeoutTextBox.MaxLength = 6;
		this.m_ticketServicesTimeoutTextBox.Name = "m_ticketServicesTimeoutTextBox";
		this.m_ticketServicesTimeoutTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_ticketServicesTimeoutTextBox.TabIndex = 47;
		this.m_ticketServicesTimeoutTextBox.Leave += new System.EventHandler(m_ticketServicesTimeoutTextBox_Leave);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(16, 97);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(87, 13);
		this.label4.TabIndex = 46;
		this.label4.Text = "Service Timeout:";
		this.m_ticketServicesUrlTextBox.BackColor = System.Drawing.Color.Gainsboro;
		this.m_ticketServicesUrlTextBox.Location = new System.Drawing.Point(109, 68);
		this.m_ticketServicesUrlTextBox.Name = "m_ticketServicesUrlTextBox";
		this.m_ticketServicesUrlTextBox.ReadOnly = true;
		this.m_ticketServicesUrlTextBox.Size = new System.Drawing.Size(399, 20);
		this.m_ticketServicesUrlTextBox.TabIndex = 45;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(32, 71);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(71, 13);
		this.label5.TabIndex = 44;
		this.label5.Text = "Service URL:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_ticketServicesTimeoutTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_ticketServicesUrlTextBox);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "GeneralPrefPage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
