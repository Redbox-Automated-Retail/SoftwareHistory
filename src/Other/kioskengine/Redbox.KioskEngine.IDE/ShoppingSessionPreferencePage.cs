using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.IDE.Properties;

namespace Redbox.KioskEngine.IDE;

public class ShoppingSessionPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private ListView m_sessionListView;

	private ColumnHeader m_idColumnHeader;

	private ColumnHeader m_statusColumnHeader;

	private ColumnHeader m_storeNumberColumnHeader;

	private ColumnHeader m_startedOnColumnHeader;

	private ColumnHeader m_endedOnColumnHeader;

	private ColumnHeader m_durationColumnHeader;

	private ToolStrip m_toolStrip;

	private ToolStripButton m_refreshToolStripButton;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripButton m_exportToolStripButton;

	private ToolStripButton m_propertiesToolStripButton;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripButton m_removeToolStripButton;

	private ToolStripButton m_clearToolStripButton;

	private Label m_headerLabel;

	public ShoppingSessionPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		RefreshList();
	}

	private void OnRefresh(object sender, EventArgs e)
	{
		RefreshList();
	}

	private void OnExport(object sender, EventArgs e)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		try
		{
			Cursor = Cursors.WaitCursor;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Title = "Export Session File";
			saveFileDialog.DefaultExt = ".xml";
			saveFileDialog.Filter = "XML Files (*.xml)|*.xml";
			SaveFileDialog saveFileDialog2 = saveFileDialog;
			DialogResult dialogResult = saveFileDialog2.ShowDialog();
			if (dialogResult == DialogResult.OK)
			{
				service.ExporToXml(saveFileDialog2.FileName);
			}
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnRemove(object sender, EventArgs e)
	{
		if (m_sessionListView.SelectedItems.Count == 0)
		{
			return;
		}
		try
		{
			Cursor = Cursors.WaitCursor;
			foreach (ListViewItem selectedItem in m_sessionListView.SelectedItems)
			{
				if (selectedItem.Tag is IShoppingSession shoppingSession)
				{
					shoppingSession.Delete();
				}
			}
			RefreshList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnClear(object sender, EventArgs e)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		try
		{
			Cursor = Cursors.WaitCursor;
			service.Clear();
			RefreshList();
		}
		finally
		{
			Cursor = Cursors.Default;
		}
	}

	private void OnProperties(object sender, EventArgs e)
	{
		if (m_sessionListView.SelectedItems.Count != 0)
		{
			ShoppingSessionDetailForm shoppingSessionDetailForm = new ShoppingSessionDetailForm();
			shoppingSessionDetailForm.Session = m_sessionListView.SelectedItems[0].Tag as IShoppingSession;
			ShoppingSessionDetailForm shoppingSessionDetailForm2 = shoppingSessionDetailForm;
			shoppingSessionDetailForm2.ShowDialog();
		}
	}

	private void OnSelectedSessionChanged(object sender, EventArgs e)
	{
		m_removeToolStripButton.Enabled = m_sessionListView.SelectedItems.Count > 0;
		m_propertiesToolStripButton.Enabled = m_sessionListView.SelectedItems.Count == 1;
	}

	private void RefreshList()
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		m_sessionListView.SuspendLayout();
		m_sessionListView.BeginUpdate();
		m_sessionListView.Items.Clear();
		service.ForEach(delegate(IShoppingSession s)
		{
			ListViewItem listViewItem = new ListViewItem(s.Id.ToString())
			{
				Tag = s
			};
			listViewItem.SubItems.Add(s.Status.ToString());
			listViewItem.SubItems.Add(s.StoreNumber);
			listViewItem.SubItems.Add(s.StartedOn.ToString());
			if (s.EndedOn.HasValue)
			{
				listViewItem.SubItems.Add(s.EndedOn.ToString());
				listViewItem.SubItems.Add((s.EndedOn - s.StartedOn).ToString());
			}
			else
			{
				listViewItem.SubItems.Add("N/A");
				listViewItem.SubItems.Add("N/A");
			}
			m_sessionListView.Items.Add(listViewItem);
		});
		m_sessionListView.EndUpdate();
		m_sessionListView.ResumeLayout();
		m_clearToolStripButton.Enabled = m_sessionListView.Items.Count > 0;
		m_exportToolStripButton.Enabled = m_sessionListView.Items.Count > 0;
		m_removeToolStripButton.Enabled = m_sessionListView.SelectedItems.Count > 0;
		m_propertiesToolStripButton.Enabled = m_sessionListView.SelectedItems.Count > 0;
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
		this.m_sessionListView = new System.Windows.Forms.ListView();
		this.m_idColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_statusColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_storeNumberColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_startedOnColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_endedOnColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_durationColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_toolStrip = new System.Windows.Forms.ToolStrip();
		this.m_refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.m_exportToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_propertiesToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.m_removeToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_clearToolStripButton = new System.Windows.Forms.ToolStripButton();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_toolStrip.SuspendLayout();
		base.SuspendLayout();
		this.m_sessionListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[6] { this.m_idColumnHeader, this.m_statusColumnHeader, this.m_storeNumberColumnHeader, this.m_startedOnColumnHeader, this.m_endedOnColumnHeader, this.m_durationColumnHeader });
		this.m_sessionListView.FullRowSelect = true;
		this.m_sessionListView.Location = new System.Drawing.Point(0, 69);
		this.m_sessionListView.Name = "m_sessionListView";
		this.m_sessionListView.Size = new System.Drawing.Size(542, 437);
		this.m_sessionListView.TabIndex = 0;
		this.m_sessionListView.UseCompatibleStateImageBehavior = false;
		this.m_sessionListView.View = System.Windows.Forms.View.Details;
		this.m_sessionListView.ItemActivate += new System.EventHandler(OnProperties);
		this.m_sessionListView.SelectedIndexChanged += new System.EventHandler(OnSelectedSessionChanged);
		this.m_idColumnHeader.Text = "ID";
		this.m_idColumnHeader.Width = 220;
		this.m_statusColumnHeader.Text = "Status";
		this.m_statusColumnHeader.Width = 65;
		this.m_storeNumberColumnHeader.Text = "Store Number";
		this.m_storeNumberColumnHeader.Width = 80;
		this.m_startedOnColumnHeader.Text = "Started On";
		this.m_startedOnColumnHeader.Width = 130;
		this.m_endedOnColumnHeader.Text = "Ended On";
		this.m_endedOnColumnHeader.Width = 130;
		this.m_durationColumnHeader.Text = "Duration";
		this.m_durationColumnHeader.Width = 100;
		this.m_toolStrip.CanOverflow = false;
		this.m_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
		this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[7] { this.m_refreshToolStripButton, this.toolStripSeparator1, this.m_exportToolStripButton, this.m_propertiesToolStripButton, this.toolStripSeparator2, this.m_removeToolStripButton, this.m_clearToolStripButton });
		this.m_toolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
		this.m_toolStrip.Location = new System.Drawing.Point(4, 43);
		this.m_toolStrip.Name = "m_toolStrip";
		this.m_toolStrip.Size = new System.Drawing.Size(105, 23);
		this.m_toolStrip.TabIndex = 6;
		this.m_refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_refreshToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Refresh;
		this.m_refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_refreshToolStripButton.Name = "m_refreshToolStripButton";
		this.m_refreshToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_refreshToolStripButton.Text = "Refresh";
		this.m_refreshToolStripButton.Click += new System.EventHandler(OnRefresh);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
		this.m_exportToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_exportToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.SaveAs;
		this.m_exportToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_exportToolStripButton.Name = "m_exportToolStripButton";
		this.m_exportToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_exportToolStripButton.Text = "Export";
		this.m_exportToolStripButton.Click += new System.EventHandler(OnExport);
		this.m_propertiesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_propertiesToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Properties;
		this.m_propertiesToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_propertiesToolStripButton.Name = "m_propertiesToolStripButton";
		this.m_propertiesToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_propertiesToolStripButton.Text = "Properties";
		this.m_propertiesToolStripButton.Click += new System.EventHandler(OnProperties);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 23);
		this.m_removeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_removeToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Delete;
		this.m_removeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_removeToolStripButton.Name = "m_removeToolStripButton";
		this.m_removeToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_removeToolStripButton.Text = "Remove";
		this.m_removeToolStripButton.Click += new System.EventHandler(OnRemove);
		this.m_clearToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.m_clearToolStripButton.Image = Redbox.KioskEngine.IDE.Properties.Resources.Clean;
		this.m_clearToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.m_clearToolStripButton.Name = "m_clearToolStripButton";
		this.m_clearToolStripButton.Size = new System.Drawing.Size(23, 20);
		this.m_clearToolStripButton.Text = "Clear";
		this.m_clearToolStripButton.Visible = false;
		this.m_clearToolStripButton.Click += new System.EventHandler(OnClear);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.MediumBlue;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 16;
		this.m_headerLabel.Text = "  Engine Core: Sessions";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_sessionListView);
		base.Controls.Add(this.m_toolStrip);
		base.Name = "ShoppingSessionPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		this.m_toolStrip.ResumeLayout(false);
		this.m_toolStrip.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
