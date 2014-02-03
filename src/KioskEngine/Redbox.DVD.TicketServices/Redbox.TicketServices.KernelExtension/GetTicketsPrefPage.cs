using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.TicketServices.Proxy;
using Redbox.TicketServices.Proxy.ComponentModel;
using Redbox.TicketServices.Proxy.service;

namespace Redbox.TicketServices.KernelExtension;

public class GetTicketsPrefPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private TicketGroup m_currentTicketGroup;

	private List<TicketGroup> m_mockTicketGroups;

	private List<string> m_selectedTicketGroupIds;

	private IContainer components;

	private Label m_headerLabel;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private Label m_getTicketsResponseTypeLabel;

	private ComboBox m_ResponseCodeComboBox;

	private CheckedListBox m_TicketGroupsCheckedListBox;

	private Label m_TicketGroupsLabel;

	private Label m_ticketTroupIdLabel;

	private TextBox m_TicketGroupIdTextBox;

	private TextBox m_LockGuidTextBox;

	private Label m_LockGuidLabel;

	private TextBox m_LockDurationTextBox;

	private Label m_lockDurationLabel;

	private Label m_suggestionTypeLabel;

	private TextBox m_sectionTextBox;

	private Label m_sectionLabel;

	private TextBox m_rowTextBox;

	private Label m_rowLabel;

	private ComboBox m_priceCodesComboBox;

	private Label m_PriceCodesLabel;

	private TextBox m_pricePerTicketTextBox;

	private Label m_pricePerTicketLabel;

	private TextBox m_quantityTextBox;

	private Label m_quantityLabel;

	private TextBox m_priceCodeIdTextBox;

	private Label m_PriceCodeId;

	private TextBox m_feePerTicketTextBox;

	private Label m_feePerTicketLabel;

	private TextBox m_taxPerTicketTextBox;

	private Label m_taxPerTicketLabel;

	private Label m_deliveryMethodLabel;

	private CheckedListBox m_DeliveryMethodCheckedListBox;

	private TextBox m_SuggestionTypeIdTextBox;

	private TextBox m_SuggestionTypeNameTextBox;

	private Label m_SuggestionTypeNameLabel;

	private Button m_GenerateNewGuidButton;

	private TextBox m_DisplayOrderTextBox;

	private Label m_DisplayOrderLabel;

	private CheckBox m_simulateServerTimeoutCheckBox;

	private TextBox m_levelTextBox;

	private Label m_levelLabel;

	private TextBox m_zoneTextBox;

	private Label m_zoneLabel;

	public GetTicketsPrefPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_liveCallCheckBox.Checked = TicketServiceProxy.Instance.LiveCallGetTickets;
		m_simulateCommunicationsFailureCheckBox.Checked = TicketServiceProxy.Instance.CommFailureGetTickets;
		m_simulateServerBusyCheckBox.Checked = TicketServiceProxy.Instance.ServerBusyGetTickets;
		m_simulateServerTimeoutCheckBox.Checked = TicketServiceProxy.Instance.ServerTimeoutGetTickets;
		m_ResponseCodeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(TicketsResponseCode));
		foreach (string item in names)
		{
			m_ResponseCodeComboBox.Items.Add(item);
		}
		m_ResponseCodeComboBox.SelectedIndex = m_ResponseCodeComboBox.FindString(TicketServiceProxy.Instance.GetTicketsResponseCode.ToString());
		m_TicketGroupsCheckedListBox.Items.Clear();
		m_mockTicketGroups = TicketServiceProxy.Instance.MockTicketGroups;
		foreach (TicketGroup mockTicketGroup in m_mockTicketGroups)
		{
			m_TicketGroupsCheckedListBox.Items.Add(mockTicketGroup.TicketGroupId.ToString());
		}
		foreach (string selectedTicketGroupId in TicketServiceProxy.Instance.SelectedTicketGroupIds)
		{
			int num = m_TicketGroupsCheckedListBox.Items.IndexOf(selectedTicketGroupId);
			if (num >= 0)
			{
				m_TicketGroupsCheckedListBox.SetItemChecked(num, value: true);
			}
		}
		m_currentTicketGroup = m_mockTicketGroups[0];
		m_DeliveryMethodCheckedListBox.Items.Clear();
		DeliveryMethod[] mockDeliveryMethods = TicketServiceProxy.Instance.MockDeliveryMethods;
		foreach (DeliveryMethod deliveryMethod in mockDeliveryMethods)
		{
			m_DeliveryMethodCheckedListBox.Items.Add(deliveryMethod.DeliveryMethodDisplayName);
		}
		m_selectedTicketGroupIds = TicketServiceProxy.Instance.SelectedTicketGroupIds;
		m_isLoaded = true;
		if (m_TicketGroupsCheckedListBox.Items.Count > 0)
		{
			m_TicketGroupsCheckedListBox.SelectedIndex = 0;
		}
	}

	private void LoadTicketGroupInfo(TicketGroup ticketGroup)
	{
		m_TicketGroupIdTextBox.Text = ticketGroup.TicketGroupId.ToString();
		m_LockGuidTextBox.Text = ticketGroup.LockGuid.ToString();
		m_LockDurationTextBox.Text = ticketGroup.LockDuration.ToString();
		m_SuggestionTypeIdTextBox.Text = ticketGroup.SuggestionTypeId.ToString();
		m_zoneTextBox.Text = ticketGroup.Zone;
		m_levelTextBox.Text = ticketGroup.Level;
		m_sectionTextBox.Text = ticketGroup.Section;
		m_rowTextBox.Text = ticketGroup.Row;
		m_priceCodesComboBox.Items.Clear();
		m_SuggestionTypeIdTextBox.Text = ticketGroup.SuggestionTypeId.ToString();
		m_SuggestionTypeNameTextBox.Text = ticketGroup.SuggestionTypeDisplayName;
		PriceCodeResponse[] priceCodes = ticketGroup.PriceCodes;
		foreach (PriceCodeResponse priceCodeResponse in priceCodes)
		{
			m_priceCodesComboBox.Items.Add(priceCodeResponse.PriceCodeId);
		}
		m_priceCodesComboBox.SelectedIndex = 0;
		m_DeliveryMethodCheckedListBox.ClearSelected();
		int checkedItemIndex;
		foreach (int checkedIndex in m_DeliveryMethodCheckedListBox.CheckedIndices)
		{
			checkedItemIndex = checkedIndex;
			DeliveryMethod deliveryMethod = ticketGroup.DeliveryMethods.FirstOrDefault((DeliveryMethod d) => d.DeliveryMethodDisplayName == m_DeliveryMethodCheckedListBox.Items[checkedItemIndex].ToString());
			m_DeliveryMethodCheckedListBox.SetItemChecked(checkedItemIndex, deliveryMethod != null);
		}
		DeliveryMethod[] deliveryMethods = ticketGroup.DeliveryMethods;
		foreach (DeliveryMethod deliveryMethod2 in deliveryMethods)
		{
			m_DeliveryMethodCheckedListBox.SetItemChecked(m_DeliveryMethodCheckedListBox.Items.IndexOf(deliveryMethod2.DeliveryMethodDisplayName), value: true);
		}
		m_DisplayOrderTextBox.Text = m_currentTicketGroup.DisplayOrder.ToString();
	}

	private void SaveCurrentTicketGroup()
	{
		m_mockTicketGroups.RemoveAll((TicketGroup t) => t.TicketGroupId == m_currentTicketGroup.TicketGroupId);
		m_mockTicketGroups.Add(m_currentTicketGroup);
		TicketServiceProxy.Instance.MockTicketGroups = m_mockTicketGroups;
	}

	private void m_liveCallCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.LiveCallGetTickets = m_liveCallCheckBox.Checked;
		}
	}

	private void m_simulateCommunicationsFailureCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.CommFailureGetTickets = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void m_simulateServerBusyCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerBusyGetTickets = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void m_simulateServerTimeoutCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.ServerTimeoutGetTickets = m_simulateServerTimeoutCheckBox.Checked;
		}
	}

	private void m_ResponseCodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			TicketServiceProxy.Instance.GetTicketsResponseCode = Enum<TicketsResponseCode>.ParseIgnoringCase(m_ResponseCodeComboBox.SelectedItem.ToString(), TicketsResponseCode.Success);
		}
	}

	private void m_TicketGroupsCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded && m_TicketGroupsCheckedListBox.SelectedIndex != -1)
		{
			SaveCurrentTicketGroup();
			m_currentTicketGroup = m_mockTicketGroups.FirstOrDefault((TicketGroup ticketGroup) => ticketGroup.TicketGroupId == int.Parse(m_TicketGroupsCheckedListBox.SelectedItem.ToString()));
			LoadTicketGroupInfo(m_currentTicketGroup);
		}
	}

	private void m_TicketGroupsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		if (!m_isLoaded)
		{
			return;
		}
		TicketGroup ticketGroup = m_mockTicketGroups.FirstOrDefault((TicketGroup ticketGroup2) => ticketGroup2.TicketGroupId == int.Parse(m_TicketGroupsCheckedListBox.SelectedItem.ToString()));
		if (ticketGroup == null)
		{
			return;
		}
		if (e.NewValue == CheckState.Checked)
		{
			if (!m_selectedTicketGroupIds.Contains(ticketGroup.TicketGroupId.ToString()))
			{
				m_selectedTicketGroupIds.Add(ticketGroup.TicketGroupId.ToString());
			}
		}
		else
		{
			m_selectedTicketGroupIds.Remove(ticketGroup.TicketGroupId.ToString());
		}
		TicketServiceProxy.Instance.SelectedTicketGroupIds = m_selectedTicketGroupIds;
	}

	private void m_priceCodesComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			PriceCodeResponse priceCodeResponse = m_currentTicketGroup.PriceCodes.FirstOrDefault((PriceCodeResponse priceCode) => priceCode.PriceCodeId == int.Parse(m_priceCodesComboBox.SelectedItem.ToString()));
			if (priceCodeResponse != null)
			{
				m_priceCodeIdTextBox.Text = priceCodeResponse.PriceCodeId.ToString();
				m_quantityTextBox.Text = priceCodeResponse.Quantity.ToString();
				m_pricePerTicketTextBox.Text = priceCodeResponse.PricePerTicket.ToString();
				m_feePerTicketTextBox.Text = priceCodeResponse.FeePerTicket.ToString();
				m_taxPerTicketTextBox.Text = priceCodeResponse.PricePerTicketTax.ToString();
			}
		}
	}

	private void m_GenerateNewGuidButton_Click(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			Guid lockGuid = Guid.NewGuid();
			m_currentTicketGroup.LockGuid = lockGuid;
			m_LockGuidTextBox.Text = lockGuid.ToString();
			SaveCurrentTicketGroup();
		}
	}

	private void m_LockDurationTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_LockDurationTextBox.Text))
		{
			if (int.TryParse(m_LockDurationTextBox.Text, out var result))
			{
				m_currentTicketGroup.LockDuration = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_LockDurationTextBox.Text = m_currentTicketGroup.LockDuration.ToString();
			}
		}
	}

	private void m_SuggestionTypeIdTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_SuggestionTypeIdTextBox.Text))
		{
			if (int.TryParse(m_SuggestionTypeIdTextBox.Text, out var result))
			{
				m_currentTicketGroup.SuggestionTypeId = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_SuggestionTypeIdTextBox.Text = m_currentTicketGroup.SuggestionTypeId.ToString();
			}
		}
	}

	private void m_SuggestionTypeNameTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_SuggestionTypeNameTextBox.Text))
		{
			m_currentTicketGroup.SuggestionTypeDisplayName = m_SuggestionTypeNameTextBox.Text;
			SaveCurrentTicketGroup();
		}
	}

	private void m_zoneTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_currentTicketGroup.Zone = m_zoneTextBox.Text;
			SaveCurrentTicketGroup();
		}
	}

	private void m_levelTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_currentTicketGroup.Level = m_levelTextBox.Text;
			SaveCurrentTicketGroup();
		}
	}

	private void m_sectionTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_sectionTextBox.Text))
		{
			m_currentTicketGroup.Section = m_sectionTextBox.Text;
			SaveCurrentTicketGroup();
		}
	}

	private void m_rowTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_rowTextBox.Text))
		{
			m_currentTicketGroup.Row = m_rowTextBox.Text;
			SaveCurrentTicketGroup();
		}
	}

	private void m_quantityTextBox_Leave(object sender, EventArgs e)
	{
		if (!m_isLoaded || string.IsNullOrEmpty(m_quantityTextBox.Text))
		{
			return;
		}
		PriceCodeResponse priceCodeResponse = m_currentTicketGroup.PriceCodes.FirstOrDefault((PriceCodeResponse priceCode) => priceCode.PriceCodeId == int.Parse(m_priceCodesComboBox.SelectedItem.ToString()));
		if (priceCodeResponse != null)
		{
			if (int.TryParse(m_quantityTextBox.Text, out var result))
			{
				priceCodeResponse.Quantity = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_quantityTextBox.Text = priceCodeResponse.Quantity.ToString();
			}
		}
	}

	private void m_pricePerTicketTextBox_Leave(object sender, EventArgs e)
	{
		if (!m_isLoaded || string.IsNullOrEmpty(m_pricePerTicketTextBox.Text))
		{
			return;
		}
		PriceCodeResponse priceCodeResponse = m_currentTicketGroup.PriceCodes.FirstOrDefault((PriceCodeResponse priceCode) => priceCode.PriceCodeId == int.Parse(m_priceCodesComboBox.SelectedItem.ToString()));
		if (priceCodeResponse != null)
		{
			if (decimal.TryParse(m_pricePerTicketTextBox.Text, out var result))
			{
				priceCodeResponse.PricePerTicket = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_pricePerTicketTextBox.Text = priceCodeResponse.PricePerTicket.ToString();
			}
		}
	}

	private void m_feePerTicketTextBox_Leave(object sender, EventArgs e)
	{
		if (!m_isLoaded || string.IsNullOrEmpty(m_feePerTicketTextBox.Text))
		{
			return;
		}
		PriceCodeResponse priceCodeResponse = m_currentTicketGroup.PriceCodes.FirstOrDefault((PriceCodeResponse priceCode) => priceCode.PriceCodeId == int.Parse(m_priceCodesComboBox.SelectedItem.ToString()));
		if (priceCodeResponse != null)
		{
			if (decimal.TryParse(m_feePerTicketTextBox.Text, out var result))
			{
				priceCodeResponse.FeePerTicket = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_feePerTicketTextBox.Text = priceCodeResponse.FeePerTicket.ToString();
			}
		}
	}

	private void m_taxPerTicketTextBox_Leave(object sender, EventArgs e)
	{
		if (!m_isLoaded || string.IsNullOrEmpty(m_taxPerTicketTextBox.Text))
		{
			return;
		}
		PriceCodeResponse priceCodeResponse = m_currentTicketGroup.PriceCodes.FirstOrDefault((PriceCodeResponse priceCode) => priceCode.PriceCodeId == int.Parse(m_priceCodesComboBox.SelectedItem.ToString()));
		if (priceCodeResponse != null)
		{
			if (decimal.TryParse(m_taxPerTicketTextBox.Text, out var result))
			{
				priceCodeResponse.PricePerTicketTax = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_taxPerTicketTextBox.Text = priceCodeResponse.PricePerTicketTax.ToString();
			}
		}
	}

	private void m_DeliveryMethodCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		if (!m_isLoaded)
		{
			return;
		}
		List<DeliveryMethod> list = m_currentTicketGroup.DeliveryMethods.ToList();
		if (e.NewValue == CheckState.Checked)
		{
			DeliveryMethod deliveryMethod = m_currentTicketGroup.DeliveryMethods.FirstOrDefault((DeliveryMethod d) => d.DeliveryMethodDisplayName == m_DeliveryMethodCheckedListBox.Items[e.Index].ToString());
			if (deliveryMethod == null)
			{
				list.Add(TicketServiceProxy.Instance.MockDeliveryMethods.FirstOrDefault((DeliveryMethod d) => d.DeliveryMethodDisplayName == m_DeliveryMethodCheckedListBox.Items[e.Index].ToString()));
			}
		}
		else
		{
			list.RemoveAll((DeliveryMethod d) => d.DeliveryMethodDisplayName == m_DeliveryMethodCheckedListBox.Items[e.Index].ToString());
		}
		m_currentTicketGroup.DeliveryMethods = list.ToArray();
		SaveCurrentTicketGroup();
	}

	private void m_DisplayOrderTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_DisplayOrderTextBox.Text))
		{
			if (int.TryParse(m_DisplayOrderTextBox.Text, out var result))
			{
				m_currentTicketGroup.DisplayOrder = result;
				SaveCurrentTicketGroup();
			}
			else
			{
				m_DisplayOrderTextBox.Text = m_currentTicketGroup.DisplayOrder.ToString();
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
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_getTicketsResponseTypeLabel = new System.Windows.Forms.Label();
		this.m_ResponseCodeComboBox = new System.Windows.Forms.ComboBox();
		this.m_TicketGroupsCheckedListBox = new System.Windows.Forms.CheckedListBox();
		this.m_TicketGroupsLabel = new System.Windows.Forms.Label();
		this.m_ticketTroupIdLabel = new System.Windows.Forms.Label();
		this.m_TicketGroupIdTextBox = new System.Windows.Forms.TextBox();
		this.m_LockGuidTextBox = new System.Windows.Forms.TextBox();
		this.m_LockGuidLabel = new System.Windows.Forms.Label();
		this.m_LockDurationTextBox = new System.Windows.Forms.TextBox();
		this.m_lockDurationLabel = new System.Windows.Forms.Label();
		this.m_suggestionTypeLabel = new System.Windows.Forms.Label();
		this.m_sectionTextBox = new System.Windows.Forms.TextBox();
		this.m_sectionLabel = new System.Windows.Forms.Label();
		this.m_rowTextBox = new System.Windows.Forms.TextBox();
		this.m_rowLabel = new System.Windows.Forms.Label();
		this.m_priceCodesComboBox = new System.Windows.Forms.ComboBox();
		this.m_PriceCodesLabel = new System.Windows.Forms.Label();
		this.m_pricePerTicketTextBox = new System.Windows.Forms.TextBox();
		this.m_pricePerTicketLabel = new System.Windows.Forms.Label();
		this.m_quantityTextBox = new System.Windows.Forms.TextBox();
		this.m_quantityLabel = new System.Windows.Forms.Label();
		this.m_priceCodeIdTextBox = new System.Windows.Forms.TextBox();
		this.m_PriceCodeId = new System.Windows.Forms.Label();
		this.m_feePerTicketTextBox = new System.Windows.Forms.TextBox();
		this.m_feePerTicketLabel = new System.Windows.Forms.Label();
		this.m_taxPerTicketTextBox = new System.Windows.Forms.TextBox();
		this.m_taxPerTicketLabel = new System.Windows.Forms.Label();
		this.m_deliveryMethodLabel = new System.Windows.Forms.Label();
		this.m_DeliveryMethodCheckedListBox = new System.Windows.Forms.CheckedListBox();
		this.m_SuggestionTypeIdTextBox = new System.Windows.Forms.TextBox();
		this.m_SuggestionTypeNameTextBox = new System.Windows.Forms.TextBox();
		this.m_SuggestionTypeNameLabel = new System.Windows.Forms.Label();
		this.m_GenerateNewGuidButton = new System.Windows.Forms.Button();
		this.m_DisplayOrderTextBox = new System.Windows.Forms.TextBox();
		this.m_DisplayOrderLabel = new System.Windows.Forms.Label();
		this.m_simulateServerTimeoutCheckBox = new System.Windows.Forms.CheckBox();
		this.m_levelTextBox = new System.Windows.Forms.TextBox();
		this.m_levelLabel = new System.Windows.Forms.Label();
		this.m_zoneTextBox = new System.Windows.Forms.TextBox();
		this.m_zoneLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkGoldenrod;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 44;
		this.m_headerLabel.Text = "  Ticket Services: Get Tickets";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(33, 62);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 1;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(m_liveCallCheckBox_CheckedChanged);
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(33, 108);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 3;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(m_simulateServerBusyCheckBox_CheckedChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(33, 85);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 2;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(m_simulateCommunicationsFailureCheckBox_CheckedChanged);
		this.m_getTicketsResponseTypeLabel.AutoSize = true;
		this.m_getTicketsResponseTypeLabel.Location = new System.Drawing.Point(30, 162);
		this.m_getTicketsResponseTypeLabel.Name = "m_getTicketsResponseTypeLabel";
		this.m_getTicketsResponseTypeLabel.Size = new System.Drawing.Size(143, 13);
		this.m_getTicketsResponseTypeLabel.TabIndex = 4;
		this.m_getTicketsResponseTypeLabel.Text = "Get Tickets Response Type:";
		this.m_ResponseCodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_ResponseCodeComboBox.FormattingEnabled = true;
		this.m_ResponseCodeComboBox.Location = new System.Drawing.Point(179, 159);
		this.m_ResponseCodeComboBox.Name = "m_ResponseCodeComboBox";
		this.m_ResponseCodeComboBox.Size = new System.Drawing.Size(200, 21);
		this.m_ResponseCodeComboBox.TabIndex = 5;
		this.m_ResponseCodeComboBox.SelectedIndexChanged += new System.EventHandler(m_ResponseCodeComboBox_SelectedIndexChanged);
		this.m_TicketGroupsCheckedListBox.FormattingEnabled = true;
		this.m_TicketGroupsCheckedListBox.Location = new System.Drawing.Point(179, 186);
		this.m_TicketGroupsCheckedListBox.Name = "m_TicketGroupsCheckedListBox";
		this.m_TicketGroupsCheckedListBox.Size = new System.Drawing.Size(200, 34);
		this.m_TicketGroupsCheckedListBox.TabIndex = 7;
		this.m_TicketGroupsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(m_TicketGroupsCheckedListBox_ItemCheck);
		this.m_TicketGroupsCheckedListBox.SelectedIndexChanged += new System.EventHandler(m_TicketGroupsCheckedListBox_SelectedIndexChanged);
		this.m_TicketGroupsLabel.AutoSize = true;
		this.m_TicketGroupsLabel.Location = new System.Drawing.Point(30, 186);
		this.m_TicketGroupsLabel.Name = "m_TicketGroupsLabel";
		this.m_TicketGroupsLabel.Size = new System.Drawing.Size(77, 13);
		this.m_TicketGroupsLabel.TabIndex = 6;
		this.m_TicketGroupsLabel.Text = "Ticket Groups:";
		this.m_ticketTroupIdLabel.AutoSize = true;
		this.m_ticketTroupIdLabel.Location = new System.Drawing.Point(30, 256);
		this.m_ticketTroupIdLabel.Name = "m_ticketTroupIdLabel";
		this.m_ticketTroupIdLabel.Size = new System.Drawing.Size(84, 13);
		this.m_ticketTroupIdLabel.TabIndex = 11;
		this.m_ticketTroupIdLabel.Text = "Ticket Group Id:";
		this.m_TicketGroupIdTextBox.Enabled = false;
		this.m_TicketGroupIdTextBox.Location = new System.Drawing.Point(126, 253);
		this.m_TicketGroupIdTextBox.Name = "m_TicketGroupIdTextBox";
		this.m_TicketGroupIdTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_TicketGroupIdTextBox.TabIndex = 12;
		this.m_LockGuidTextBox.Enabled = false;
		this.m_LockGuidTextBox.Location = new System.Drawing.Point(126, 226);
		this.m_LockGuidTextBox.Name = "m_LockGuidTextBox";
		this.m_LockGuidTextBox.Size = new System.Drawing.Size(253, 20);
		this.m_LockGuidTextBox.TabIndex = 9;
		this.m_LockGuidLabel.AutoSize = true;
		this.m_LockGuidLabel.Location = new System.Drawing.Point(30, 229);
		this.m_LockGuidLabel.Name = "m_LockGuidLabel";
		this.m_LockGuidLabel.Size = new System.Drawing.Size(59, 13);
		this.m_LockGuidLabel.TabIndex = 8;
		this.m_LockGuidLabel.Text = "Lock Guid:";
		this.m_LockDurationTextBox.Location = new System.Drawing.Point(126, 279);
		this.m_LockDurationTextBox.Name = "m_LockDurationTextBox";
		this.m_LockDurationTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_LockDurationTextBox.TabIndex = 14;
		this.m_LockDurationTextBox.Leave += new System.EventHandler(m_LockDurationTextBox_Leave);
		this.m_lockDurationLabel.AutoSize = true;
		this.m_lockDurationLabel.Location = new System.Drawing.Point(30, 282);
		this.m_lockDurationLabel.Name = "m_lockDurationLabel";
		this.m_lockDurationLabel.Size = new System.Drawing.Size(77, 13);
		this.m_lockDurationLabel.TabIndex = 13;
		this.m_lockDurationLabel.Text = "Lock Duration:";
		this.m_suggestionTypeLabel.AutoSize = true;
		this.m_suggestionTypeLabel.Location = new System.Drawing.Point(277, 256);
		this.m_suggestionTypeLabel.Name = "m_suggestionTypeLabel";
		this.m_suggestionTypeLabel.Size = new System.Drawing.Size(102, 13);
		this.m_suggestionTypeLabel.TabIndex = 17;
		this.m_suggestionTypeLabel.Text = "Suggestion Type Id:";
		this.m_sectionTextBox.Location = new System.Drawing.Point(126, 305);
		this.m_sectionTextBox.Name = "m_sectionTextBox";
		this.m_sectionTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_sectionTextBox.TabIndex = 16;
		this.m_sectionTextBox.Leave += new System.EventHandler(m_sectionTextBox_Leave);
		this.m_sectionLabel.AutoSize = true;
		this.m_sectionLabel.Location = new System.Drawing.Point(30, 308);
		this.m_sectionLabel.Name = "m_sectionLabel";
		this.m_sectionLabel.Size = new System.Drawing.Size(46, 13);
		this.m_sectionLabel.TabIndex = 15;
		this.m_sectionLabel.Text = "Section:";
		this.m_rowTextBox.Location = new System.Drawing.Point(385, 305);
		this.m_rowTextBox.Name = "m_rowTextBox";
		this.m_rowTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_rowTextBox.TabIndex = 22;
		this.m_rowTextBox.Leave += new System.EventHandler(m_rowTextBox_Leave);
		this.m_rowLabel.AutoSize = true;
		this.m_rowLabel.Location = new System.Drawing.Point(277, 308);
		this.m_rowLabel.Name = "m_rowLabel";
		this.m_rowLabel.Size = new System.Drawing.Size(32, 13);
		this.m_rowLabel.TabIndex = 21;
		this.m_rowLabel.Text = "Row:";
		this.m_priceCodesComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_priceCodesComboBox.FormattingEnabled = true;
		this.m_priceCodesComboBox.Location = new System.Drawing.Point(126, 357);
		this.m_priceCodesComboBox.Name = "m_priceCodesComboBox";
		this.m_priceCodesComboBox.Size = new System.Drawing.Size(100, 21);
		this.m_priceCodesComboBox.TabIndex = 24;
		this.m_priceCodesComboBox.SelectedIndexChanged += new System.EventHandler(m_priceCodesComboBox_SelectedIndexChanged);
		this.m_PriceCodesLabel.AutoSize = true;
		this.m_PriceCodesLabel.Location = new System.Drawing.Point(30, 361);
		this.m_PriceCodesLabel.Name = "m_PriceCodesLabel";
		this.m_PriceCodesLabel.Size = new System.Drawing.Size(62, 13);
		this.m_PriceCodesLabel.TabIndex = 23;
		this.m_PriceCodesLabel.Text = "Price Code:";
		this.m_pricePerTicketTextBox.Location = new System.Drawing.Point(385, 357);
		this.m_pricePerTicketTextBox.Name = "m_pricePerTicketTextBox";
		this.m_pricePerTicketTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_pricePerTicketTextBox.TabIndex = 30;
		this.m_pricePerTicketTextBox.Leave += new System.EventHandler(m_pricePerTicketTextBox_Leave);
		this.m_pricePerTicketLabel.AutoSize = true;
		this.m_pricePerTicketLabel.Location = new System.Drawing.Point(277, 361);
		this.m_pricePerTicketLabel.Name = "m_pricePerTicketLabel";
		this.m_pricePerTicketLabel.Size = new System.Drawing.Size(86, 13);
		this.m_pricePerTicketLabel.TabIndex = 29;
		this.m_pricePerTicketLabel.Text = "Price Per Ticket:";
		this.m_quantityTextBox.Location = new System.Drawing.Point(126, 408);
		this.m_quantityTextBox.Name = "m_quantityTextBox";
		this.m_quantityTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_quantityTextBox.TabIndex = 28;
		this.m_quantityTextBox.Leave += new System.EventHandler(m_quantityTextBox_Leave);
		this.m_quantityLabel.AutoSize = true;
		this.m_quantityLabel.Location = new System.Drawing.Point(30, 411);
		this.m_quantityLabel.Name = "m_quantityLabel";
		this.m_quantityLabel.Size = new System.Drawing.Size(49, 13);
		this.m_quantityLabel.TabIndex = 27;
		this.m_quantityLabel.Text = "Quantity:";
		this.m_priceCodeIdTextBox.Enabled = false;
		this.m_priceCodeIdTextBox.Location = new System.Drawing.Point(126, 382);
		this.m_priceCodeIdTextBox.Name = "m_priceCodeIdTextBox";
		this.m_priceCodeIdTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_priceCodeIdTextBox.TabIndex = 26;
		this.m_PriceCodeId.AutoSize = true;
		this.m_PriceCodeId.Location = new System.Drawing.Point(30, 385);
		this.m_PriceCodeId.Name = "m_PriceCodeId";
		this.m_PriceCodeId.Size = new System.Drawing.Size(74, 13);
		this.m_PriceCodeId.TabIndex = 25;
		this.m_PriceCodeId.Text = "Price Code Id:";
		this.m_feePerTicketTextBox.Location = new System.Drawing.Point(385, 382);
		this.m_feePerTicketTextBox.Name = "m_feePerTicketTextBox";
		this.m_feePerTicketTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_feePerTicketTextBox.TabIndex = 32;
		this.m_feePerTicketTextBox.Leave += new System.EventHandler(m_feePerTicketTextBox_Leave);
		this.m_feePerTicketLabel.AutoSize = true;
		this.m_feePerTicketLabel.Location = new System.Drawing.Point(277, 385);
		this.m_feePerTicketLabel.Name = "m_feePerTicketLabel";
		this.m_feePerTicketLabel.Size = new System.Drawing.Size(80, 13);
		this.m_feePerTicketLabel.TabIndex = 31;
		this.m_feePerTicketLabel.Text = "Fee Per Ticket:";
		this.m_taxPerTicketTextBox.Location = new System.Drawing.Point(385, 408);
		this.m_taxPerTicketTextBox.Name = "m_taxPerTicketTextBox";
		this.m_taxPerTicketTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_taxPerTicketTextBox.TabIndex = 34;
		this.m_taxPerTicketTextBox.Leave += new System.EventHandler(m_taxPerTicketTextBox_Leave);
		this.m_taxPerTicketLabel.AutoSize = true;
		this.m_taxPerTicketLabel.Location = new System.Drawing.Point(277, 411);
		this.m_taxPerTicketLabel.Name = "m_taxPerTicketLabel";
		this.m_taxPerTicketLabel.Size = new System.Drawing.Size(80, 13);
		this.m_taxPerTicketLabel.TabIndex = 33;
		this.m_taxPerTicketLabel.Text = "Tax Per Ticket:";
		this.m_deliveryMethodLabel.AutoSize = true;
		this.m_deliveryMethodLabel.Location = new System.Drawing.Point(30, 443);
		this.m_deliveryMethodLabel.Name = "m_deliveryMethodLabel";
		this.m_deliveryMethodLabel.Size = new System.Drawing.Size(87, 13);
		this.m_deliveryMethodLabel.TabIndex = 35;
		this.m_deliveryMethodLabel.Text = "Delivery Method:";
		this.m_DeliveryMethodCheckedListBox.CheckOnClick = true;
		this.m_DeliveryMethodCheckedListBox.FormattingEnabled = true;
		this.m_DeliveryMethodCheckedListBox.Location = new System.Drawing.Point(179, 437);
		this.m_DeliveryMethodCheckedListBox.Name = "m_DeliveryMethodCheckedListBox";
		this.m_DeliveryMethodCheckedListBox.Size = new System.Drawing.Size(200, 34);
		this.m_DeliveryMethodCheckedListBox.TabIndex = 36;
		this.m_DeliveryMethodCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(m_DeliveryMethodCheckedListBox_ItemCheck);
		this.m_SuggestionTypeIdTextBox.Location = new System.Drawing.Point(385, 253);
		this.m_SuggestionTypeIdTextBox.Name = "m_SuggestionTypeIdTextBox";
		this.m_SuggestionTypeIdTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_SuggestionTypeIdTextBox.TabIndex = 18;
		this.m_SuggestionTypeIdTextBox.Leave += new System.EventHandler(m_SuggestionTypeIdTextBox_Leave);
		this.m_SuggestionTypeNameTextBox.Location = new System.Drawing.Point(385, 279);
		this.m_SuggestionTypeNameTextBox.Name = "m_SuggestionTypeNameTextBox";
		this.m_SuggestionTypeNameTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_SuggestionTypeNameTextBox.TabIndex = 20;
		this.m_SuggestionTypeNameTextBox.Leave += new System.EventHandler(m_SuggestionTypeNameTextBox_Leave);
		this.m_SuggestionTypeNameLabel.AutoSize = true;
		this.m_SuggestionTypeNameLabel.Location = new System.Drawing.Point(277, 282);
		this.m_SuggestionTypeNameLabel.Name = "m_SuggestionTypeNameLabel";
		this.m_SuggestionTypeNameLabel.Size = new System.Drawing.Size(90, 13);
		this.m_SuggestionTypeNameLabel.TabIndex = 19;
		this.m_SuggestionTypeNameLabel.Text = "Suggestion Type:";
		this.m_GenerateNewGuidButton.Location = new System.Drawing.Point(385, 224);
		this.m_GenerateNewGuidButton.Name = "m_GenerateNewGuidButton";
		this.m_GenerateNewGuidButton.Size = new System.Drawing.Size(100, 23);
		this.m_GenerateNewGuidButton.TabIndex = 10;
		this.m_GenerateNewGuidButton.Text = "Generate New";
		this.m_GenerateNewGuidButton.UseVisualStyleBackColor = true;
		this.m_GenerateNewGuidButton.Click += new System.EventHandler(m_GenerateNewGuidButton_Click);
		this.m_DisplayOrderTextBox.Location = new System.Drawing.Point(126, 477);
		this.m_DisplayOrderTextBox.Name = "m_DisplayOrderTextBox";
		this.m_DisplayOrderTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_DisplayOrderTextBox.TabIndex = 38;
		this.m_DisplayOrderTextBox.Leave += new System.EventHandler(m_DisplayOrderTextBox_Leave);
		this.m_DisplayOrderLabel.AutoSize = true;
		this.m_DisplayOrderLabel.Location = new System.Drawing.Point(30, 480);
		this.m_DisplayOrderLabel.Name = "m_DisplayOrderLabel";
		this.m_DisplayOrderLabel.Size = new System.Drawing.Size(73, 13);
		this.m_DisplayOrderLabel.TabIndex = 37;
		this.m_DisplayOrderLabel.Text = "Display Order:";
		this.m_simulateServerTimeoutCheckBox.AutoSize = true;
		this.m_simulateServerTimeoutCheckBox.Location = new System.Drawing.Point(33, 131);
		this.m_simulateServerTimeoutCheckBox.Name = "m_simulateServerTimeoutCheckBox";
		this.m_simulateServerTimeoutCheckBox.Size = new System.Drawing.Size(141, 17);
		this.m_simulateServerTimeoutCheckBox.TabIndex = 45;
		this.m_simulateServerTimeoutCheckBox.Text = "Simulate Server Timeout";
		this.m_simulateServerTimeoutCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerTimeoutCheckBox.CheckedChanged += new System.EventHandler(m_simulateServerTimeoutCheckBox_CheckedChanged);
		this.m_levelTextBox.Location = new System.Drawing.Point(385, 332);
		this.m_levelTextBox.Name = "m_levelTextBox";
		this.m_levelTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_levelTextBox.TabIndex = 49;
		this.m_levelTextBox.Leave += new System.EventHandler(m_levelTextBox_Leave);
		this.m_levelLabel.AutoSize = true;
		this.m_levelLabel.Location = new System.Drawing.Point(277, 335);
		this.m_levelLabel.Name = "m_levelLabel";
		this.m_levelLabel.Size = new System.Drawing.Size(36, 13);
		this.m_levelLabel.TabIndex = 48;
		this.m_levelLabel.Text = "Level:";
		this.m_zoneTextBox.Location = new System.Drawing.Point(126, 332);
		this.m_zoneTextBox.Name = "m_zoneTextBox";
		this.m_zoneTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_zoneTextBox.TabIndex = 47;
		this.m_zoneTextBox.Leave += new System.EventHandler(m_zoneTextBox_Leave);
		this.m_zoneLabel.AutoSize = true;
		this.m_zoneLabel.Location = new System.Drawing.Point(30, 335);
		this.m_zoneLabel.Name = "m_zoneLabel";
		this.m_zoneLabel.Size = new System.Drawing.Size(35, 13);
		this.m_zoneLabel.TabIndex = 46;
		this.m_zoneLabel.Text = "Zone:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_levelTextBox);
		base.Controls.Add(this.m_levelLabel);
		base.Controls.Add(this.m_zoneTextBox);
		base.Controls.Add(this.m_zoneLabel);
		base.Controls.Add(this.m_simulateServerTimeoutCheckBox);
		base.Controls.Add(this.m_DisplayOrderTextBox);
		base.Controls.Add(this.m_DisplayOrderLabel);
		base.Controls.Add(this.m_GenerateNewGuidButton);
		base.Controls.Add(this.m_SuggestionTypeNameTextBox);
		base.Controls.Add(this.m_SuggestionTypeNameLabel);
		base.Controls.Add(this.m_SuggestionTypeIdTextBox);
		base.Controls.Add(this.m_DeliveryMethodCheckedListBox);
		base.Controls.Add(this.m_deliveryMethodLabel);
		base.Controls.Add(this.m_taxPerTicketTextBox);
		base.Controls.Add(this.m_taxPerTicketLabel);
		base.Controls.Add(this.m_feePerTicketTextBox);
		base.Controls.Add(this.m_feePerTicketLabel);
		base.Controls.Add(this.m_pricePerTicketTextBox);
		base.Controls.Add(this.m_pricePerTicketLabel);
		base.Controls.Add(this.m_quantityTextBox);
		base.Controls.Add(this.m_quantityLabel);
		base.Controls.Add(this.m_priceCodeIdTextBox);
		base.Controls.Add(this.m_PriceCodeId);
		base.Controls.Add(this.m_priceCodesComboBox);
		base.Controls.Add(this.m_PriceCodesLabel);
		base.Controls.Add(this.m_rowTextBox);
		base.Controls.Add(this.m_rowLabel);
		base.Controls.Add(this.m_sectionTextBox);
		base.Controls.Add(this.m_sectionLabel);
		base.Controls.Add(this.m_suggestionTypeLabel);
		base.Controls.Add(this.m_LockDurationTextBox);
		base.Controls.Add(this.m_lockDurationLabel);
		base.Controls.Add(this.m_LockGuidTextBox);
		base.Controls.Add(this.m_LockGuidLabel);
		base.Controls.Add(this.m_TicketGroupIdTextBox);
		base.Controls.Add(this.m_ticketTroupIdLabel);
		base.Controls.Add(this.m_TicketGroupsLabel);
		base.Controls.Add(this.m_TicketGroupsCheckedListBox);
		base.Controls.Add(this.m_ResponseCodeComboBox);
		base.Controls.Add(this.m_getTicketsResponseTypeLabel);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "GetTicketsPrefPage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
