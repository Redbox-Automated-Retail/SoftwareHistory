using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public class PropertiesView : Form
{
	private ExternalDevice m_device;

	private DataTable m_table;

	private IContainer components;

	private Button m_okButton;

	private Button m_cancelButton;

	private DataGridView m_dataGridView;

	public bool Modified { get; private set; }

	public PropertiesView(ExternalDevice device)
	{
		InitializeComponent();
		m_device = device;
		m_dataGridView.DataSource = MakeTable();
	}

	private DataTable MakeTable()
	{
		DataTable dataTable = new DataTable();
		DataColumn dataColumn = new DataColumn();
		dataColumn.DataType = Type.GetType("System.String");
		dataColumn.ColumnName = "Name";
		dataTable.Columns.Add(dataColumn);
		DataColumn dataColumn2 = new DataColumn();
		dataColumn2.DataType = Type.GetType("System.String");
		dataColumn2.ColumnName = "Value";
		dataTable.Columns.Add(dataColumn2);
		for (int i = 0; i < m_device.Properties.Count; i++)
		{
			if (!m_device.Properties[i].ReadOnly)
			{
				dataTable.Rows.Add(m_device.Properties[i].Key, m_device.Properties[i].Value);
			}
		}
		return m_table = dataTable;
	}

	private void m_okButton_Click(object sender, EventArgs e)
	{
		int num = 0;
		foreach (DataRow row in m_table.Rows)
		{
			m_device.Properties[num].Value = row[1] as string;
			num++;
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void m_cancelButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.Cancel;
		Close();
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
		this.m_okButton = new System.Windows.Forms.Button();
		this.m_cancelButton = new System.Windows.Forms.Button();
		this.m_dataGridView = new System.Windows.Forms.DataGridView();
		((System.ComponentModel.ISupportInitialize)this.m_dataGridView).BeginInit();
		base.SuspendLayout();
		this.m_okButton.Location = new System.Drawing.Point(23, 180);
		this.m_okButton.Name = "m_okButton";
		this.m_okButton.Size = new System.Drawing.Size(128, 41);
		this.m_okButton.TabIndex = 2;
		this.m_okButton.Text = "Ok";
		this.m_okButton.UseVisualStyleBackColor = true;
		this.m_okButton.Click += new System.EventHandler(m_okButton_Click);
		this.m_cancelButton.Location = new System.Drawing.Point(257, 180);
		this.m_cancelButton.Name = "m_cancelButton";
		this.m_cancelButton.Size = new System.Drawing.Size(120, 41);
		this.m_cancelButton.TabIndex = 3;
		this.m_cancelButton.Text = "Cancel";
		this.m_cancelButton.UseVisualStyleBackColor = true;
		this.m_cancelButton.Click += new System.EventHandler(m_cancelButton_Click);
		this.m_dataGridView.AllowUserToAddRows = false;
		this.m_dataGridView.AllowUserToDeleteRows = false;
		this.m_dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.m_dataGridView.Location = new System.Drawing.Point(23, 12);
		this.m_dataGridView.Name = "m_dataGridView";
		this.m_dataGridView.Size = new System.Drawing.Size(354, 150);
		this.m_dataGridView.TabIndex = 4;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(405, 252);
		base.Controls.Add(this.m_dataGridView);
		base.Controls.Add(this.m_cancelButton);
		base.Controls.Add(this.m_okButton);
		base.Name = "PropertiesView";
		this.Text = "PropertiesView";
		((System.ComponentModel.ISupportInitialize)this.m_dataGridView).EndInit();
		base.ResumeLayout(false);
	}
}
