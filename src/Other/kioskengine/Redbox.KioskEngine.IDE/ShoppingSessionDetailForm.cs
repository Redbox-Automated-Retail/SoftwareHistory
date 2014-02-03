using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.IDE;

public class ShoppingSessionDetailForm : Form
{
	private IContainer components;

	private Panel m_shoppingCartPanel;

	private Panel m_eventsPanel;

	private ListView m_eventListView;

	private Label label1;

	private ColumnHeader m_createdOnColumnHeader;

	private ColumnHeader m_typeColumnHeader;

	private ColumnHeader m_descriptionColumnHeader;

	private Label label5;

	private Label label4;

	private Label label3;

	private Label label9;

	private Label label8;

	private Label label7;

	private Label label6;

	private TextBox m_createdOnTextBox;

	private TextBox m_storeNumberTextBox;

	private TextBox m_sessionIdTextBox;

	private TextBox m_referenceNumberTextBox;

	private TextBox m_typeTextBox;

	private TextBox m_discountValueTextBox;

	private TextBox m_discountCodeTextBox;

	private ListView m_lineItemListView;

	private Panel m_topPanel;

	private ColumnHeader m_barcodeColumnHeader;

	private ColumnHeader m_groupColumnHeader;

	private ColumnHeader m_quantityColumnHeader;

	private ColumnHeader m_amountColumnHeader;

	private ColumnHeader m_extensionColumnHeader;

	private Label label10;

	private ColumnHeader m_classColumnHeader;

	private Splitter m_splitter;

	private TextBox m_sessionStatusTextBox;

	private Label label11;

	private TextBox m_lastFourTextBox;

	private TextBox m_expiryDateTextBox;

	private TextBox m_firstNameTextBox;

	private TextBox m_lastNameTextBox;

	private Label label12;

	private Label label2;

	private Label label14;

	private Label label13;

	private GroupBox groupBox1;

	private ColumnHeader m_statusColumnHeader;

	private Label label15;

	private TextBox m_emailTextBox;

	private Panel m_errorsPanel;

	private ListView m_errorListView;

	private ColumnHeader m_errorCreatedOnColumnHeader;

	private Label label16;

	private ColumnHeader m_errorCodeColumnHeader;

	private ColumnHeader m_errorDescriptionColumnHeader;

	private Panel m_lowerPanel;

	private Splitter m_splitter2;

	public IShoppingSession Session { get; set; }

	public ShoppingSessionDetailForm()
	{
		InitializeComponent();
	}

