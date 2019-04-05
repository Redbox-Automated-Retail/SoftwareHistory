using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.FraudServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy;

namespace Redbox.KioskServices.KernelExtension;

public class InventoryServicesPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_enableOutOfStockDisplayCheckBox;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_enableSellThruOfNewProduct;

	private Label m_headerLabel;

	private Panel pnlFraudStatusTest;

	private Label label1;

	private TextBox txtBarcode;

	private CheckBox chkIsFraud;

	private Label label4;

	private TextBox txtFraudType;

	private Label label3;

	private TextBox txtTransactionId;

	private Label label2;

	private TextBox txtKioskId;

	private Button btnDiscFraud;

	private Label label9;

	private TextBox txtRentedKioskId;

	private Label label8;

	private TextBox txtRentDate;

	private Label label7;

	private TextBox txtCPAccountNum;

	private Label label6;

	private TextBox txtCPCustNumber;

	private Label label5;

	private TextBox txtCustomerId;

	private Label label10;

	private Label label11;

	private TextBox txtReturnDate;

	private Label label12;

	private TextBox txtDiscId;

	private Button btnUploadImage;

	private TextBox txtFileName;

	private ComboBox cboImageType;

	private Label label14;

	private Label label13;

	private TextBox txtImageBytes;

	private Label label15;

	private Label label16;

	private Panel panel3;

	private Panel panel2;

	private Panel panel1;

	private Label label17;

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
		m_simulateServerBusyCheckBox.Checked = InventoryServicesProxy.Instance.SimulateServerBusyError;
		m_enableOutOfStockDisplayCheckBox.Checked = InventoryServicesProxy.Instance.EnableOutOfStockDisplay;
		m_simulateCommunicationsFailureCheckBox.Checked = InventoryServicesProxy.Instance.SimulateCommunicationsFailure;
		m_isLoaded = true;
		pnlFraudStatusTest.Visible = false;
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

	private void btnDiscFraud_Click(object sender, EventArgs e)
	{
	}

	private void FraudCallback(IRemoteServiceResult result)
	{
		if (result.Success)
		{
			MessageBox.Show("Call successful");
		}
		else
		{
			MessageBox.Show("Call unsuccessful");
		}
	}

	private void btnUploadImage_Click(object sender, EventArgs e)
	{
	}

	private void InventoryServicesPreferencePage_Load(object sender, EventArgs e)
	{
		cboImageType.DataSource = Enum.GetValues(typeof(ImageType));
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
		this.m_enableOutOfStockDisplayCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_enableSellThruOfNewProduct = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.pnlFraudStatusTest = new System.Windows.Forms.Panel();
		this.btnUploadImage = new System.Windows.Forms.Button();
		this.txtFileName = new System.Windows.Forms.TextBox();
		this.cboImageType = new System.Windows.Forms.ComboBox();
		this.label14 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.txtDiscId = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.txtReturnDate = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.txtRentedKioskId = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.txtRentDate = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.txtCPAccountNum = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.txtCPCustNumber = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.txtCustomerId = new System.Windows.Forms.TextBox();
		this.btnDiscFraud = new System.Windows.Forms.Button();
		this.chkIsFraud = new System.Windows.Forms.CheckBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtFraudType = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.txtTransactionId = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.txtKioskId = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtBarcode = new System.Windows.Forms.TextBox();
		this.txtImageBytes = new System.Windows.Forms.TextBox();
		this.label15 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.panel2 = new System.Windows.Forms.Panel();
		this.panel3 = new System.Windows.Forms.Panel();
		this.label17 = new System.Windows.Forms.Label();
		this.pnlFraudStatusTest.SuspendLayout();
		this.panel1.SuspendLayout();
		this.panel2.SuspendLayout();
		this.panel3.SuspendLayout();
		base.SuspendLayout();
		this.m_enableOutOfStockDisplayCheckBox.AutoSize = true;
		this.m_enableOutOfStockDisplayCheckBox.Location = new System.Drawing.Point(33, 135);
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
		this.m_enableSellThruOfNewProduct.Location = new System.Drawing.Point(33, 169);
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
		this.pnlFraudStatusTest.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.pnlFraudStatusTest.Controls.Add(this.panel3);
		this.pnlFraudStatusTest.Controls.Add(this.panel2);
		this.pnlFraudStatusTest.Controls.Add(this.panel1);
		this.pnlFraudStatusTest.Controls.Add(this.label10);
		this.pnlFraudStatusTest.Location = new System.Drawing.Point(14, 215);
		this.pnlFraudStatusTest.Name = "pnlFraudStatusTest";
		this.pnlFraudStatusTest.Size = new System.Drawing.Size(513, 288);
		this.pnlFraudStatusTest.TabIndex = 41;
		this.btnUploadImage.Location = new System.Drawing.Point(60, 88);
		this.btnUploadImage.Name = "btnUploadImage";
		this.btnUploadImage.Size = new System.Drawing.Size(121, 23);
		this.btnUploadImage.TabIndex = 51;
		this.btnUploadImage.Text = "Upload Disc Image";
		this.btnUploadImage.UseVisualStyleBackColor = true;
		this.btnUploadImage.Click += new System.EventHandler(btnUploadImage_Click);
		this.txtFileName.Location = new System.Drawing.Point(124, 36);
		this.txtFileName.Name = "txtFileName";
		this.txtFileName.Size = new System.Drawing.Size(100, 20);
		this.txtFileName.TabIndex = 50;
		this.cboImageType.FormattingEnabled = true;
		this.cboImageType.Location = new System.Drawing.Point(124, 10);
		this.cboImageType.Name = "cboImageType";
		this.cboImageType.Size = new System.Drawing.Size(100, 21);
		this.cboImageType.TabIndex = 49;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(8, 39);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(55, 13);
		this.label14.TabIndex = 48;
		this.label14.Text = "Image File";
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(8, 13);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(63, 13);
		this.label13.TabIndex = 47;
		this.label13.Text = "Image Type";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(258, 83);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(42, 13);
		this.label12.TabIndex = 46;
		this.label12.Text = "Disc ID";
		this.txtDiscId.Location = new System.Drawing.Point(374, 80);
		this.txtDiscId.Name = "txtDiscId";
		this.txtDiscId.Size = new System.Drawing.Size(100, 20);
		this.txtDiscId.TabIndex = 45;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(258, 57);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(65, 13);
		this.label11.TabIndex = 44;
		this.label11.Text = "Return Date";
		this.txtReturnDate.Location = new System.Drawing.Point(374, 54);
		this.txtReturnDate.Name = "txtReturnDate";
		this.txtReturnDate.Size = new System.Drawing.Size(100, 20);
		this.txtReturnDate.TabIndex = 43;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(172, 1);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(172, 13);
		this.label10.TabIndex = 42;
		this.label10.Text = "Update Fraud Status Test harness:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(258, 31);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(85, 13);
		this.label9.TabIndex = 19;
		this.label9.Text = "Rented Kiosk ID";
		this.txtRentedKioskId.Location = new System.Drawing.Point(374, 28);
		this.txtRentedKioskId.Name = "txtRentedKioskId";
		this.txtRentedKioskId.Size = new System.Drawing.Size(100, 20);
		this.txtRentedKioskId.TabIndex = 18;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(258, 5);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(56, 13);
		this.label8.TabIndex = 17;
		this.label8.Text = "Rent Date";
		this.txtRentDate.Location = new System.Drawing.Point(374, 2);
		this.txtRentDate.Name = "txtRentDate";
		this.txtRentDate.Size = new System.Drawing.Size(100, 20);
		this.txtRentDate.TabIndex = 16;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(17, 83);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(104, 13);
		this.label7.TabIndex = 15;
		this.label7.Text = "CP Account Number";
		this.txtCPAccountNum.Location = new System.Drawing.Point(133, 80);
		this.txtCPAccountNum.Name = "txtCPAccountNum";
		this.txtCPAccountNum.Size = new System.Drawing.Size(100, 20);
		this.txtCPAccountNum.TabIndex = 14;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(17, 57);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(108, 13);
		this.label6.TabIndex = 13;
		this.label6.Text = "CP Customer Number";
		this.txtCPCustNumber.Location = new System.Drawing.Point(133, 54);
		this.txtCPCustNumber.Name = "txtCPCustNumber";
		this.txtCPCustNumber.Size = new System.Drawing.Size(100, 20);
		this.txtCPCustNumber.TabIndex = 12;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(17, 31);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(60, 13);
		this.label5.TabIndex = 11;
		this.label5.Text = "CustomerId";
		this.txtCustomerId.Location = new System.Drawing.Point(133, 28);
		this.txtCustomerId.Name = "txtCustomerId";
		this.txtCustomerId.Size = new System.Drawing.Size(100, 20);
		this.txtCustomerId.TabIndex = 10;
		this.btnDiscFraud.Location = new System.Drawing.Point(209, 104);
		this.btnDiscFraud.Name = "btnDiscFraud";
		this.btnDiscFraud.Size = new System.Drawing.Size(114, 23);
		this.btnDiscFraud.TabIndex = 9;
		this.btnDiscFraud.Text = "Update Fraud Status";
		this.btnDiscFraud.UseVisualStyleBackColor = true;
		this.btnDiscFraud.Click += new System.EventHandler(btnDiscFraud_Click);
		this.chkIsFraud.AutoSize = true;
		this.chkIsFraud.Location = new System.Drawing.Point(24, 108);
		this.chkIsFraud.Name = "chkIsFraud";
		this.chkIsFraud.Size = new System.Drawing.Size(64, 17);
		this.chkIsFraud.TabIndex = 8;
		this.chkIsFraud.Text = "Is Fraud";
		this.chkIsFraud.UseVisualStyleBackColor = true;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(17, 4);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(61, 13);
		this.label4.TabIndex = 7;
		this.label4.Text = "Fraud Type";
		this.txtFraudType.Location = new System.Drawing.Point(133, 1);
		this.txtFraudType.Name = "txtFraudType";
		this.txtFraudType.Size = new System.Drawing.Size(100, 20);
		this.txtFraudType.TabIndex = 6;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(17, 87);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(77, 13);
		this.label3.TabIndex = 5;
		this.label3.Text = "Transaction ID";
		this.txtTransactionId.Location = new System.Drawing.Point(133, 84);
		this.txtTransactionId.Name = "txtTransactionId";
		this.txtTransactionId.Size = new System.Drawing.Size(100, 20);
		this.txtTransactionId.TabIndex = 4;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(17, 61);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(47, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "Kiosk ID";
		this.txtKioskId.Location = new System.Drawing.Point(133, 58);
		this.txtKioskId.Name = "txtKioskId";
		this.txtKioskId.Size = new System.Drawing.Size(100, 20);
		this.txtKioskId.TabIndex = 2;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(17, 35);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(47, 13);
		this.label1.TabIndex = 1;
		this.label1.Text = "Barcode";
		this.txtBarcode.Location = new System.Drawing.Point(133, 32);
		this.txtBarcode.Name = "txtBarcode";
		this.txtBarcode.Size = new System.Drawing.Size(100, 20);
		this.txtBarcode.TabIndex = 0;
		this.txtImageBytes.Location = new System.Drawing.Point(124, 62);
		this.txtImageBytes.Name = "txtImageBytes";
		this.txtImageBytes.Size = new System.Drawing.Size(100, 20);
		this.txtImageBytes.TabIndex = 53;
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(8, 65);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(65, 13);
		this.label15.TabIndex = 52;
		this.label15.Text = "Image Bytes";
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(8, 52);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(23, 13);
		this.label16.TabIndex = 54;
		this.label16.Text = "OR";
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.label17);
		this.panel1.Controls.Add(this.txtTransactionId);
		this.panel1.Controls.Add(this.txtBarcode);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Controls.Add(this.txtKioskId);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.label3);
		this.panel1.Location = new System.Drawing.Point(6, 17);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(244, 122);
		this.panel1.TabIndex = 55;
		this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel2.Controls.Add(this.label14);
		this.panel2.Controls.Add(this.label13);
		this.panel2.Controls.Add(this.label16);
		this.panel2.Controls.Add(this.btnUploadImage);
		this.panel2.Controls.Add(this.cboImageType);
		this.panel2.Controls.Add(this.txtImageBytes);
		this.panel2.Controls.Add(this.txtFileName);
		this.panel2.Controls.Add(this.label15);
		this.panel2.Location = new System.Drawing.Point(267, 17);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(229, 122);
		this.panel2.TabIndex = 56;
		this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel3.Controls.Add(this.label4);
		this.panel3.Controls.Add(this.txtFraudType);
		this.panel3.Controls.Add(this.chkIsFraud);
		this.panel3.Controls.Add(this.txtCustomerId);
		this.panel3.Controls.Add(this.label12);
		this.panel3.Controls.Add(this.btnDiscFraud);
		this.panel3.Controls.Add(this.label5);
		this.panel3.Controls.Add(this.txtDiscId);
		this.panel3.Controls.Add(this.txtCPCustNumber);
		this.panel3.Controls.Add(this.label11);
		this.panel3.Controls.Add(this.label6);
		this.panel3.Controls.Add(this.txtReturnDate);
		this.panel3.Controls.Add(this.txtCPAccountNum);
		this.panel3.Controls.Add(this.label9);
		this.panel3.Controls.Add(this.label7);
		this.panel3.Controls.Add(this.txtRentedKioskId);
		this.panel3.Controls.Add(this.txtRentDate);
		this.panel3.Controls.Add(this.label8);
		this.panel3.Location = new System.Drawing.Point(6, 145);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(496, 136);
		this.panel3.TabIndex = 57;
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(20, 0);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(80, 13);
		this.label17.TabIndex = 6;
		this.label17.Text = "Applies to both:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.pnlFraudStatusTest);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_enableSellThruOfNewProduct);
		base.Controls.Add(this.m_enableOutOfStockDisplayCheckBox);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "InventoryServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.Load += new System.EventHandler(InventoryServicesPreferencePage_Load);
		this.pnlFraudStatusTest.ResumeLayout(false);
		this.pnlFraudStatusTest.PerformLayout();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		this.panel3.ResumeLayout(false);
		this.panel3.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
