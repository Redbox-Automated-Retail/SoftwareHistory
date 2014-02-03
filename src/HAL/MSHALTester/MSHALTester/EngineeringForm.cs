using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Core;
using Redbox.HAL.MSHALTester.Properties;

namespace Redbox.HAL.MSHALTester;

public class EngineeringForm : Form
{
	private delegate void SetTextCallback(string s);

	private HardwareJob m_rotationJob;

	private IContainer components;

	private Button button1;

	private Button button2;

	private ListBox m_outputBox;

	private Button button3;

	private Button m_exitButton;

	private Button m_clearOutputButton;

	private ConfigurationPanel configurationPanel1;

	private GroupBox Configuration;

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
			lock (this)
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
				string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "rotation-test.hs");
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
				NumberPad numberPad = new NumberPad();
				if (numberPad.ShowDialog() == DialogResult.OK)
				{
					num = numberPad.Number;
				}
				HardwareCommandResult hardwareCommandResult2 = Service.ScheduleJob(text, "Engineering rotation test", enableDebugging: false, new HardwareJobSchedule
				{
					Priority = HardwareJobPriority.Highest,
					StartTime = DateTime.Now
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
			new Thread(WaitforRotation).Start();
		}
	}

	private void WaitforRotation()
	{
		lock (this)
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
		hardwareJobSchedule.StartTime = DateTime.Now;
		return hardwareJobSchedule;
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
		this.configurationPanel1 = new Redbox.HAL.Common.GUI.Functions.ConfigurationPanel();
		this.Configuration = new System.Windows.Forms.GroupBox();
		this.Configuration.SuspendLayout();
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
		this.configurationPanel1.Location = new System.Drawing.Point(6, 19);
		this.configurationPanel1.Name = "configurationPanel1";
		this.configurationPanel1.Size = new System.Drawing.Size(350, 484);
		this.configurationPanel1.TabIndex = 6;
		this.Configuration.Controls.Add(this.configurationPanel1);
		this.Configuration.Location = new System.Drawing.Point(15, 249);
		this.Configuration.Name = "Configuration";
		this.Configuration.Size = new System.Drawing.Size(356, 465);
		this.Configuration.TabIndex = 7;
		this.Configuration.TabStop = false;
		this.Configuration.Text = "groupBox1";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(728, 826);
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
		base.ResumeLayout(false);
	}
}
