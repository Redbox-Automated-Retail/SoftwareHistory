using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;
using Redbox.Lua;

namespace Redbox.KioskEngine.IDE;

public class LoginForm : Form
{
	public bool ShowDefaultBundle = true;

	private IContainer components;

	private Panel panel1;

	private Panel panel2;

	private Label label1;

	private Panel panel3;

	private Label label2;

	private TextBox m_userIdTextBox;

	private Label label3;

	private TextBox m_passwordTextBox;

	private Button m_loginButton;

	private Button m_closeButton;

	private ErrorProvider m_errorProvider;

	private System.Windows.Forms.PictureBox m_spinnerPictureBox;

	private Timer m_idleTimer;

	public bool IsValid { get; set; }

	public LuaFunction LoginCallback { get; set; }

	public LuaFunction CancelLoginCallback { get; set; }

	public LoginForm()
	{
		InitializeComponent();
	}

	public void CloseAsCancel()
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	public void CloseAsLoggedIn()
	{
		ShowDefaultBundle = false;
		base.DialogResult = DialogResult.OK;
		Close();
	}

	public void SetTimeout(int timeout)
	{
		m_idleTimer.Interval = timeout;
	}

	public void ShowError(string message)
	{
		m_errorProvider.SetError(m_userIdTextBox, "Invalid UserName / Password");
		m_spinnerPictureBox.Visible = false;
		m_userIdTextBox.Enabled = true;
		m_passwordTextBox.Enabled = true;
		m_loginButton.Enabled = true;
		m_closeButton.Enabled = true;
		m_userIdTextBox.Focus();
		ShowDefaultBundle = true;
	}

	private void OnLogin(object sender, EventArgs e)
	{
		m_errorProvider.SetError(m_userIdTextBox, string.Empty);
		m_errorProvider.SetError(m_passwordTextBox, string.Empty);
		if (string.IsNullOrEmpty(m_userIdTextBox.Text))
		{
			m_errorProvider.SetError(m_userIdTextBox, "User ID is a required field.");
		}
		else if (string.IsNullOrEmpty(m_passwordTextBox.Text))
		{
			m_errorProvider.SetError(m_passwordTextBox, "Password is a required field.");
		}
		else if (LoginCallback != null)
		{
			m_spinnerPictureBox.Visible = true;
			m_loginButton.Enabled = false;
			m_closeButton.Enabled = false;
			m_userIdTextBox.Enabled = false;
			m_passwordTextBox.Enabled = false;
			Application.DoEvents();
			LuaTable luaTable = (LuaTable)ServiceLocator.Instance.GetService<IKernelService>().NewTable();
			luaTable["user_id"] = m_userIdTextBox.Text;
			luaTable["password"] = m_passwordTextBox.Text;
			try
			{
				LoginCallback.Call(luaTable);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("An unhandled exception was raised invoking LoginCallback.", e2);
			}
		}
	}

	private void OnClose(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
	}

	private void OnIdleTimeout(object sender, EventArgs e)
	{
		OnClose(sender, e);
	}

