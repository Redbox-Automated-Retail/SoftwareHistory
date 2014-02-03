using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy;

namespace Redbox.TicketServices.KernelExtension;

public class EmailPrefPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private Label m_headerLabel;

	private Label label1;

	private TextBox m_ticketServicesEmailForCard;

	private CheckBox m_simServerBusyGetEmailByCardIDCheckBox;

	private CheckBox m_simComFailureGetEmailByCardIDCheckBox;

	private GroupBox groupBox1;

	private CheckBox m_liveCallGetEmailByCardID;

	private GroupBox groupBox2;

	private CheckBox m_liveCallSendNotification;

	private CheckBox m_simComFailureSendNotification;

	private CheckBox m_simServerBusySendNotification;

	public EmailPrefPage()
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
		m_liveCallGetEmailByCardID.Checked = TicketServiceProxy.Instance.LiveCallGetEmailByCardID;
		m_ticketServicesEmailForCard.Text = TicketServiceProxy.Instance.GetEmailByCardReturnValue;
		m_simComFailureGetEmailByCardIDCheckBox.Checked = TicketServiceProxy.Instance.CommFailureGetEmailByCardID;
		m_simServerBusyGetEmailByCardIDCheckBox.Checked = TicketServiceProxy.Instance.ServerBusyGetEmailByCardID;
		m_liveCallSendNotification.Checked = TicketServiceProxy.Instance.LiveCallSendNotification;
		m_simComFailureSendNotification.Checked = TicketServiceProxy.Instance.CommFailureSendNotification;
		m_simServerBusySendNotification.Checked = TicketServiceProxy.Instance.ServerBusySendNotification;
		m_isLoaded = true;
	}

	private void m_simulateCommunicationsFailureCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.CommFailureGetEmailByCardID = m_simComFailureGetEmailByCardIDCheckBox.Checked;
		}
	}

	private void m_simulateServerBusyCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerBusyGetEmailByCardID = m_simServerBusyGetEmailByCardIDCheckBox.Checked;
		}
	}

	private void m_ticketServicesEmailForCard_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.GetEmailByCardReturnValue = m_ticketServicesEmailForCard.Text;
		}
	}

	private void m_liveCallGetEmailByCardID_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.LiveCallGetEmailByCardID = m_liveCallGetEmailByCardID.Checked;
		}
	}

	private void m_liveCallSendNotification_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.LiveCallSendNotification = m_liveCallSendNotification.Checked;
		}
	}

	private void m_simComFailureSendNotification_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.CommFailureSendNotification = m_simComFailureSendNotification.Checked;
		}
	}

	private void m_simServerBusySendNotification_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerBusySendNotification = m_simServerBusySendNotification.Checked;
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
		this.label1 = new System.Windows.Forms.Label();
		this.m_ticketServicesEmailForCard = new System.Windows.Forms.TextBox();
		this.m_simServerBusyGetEmailByCardIDCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simComFailureGetEmailByCardIDCheckBox = new System.Windows.Forms.CheckBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_liveCallGetEmailByCardID = new System.Windows.Forms.CheckBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_liveCallSendNotification = new System.Windows.Forms.CheckBox();
		this.m_simComFailureSendNotification = new System.Windows.Forms.CheckBox();
		this.m_simServerBusySendNotification = new System.Windows.Forms.CheckBox();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkGoldenrod;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 46;
		this.m_headerLabel.Text = "  Ticket Services: Email Messages";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(20, 113);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(35, 13);
		this.label1.TabIndex = 47;
		this.label1.Text = "Email:";
		this.m_ticketServicesEmailForCard.Location = new System.Drawing.Point(61, 110);
		this.m_ticketServicesEmailForCard.Name = "m_ticketServicesEmailForCard";
		this.m_ticketServicesEmailForCard.Size = new System.Drawing.Size(200, 20);
		this.m_ticketServicesEmailForCard.TabIndex = 48;
		this.m_ticketServicesEmailForCard.Leave += new System.EventHandler(m_ticketServicesEmailForCard_Leave);
		this.m_simServerBusyGetEmailByCardIDCheckBox.AutoSize = true;
		this.m_simServerBusyGetEmailByCardIDCheckBox.Location = new System.Drawing.Point(23, 76);
		this.m_simServerBusyGetEmailByCardIDCheckBox.Name = "m_simServerBusyGetEmailByCardIDCheckBox";
		this.m_simServerBusyGetEmailByCardIDCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simServerBusyGetEmailByCardIDCheckBox.TabIndex = 57;
		this.m_simServerBusyGetEmailByCardIDCheckBox.Text = "Simulate Server Busy";
		this.m_simServerBusyGetEmailByCardIDCheckBox.UseVisualStyleBackColor = true;
		this.m_simServerBusyGetEmailByCardIDCheckBox.CheckedChanged += new System.EventHandler(m_simulateServerBusyCheckBox_CheckedChanged);
		this.m_simComFailureGetEmailByCardIDCheckBox.AutoSize = true;
		this.m_simComFailureGetEmailByCardIDCheckBox.Location = new System.Drawing.Point(23, 53);
		this.m_simComFailureGetEmailByCardIDCheckBox.Name = "m_simComFailureGetEmailByCardIDCheckBox";
		this.m_simComFailureGetEmailByCardIDCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simComFailureGetEmailByCardIDCheckBox.TabIndex = 56;
		this.m_simComFailureGetEmailByCardIDCheckBox.Text = "Simulate Communications Failure";
		this.m_simComFailureGetEmailByCardIDCheckBox.UseVisualStyleBackColor = true;
		this.m_simComFailureGetEmailByCardIDCheckBox.CheckedChanged += new System.EventHandler(m_simulateCommunicationsFailureCheckBox_CheckedChanged);
		this.groupBox1.Controls.Add(this.m_liveCallGetEmailByCardID);
		this.groupBox1.Controls.Add(this.m_ticketServicesEmailForCard);
		this.groupBox1.Controls.Add(this.m_simComFailureGetEmailByCardIDCheckBox);
		this.groupBox1.Controls.Add(this.m_simServerBusyGetEmailByCardIDCheckBox);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Location = new System.Drawing.Point(17, 239);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(507, 145);
		this.groupBox1.TabIndex = 58;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Get Email by Card ID";
		this.m_liveCallGetEmailByCardID.AutoSize = true;
		this.m_liveCallGetEmailByCardID.Location = new System.Drawing.Point(23, 30);
		this.m_liveCallGetEmailByCardID.Name = "m_liveCallGetEmailByCardID";
		this.m_liveCallGetEmailByCardID.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallGetEmailByCardID.TabIndex = 58;
		this.m_liveCallGetEmailByCardID.Text = "Live Call";
		this.m_liveCallGetEmailByCardID.UseVisualStyleBackColor = true;
		this.m_liveCallGetEmailByCardID.CheckedChanged += new System.EventHandler(m_liveCallGetEmailByCardID_CheckedChanged);
		this.groupBox2.Controls.Add(this.m_liveCallSendNotification);
		this.groupBox2.Controls.Add(this.m_simComFailureSendNotification);
		this.groupBox2.Controls.Add(this.m_simServerBusySendNotification);
		this.groupBox2.Location = new System.Drawing.Point(17, 53);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(507, 168);
		this.groupBox2.TabIndex = 59;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Send Notifications";
		this.m_liveCallSendNotification.AutoSize = true;
		this.m_liveCallSendNotification.Location = new System.Drawing.Point(23, 28);
		this.m_liveCallSendNotification.Name = "m_liveCallSendNotification";
		this.m_liveCallSendNotification.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallSendNotification.TabIndex = 61;
		this.m_liveCallSendNotification.Text = "Live Call";
		this.m_liveCallSendNotification.UseVisualStyleBackColor = true;
		this.m_liveCallSendNotification.CheckedChanged += new System.EventHandler(m_liveCallSendNotification_CheckedChanged);
		this.m_simComFailureSendNotification.AutoSize = true;
		this.m_simComFailureSendNotification.Location = new System.Drawing.Point(23, 51);
		this.m_simComFailureSendNotification.Name = "m_simComFailureSendNotification";
		this.m_simComFailureSendNotification.Size = new System.Drawing.Size(180, 17);
		this.m_simComFailureSendNotification.TabIndex = 59;
		this.m_simComFailureSendNotification.Text = "Simulate Communications Failure";
		this.m_simComFailureSendNotification.UseVisualStyleBackColor = true;
		this.m_simComFailureSendNotification.CheckedChanged += new System.EventHandler(m_simComFailureSendNotification_CheckedChanged);
		this.m_simServerBusySendNotification.AutoSize = true;
		this.m_simServerBusySendNotification.Location = new System.Drawing.Point(23, 74);
		this.m_simServerBusySendNotification.Name = "m_simServerBusySendNotification";
		this.m_simServerBusySendNotification.Size = new System.Drawing.Size(126, 17);
		this.m_simServerBusySendNotification.TabIndex = 60;
		this.m_simServerBusySendNotification.Text = "Simulate Server Busy";
		this.m_simServerBusySendNotification.UseVisualStyleBackColor = true;
		this.m_simServerBusySendNotification.CheckedChanged += new System.EventHandler(m_simServerBusySendNotification_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "EmailPrefPage";
		base.Size = new System.Drawing.Size(542, 506);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}
