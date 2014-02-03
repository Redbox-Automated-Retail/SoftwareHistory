using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.KernelExtension;

public class PlaceOrderPrefPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private Label m_headerLabel;

	private ComboBox m_ResponseCodeComboBox;

	private Label label1;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private Label label2;

	private TextBox m_orderIdTextBox;

	private CheckBox m_simulateServerTimeoutCheckBox;

	public PlaceOrderPrefPage()
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
		m_liveCallCheckBox.Checked = TicketServiceProxy.Instance.LiveCallPlaceOrder;
		m_simulateServerBusyCheckBox.Checked = TicketServiceProxy.Instance.ServerBusyPlaceOrder;
		m_simulateCommunicationsFailureCheckBox.Checked = TicketServiceProxy.Instance.CommFailurePlaceOrder;
		m_simulateServerTimeoutCheckBox.Checked = TicketServiceProxy.Instance.ServerTimeoutPlaceOrder;
		m_orderIdTextBox.Text = TicketServiceProxy.Instance.PlaceOrderOrderId.ToString();
		m_ResponseCodeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(TicketsResponseCode));
		foreach (string item in names)
		{
			m_ResponseCodeComboBox.Items.Add(item);
		}
		m_ResponseCodeComboBox.SelectedIndex = m_ResponseCodeComboBox.FindString(TicketServiceProxy.Instance.PlaceOrderResponseCode.ToString());
		m_isLoaded = true;
	}

	private void m_liveCallCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.LiveCallPlaceOrder = m_liveCallCheckBox.Checked;
		}
	}

	private void m_simulateCommunicationsFailureCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.CommFailurePlaceOrder = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void m_simulateServerBusyCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerBusyPlaceOrder = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void m_simulateServerTimeoutCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerTimeoutPlaceOrder = m_simulateServerTimeoutCheckBox.Checked;
		}
	}

	private void m_ResponseCodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.PlaceOrderResponseCode = Enum<TicketsResponseCode>.ParseIgnoringCase(m_ResponseCodeComboBox.SelectedItem.ToString(), TicketsResponseCode.Success);
		}
	}

	private void m_orderIdTextBox_TextChanged(object sender, EventArgs e)
	{
		if (int.TryParse(m_orderIdTextBox.Text, out var result))
		{
			TicketServiceProxy.Instance.PlaceOrderOrderId = result;
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
		this.m_ResponseCodeComboBox = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_orderIdTextBox = new System.Windows.Forms.TextBox();
		this.m_simulateServerTimeoutCheckBox = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkGoldenrod;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 45;
		this.m_headerLabel.Text = "  Ticket Services: Place Order";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_ResponseCodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_ResponseCodeComboBox.FormattingEnabled = true;
		this.m_ResponseCodeComboBox.Location = new System.Drawing.Point(111, 155);
		this.m_ResponseCodeComboBox.Name = "m_ResponseCodeComboBox";
		this.m_ResponseCodeComboBox.Size = new System.Drawing.Size(329, 21);
		this.m_ResponseCodeComboBox.TabIndex = 48;
		this.m_ResponseCodeComboBox.SelectedIndexChanged += new System.EventHandler(m_ResponseCodeComboBox_SelectedIndexChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(19, 158);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(86, 13);
		this.label1.TabIndex = 47;
		this.label1.Text = "Response Code:";
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(22, 103);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 55;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(m_simulateServerBusyCheckBox_CheckedChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(22, 80);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 54;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(m_simulateCommunicationsFailureCheckBox_CheckedChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(22, 57);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 46;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(m_liveCallCheckBox_CheckedChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(55, 191);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(50, 13);
		this.label2.TabIndex = 50;
		this.label2.Text = "Order ID:";
		this.m_orderIdTextBox.Enabled = false;
		this.m_orderIdTextBox.Location = new System.Drawing.Point(111, 188);
		this.m_orderIdTextBox.Name = "m_orderIdTextBox";
		this.m_orderIdTextBox.Size = new System.Drawing.Size(132, 20);
		this.m_orderIdTextBox.TabIndex = 51;
		this.m_orderIdTextBox.TextChanged += new System.EventHandler(m_orderIdTextBox_TextChanged);
		this.m_simulateServerTimeoutCheckBox.AutoSize = true;
		this.m_simulateServerTimeoutCheckBox.Location = new System.Drawing.Point(22, 126);
		this.m_simulateServerTimeoutCheckBox.Name = "m_simulateServerTimeoutCheckBox";
		this.m_simulateServerTimeoutCheckBox.Size = new System.Drawing.Size(141, 17);
		this.m_simulateServerTimeoutCheckBox.TabIndex = 56;
		this.m_simulateServerTimeoutCheckBox.Text = "Simulate Server Timeout";
		this.m_simulateServerTimeoutCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerTimeoutCheckBox.CheckedChanged += new System.EventHandler(m_simulateServerTimeoutCheckBox_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_simulateServerTimeoutCheckBox);
		base.Controls.Add(this.m_orderIdTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_ResponseCodeComboBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "PlaceOrderPrefPage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
