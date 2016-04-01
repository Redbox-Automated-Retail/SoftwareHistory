using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.MSHALTester.Properties;

namespace Redbox.HAL.MSHALTester;

public class EngineeringForm : Form
{
	private delegate void SetTextCallback(string s);

	private HardwareJob m_rotationJob;

	private readonly object Lock = new object();

	private IContainer components;

	private Button button1;

	private Button button2;

	private ListBox m_outputBox;

	private Button button3;

	private Button m_exitButton;

	private Button m_clearOutputButton;

	private ConfigurationPanel configurationPanel1;

	private GroupBox Configuration;

	private Button button4;

	private Button button5;

	private CheckBox m_generateDateCB;

	private GroupBox groupBox1;

	private GroupBox groupBox2;

	private Button m_vendUnknownButton;

	private Label label2;

	private Label label1;

	private TextBox m_vendslot;

	private TextBox m_vendDeck;

	private HardwareService Service { get; set; }

	private ButtonAspectsManager Manager { get; set; }

	private ClientHelper Helper { get; set; }

	public EngineeringForm(ClientHelper helper, HardwareService service, ButtonAspectsManager manager)
	{
		InitializeComponent();
		Service = service;
		Manager = manager;
		Helper = helper;
	}

	private void WriteToOutput(string msg)
	{
		if (m_outputBox.InvokeRequired)
		{
			SetTextCallback method = WriteToOutput;
			Invoke(method, msg);
		}
		else
		{
			m_outputBox.Items.Insert(0, msg);
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			HardwareCommandResult hardwareCommandResult = Service.ScheduleJob("ms-take-disk-at-door", string.Empty, enableDebugging: false, MakeSchedule(), out var job);
			if (!hardwareCommandResult.Success)
			{
				WriteToOutput("Can't communicate with service.");
			}
			else
			{
				if (!Helper.WaitForJob(job, out var endStatus))
				{
					return;
				}
				if (!job.GetResults(out var results).Success)
				{
					WriteToOutput("Failed to get vend-disk job results.");
				}
				ProgramResult[] array = results;
				foreach (ProgramResult programResult in array)
				{
					if (programResult.Code.Equals("ItemReceived", StringComparison.CurrentCultureIgnoreCase))
					{
						break;
					}
					if (programResult.Code.Equals("HardwareError", StringComparison.CurrentCultureIgnoreCase))
					{
						WriteToOutput("There was a HW error: " + programResult.Message);
						return;
					}
					if (programResult.Code == "ReceiveFailure")
					{
						WriteToOutput($"Disk not placed into picker: {programResult.Message}");
						return;
					}
				}
				ReadBarcodeResult readBarcodeResult = ReadBarcodeResult.ReadBarcodeOfDiskInPicker(Service);
				WriteToOutput(string.Format("Found {0} barcodes ({1}), time={2} {3}", readBarcodeResult.Count, readBarcodeResult.Barcode, readBarcodeResult.ScanTime, readBarcodeResult.IsDuplicate ? "DUPLICATE" : string.Empty));
				Service.TrashJob(job.ID);
				hardwareCommandResult = Service.ScheduleJob("ms-vend-disk-in-picker", string.Empty, enableDebugging: false, MakeSchedule(), out job);
				if (Helper.WaitForJob(job, out endStatus))
				{
					Service.TrashJob(job.ID);
				}
			}
		}
	}

