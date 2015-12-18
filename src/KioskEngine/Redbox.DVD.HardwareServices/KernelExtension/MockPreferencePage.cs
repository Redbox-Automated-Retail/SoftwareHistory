using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.HardwareServices.KernelExtension.Properties;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.KernelExtension;

public class MockPreferencePage : UserControl, IPreferencePageHost
{
	private sealed class Script
	{
		public bool IsActive;

		public string Name;

		public string Scenario;

		public string RunTime;

		public string Code;
	}

	private bool m_isRefreshing;

	private readonly List<Script> m_scripts = new List<Script>();

	private IContainer components;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_addToolStripButton;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton m_removeToolStripButton;

	private ToolStripButton m_clearToolStripButton;

	private ColumnHeader m_scriptNameColumnHeader;

	private ColumnHeader m_scenarioNameColumnHeader;

	private ColumnHeader m_runTimeColumnHeader;

	private ListView m_listView;

	private ColumnHeader m_scriptSizeColumnHeader;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton m_exportToolStripButton;

	private ToolStripButton m_importToolStripButton;

	private ToolStripButton m_editToolStripButton;

	private ToolStripButton m_makeActiveToolStripButton;

	private Label m_headerLabel;

	public MockPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		List<string> list = new List<string>();
		foreach (Script script in m_scripts)
		{
			list.Add(script.ToJson());
		}
		preferencePage.SetValue("MockScripts", list.ToArray());
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_scripts.Clear();
		string[] value = preferencePage.GetValue("MockScripts", new string[0]);
		foreach (string json in value)
		{
			m_scripts.Add(json.ToObject<Script>());
		}
		RefreshList();
	}

	private void OnSelectedScenarioChanged(object sender, EventArgs e)
	{
		m_removeToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_editToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		m_makeActiveToolStripButton.Enabled = m_listView.SelectedItems.Count > 0;
		if (m_listView.SelectedItems.Count > 0 && m_listView.SelectedItems[0].Tag is Script script)
		{
			m_makeActiveToolStripButton.Checked = script.IsActive;
		}
	}

	private void OnAdd(object sender, EventArgs e)
	{
		ScriptEditorForm scriptEditorForm = new ScriptEditorForm();
		scriptEditorForm.ShowDialog();
		if (!string.IsNullOrEmpty(scriptEditorForm.ScriptName) && !string.IsNullOrEmpty(scriptEditorForm.ScenarioName) && !string.IsNullOrEmpty(scriptEditorForm.RunTime))
		{
			m_scripts.Add(new Script
			{
				Name = scriptEditorForm.ScriptName,
				Scenario = scriptEditorForm.ScenarioName,
				RunTime = scriptEditorForm.RunTime,
				Code = scriptEditorForm.Code
			});
			RefreshList();
		}
	}

	private void OnRemove(object sender, EventArgs e)
	{
		if (m_listView.SelectedItems.Count != 0 && m_listView.SelectedItems[0].Tag is Script item)
		{
			m_scripts.Remove(item);
			RefreshList();
		}
	}

	private void OnClear(object sender, EventArgs e)
	{
		m_scripts.Clear();
		RefreshList();
	}

	private void OnMakeActive(object sender, EventArgs e)
	{
		if (!(m_listView.SelectedItems[0].Tag is Script script))
		{
			return;
		}
		foreach (Script script2 in m_scripts)
		{
			if (script2.Name == script.Name)
			{
				script2.IsActive = false;
			}
		}
		script.IsActive = true;
		m_makeActiveToolStripButton.Checked = true;
		RefreshList();
	}

	private void OnExport(object sender, EventArgs e)
	{
	}

	private void OnImport(object sender, EventArgs e)
	{
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshList();
	}

	private void OnItemActivate(object sender, EventArgs e)
	{
		if (m_listView.SelectedItems.Count != 0 && m_listView.SelectedItems[0].Tag is Script script)
		{
			ScriptEditorForm scriptEditorForm = new ScriptEditorForm();
			scriptEditorForm.ScriptName = script.Name;
			scriptEditorForm.ScenarioName = script.Scenario;
			scriptEditorForm.RunTime = script.RunTime;
			scriptEditorForm.Code = script.Code;
			scriptEditorForm.ShowDialog();
			RefreshList();
		}
	}

	private void RefreshList()
	{
		if (m_isRefreshing)
		{
			return;
		}
		try
		{
			m_isRefreshing = true;
			m_listView.SuspendLayout();
			m_listView.BeginUpdate();
			m_listView.Items.Clear();
			foreach (Script script in m_scripts)
			{
				ListViewItem listViewItem = new ListViewItem(script.Scenario)
				{
					Tag = script
				};
				listViewItem.SubItems.Add(script.Name);
				listViewItem.SubItems.Add(script.RunTime);
				listViewItem.SubItems.Add(script.Code.Length.FormatAsByteSize());
				listViewItem.BackColor = (script.IsActive ? Color.Yellow : m_listView.BackColor);
				m_listView.Items.Add(listViewItem);
			}
			m_listView.EndUpdate();
			m_listView.ResumeLayout();
			m_clearToolStripButton.Enabled = m_listView.Items.Count > 0;
		}
		finally
		{
			m_isRefreshing = false;
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
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_addToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_removeToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_editToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_makeActiveToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_clearToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_exportToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_importToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_scriptNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_scenarioNameColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_runTimeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_listView = new System.Windows.Forms.ListView();
		this.m_scriptSizeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.m_refreshToolStripButton, this.toolStripSeparator1, this.m_addToolStripButton, this.m_removeToolStripButton, this.m_editToolStripButton, this.m_makeActiveToolStripButton, this.toolStripSeparator2, this.m_clearToolStripButton, this.m_exportToolStripButton, this.m_importToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(0, 38);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(197, 23);
		this.m_toolStrip.TabIndex = 0;
		this.m_toolStrip.Text = "toolStrip1";
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.Text = "Refresh";
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.m_addToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_addToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Plus;
		this.m_addToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_addToolStripButton.Name = "m_addToolStripButton";
		this.m_addToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_addToolStripButton.Text = "Add Script Scenario";
		this.m_addToolStripButton.Click += new System.EventHandler(OnAdd);
		this.m_removeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_removeToolStripButton.Enabled = false;
		this.m_removeToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Minus;
		this.m_removeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_removeToolStripButton.Name = "m_removeToolStripButton";
		this.m_removeToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_removeToolStripButton.Text = "Remove";
		this.m_removeToolStripButton.Click += new System.EventHandler(OnRemove);
		this.m_editToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_editToolStripButton.Enabled = false;
		this.m_editToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Edit;
		this.m_editToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_editToolStripButton.Name = "m_editToolStripButton";
		this.m_editToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_editToolStripButton.Text = "Edit";
		this.m_editToolStripButton.Click += new System.EventHandler(OnItemActivate);
		this.m_makeActiveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_makeActiveToolStripButton.Enabled = false;
		this.m_makeActiveToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Execute;
		this.m_makeActiveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_makeActiveToolStripButton.Name = "m_makeActiveToolStripButton";
		this.m_makeActiveToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_makeActiveToolStripButton.Text = "Make Active";
		this.m_makeActiveToolStripButton.Click += new System.EventHandler(OnMakeActive);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
		this.m_clearToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_clearToolStripButton.Enabled = false;
		this.m_clearToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Clean;
		this.m_clearToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_clearToolStripButton.Name = "m_clearToolStripButton";
		this.m_clearToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_clearToolStripButton.Text = "Clear List";
		this.m_clearToolStripButton.Click += new System.EventHandler(OnClear);
		this.m_exportToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_exportToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.SaveAs;
		this.m_exportToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_exportToolStripButton.Name = "m_exportToolStripButton";
		this.m_exportToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_exportToolStripButton.Text = "Export";
		this.m_exportToolStripButton.Click += new System.EventHandler(OnExport);
		this.m_importToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_importToolStripButton.Image = Redbox.HardwareServices.KernelExtension.Properties.Resources.Open;
		this.m_importToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_importToolStripButton.Name = "m_importToolStripButton";
		this.m_importToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_importToolStripButton.Text = "Import";
		this.m_importToolStripButton.Click += new System.EventHandler(OnImport);
		this.m_scriptNameColumnHeader.Text = "Script Name";
		this.m_scriptNameColumnHeader.Width = 150;
		this.m_scenarioNameColumnHeader.Text = "Scenario Name";
		this.m_scenarioNameColumnHeader.Width = 150;
		this.m_runTimeColumnHeader.Text = "Run time";
		this.m_listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[4] { this.m_scenarioNameColumnHeader, this.m_scriptNameColumnHeader, this.m_runTimeColumnHeader, this.m_scriptSizeColumnHeader });
		this.m_listView.FullRowSelect = true;
		this.m_listView.HideSelection = false;
		this.m_listView.Location = new System.Drawing.Point(0, 64);
		this.m_listView.MultiSelect = false;
		this.m_listView.Name = "m_listView";
		this.m_listView.Size = new System.Drawing.Size(542, 442);
		this.m_listView.TabIndex = 1;
		this.m_listView.UseCompatibleStateImageBehavior = false;
		this.m_listView.View = System.Windows.Forms.View.Details;
		this.m_listView.ItemActivate += new System.EventHandler(OnItemActivate);
		this.m_listView.SelectedIndexChanged += new System.EventHandler(OnSelectedScenarioChanged);
		this.m_scriptSizeColumnHeader.Text = "Size (bytes)";
		this.m_scriptSizeColumnHeader.Width = 110;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.Gold;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.Black;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Hardware Service: Mock Settings";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_listView);
		base.Controls.Add(this.m_toolStrip);
		base.Name = "MockPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
