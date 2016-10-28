using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;

namespace Redbox.KioskServices.KernelExtension;

public class InventoryServicesPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_enableSellThruOfUsedCheckBox;

	private CheckBox m_enableOutOfStockDisplayCheckBox;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_enableSellThruOfNewProduct;

	private Label m_headerLabel;

	public InventoryServicesPreferencePage()
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
		m_liveCallCheckBox.Checked = InventoryServicesProxy.Instance.LiveCall;
		m_enableSellThruOfNewProduct.Checked = InventoryServicesProxy.Instance.EnableSellThryOfNew;
		m_enableSellThruOfUsedCheckBox.Checked = InventoryServicesProxy.Instance.EnableSellThryOfUsed;
		m_simulateServerBusyCheckBox.Checked = InventoryServicesProxy.Instance.SimulateServerBusyError;
		m_enableOutOfStockDisplayCheckBox.Checked = InventoryServicesProxy.Instance.EnableOutOfStockDisplay;
		m_simulateCommunicationsFailureCheckBox.Checked = InventoryServicesProxy.Instance.SimulateCommunicationsFailure;
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			InventoryServicesProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnOutOfStockCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			InventoryServicesProxy.Instance.EnableOutOfStockDisplay = m_enableOutOfStockDisplayCheckBox.Checked;
		}
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			InventoryServicesProxy.Instance.SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnEnableSellThruOfUsedCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			InventoryServicesProxy.Instance.EnableSellThryOfUsed = m_enableSellThruOfUsedCheckBox.Checked;
		}
	}

	private void OnSellThruNewCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			InventoryServicesProxy.Instance.EnableSellThryOfNew = m_enableSellThruOfNewProduct.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			InventoryServicesProxy.Instance.SimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
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
		this.m_enableSellThruOfUsedCheckBox = new System.Windows.Forms.CheckBox();
		this.m_enableOutOfStockDisplayCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_enableSellThruOfNewProduct = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_enableSellThruOfUsedCheckBox.AutoSize = true;
		this.m_enableSellThruOfUsedCheckBox.Location = new System.Drawing.Point(33, 176);
		this.m_enableSellThruOfUsedCheckBox.Name = "m_enableSellThruOfUsedCheckBox";
		this.m_enableSellThruOfUsedCheckBox.Size = new System.Drawing.Size(175, 17);
		this.m_enableSellThruOfUsedCheckBox.TabIndex = 7;
		this.m_enableSellThruOfUsedCheckBox.Text = "Enable sell thru of used product";
		this.m_enableSellThruOfUsedCheckBox.UseVisualStyleBackColor = true;
		this.m_enableSellThruOfUsedCheckBox.CheckedChanged += new System.EventHandler(OnEnableSellThruOfUsedCheckChanged);
		this.m_enableOutOfStockDisplayCheckBox.AutoSize = true;
		this.m_enableOutOfStockDisplayCheckBox.Location = new System.Drawing.Point(33, 153);
		this.m_enableOutOfStockDisplayCheckBox.Name = "m_enableOutOfStockDisplayCheckBox";
		this.m_enableOutOfStockDisplayCheckBox.Size = new System.Drawing.Size(153, 17);
		this.m_enableOutOfStockDisplayCheckBox.TabIndex = 6;
		this.m_enableOutOfStockDisplayCheckBox.Text = "Enable out-of-stock display";
		this.m_enableOutOfStockDisplayCheckBox.UseVisualStyleBackColor = true;
		this.m_enableOutOfStockDisplayCheckBox.CheckedChanged += new System.EventHandler(OnOutOfStockCheckChanged);
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(33, 103);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 9;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(33, 80);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 8;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(33, 57);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 5;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_enableSellThruOfNewProduct.AutoSize = true;
		this.m_enableSellThruOfNewProduct.Location = new System.Drawing.Point(33, 199);
		this.m_enableSellThruOfNewProduct.Name = "m_enableSellThruOfNewProduct";
		this.m_enableSellThruOfNewProduct.Size = new System.Drawing.Size(172, 17);
		this.m_enableSellThruOfNewProduct.TabIndex = 10;
		this.m_enableSellThruOfNewProduct.Text = "Enable sell thru of new product";
		this.m_enableSellThruOfNewProduct.UseVisualStyleBackColor = true;
		this.m_enableSellThruOfNewProduct.CheckedChanged += new System.EventHandler(OnSellThruNewCheckChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 40;
		this.m_headerLabel.Text = "  Kiosk Services: Inventory";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_enableSellThruOfNewProduct);
		base.Controls.Add(this.m_enableSellThruOfUsedCheckBox);
		base.Controls.Add(this.m_enableOutOfStockDisplayCheckBox);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "InventoryServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