	private void OnFormClosed(object sender, FormClosedEventArgs e)
	{
		try
		{
			if (ShowDefaultBundle && CancelLoginCallback != null)
			{
				CancelLoginCallback.Call();
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was raised invoking CancelLoginCallback.", e2);
		}
		Cursor.Hide();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		Cursor.Show();
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.LoginForm));
		this.panel1 = new System.Windows.Forms.Panel();
		this.m_spinnerPictureBox = new System.Windows.Forms.PictureBox();
		this.panel3 = new System.Windows.Forms.Panel();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_userIdTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_passwordTextBox = new System.Windows.Forms.TextBox();
		this.m_loginButton = new System.Windows.Forms.Button();
		this.m_closeButton = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_idleTimer = new System.Windows.Forms.Timer(this.components);
		this.panel1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_spinnerPictureBox).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.panel1.BackColor = System.Drawing.SystemColors.Window;
		this.panel1.Controls.Add(this.m_spinnerPictureBox);
		this.panel1.Controls.Add(this.panel3);
		this.panel1.Controls.Add(this.panel2);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(641, 153);
		this.panel1.TabIndex = 0;
		this.m_spinnerPictureBox.Image = (System.Drawing.Image)resources.GetObject("m_spinnerPictureBox.Image");
		this.m_spinnerPictureBox.Location = new System.Drawing.Point(299, 118);
		this.m_spinnerPictureBox.Name = "m_spinnerPictureBox";
		this.m_spinnerPictureBox.Size = new System.Drawing.Size(36, 32);
		this.m_spinnerPictureBox.TabIndex = 3;
		this.m_spinnerPictureBox.TabStop = false;
		this.m_spinnerPictureBox.Visible = false;
		this.panel3.BackgroundImage = Redbox.KioskEngine.IDE.Properties.Resources.RedboxNameLogo;
		this.panel3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.panel3.Location = new System.Drawing.Point(194, 12);
		this.panel3.Name = "panel3";
		this.panel3.Size = new System.Drawing.Size(297, 100);
		this.panel3.TabIndex = 1;
		this.panel2.BackgroundImage = Redbox.KioskEngine.IDE.Properties.Resources.RedboxBoxLogo;
		this.panel2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
		this.panel2.Location = new System.Drawing.Point(86, 12);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(102, 100);
		this.panel2.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(4, 137);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(153, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "Redbox Automated Retail, LLC";
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label2.Location = new System.Drawing.Point(51, 183);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(76, 20);
		this.label2.TabIndex = 1;
		this.label2.Text = "User ID:";
		this.m_userIdTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_userIdTextBox.Location = new System.Drawing.Point(133, 180);
		this.m_userIdTextBox.MaxLength = 64;
		this.m_userIdTextBox.Name = "m_userIdTextBox";
		this.m_userIdTextBox.Size = new System.Drawing.Size(420, 26);
		this.m_userIdTextBox.TabIndex = 2;
		this.label3.AutoSize = true;
		this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.label3.Location = new System.Drawing.Point(36, 224);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(91, 20);
		this.label3.TabIndex = 3;
		this.label3.Text = "Password:";
		this.m_passwordTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_passwordTextBox.Location = new System.Drawing.Point(133, 224);
		this.m_passwordTextBox.MaxLength = 64;
		this.m_passwordTextBox.Name = "m_passwordTextBox";
		this.m_passwordTextBox.Size = new System.Drawing.Size(420, 26);
		this.m_passwordTextBox.TabIndex = 4;
		this.m_passwordTextBox.UseSystemPasswordChar = true;
		this.m_loginButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_loginButton.Location = new System.Drawing.Point(133, 274);
		this.m_loginButton.Name = "m_loginButton";
		this.m_loginButton.Size = new System.Drawing.Size(202, 55);
		this.m_loginButton.TabIndex = 5;
		this.m_loginButton.Text = "Login";
		this.m_loginButton.UseVisualStyleBackColor = true;
		this.m_loginButton.Click += new System.EventHandler(OnLogin);
		this.m_closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.m_closeButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_closeButton.Location = new System.Drawing.Point(341, 274);
		this.m_closeButton.Name = "m_closeButton";
		this.m_closeButton.Size = new System.Drawing.Size(212, 55);
		this.m_closeButton.TabIndex = 6;
		this.m_closeButton.Text = "Close";
		this.m_closeButton.UseVisualStyleBackColor = true;
		this.m_closeButton.Click += new System.EventHandler(OnClose);
		this.m_errorProvider.ContainerControl = this;
		this.m_idleTimer.Enabled = true;
		this.m_idleTimer.Interval = 300000;
		this.m_idleTimer.Tick += new System.EventHandler(OnIdleTimeout);
		base.AcceptButton = this.m_loginButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.CancelButton = this.m_closeButton;
		base.ClientSize = new System.Drawing.Size(641, 349);
		base.Controls.Add(this.m_closeButton);
		base.Controls.Add(this.m_loginButton);
		base.Controls.Add(this.m_passwordTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_userIdTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.panel1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "LoginForm";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Redbox Login";
		base.TopMost = true;
		base.Load += new System.EventHandler(OnLoad);
		base.FormClosed += new System.Windows.Forms.FormClosedEventHandler(OnFormClosed);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.m_spinnerPictureBox).EndInit();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
