using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using HALUtilities.Properties;
using Redbox.HAL.Core;

namespace HALUtilities;

public class ResetForm : Form
{
	private readonly AutoResetEvent PseudoWait = new AutoResetEvent(initialState: false);

	private readonly AutoResetEvent ResetEvent = new AutoResetEvent(initialState: false);

	private readonly DeviceDescriptor Descriptor;

	private IContainer components;

	private TextBox m_messageBox;

	private Label label1;

	private Label label2;

	private Label label3;

	public ResetForm(DeviceDescriptor descriptor)
	{
		InitializeComponent();
		base.Shown += ResetForm_Shown;
		Descriptor = descriptor;
	}

	private void ResetForm_Shown(object o, EventArgs args)
	{
		Application.DoEvents();
		LogHelper.Instance.Log("Start of reset form.");
		Thread.Sleep(5000);
		Thread thread = new Thread(ResetThread);
		thread.Start();
		try
		{
			if (!PseudoWait.WaitOne(20000))
			{
				LogHelper.Instance.Log("The reset thread didn't start after 20s.");
				base.DialogResult = DialogResult.Cancel;
				Close();
			}
			if (!ResetEvent.WaitOne(Settings.Default.TouchDriverResetTimeout))
			{
				LogHelper.Instance.Log("After 15s, didn't receive continue signal - driver may be stuck.");
				base.DialogResult = DialogResult.Abort;
				Close();
				return;
			}
			for (int num = Settings.Default.TouchDriverWakeup; num >= 0; num--)
			{
				m_messageBox.Text = $"Touch screen available in another {num} seconds.";
				Application.DoEvents();
				Thread.Sleep(1000);
			}
			base.DialogResult = DialogResult.OK;
			LogHelper.Instance.Log("Successfully reset screen.");
			Close();
		}
		finally
		{
			if (!thread.Join(5000))
			{
				LogHelper.Instance.Log("The reset thread did not join after 5s.");
			}
		}
	}

	private void ResetThread()
	{
		PseudoWait.Set();
		int num = 0;
		using (DeviceHelper deviceHelper = new DeviceHelper(Settings.Default.DebugHardware))
		{
			if (!deviceHelper.ChangeWithDriverMatch(Descriptor, Descriptor.DeviceGuid, DeviceState.Disable))
			{
				LogHelper.Instance.Log("** DISABLE driver failed.");
			}
			else
			{
				num++;
			}
			Thread.Sleep(1000);
			if (!deviceHelper.ChangeWithDriverMatch(Descriptor, Descriptor.DeviceGuid, DeviceState.Enable))
			{
				LogHelper.Instance.Log("** ENABLE driver failed.");
			}
			else
			{
				num++;
			}
			LogHelper.Instance.Log("Device reset complete.");
		}
		if (num == 2)
		{
			ResetEvent.Set();
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
		this.m_messageBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_messageBox.BackColor = System.Drawing.Color.Maroon;
		this.m_messageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.m_messageBox.Font = new System.Drawing.Font("Calibri", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_messageBox.ForeColor = System.Drawing.Color.White;
		this.m_messageBox.Location = new System.Drawing.Point(72, 213);
		this.m_messageBox.Name = "m_messageBox";
		this.m_messageBox.ReadOnly = true;
		this.m_messageBox.Size = new System.Drawing.Size(532, 30);
		this.m_messageBox.TabIndex = 2;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Calibri", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label1.ForeColor = System.Drawing.Color.White;
		this.label1.Location = new System.Drawing.Point(67, 156);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(743, 29);
		this.label1.TabIndex = 3;
		this.label1.Text = "We are creating a better experience for you - should take about 15 seconds";
		this.label2.AutoSize = true;
		this.label2.BackColor = System.Drawing.Color.Maroon;
		this.label2.Font = new System.Drawing.Font("Calibri", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.ForeColor = System.Drawing.Color.White;
		this.label2.Location = new System.Drawing.Point(65, 413);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(288, 29);
		this.label2.TabIndex = 4;
		this.label2.Text = "Thank you for your patience";
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Calibri", 18f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.ForeColor = System.Drawing.Color.White;
		this.label3.Location = new System.Drawing.Point(69, 103);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(315, 29);
		this.label3.TabIndex = 5;
		this.label3.Text = "Please do not touch the screen";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Maroon;
		base.ClientSize = new System.Drawing.Size(951, 562);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_messageBox);
		base.Name = "ResetForm";
		this.Text = "ResetForm";
		base.TopMost = true;
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
