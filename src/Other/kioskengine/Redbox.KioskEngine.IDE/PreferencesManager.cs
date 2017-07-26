using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class PreferencesManager : Form
{
	private bool m_isActivated;

	private IContainer components;

	private TreeView m_treeView;

	private Button m_closeButton;

	private GroupBox m_groupBox;

	private Panel m_pagePanel;

	private ImageList m_imageList;

	public PreferencesManager()
	{
		InitializeComponent();
	}

	private void OnClose(object sender, EventArgs e)
	{
		if (!DeactivateCurrentPage())
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			if (MessageBox.Show("There are unsaved changes because of errors.\n\nWould you like to correct them or close the window and lose your changes?", service.ExpandProperties("${ProductName} ${ProductVersion}"), MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
			{
				return;
			}
		}
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void OnAfterSelect(object sender, TreeViewEventArgs e)
	{
		e.Node.SelectedImageIndex = e.Node.ImageIndex;
		ServiceLocator.Instance.GetService<IUserSettingsStore>().SetValue("Shell\\Preferences", "CurrentSelection", m_treeView.SelectedNode.FullPath);
		m_pagePanel.Controls.Clear();
		if (e.Node.Tag is IPreferencePage { Host: not null } preferencePage)
		{
			preferencePage.RaiseActivate();
			if (preferencePage.Host is Control control)
			{
				preferencePage.Host.LoadValues(preferencePage);
				control.Dock = DockStyle.Fill;
				m_pagePanel.Controls.Add(control);
			}
		}
	}

	private void OnBeforeSelect(object sender, TreeViewCancelEventArgs e)
	{
		if (!DeactivateCurrentPage())
		{
			e.Cancel = true;
		}
	}

	private void OnAfterCollapse(object sender, TreeViewEventArgs e)
	{
		e.Node.ImageIndex = 0;
		e.Node.SelectedImageIndex = e.Node.ImageIndex;
	}

	private void OnAfterExpand(object sender, TreeViewEventArgs e)
	{
		e.Node.ImageIndex = 1;
		e.Node.SelectedImageIndex = e.Node.ImageIndex;
	}

	private void OnActivated(object sender, EventArgs e)
	{
		if (m_isActivated)
		{
			return;
		}
		m_isActivated = true;
		RefreshTree();
		IUserSettingsStore service = ServiceLocator.Instance.GetService<IUserSettingsStore>();
		if (m_treeView.SelectedNode != null)
		{
			service.SetValue("Shell\\Preferences", "CurrentSelection", m_treeView.SelectedNode.FullPath);
		}
		string value = service.GetValue<string>("Shell\\Preferences", "CurrentSelection");
		if (value == null)
		{
			return;
		}
		string[] array = value.Split(m_treeView.PathSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		TreeNode treeNode = null;
		string[] array2 = array;
		foreach (string key in array2)
		{
			TreeNode[] array3 = ((treeNode == null) ? m_treeView.Nodes.Find(key, searchAllChildren: false) : treeNode.Nodes.Find(key, searchAllChildren: false));
			if (array3.Length == 0)
			{
				break;
			}
			treeNode = array3[0];
			treeNode.Expand();
		}
		m_treeView.SelectedNode = treeNode;
	}

	private void RefreshTree()
	{
		IPreferenceService service = ServiceLocator.Instance.GetService<IPreferenceService>();
		if (service == null)
		{
			return;
		}
		m_treeView.SuspendLayout();
		m_treeView.BeginUpdate();
		foreach (IPreferencePage preferencePage in service.PreferencePages)
		{
			List<string> list = new List<string>();
			if (string.IsNullOrEmpty(preferencePage.DisplayPath))
			{
				list.Add(preferencePage.Name);
			}
			else
			{
				list.AddRange(preferencePage.DisplayPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));
			}
			TreeNode treeNode = null;
			foreach (string item in list)
			{
				TreeNode[] array = ((treeNode == null) ? m_treeView.Nodes.Find(item, searchAllChildren: false) : treeNode.Nodes.Find(item, searchAllChildren: false));
				if (array.Length != 0)
				{
					treeNode = array[0];
					continue;
				}
				treeNode = ((treeNode == null) ? m_treeView.Nodes.Add(item, item) : treeNode.Nodes.Add(item, item));
				if (item == list[list.Count - 1])
				{
					treeNode.Tag = preferencePage;
				}
				treeNode.ImageIndex = 2;
				if (treeNode.Parent != null && treeNode.Parent.Nodes.Count > 0)
				{
					treeNode.Parent.ImageIndex = 0;
				}
			}
		}
		m_treeView.EndUpdate();
		m_treeView.ResumeLayout();
	}

	private bool DeactivateCurrentPage()
	{
		if (m_treeView.SelectedNode == null)
		{
			return true;
		}
		if (!(m_treeView.SelectedNode.Tag is IPreferencePage { Host: not null } preferencePage))
		{
			return true;
		}
		if (!preferencePage.Host.SaveValues(preferencePage))
		{
			return false;
		}
		preferencePage.RaiseDeactivate();
		return true;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.IDE.PreferencesManager));
		this.m_treeView = new System.Windows.Forms.TreeView();
		this.m_imageList = new System.Windows.Forms.ImageList(this.components);
		this.m_closeButton = new System.Windows.Forms.Button();
		this.m_groupBox = new System.Windows.Forms.GroupBox();
		this.m_pagePanel = new System.Windows.Forms.Panel();
		base.SuspendLayout();
		this.m_treeView.FullRowSelect = true;
		this.m_treeView.HideSelection = false;
		this.m_treeView.ImageIndex = 0;
		this.m_treeView.ImageList = this.m_imageList;
		this.m_treeView.ItemHeight = 19;
		this.m_treeView.Location = new System.Drawing.Point(12, 12);
		this.m_treeView.Name = "m_treeView";
		this.m_treeView.PathSeparator = "/";
		this.m_treeView.SelectedImageIndex = 0;
		this.m_treeView.Size = new System.Drawing.Size(222, 514);
		this.m_treeView.TabIndex = 0;
		this.m_treeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(OnAfterCollapse);
		this.m_treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(OnAfterSelect);
		this.m_treeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(OnBeforeSelect);
		this.m_treeView.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(OnAfterExpand);
		this.m_imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("m_imageList.ImageStream");
		this.m_imageList.TransparentColor = System.Drawing.Color.Transparent;
		this.m_imageList.Images.SetKeyName(0, "folder-closed.png");
		this.m_imageList.Images.SetKeyName(1, "folder.png");
		this.m_imageList.Images.SetKeyName(2, "settings.png");
		this.m_closeButton.Location = new System.Drawing.Point(707, 540);
		this.m_closeButton.Name = "m_closeButton";
		this.m_closeButton.Size = new System.Drawing.Size(75, 23);
		this.m_closeButton.TabIndex = 1;
		this.m_closeButton.Text = "Close";
		this.m_closeButton.UseVisualStyleBackColor = true;
		this.m_closeButton.Click += new System.EventHandler(OnClose);
		this.m_groupBox.Location = new System.Drawing.Point(240, 524);
		this.m_groupBox.Name = "m_groupBox";
		this.m_groupBox.Size = new System.Drawing.Size(543, 2);
		this.m_groupBox.TabIndex = 3;
		this.m_groupBox.TabStop = false;
		this.m_pagePanel.Location = new System.Drawing.Point(240, 12);
		this.m_pagePanel.Name = "m_pagePanel";
		this.m_pagePanel.Size = new System.Drawing.Size(542, 506);
		this.m_pagePanel.TabIndex = 4;
		base.AcceptButton = this.m_closeButton;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(794, 575);
		base.Controls.Add(this.m_pagePanel);
		base.Controls.Add(this.m_groupBox);
		base.Controls.Add(this.m_closeButton);
		base.Controls.Add(this.m_treeView);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		base.HelpButton = true;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "PreferencesManager";
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Engine Preferences & Configuration";
		base.Activated += new System.EventHandler(OnActivated);
		base.ResumeLayout(false);
	}
}
