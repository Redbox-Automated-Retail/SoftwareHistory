using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.HardwareServices.TestProxy;
using Redbox.HardwareServices.TestProxy.Interfaces;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.HardwareServices.KernelExtension;

public class ReturnPreferencePage : UserControl, IPreferencePageHost
{
	private readonly TestHardwareService _hardwareService;

	private readonly IInventoryTestService _inventoryService;

	private IContainer components;

	private Label m_headerLabel;

	private Label label1;

	private Button btn_unknownBarcode;

	private TextBox txt_barcodeInput;

	private ComboBox cmb_returnType;

	private Label label2;

	private CheckBox cb_fraudSensor;

	private ComboBox comboBox2;

	private Label label3;

	private Button btn_addBarcode;

	private DataGridView dgv_returnQueue;

	private DataGridViewTextBoxColumn Barcodes;

	private DateTimePicker dtp_IRInstallDate;

	private Label label4;

	private TextBox textBoxDiscImageFilename;

	private Label labelDiscImageFilename;

	private Label labelDiscImageFolder;

	private TextBox textBoxDiscImageFolder;

	private Button buttonCreateDummyFile;

	public ReturnPreferencePage()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base._002Ector();
		InitializeComponent();
		_hardwareService = (TestHardwareService)ServiceLocator.Instance.GetService<IHardwareService>();
		_inventoryService = ServiceLocator.Instance.GetService<IInventoryTestService>();
		LoadHardwareInfo();
	}

	private void LoadHardwareInfo()
	{
		dgv_returnQueue.DataSource = _inventoryService.VendedInventory;
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		_hardwareService.IRCameraInstalledOn = dtp_IRInstallDate.Value.ToUniversalTime();
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		cb_fraudSensor.Checked = _hardwareService.HasFraudSensor;
		cmb_returnType.DataSource = Enum.GetValues(typeof(ReturnType));
		cmb_returnType.SelectedItem = _hardwareService.ReturnType;
		if (_hardwareService.IRCameraInstalledOn.HasValue)
		{
			dtp_IRInstallDate.Value = _hardwareService.IRCameraInstalledOn.Value.ToLocalTime();
		}
		IFraudService service = ServiceLocator.Instance.GetService<IFraudService>();
		textBoxDiscImageFolder.Text = service?.ImageFolderPath;
	}

	private void OnAddUnknown(object sender, EventArgs e)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		(dgv_returnQueue.DataSource as BindingList<InventoryData>)?.Add(new InventoryData
		{
			Barcode = "UNKNOWN"
		});
	}

	private void OnAddBarcode(object sender, EventArgs e)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		if (dgv_returnQueue.DataSource is BindingList<InventoryData> bindingList && !string.IsNullOrWhiteSpace(txt_barcodeInput.Text))
		{
			bindingList.Add(new InventoryData
			{
				Barcode = txt_barcodeInput.Text
			});
			txt_barcodeInput.Text = string.Empty;
		}
	}

	private void OnReturnTypeChanged(object sender, EventArgs e)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_hardwareService.ReturnType = (ReturnType)cmb_returnType.SelectedItem;
	}

	private void buttonCreateDummyFile_Click(object sender, EventArgs e)
	{
		string randomFileName = Path.GetRandomFileName();
		if (!File.Exists(randomFileName))
		{
			IFraudService service = ServiceLocator.Instance.GetService<IFraudService>();
			if (service != null)
			{
				if (!Directory.Exists(service.ImageFolderPath))
				{
					Directory.CreateDirectory(service.ImageFolderPath);
				}
				File.WriteAllText(Path.Combine(service.ImageFolderPath, randomFileName), "dummy");
			}
		}
		textBoxDiscImageFilename.Text = randomFileName;
	}

	private void textBoxDiscImageFilename_TextChanged(object sender, EventArgs e)
	{
		_hardwareService.ImageFileName = textBoxDiscImageFilename.Text;
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
		this.btn_unknownBarcode = new System.Windows.Forms.Button();
		this.txt_barcodeInput = new System.Windows.Forms.TextBox();
		this.cmb_returnType = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.cb_fraudSensor = new System.Windows.Forms.CheckBox();
		this.comboBox2 = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.btn_addBarcode = new System.Windows.Forms.Button();
		this.dgv_returnQueue = new System.Windows.Forms.DataGridView();
		this.Barcodes = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.dtp_IRInstallDate = new System.Windows.Forms.DateTimePicker();
		this.label4 = new System.Windows.Forms.Label();
		this.textBoxDiscImageFilename = new System.Windows.Forms.TextBox();
		this.labelDiscImageFilename = new System.Windows.Forms.Label();
		this.labelDiscImageFolder = new System.Windows.Forms.Label();
		this.textBoxDiscImageFolder = new System.Windows.Forms.TextBox();
		this.buttonCreateDummyFile = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.dgv_returnQueue).BeginInit();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Hardware Service: Return";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.Location = new System.Drawing.Point(340, 42);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(154, 24);
		this.label1.TabIndex = 9;
		this.label1.Text = "Return Queue: ";
		this.btn_unknownBarcode.Location = new System.Drawing.Point(340, 422);
		this.btn_unknownBarcode.Name = "btn_unknownBarcode";
		this.btn_unknownBarcode.Size = new System.Drawing.Size(91, 42);
		this.btn_unknownBarcode.TabIndex = 10;
		this.btn_unknownBarcode.Text = "Add Unknown";
		this.btn_unknownBarcode.UseVisualStyleBackColor = true;
		this.btn_unknownBarcode.Click += new System.EventHandler(OnAddUnknown);
		this.txt_barcodeInput.Location = new System.Drawing.Point(340, 396);
		this.txt_barcodeInput.Name = "txt_barcodeInput";
		this.txt_barcodeInput.Size = new System.Drawing.Size(184, 20);
		this.txt_barcodeInput.TabIndex = 11;
		this.cmb_returnType.FormattingEnabled = true;
		this.cmb_returnType.Location = new System.Drawing.Point(16, 88);
		this.cmb_returnType.Name = "cmb_returnType";
		this.cmb_returnType.Size = new System.Drawing.Size(180, 21);
		this.cmb_returnType.TabIndex = 12;
		this.cmb_returnType.SelectedIndexChanged += new System.EventHandler(OnReturnTypeChanged);
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(13, 69);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(97, 16);
		this.label2.TabIndex = 13;
		this.label2.Text = "Return Type:";
		this.cb_fraudSensor.AutoSize = true;
		this.cb_fraudSensor.Location = new System.Drawing.Point(16, 125);
		this.cb_fraudSensor.Name = "cb_fraudSensor";
		this.cb_fraudSensor.Size = new System.Drawing.Size(144, 17);
		this.cb_fraudSensor.TabIndex = 14;
		this.cb_fraudSensor.Text = "Fraud Sensor Connected";
		this.cb_fraudSensor.UseVisualStyleBackColor = true;
		this.comboBox2.FormattingEnabled = true;
		this.comboBox2.Location = new System.Drawing.Point(16, 179);
		this.comboBox2.Name = "comboBox2";
		this.comboBox2.Size = new System.Drawing.Size(180, 21);
		this.comboBox2.TabIndex = 15;
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.Location = new System.Drawing.Point(13, 160);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(149, 16);
		this.label3.TabIndex = 16;
		this.label3.Text = "Fraud Sensor Result";
		this.btn_addBarcode.Location = new System.Drawing.Point(437, 422);
		this.btn_addBarcode.Name = "btn_addBarcode";
		this.btn_addBarcode.Size = new System.Drawing.Size(88, 42);
		this.btn_addBarcode.TabIndex = 17;
		this.btn_addBarcode.Text = "Add Barcode";
		this.btn_addBarcode.UseVisualStyleBackColor = true;
		this.btn_addBarcode.Click += new System.EventHandler(OnAddBarcode);
		this.dgv_returnQueue.AllowUserToAddRows = false;
		this.dgv_returnQueue.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgv_returnQueue.ColumnHeadersVisible = false;
		this.dgv_returnQueue.Columns.AddRange(this.Barcodes);
		this.dgv_returnQueue.Location = new System.Drawing.Point(340, 70);
		this.dgv_returnQueue.Name = "dgv_returnQueue";
		this.dgv_returnQueue.ReadOnly = true;
		this.dgv_returnQueue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.dgv_returnQueue.Size = new System.Drawing.Size(184, 320);
		this.dgv_returnQueue.TabIndex = 18;
		this.Barcodes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Barcodes.DataPropertyName = "Barcode";
		this.Barcodes.HeaderText = "";
		this.Barcodes.Name = "Barcodes";
		this.Barcodes.ReadOnly = true;
		this.dtp_IRInstallDate.Location = new System.Drawing.Point(16, 241);
		this.dtp_IRInstallDate.Name = "dtp_IRInstallDate";
		this.dtp_IRInstallDate.Size = new System.Drawing.Size(236, 20);
		this.dtp_IRInstallDate.TabIndex = 65;
		this.label4.AutoSize = true;
		this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label4.Location = new System.Drawing.Point(13, 222);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(197, 16);
		this.label4.TabIndex = 66;
		this.label4.Text = "IR Camera Installation Date";
		this.textBoxDiscImageFilename.Location = new System.Drawing.Point(16, 302);
		this.textBoxDiscImageFilename.Name = "textBoxDiscImageFilename";
		this.textBoxDiscImageFilename.Size = new System.Drawing.Size(184, 20);
		this.textBoxDiscImageFilename.TabIndex = 67;
		this.textBoxDiscImageFilename.TextChanged += new System.EventHandler(textBoxDiscImageFilename_TextChanged);
		this.labelDiscImageFilename.AutoSize = true;
		this.labelDiscImageFilename.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelDiscImageFilename.Location = new System.Drawing.Point(13, 283);
		this.labelDiscImageFilename.Name = "labelDiscImageFilename";
		this.labelDiscImageFilename.Size = new System.Drawing.Size(149, 16);
		this.labelDiscImageFilename.TabIndex = 68;
		this.labelDiscImageFilename.Text = "Disc Image filename";
		this.labelDiscImageFolder.AutoSize = true;
		this.labelDiscImageFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.labelDiscImageFolder.Location = new System.Drawing.Point(13, 335);
		this.labelDiscImageFolder.Name = "labelDiscImageFolder";
		this.labelDiscImageFolder.Size = new System.Drawing.Size(134, 16);
		this.labelDiscImageFolder.TabIndex = 69;
		this.labelDiscImageFolder.Text = "Disc Image folder:";
		this.textBoxDiscImageFolder.Enabled = false;
		this.textBoxDiscImageFolder.Location = new System.Drawing.Point(12, 354);
		this.textBoxDiscImageFolder.Name = "textBoxDiscImageFolder";
		this.textBoxDiscImageFolder.Size = new System.Drawing.Size(310, 20);
		this.textBoxDiscImageFolder.TabIndex = 70;
		this.buttonCreateDummyFile.Location = new System.Drawing.Point(211, 296);
		this.buttonCreateDummyFile.Name = "buttonCreateDummyFile";
		this.buttonCreateDummyFile.Size = new System.Drawing.Size(118, 30);
		this.buttonCreateDummyFile.TabIndex = 71;
		this.buttonCreateDummyFile.Text = "Create Dummy file";
		this.buttonCreateDummyFile.UseVisualStyleBackColor = true;
		this.buttonCreateDummyFile.Click += new System.EventHandler(buttonCreateDummyFile_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.buttonCreateDummyFile);
		base.Controls.Add(this.textBoxDiscImageFolder);
		base.Controls.Add(this.labelDiscImageFolder);
		base.Controls.Add(this.labelDiscImageFilename);
		base.Controls.Add(this.textBoxDiscImageFilename);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.dtp_IRInstallDate);
		base.Controls.Add(this.dgv_returnQueue);
		base.Controls.Add(this.btn_addBarcode);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.comboBox2);
		base.Controls.Add(this.cb_fraudSensor);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.cmb_returnType);
		base.Controls.Add(this.txt_barcodeInput);
		base.Controls.Add(this.btn_unknownBarcode);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "ReturnPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.dgv_returnQueue).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
