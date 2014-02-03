using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy;

namespace Redbox.TicketServices.KernelExtension;

public class LockingPrefPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private Label m_headerLabel;

	private GroupBox groupBox1;

	private CheckBox m_liveCallReleaseTicketLock;

	private CheckBox m_simComFailureReleaseTicketLock;

	private CheckBox m_simServerBusyReleaseTicketLock;

	private GroupBox groupBox2;

	private CheckBox m_simComFailureUpdateTicketLock;

	private CheckBox m_simServerBusyUpdateTicketLock;

	private CheckBox m_liveCallUpdateTicketLock;

	public LockingPrefPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_liveCallUpdateTicketLock.Checked = TicketServiceProxy.Instance.LiveCallUpdateTicketLock;
		m_simComFailureUpdateTicketLock.Checked = TicketServiceProxy.Instance.CommFailureUpdateTicketLock;
		m_simServerBusyUpdateTicketLock.Checked = TicketServiceProxy.Instance.ServerBusyUpdateTicketLock;
		m_liveCallReleaseTicketLock.Checked = TicketServiceProxy.Instance.LiveCallReleaseTicketLock;
		m_simComFailureReleaseTicketLock.Checked = TicketServiceProxy.Instance.CommFailureReleaseTicketLock;
		m_simServerBusyReleaseTicketLock.Checked = TicketServiceProxy.Instance.ServerBusyReleaseTicketLock;
		m_isLoaded = true;
	}

	private void m_liveCallReleaseTicketLock_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.LiveCallReleaseTicketLock = m_liveCallReleaseTicketLock.Checked;
		}
	}

	private void m_simComFailureReleaseTicketLock_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.CommFailureReleaseTicketLock = m_simComFailureReleaseTicketLock.Checked;
		}
	}

	private void m_simServerBusyReleaseTicketLock_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerBusyReleaseTicketLock = m_simServerBusyReleaseTicketLock.Checked;
		}
	}

	private void m_liveCallUpdateTicketLock_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.LiveCallUpdateTicketLock = m_liveCallUpdateTicketLock.Checked;
		}
	}

	private void m_simComFailureUpdateTicketLock_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.CommFailureUpdateTicketLock = m_simComFailureUpdateTicketLock.Checked;
		}
	}

	private void m_simServerBusyUpdateTicketLock_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerBusyUpdateTicketLock = m_simServerBusyUpdateTicketLock.Checked;
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.m_simComFailureReleaseTicketLock = new System.Windows.Forms.CheckBox();
		this.m_simServerBusyReleaseTicketLock = new System.Windows.Forms.CheckBox();
		this.m_liveCallReleaseTicketLock = new System.Windows.Forms.CheckBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_simComFailureUpdateTicketLock = new System.Windows.Forms.CheckBox();
		this.m_simServerBusyUpdateTicketLock = new System.Windows.Forms.CheckBox();
		this.m_liveCallUpdateTicketLock = new System.Windows.Forms.CheckBox();
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
		this.m_headerLabel.TabIndex = 44;
		this.m_headerLabel.Text = "  Ticket Services: Locking";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.groupBox1.Controls.Add(this.m_simComFailureReleaseTicketLock);
		this.groupBox1.Controls.Add(this.m_simServerBusyReleaseTicketLock);
		this.groupBox1.Controls.Add(this.m_liveCallReleaseTicketLock);
		this.groupBox1.Location = new System.Drawing.Point(12, 221);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(507, 159);
		this.groupBox1.TabIndex = 45;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Release Ticket Lock";
		this.m_simComFailureReleaseTicketLock.AutoSize = true;
		this.m_simComFailureReleaseTicketLock.Location = new System.Drawing.Point(28, 51);
		this.m_simComFailureReleaseTicketLock.Name = "m_simComFailureReleaseTicketLock";
		this.m_simComFailureReleaseTicketLock.Size = new System.Drawing.Size(180, 17);
		this.m_simComFailureReleaseTicketLock.TabIndex = 61;
		this.m_simComFailureReleaseTicketLock.Text = "Simulate Communications Failure";
		this.m_simComFailureReleaseTicketLock.UseVisualStyleBackColor = true;
		this.m_simComFailureReleaseTicketLock.CheckedChanged += new System.EventHandler(m_simComFailureReleaseTicketLock_CheckedChanged);
		this.m_simServerBusyReleaseTicketLock.AutoSize = true;
		this.m_simServerBusyReleaseTicketLock.Location = new System.Drawing.Point(28, 74);
		this.m_simServerBusyReleaseTicketLock.Name = "m_simServerBusyReleaseTicketLock";
		this.m_simServerBusyReleaseTicketLock.Size = new System.Drawing.Size(126, 17);
		this.m_simServerBusyReleaseTicketLock.TabIndex = 62;
		this.m_simServerBusyReleaseTicketLock.Text = "Simulate Server Busy";
		this.m_simServerBusyReleaseTicketLock.UseVisualStyleBackColor = true;
		this.m_simServerBusyReleaseTicketLock.CheckedChanged += new System.EventHandler(m_simServerBusyReleaseTicketLock_CheckedChanged);
		this.m_liveCallReleaseTicketLock.AutoSize = true;
		this.m_liveCallReleaseTicketLock.Location = new System.Drawing.Point(28, 28);
		this.m_liveCallReleaseTicketLock.Name = "m_liveCallReleaseTicketLock";
		this.m_liveCallReleaseTicketLock.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallReleaseTicketLock.TabIndex = 0;
		this.m_liveCallReleaseTicketLock.Text = "Live Call";
		this.m_liveCallReleaseTicketLock.UseVisualStyleBackColor = true;
		this.m_liveCallReleaseTicketLock.CheckedChanged += new System.EventHandler(m_liveCallReleaseTicketLock_CheckedChanged);
		this.groupBox2.Controls.Add(this.m_simComFailureUpdateTicketLock);
		this.groupBox2.Controls.Add(this.m_simServerBusyUpdateTicketLock);
		this.groupBox2.Controls.Add(this.m_liveCallUpdateTicketLock);
		this.groupBox2.Location = new System.Drawing.Point(12, 56);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(507, 159);
		this.groupBox2.TabIndex = 46;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Update Ticket Lock";
		this.m_simComFailureUpdateTicketLock.AutoSize = true;
		this.m_simComFailureUpdateTicketLock.Location = new System.Drawing.Point(28, 51);
		this.m_simComFailureUpdateTicketLock.Name = "m_simComFailureUpdateTicketLock";
		this.m_simComFailureUpdateTicketLock.Size = new System.Drawing.Size(180, 17);
		this.m_simComFailureUpdateTicketLock.TabIndex = 61;
		this.m_simComFailureUpdateTicketLock.Text = "Simulate Communications Failure";
		this.m_simComFailureUpdateTicketLock.UseVisualStyleBackColor = true;
		this.m_simComFailureUpdateTicketLock.CheckedChanged += new System.EventHandler(m_simComFailureUpdateTicketLock_CheckedChanged);
		this.m_simServerBusyUpdateTicketLock.AutoSize = true;
		this.m_simServerBusyUpdateTicketLock.Location = new System.Drawing.Point(28, 74);
		this.m_simServerBusyUpdateTicketLock.Name = "m_simServerBusyUpdateTicketLock";
		this.m_simServerBusyUpdateTicketLock.Size = new System.Drawing.Size(126, 17);
		this.m_simServerBusyUpdateTicketLock.TabIndex = 62;
		this.m_simServerBusyUpdateTicketLock.Text = "Simulate Server Busy";
		this.m_simServerBusyUpdateTicketLock.UseVisualStyleBackColor = true;
		this.m_simServerBusyUpdateTicketLock.CheckedChanged += new System.EventHandler(m_simServerBusyUpdateTicketLock_CheckedChanged);
		this.m_liveCallUpdateTicketLock.AutoSize = true;
		this.m_liveCallUpdateTicketLock.Location = new System.Drawing.Point(28, 28);
		this.m_liveCallUpdateTicketLock.Name = "m_liveCallUpdateTicketLock";
		this.m_liveCallUpdateTicketLock.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallUpdateTicketLock.TabIndex = 0;
		this.m_liveCallUpdateTicketLock.Text = "Live Call";
		this.m_liveCallUpdateTicketLock.UseVisualStyleBackColor = true;
		this.m_liveCallUpdateTicketLock.CheckedChanged += new System.EventHandler(m_liveCallUpdateTicketLock_CheckedChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "LockingPrefPage";
		base.Size = new System.Drawing.Size(542, 506);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}