	private void OnLoad(object sender, EventArgs e)
	{
		Session.LoadShoppingCart();
		m_sessionIdTextBox.Text = Session.Id.ToString();
		m_storeNumberTextBox.Text = Session.StoreNumber;
		m_sessionStatusTextBox.Text = Session.Status.ToString();
		if (Session.ShoppingCart != null)
		{
			m_emailTextBox.Text = Session.ShoppingCart.Email;
			m_createdOnTextBox.Text = Session.ShoppingCart.CreatedOn.ToString();
			m_discountCodeTextBox.Text = Session.ShoppingCart.DiscountCode;
			m_discountValueTextBox.Text = Session.ShoppingCart.DiscountValue.ToString("c");
			m_typeTextBox.Text = Session.ShoppingCart.Type.ToString();
			m_lastNameTextBox.Text = Session.ShoppingCart.LastName;
			m_firstNameTextBox.Text = Session.ShoppingCart.FirstName;
			m_expiryDateTextBox.Text = $"{Session.ShoppingCart.ExpiryMonth}/{Session.ShoppingCart.ExpiryYear}";
			m_lastFourTextBox.Text = Session.ShoppingCart.LastFour;
			m_referenceNumberTextBox.Text = (Session.ShoppingCart.ReferenceNumber.HasValue ? Session.ShoppingCart.ReferenceNumber.ToString() : "N/A");
			m_lineItemListView.BeginUpdate();
			decimal num = 0m;
			foreach (KeyValuePair<string, IShoppingCartLineItemGroup> group in Session.ShoppingCart.Groups)
			{
				foreach (IShoppingCartLineItem item in group.Value.Items)
				{
					ListViewItem listViewItem = new ListViewItem(group.Key);
					listViewItem.SubItems.Add("Item");
					listViewItem.SubItems.Add(item.Barcode);
					listViewItem.SubItems.Add(item.Status.ToString());
					listViewItem.SubItems.Add(item.Quantity.ToString());
					listViewItem.SubItems.Add(item.Price.ToString("C"));
					listViewItem.SubItems.Add((item.Quantity * item.Price).ToString("C"));
					m_lineItemListView.Items.Add(listViewItem);
				}
				if (!string.IsNullOrEmpty(Session.ShoppingCart.DiscountCode) && group.Key == "rental")
				{
					ListViewItem value = new ListViewItem(new string[7]
					{
						group.Key,
						"Discount",
						Session.ShoppingCart.DiscountCode,
						string.Empty,
						"1",
						(-Session.ShoppingCart.DiscountValue).ToString("C"),
						(-Session.ShoppingCart.DiscountValue).ToString("C")
					});
					m_lineItemListView.Items.Add(value);
					ListViewItem value2 = new ListViewItem(new string[7]
					{
						group.Key,
						"Subtotal",
						string.Empty,
						string.Empty,
						"1",
						group.Value.Totals.DiscountedSubtotal.ToString("C"),
						group.Value.Totals.DiscountedSubtotal.ToString("C")
					});
					m_lineItemListView.Items.Add(value2);
				}
				else
				{
					ListViewItem value3 = new ListViewItem(new string[7]
					{
						group.Key,
						"Subtotal",
						string.Empty,
						string.Empty,
						"1",
						group.Value.Totals.SubTotal.ToString("C"),
						group.Value.Totals.SubTotal.ToString("C")
					});
					m_lineItemListView.Items.Add(value3);
				}
				ListViewItem value4 = new ListViewItem(new string[7]
				{
					group.Key,
					"Tax",
					string.Empty,
					string.Empty,
					"1",
					group.Value.Totals.TaxAmount.ToString("C"),
					group.Value.Totals.TaxAmount.ToString("C")
				});
				m_lineItemListView.Items.Add(value4);
				ListViewItem value5 = new ListViewItem(new string[7]
				{
					group.Key,
					"Grand Total",
					string.Empty,
					string.Empty,
					"1",
					group.Value.Totals.GrandTotal.ToString("C"),
					group.Value.Totals.GrandTotal.ToString("C")
				});
				m_lineItemListView.Items.Add(value5);
				num += group.Value.Totals.GrandTotal;
			}
			ListViewItem value6 = new ListViewItem(new string[7]
			{
				string.Empty,
				"Cart Total",
				string.Empty,
				string.Empty,
				"1",
				num.ToString("C"),
				num.ToString("C")
			});
			m_lineItemListView.Items.Add(value6);
			m_lineItemListView.EndUpdate();
		}
		Session.ForEachEvent(delegate(IShoppingSessionEvent se)
		{
			ListViewItem value7 = new ListViewItem(se.TimeStamp.ToString())
			{
				SubItems = 
				{
					se.Type.ToString(),
					se.Description
				}
			};
			m_eventListView.Items.Add(value7);
		});
		Session.ForEachError(delegate(IShoppingSessionError se)
		{
			ListViewItem value7 = new ListViewItem(se.TimeStamp.ToString())
			{
				SubItems = 
				{
					se.Code.ToString(),
					se.Description
				}
			};
			m_eventListView.Items.Add(value7);
		});
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
		this.m_shoppingCartPanel = new System.Windows.Forms.Panel();
		this.m_lineItemListView = new System.Windows.Forms.ListView();
		this.m_groupColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_classColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_barcodeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_statusColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_quantityColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_amountColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_extensionColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_topPanel = new System.Windows.Forms.Panel();
		this.m_emailTextBox = new System.Windows.Forms.TextBox();
		this.label15 = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.label14 = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.m_lastFourTextBox = new System.Windows.Forms.TextBox();
		this.m_expiryDateTextBox = new System.Windows.Forms.TextBox();
		this.m_firstNameTextBox = new System.Windows.Forms.TextBox();
		this.m_lastNameTextBox = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.m_sessionStatusTextBox = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.m_referenceNumberTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_typeTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_discountValueTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_discountCodeTextBox = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.m_createdOnTextBox = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.m_storeNumberTextBox = new System.Windows.Forms.TextBox();
		this.m_sessionIdTextBox = new System.Windows.Forms.TextBox();
		this.m_eventsPanel = new System.Windows.Forms.Panel();
		this.m_eventListView = new System.Windows.Forms.ListView();
		this.m_createdOnColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_typeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.label1 = new System.Windows.Forms.Label();
		this.m_splitter = new System.Windows.Forms.Splitter();
		this.m_errorsPanel = new System.Windows.Forms.Panel();
		this.m_errorListView = new System.Windows.Forms.ListView();
		this.m_errorCreatedOnColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_errorCodeColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.m_errorDescriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
		this.label16 = new System.Windows.Forms.Label();
		this.m_lowerPanel = new System.Windows.Forms.Panel();
		this.m_splitter2 = new System.Windows.Forms.Splitter();
		this.m_shoppingCartPanel.SuspendLayout();
		this.m_topPanel.SuspendLayout();
		this.m_eventsPanel.SuspendLayout();
		this.m_errorsPanel.SuspendLayout();
		this.m_lowerPanel.SuspendLayout();
		base.SuspendLayout();
		this.m_shoppingCartPanel.Controls.Add(this.m_lineItemListView);
		this.m_shoppingCartPanel.Controls.Add(this.m_topPanel);
		this.m_shoppingCartPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_shoppingCartPanel.Location = new System.Drawing.Point(0, 0);
		this.m_shoppingCartPanel.Name = "m_shoppingCartPanel";
		this.m_shoppingCartPanel.Size = new System.Drawing.Size(747, 308);
		this.m_shoppingCartPanel.TabIndex = 0;
		this.m_lineItemListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[7] { this.m_groupColumnHeader, this.m_classColumnHeader, this.m_barcodeColumnHeader, this.m_statusColumnHeader, this.m_quantityColumnHeader, this.m_amountColumnHeader, this.m_extensionColumnHeader });
		this.m_lineItemListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_lineItemListView.FullRowSelect = true;
		this.m_lineItemListView.Location = new System.Drawing.Point(0, 184);
		this.m_lineItemListView.Name = "m_lineItemListView";
		this.m_lineItemListView.Size = new System.Drawing.Size(747, 124);
		this.m_lineItemListView.TabIndex = 16;
		this.m_lineItemListView.UseCompatibleStateImageBehavior = false;
		this.m_lineItemListView.View = System.Windows.Forms.View.Details;
		this.m_groupColumnHeader.Text = "Group";
		this.m_groupColumnHeader.Width = 65;
		this.m_classColumnHeader.Text = "Class";
		this.m_classColumnHeader.Width = 100;
		this.m_barcodeColumnHeader.Text = "Barcode";
		this.m_barcodeColumnHeader.Width = 120;
		this.m_statusColumnHeader.Text = "Status";
		this.m_statusColumnHeader.Width = 95;
		this.m_quantityColumnHeader.Text = "Qty";
		this.m_quantityColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_quantityColumnHeader.Width = 35;
		this.m_amountColumnHeader.Text = "Amount";
		this.m_amountColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_amountColumnHeader.Width = 85;
		this.m_extensionColumnHeader.Text = "Extension";
		this.m_extensionColumnHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.m_extensionColumnHeader.Width = 95;
		this.m_topPanel.Controls.Add(this.m_emailTextBox);
		this.m_topPanel.Controls.Add(this.label15);
		this.m_topPanel.Controls.Add(this.groupBox1);
		this.m_topPanel.Controls.Add(this.label14);
		this.m_topPanel.Controls.Add(this.label13);
		this.m_topPanel.Controls.Add(this.m_lastFourTextBox);
		this.m_topPanel.Controls.Add(this.m_expiryDateTextBox);
		this.m_topPanel.Controls.Add(this.m_firstNameTextBox);
		this.m_topPanel.Controls.Add(this.m_lastNameTextBox);
		this.m_topPanel.Controls.Add(this.label12);
		this.m_topPanel.Controls.Add(this.label2);
		this.m_topPanel.Controls.Add(this.m_sessionStatusTextBox);
		this.m_topPanel.Controls.Add(this.label11);
		this.m_topPanel.Controls.Add(this.label10);
		this.m_topPanel.Controls.Add(this.label9);
		this.m_topPanel.Controls.Add(this.m_referenceNumberTextBox);
		this.m_topPanel.Controls.Add(this.label3);
		this.m_topPanel.Controls.Add(this.m_typeTextBox);
		this.m_topPanel.Controls.Add(this.label4);
		this.m_topPanel.Controls.Add(this.m_discountValueTextBox);
		this.m_topPanel.Controls.Add(this.label5);
		this.m_topPanel.Controls.Add(this.m_discountCodeTextBox);
		this.m_topPanel.Controls.Add(this.label6);
		this.m_topPanel.Controls.Add(this.m_createdOnTextBox);
		this.m_topPanel.Controls.Add(this.label7);
		this.m_topPanel.Controls.Add(this.label8);
		this.m_topPanel.Controls.Add(this.m_storeNumberTextBox);
		this.m_topPanel.Controls.Add(this.m_sessionIdTextBox);
		this.m_topPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_topPanel.Location = new System.Drawing.Point(0, 0);
		this.m_topPanel.Name = "m_topPanel";
		this.m_topPanel.Size = new System.Drawing.Size(747, 184);
		this.m_topPanel.TabIndex = 17;
		this.m_emailTextBox.Location = new System.Drawing.Point(565, 84);
		this.m_emailTextBox.Name = "m_emailTextBox";
		this.m_emailTextBox.ReadOnly = true;
		this.m_emailTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_emailTextBox.TabIndex = 29;
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(524, 87);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(35, 13);
		this.label15.TabIndex = 28;
		this.label15.Text = "Email:";
		this.groupBox1.Location = new System.Drawing.Point(8, 69);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(729, 2);
		this.groupBox1.TabIndex = 27;
		this.groupBox1.TabStop = false;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(282, 113);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(54, 13);
		this.label14.TabIndex = 26;
		this.label14.Text = "Last Four:";
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(21, 113);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(64, 13);
		this.label13.TabIndex = 25;
		this.label13.Text = "Expiry Date:";
		this.m_lastFourTextBox.Location = new System.Drawing.Point(342, 110);
		this.m_lastFourTextBox.Name = "m_lastFourTextBox";
		this.m_lastFourTextBox.ReadOnly = true;
		this.m_lastFourTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_lastFourTextBox.TabIndex = 24;
		this.m_expiryDateTextBox.Location = new System.Drawing.Point(91, 110);
		this.m_expiryDateTextBox.Name = "m_expiryDateTextBox";
		this.m_expiryDateTextBox.ReadOnly = true;
		this.m_expiryDateTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_expiryDateTextBox.TabIndex = 23;
		this.m_firstNameTextBox.Location = new System.Drawing.Point(342, 84);
		this.m_firstNameTextBox.Name = "m_firstNameTextBox";
		this.m_firstNameTextBox.ReadOnly = true;
		this.m_firstNameTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_firstNameTextBox.TabIndex = 22;
		this.m_lastNameTextBox.Location = new System.Drawing.Point(91, 84);
		this.m_lastNameTextBox.Name = "m_lastNameTextBox";
		this.m_lastNameTextBox.ReadOnly = true;
		this.m_lastNameTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_lastNameTextBox.TabIndex = 21;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(276, 87);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(60, 13);
		this.label12.TabIndex = 20;
		this.label12.Text = "First Name:";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(24, 87);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(61, 13);
		this.label2.TabIndex = 19;
		this.label2.Text = "Last Name:";
		this.m_sessionStatusTextBox.Location = new System.Drawing.Point(91, 37);
		this.m_sessionStatusTextBox.Name = "m_sessionStatusTextBox";
		this.m_sessionStatusTextBox.ReadOnly = true;
		this.m_sessionStatusTextBox.Size = new System.Drawing.Size(180, 20);
		this.m_sessionStatusTextBox.TabIndex = 18;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(5, 40);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(80, 13);
		this.label11.TabIndex = 17;
		this.label11.Text = "Session Status:";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(5, 168);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(58, 13);
		this.label10.TabIndex = 16;
		this.label10.Text = "Line Items:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(277, 12);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(37, 13);
		this.label9.TabIndex = 7;
		this.label9.Text = "Ref #:";
		this.m_referenceNumberTextBox.Location = new System.Drawing.Point(320, 9);
		this.m_referenceNumberTextBox.Name = "m_referenceNumberTextBox";
		this.m_referenceNumberTextBox.ReadOnly = true;
		this.m_referenceNumberTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_referenceNumberTextBox.TabIndex = 15;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(424, 12);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(64, 13);
		this.label3.TabIndex = 1;
		this.label3.Text = "Created On:";
		this.m_typeTextBox.Location = new System.Drawing.Point(320, 37);
		this.m_typeTextBox.Name = "m_typeTextBox";
		this.m_typeTextBox.ReadOnly = true;
		this.m_typeTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_typeTextBox.TabIndex = 14;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(24, 12);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(61, 13);
		this.label4.TabIndex = 2;
		this.label4.Text = "Session ID:";
		this.m_discountValueTextBox.Location = new System.Drawing.Point(342, 136);
		this.m_discountValueTextBox.Name = "m_discountValueTextBox";
		this.m_discountValueTextBox.ReadOnly = true;
		this.m_discountValueTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_discountValueTextBox.TabIndex = 13;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(443, 40);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(45, 13);
		this.label5.TabIndex = 3;
		this.label5.Text = "Store #:";
		this.m_discountCodeTextBox.Location = new System.Drawing.Point(91, 136);
		this.m_discountCodeTextBox.Name = "m_discountCodeTextBox";
		this.m_discountCodeTextBox.ReadOnly = true;
		this.m_discountCodeTextBox.Size = new System.Drawing.Size(141, 20);
		this.m_discountCodeTextBox.TabIndex = 12;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(5, 138);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(80, 13);
		this.label6.TabIndex = 4;
		this.label6.Text = "Discount Code:";
		this.m_createdOnTextBox.Location = new System.Drawing.Point(494, 9);
		this.m_createdOnTextBox.Name = "m_createdOnTextBox";
		this.m_createdOnTextBox.ReadOnly = true;
		this.m_createdOnTextBox.Size = new System.Drawing.Size(126, 20);
		this.m_createdOnTextBox.TabIndex = 11;
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(254, 138);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(82, 13);
		this.label7.TabIndex = 5;
		this.label7.Text = "Discount Value:";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(280, 39);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(34, 13);
		this.label8.TabIndex = 6;
		this.label8.Text = "Type:";
		this.m_storeNumberTextBox.Location = new System.Drawing.Point(494, 36);
		this.m_storeNumberTextBox.Name = "m_storeNumberTextBox";
		this.m_storeNumberTextBox.ReadOnly = true;
		this.m_storeNumberTextBox.Size = new System.Drawing.Size(126, 20);
		this.m_storeNumberTextBox.TabIndex = 9;
		this.m_sessionIdTextBox.Location = new System.Drawing.Point(91, 9);
		this.m_sessionIdTextBox.Name = "m_sessionIdTextBox";
		this.m_sessionIdTextBox.ReadOnly = true;
		this.m_sessionIdTextBox.Size = new System.Drawing.Size(180, 20);
		this.m_sessionIdTextBox.TabIndex = 8;
		this.m_eventsPanel.Controls.Add(this.m_eventListView);
		this.m_eventsPanel.Controls.Add(this.label1);
		this.m_eventsPanel.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_eventsPanel.Location = new System.Drawing.Point(0, 0);
		this.m_eventsPanel.Name = "m_eventsPanel";
		this.m_eventsPanel.Padding = new System.Windows.Forms.Padding(3);
		this.m_eventsPanel.Size = new System.Drawing.Size(747, 142);
		this.m_eventsPanel.TabIndex = 1;
		this.m_eventListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_createdOnColumnHeader, this.m_typeColumnHeader, this.m_descriptionColumnHeader });
		this.m_eventListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_eventListView.FullRowSelect = true;
		this.m_eventListView.Location = new System.Drawing.Point(3, 19);
		this.m_eventListView.Name = "m_eventListView";
		this.m_eventListView.Size = new System.Drawing.Size(741, 120);
		this.m_eventListView.TabIndex = 1;
		this.m_eventListView.UseCompatibleStateImageBehavior = false;
		this.m_eventListView.View = System.Windows.Forms.View.Details;
		this.m_createdOnColumnHeader.Text = "Created On";
		this.m_createdOnColumnHeader.Width = 130;
		this.m_typeColumnHeader.Text = "Type";
		this.m_typeColumnHeader.Width = 85;
		this.m_descriptionColumnHeader.Text = "Description";
		this.m_descriptionColumnHeader.Width = 500;
		this.label1.AutoSize = true;
		this.label1.Dock = System.Windows.Forms.DockStyle.Top;
		this.label1.Location = new System.Drawing.Point(3, 3);
		this.label1.Name = "label1";
		this.label1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
		this.label1.Size = new System.Drawing.Size(43, 16);
		this.label1.TabIndex = 0;
		this.label1.Text = "Events:";
		this.m_splitter.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_splitter.Location = new System.Drawing.Point(0, 308);
		this.m_splitter.Name = "m_splitter";
		this.m_splitter.Size = new System.Drawing.Size(747, 3);
		this.m_splitter.TabIndex = 2;
		this.m_splitter.TabStop = false;
		this.m_errorsPanel.Controls.Add(this.m_errorListView);
		this.m_errorsPanel.Controls.Add(this.label16);
		this.m_errorsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_errorsPanel.Location = new System.Drawing.Point(0, 145);
		this.m_errorsPanel.Name = "m_errorsPanel";
		this.m_errorsPanel.Size = new System.Drawing.Size(747, 160);
		this.m_errorsPanel.TabIndex = 3;
		this.m_errorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[3] { this.m_errorCreatedOnColumnHeader, this.m_errorCodeColumnHeader, this.m_errorDescriptionColumnHeader });
		this.m_errorListView.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_errorListView.FullRowSelect = true;
		this.m_errorListView.Location = new System.Drawing.Point(0, 16);
		this.m_errorListView.Name = "m_errorListView";
		this.m_errorListView.Size = new System.Drawing.Size(747, 144);
		this.m_errorListView.TabIndex = 2;
		this.m_errorListView.UseCompatibleStateImageBehavior = false;
		this.m_errorListView.View = System.Windows.Forms.View.Details;
		this.m_errorCreatedOnColumnHeader.Text = "Created On";
		this.m_errorCreatedOnColumnHeader.Width = 130;
		this.m_errorCodeColumnHeader.Text = "Code";
		this.m_errorCodeColumnHeader.Width = 85;
		this.m_errorDescriptionColumnHeader.Text = "Description";
		this.m_errorDescriptionColumnHeader.Width = 500;
		this.label16.AutoSize = true;
		this.label16.Dock = System.Windows.Forms.DockStyle.Top;
		this.label16.Location = new System.Drawing.Point(0, 0);
		this.label16.Name = "label16";
		this.label16.Padding = new System.Windows.Forms.Padding(0, 0, 0, 3);
		this.label16.Size = new System.Drawing.Size(37, 16);
		this.label16.TabIndex = 1;
		this.label16.Text = "Errors:";
		this.m_lowerPanel.Controls.Add(this.m_errorsPanel);
		this.m_lowerPanel.Controls.Add(this.m_splitter2);
		this.m_lowerPanel.Controls.Add(this.m_eventsPanel);
		this.m_lowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
		this.m_lowerPanel.Location = new System.Drawing.Point(0, 311);
		this.m_lowerPanel.Name = "m_lowerPanel";
		this.m_lowerPanel.Size = new System.Drawing.Size(747, 305);
		this.m_lowerPanel.TabIndex = 5;
		this.m_splitter2.Dock = System.Windows.Forms.DockStyle.Top;
		this.m_splitter2.Location = new System.Drawing.Point(0, 142);
		this.m_splitter2.Name = "m_splitter2";
		this.m_splitter2.Size = new System.Drawing.Size(747, 3);
		this.m_splitter2.TabIndex = 2;
		this.m_splitter2.TabStop = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(747, 616);
		base.Controls.Add(this.m_lowerPanel);
		base.Controls.Add(this.m_splitter);
		base.Controls.Add(this.m_shoppingCartPanel);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		this.MinimumSize = new System.Drawing.Size(320, 240);
		base.Name = "ShoppingSessionDetailForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Shopping Session Details";
		base.Load += new System.EventHandler(OnLoad);
		this.m_shoppingCartPanel.ResumeLayout(false);
		this.m_topPanel.ResumeLayout(false);
		this.m_topPanel.PerformLayout();
		this.m_eventsPanel.ResumeLayout(false);
		this.m_eventsPanel.PerformLayout();
		this.m_errorsPanel.ResumeLayout(false);
		this.m_errorsPanel.PerformLayout();
		this.m_lowerPanel.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