	private void button2_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			lock (Lock)
			{
				byte[] rotationTest = Resources.RotationTest;
				HardwareCommandResult programs = Service.GetPrograms();
				bool flag = false;
				string text = "rotation-test";
				foreach (string commandMessage in programs.CommandMessages)
				{
					if (commandMessage.StartsWith(text))
					{
						flag = true;
						break;
					}
				}
				IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
				string path = service.RuntimePath("rotation-test.hs");
				if (!flag)
				{
					using (File.CreateText(path))
					{
					}
					File.WriteAllBytes(path, rotationTest);
					HardwareCommandResult hardwareCommandResult = Service.CompileProgram(path, text, requiresClientConnection: false);
					if (!hardwareCommandResult.Success)
					{
						WriteToOutput("Failed to compile program.");
						return;
					}
					FileHelper.SafeDeleteFile(path);
				}
				int num = 1000;
				LocationNumberPad locationNumberPad = new LocationNumberPad();
				if (locationNumberPad.ShowDialog() == DialogResult.OK)
				{
					num = locationNumberPad.Number;
				}
				HardwareCommandResult hardwareCommandResult2 = Service.ScheduleJob(text, "Engineering rotation test", enableDebugging: false, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.Highest
				}, out m_rotationJob);
				if (!hardwareCommandResult2.Success)
				{
					WriteToOutput("Unable to schedule test job - tell Jim he's a dumbass.");
					return;
				}
				m_rotationJob.Push(num);
				WriteToOutput($"Scheduling rotation test job ID {m_rotationJob.ID}");
				m_rotationJob.Pend();
			}
			ThreadPool.QueueUserWorkItem(delegate
			{
				WaitforRotation();
			});
		}
	}

	private void WaitforRotation()
	{
		lock (Lock)
		{
			if (!Helper.WaitForJob(m_rotationJob, out var _))
			{
				WriteToOutput("Jim's shame - wait for job failed.");
				return;
			}
			if (m_rotationJob != null)
			{
				m_rotationJob.Trash();
			}
			m_rotationJob = null;
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		if (m_rotationJob != null)
		{
			m_rotationJob.Terminate();
			WriteToOutput($"Terminating job {m_rotationJob.ID}");
		}
	}

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private HardwareJobSchedule MakeSchedule()
	{
		HardwareJobSchedule hardwareJobSchedule = new HardwareJobSchedule();
		hardwareJobSchedule.Priority = HardwareJobPriority.Highest;
		return hardwareJobSchedule;
	}

	private void button4_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string param = "NONE";
			if (m_generateDateCB.Checked)
			{
				param = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString();
			}
			ScheduleReturnUnknown(param);
			m_generateDateCB.Checked = false;
		}
	}

	private void button5_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			ScheduleReturnUnknown("FMA-INSERT");
		}
	}

	private bool ScheduleReturnUnknown(string param)
	{
		HardwareCommandResult hardwareCommandResult = Service.ReturnUnknown(MakeSchedule(), param, out var job);
		if (!hardwareCommandResult.Success)
		{
			return false;
		}
		Helper.WaitForJob(job, out var _);
		PopulateResults(job, showMsg: true);
		return true;
	}

	private void m_vendUnknownButton_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			if (!string.IsNullOrEmpty(m_vendDeck.Text) && !string.IsNullOrEmpty(m_vendslot.Text))
			{
				int deck = int.Parse(m_vendDeck.Text);
				int slot = int.Parse(m_vendslot.Text);
				HardwareCommandResult hardwareCommandResult = Service.VendUnknown(deck, slot, MakeSchedule(), out var job);
				if (hardwareCommandResult.Success)
				{
					Helper.WaitForJob(job, out var _);
					PopulateResults(job, showMsg: true);
				}
			}
		}
	}

	private void PopulateResults(HardwareJob job, bool showMsg)
	{
		job.GetResults(out var results);
		ProgramResult[] array = results;
		foreach (ProgramResult programResult in array)
		{
			string text = "NONE";
			if (programResult.ItemID != null && programResult.ItemID.RawResult != null)
			{
				text = programResult.ItemID.RawResult;
			}
			if (showMsg)
			{
				m_outputBox.Items.Insert(0, $"Code: {programResult.Code} Msg: {programResult.Message} Deck = {programResult.Deck} Slot = {programResult.Slot} ID: {text}");
			}
			else
			{
				m_outputBox.Items.Insert(0, $"Code: {programResult.Code} Deck = {programResult.Deck} Slot = {programResult.Slot} ID: {text}");
			}
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
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.m_outputBox = new System.Windows.Forms.ListBox();
		this.button3 = new System.Windows.Forms.Button();
		this.m_exitButton = new System.Windows.Forms.Button();
		this.m_clearOutputButton = new System.Windows.Forms.Button();
		this.Configuration = new System.Windows.Forms.GroupBox();
		this.button4 = new System.Windows.Forms.Button();
		this.button5 = new System.Windows.Forms.Button();
		this.m_generateDateCB = new System.Windows.Forms.CheckBox();
		this.configurationPanel1 = new Redbox.HAL.Common.GUI.Functions.ConfigurationPanel();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_vendUnknownButton = new System.Windows.Forms.Button();
		this.m_vendDeck = new System.Windows.Forms.TextBox();
		this.m_vendslot = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.Configuration.SuspendLayout();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		this.button1.Location = new System.Drawing.Point(12, 12);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(208, 100);
		this.button1.TabIndex = 0;
		this.button1.Text = "Take, Read and Vend Disk";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(button1_Click);
		this.button2.Location = new System.Drawing.Point(12, 134);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(208, 87);
		this.button2.TabIndex = 1;
		this.button2.Text = "Rotation Test";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(button2_Click);
		this.m_outputBox.FormattingEnabled = true;
		this.m_outputBox.Location = new System.Drawing.Point(399, 12);
		this.m_outputBox.Name = "m_outputBox";
		this.m_outputBox.Size = new System.Drawing.Size(285, 264);
		this.m_outputBox.TabIndex = 2;
		this.button3.Location = new System.Drawing.Point(226, 168);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(110, 53);
		this.button3.TabIndex = 3;
		this.button3.Text = "Stop Rotation Test";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(button3_Click);
		this.m_exitButton.BackColor = System.Drawing.Color.Pink;
		this.m_exitButton.Location = new System.Drawing.Point(586, 282);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(98, 59);
		this.m_exitButton.TabIndex = 4;
		this.m_exitButton.Text = "Exit";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		this.m_clearOutputButton.Location = new System.Drawing.Point(399, 282);
		this.m_clearOutputButton.Name = "m_clearOutputButton";
		this.m_clearOutputButton.Size = new System.Drawing.Size(115, 59);
		this.m_clearOutputButton.TabIndex = 5;
		this.m_clearOutputButton.Text = "Clear Output";
		this.m_clearOutputButton.UseVisualStyleBackColor = true;
		this.Configuration.Controls.Add(this.configurationPanel1);
		this.Configuration.Location = new System.Drawing.Point(15, 249);
		this.Configuration.Name = "Configuration";
		this.Configuration.Size = new System.Drawing.Size(356, 465);
		this.Configuration.TabIndex = 7;
		this.Configuration.TabStop = false;
		this.Configuration.Text = "groupBox1";
		this.button4.Location = new System.Drawing.Point(6, 19);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(115, 74);
		this.button4.TabIndex = 8;
		this.button4.Text = "Return Unknown";
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(button4_Click);
		this.button5.Location = new System.Drawing.Point(150, 19);
		this.button5.Name = "button5";
		this.button5.Size = new System.Drawing.Size(98, 74);
		this.button5.TabIndex = 9;
		this.button5.Text = "FMA Insert";
		this.button5.UseVisualStyleBackColor = true;
		this.button5.Click += new System.EventHandler(button5_Click);
		this.m_generateDateCB.AutoSize = true;
		this.m_generateDateCB.Location = new System.Drawing.Point(6, 99);
		this.m_generateDateCB.Name = "m_generateDateCB";
		this.m_generateDateCB.Size = new System.Drawing.Size(96, 17);
		this.m_generateDateCB.TabIndex = 10;
		this.m_generateDateCB.Text = "Generate Date";
		this.m_generateDateCB.UseVisualStyleBackColor = true;
		this.configurationPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.configurationPanel1.Location = new System.Drawing.Point(3, 16);
		this.configurationPanel1.Name = "configurationPanel1";
		this.configurationPanel1.Size = new System.Drawing.Size(350, 446);
		this.configurationPanel1.TabIndex = 6;
		this.groupBox1.Controls.Add(this.m_generateDateCB);
		this.groupBox1.Controls.Add(this.button5);
		this.groupBox1.Controls.Add(this.button4);
		this.groupBox1.Location = new System.Drawing.Point(399, 362);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(271, 143);
		this.groupBox1.TabIndex = 11;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Return Tests";
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.label1);
		this.groupBox2.Controls.Add(this.m_vendslot);
		this.groupBox2.Controls.Add(this.m_vendDeck);
		this.groupBox2.Controls.Add(this.m_vendUnknownButton);
		this.groupBox2.Location = new System.Drawing.Point(405, 523);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(265, 188);
		this.groupBox2.TabIndex = 12;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Vend Tests";
		this.m_vendUnknownButton.Location = new System.Drawing.Point(6, 19);
		this.m_vendUnknownButton.Name = "m_vendUnknownButton";
		this.m_vendUnknownButton.Size = new System.Drawing.Size(109, 65);
		this.m_vendUnknownButton.TabIndex = 0;
		this.m_vendUnknownButton.Text = "Vend Unknown";
		this.m_vendUnknownButton.UseVisualStyleBackColor = true;
		this.m_vendUnknownButton.Click += new System.EventHandler(m_vendUnknownButton_Click);
		this.m_vendDeck.Location = new System.Drawing.Point(57, 90);
		this.m_vendDeck.Name = "m_vendDeck";
		this.m_vendDeck.Size = new System.Drawing.Size(52, 20);
		this.m_vendDeck.TabIndex = 1;
		this.m_vendslot.Location = new System.Drawing.Point(57, 116);
		this.m_vendslot.Name = "m_vendslot";
		this.m_vendslot.Size = new System.Drawing.Size(52, 20);
		this.m_vendslot.TabIndex = 2;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(6, 97);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(33, 13);
		this.label1.TabIndex = 3;
		this.label1.Text = "Deck";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(10, 121);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(25, 13);
		this.label2.TabIndex = 4;
		this.label2.Text = "Slot";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(728, 826);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.Configuration);
		base.Controls.Add(this.m_clearOutputButton);
		base.Controls.Add(this.m_exitButton);
		base.Controls.Add(this.button3);
		base.Controls.Add(this.m_outputBox);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.Name = "EngineeringForm";
		this.Text = "EngineeringForm";
		this.Configuration.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}
