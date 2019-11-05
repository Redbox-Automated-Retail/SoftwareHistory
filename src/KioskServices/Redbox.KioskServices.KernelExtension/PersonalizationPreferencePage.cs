using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.KernelExtension;

public class PersonalizationPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private ComboBox cb_KioskLoginResponseErrors;

	private Label lbl_KioskLoginResponseErrors;

	private Label label3;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label9;

	private TextBox tb_firstName;

	private TextBox tb_phoneNum;

	private TextBox tb_postalCode;

	private TextBox tb_loyaltyPointBal;

	private TextBox tb_loyaltyTier;

	private DateTimePicker dtp_birthDate;

	private TextBox tb_loyaltyTierCounter;

	private Label label2;

	private TextBox tb_pointsExpiring;

	private Label label10;

	private DateTimePicker dtp_PointsExpirationDate;

	private Label label11;

	private CheckBox cb_promptForPerks;

	private CheckBox cbPromptForEarlyId;

	private CheckBox cbTextClubEnabled;

	private Label label1;

	private TextBox txtRecoTitlesOfferCode;

	private Label label4;

	private TextBox txtRecoTitleIds;

	private Label label12;

	private RadioButton rbGroupIds;

	private RadioButton rbTitleIds;

	private TextBox txtTagsForEachTitle;

	private Label lblTagsForEachTitle;

	private Label label13;

	private Label lblTivoQueryId;

	private TextBox txtTivoQueryId;

	public PersonalizationPreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		cb_KioskLoginResponseErrors.DataSource = Enum.GetValues(typeof(KioskLoginResponseErrors));
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			cb_KioskLoginResponseErrors.SelectedItem = instance.ResponseCode;
			tb_firstName.Text = instance.FirstName;
			tb_loyaltyPointBal.Text = instance.LoyaltyPointBalance.ToString();
			tb_loyaltyTier.Text = instance.LoyaltyTier;
			tb_phoneNum.Text = instance.PhoneNumber;
			tb_postalCode.Text = instance.PostalCode;
			tb_loyaltyTierCounter.Text = instance.LoyaltyTierCounter.ToString();
			tb_pointsExpiring.Text = instance.PointsExpiring.ToString();
			cbPromptForEarlyId.Checked = instance.PromptForEarlyId;
			cb_promptForPerks.Checked = instance.PromptForPerks;
			cbTextClubEnabled.Checked = instance.TextClubEnabled;
			txtTivoQueryId.Text = instance.TivoQueryId;
			txtRecoTitlesOfferCode.Text = instance.RecommendedTitlesOfferCode;
			txtRecoTitleIds.Text = instance.RecommendedTitleIds;
			rbGroupIds.Checked = instance.RecommendedTitlesIdType == RecommendedTitlesIdType.ProductGroupID;
			rbTitleIds.Checked = instance.RecommendedTitlesIdType == RecommendedTitlesIdType.TitleID;
			txtTagsForEachTitle.Text = (string.IsNullOrEmpty(instance.RecommendedTitleIdTags) ? txtTagsForEachTitle.Text : instance.RecommendedTitleIdTags);
		}
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance == null)
		{
			return false;
		}
		instance.BirthDate = dtp_birthDate.Value;
		instance.FirstName = tb_firstName.Text;
		int result = 0;
		instance.LoyaltyPointBalance = (int.TryParse(tb_loyaltyPointBal.Text, out result) ? result : 0);
		instance.LoyaltyTier = tb_loyaltyTier.Text;
		instance.PhoneNumber = tb_phoneNum.Text;
		instance.PostalCode = tb_postalCode.Text;
		instance.LoyaltyTierCounter = (int.TryParse(tb_loyaltyTierCounter.Text, out var result2) ? result2 : 0);
		instance.PointsExpiring = (int.TryParse(tb_pointsExpiring.Text, out var result3) ? result3 : 0);
		instance.PointsExpirationDate = dtp_PointsExpirationDate.Value;
		instance.PromptForEarlyId = cbPromptForEarlyId.Checked;
		instance.PromptForPerks = cb_promptForPerks.Checked;
		instance.TextClubEnabled = cbTextClubEnabled.Checked;
		instance.TivoQueryId = txtTivoQueryId.Text;
		instance.RecommendedTitlesOfferCode = txtRecoTitlesOfferCode.Text;
		instance.RecommendedTitleIds = txtRecoTitleIds.Text;
		instance.RecommendedTitlesIdType = (rbTitleIds.Checked ? RecommendedTitlesIdType.TitleID : RecommendedTitlesIdType.ProductGroupID);
		instance.RecommendedTitleIdTags = txtTagsForEachTitle.Text;
		return true;
	}

	private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.ResponseCode = (KioskLoginResponseErrors)cb_KioskLoginResponseErrors.SelectedItem;
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
		this.cb_KioskLoginResponseErrors = new System.Windows.Forms.ComboBox();
		this.lbl_KioskLoginResponseErrors = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.tb_firstName = new System.Windows.Forms.TextBox();
		this.tb_phoneNum = new System.Windows.Forms.TextBox();
		this.tb_postalCode = new System.Windows.Forms.TextBox();
		this.tb_loyaltyPointBal = new System.Windows.Forms.TextBox();
		this.tb_loyaltyTier = new System.Windows.Forms.TextBox();
		this.dtp_birthDate = new System.Windows.Forms.DateTimePicker();
		this.tb_loyaltyTierCounter = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.tb_pointsExpiring = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.dtp_PointsExpirationDate = new System.Windows.Forms.DateTimePicker();
		this.label11 = new System.Windows.Forms.Label();
		this.cb_promptForPerks = new System.Windows.Forms.CheckBox();
		this.cbPromptForEarlyId = new System.Windows.Forms.CheckBox();
		this.cbTextClubEnabled = new System.Windows.Forms.CheckBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtRecoTitlesOfferCode = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtRecoTitleIds = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.rbGroupIds = new System.Windows.Forms.RadioButton();
		this.rbTitleIds = new System.Windows.Forms.RadioButton();
		this.txtTagsForEachTitle = new System.Windows.Forms.TextBox();
		this.lblTagsForEachTitle = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		this.lblTivoQueryId = new System.Windows.Forms.Label();
		this.txtTivoQueryId = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 42;
		this.m_headerLabel.Text = "Personalization Services: Login";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cb_KioskLoginResponseErrors.FormattingEnabled = true;
		this.cb_KioskLoginResponseErrors.Location = new System.Drawing.Point(188, 52);
		this.cb_KioskLoginResponseErrors.Name = "cb_KioskLoginResponseErrors";
		this.cb_KioskLoginResponseErrors.Size = new System.Drawing.Size(236, 21);
		this.cb_KioskLoginResponseErrors.TabIndex = 43;
		this.cb_KioskLoginResponseErrors.SelectionChangeCommitted += new System.EventHandler(comboBox1_SelectedIndexChanged);
		this.lbl_KioskLoginResponseErrors.AutoSize = true;
		this.lbl_KioskLoginResponseErrors.Location = new System.Drawing.Point(15, 52);
		this.lbl_KioskLoginResponseErrors.Name = "lbl_KioskLoginResponseErrors";
		this.lbl_KioskLoginResponseErrors.Size = new System.Drawing.Size(146, 13);
		this.lbl_KioskLoginResponseErrors.TabIndex = 45;
		this.lbl_KioskLoginResponseErrors.Text = "Kiosk Login Response Errors:";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(15, 79);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(60, 13);
		this.label3.TabIndex = 48;
		this.label3.Text = "First Name:";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(15, 105);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(81, 13);
		this.label5.TabIndex = 50;
		this.label5.Text = "Phone Number:";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(15, 131);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(67, 13);
		this.label6.TabIndex = 51;
		this.label6.Text = "Postal Code:";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(15, 157);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(57, 13);
		this.label7.TabIndex = 52;
		this.label7.Text = "Birth Date:";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(15, 183);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(112, 13);
		this.label8.TabIndex = 53;
		this.label8.Text = "Loyalty Point Balance:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(15, 209);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(64, 13);
		this.label9.TabIndex = 54;
		this.label9.Text = "Loyalty Tier:";
		this.tb_firstName.Location = new System.Drawing.Point(188, 79);
		this.tb_firstName.Name = "tb_firstName";
		this.tb_firstName.Size = new System.Drawing.Size(236, 20);
		this.tb_firstName.TabIndex = 57;
		this.tb_phoneNum.Location = new System.Drawing.Point(188, 105);
		this.tb_phoneNum.Name = "tb_phoneNum";
		this.tb_phoneNum.Size = new System.Drawing.Size(236, 20);
		this.tb_phoneNum.TabIndex = 59;
		this.tb_postalCode.Location = new System.Drawing.Point(188, 131);
		this.tb_postalCode.Name = "tb_postalCode";
		this.tb_postalCode.Size = new System.Drawing.Size(236, 20);
		this.tb_postalCode.TabIndex = 60;
		this.tb_loyaltyPointBal.Location = new System.Drawing.Point(188, 183);
		this.tb_loyaltyPointBal.Name = "tb_loyaltyPointBal";
		this.tb_loyaltyPointBal.Size = new System.Drawing.Size(236, 20);
		this.tb_loyaltyPointBal.TabIndex = 62;
		this.tb_loyaltyTier.Location = new System.Drawing.Point(188, 209);
		this.tb_loyaltyTier.Name = "tb_loyaltyTier";
		this.tb_loyaltyTier.Size = new System.Drawing.Size(236, 20);
		this.tb_loyaltyTier.TabIndex = 63;
		this.dtp_birthDate.Location = new System.Drawing.Point(188, 157);
		this.dtp_birthDate.Name = "dtp_birthDate";
		this.dtp_birthDate.Size = new System.Drawing.Size(236, 20);
		this.dtp_birthDate.TabIndex = 64;
		this.tb_loyaltyTierCounter.Location = new System.Drawing.Point(188, 235);
		this.tb_loyaltyTierCounter.Name = "tb_loyaltyTierCounter";
		this.tb_loyaltyTierCounter.Size = new System.Drawing.Size(236, 20);
		this.tb_loyaltyTierCounter.TabIndex = 66;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 235);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(104, 13);
		this.label2.TabIndex = 67;
		this.label2.Text = "Loyalty Tier Counter:";
		this.tb_pointsExpiring.Location = new System.Drawing.Point(188, 261);
		this.tb_pointsExpiring.Name = "tb_pointsExpiring";
		this.tb_pointsExpiring.Size = new System.Drawing.Size(236, 20);
		this.tb_pointsExpiring.TabIndex = 68;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(15, 261);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(79, 13);
		this.label10.TabIndex = 69;
		this.label10.Text = "Points Expiring:";
		this.dtp_PointsExpirationDate.Location = new System.Drawing.Point(188, 287);
		this.dtp_PointsExpirationDate.Name = "dtp_PointsExpirationDate";
		this.dtp_PointsExpirationDate.Size = new System.Drawing.Size(236, 20);
		this.dtp_PointsExpirationDate.TabIndex = 70;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(15, 287);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(114, 13);
		this.label11.TabIndex = 71;
		this.label11.Text = "Points Expiration Date:";
		this.cb_promptForPerks.AutoSize = true;
		this.cb_promptForPerks.Location = new System.Drawing.Point(123, 322);
		this.cb_promptForPerks.Name = "cb_promptForPerks";
		this.cb_promptForPerks.Size = new System.Drawing.Size(107, 17);
		this.cb_promptForPerks.TabIndex = 72;
		this.cb_promptForPerks.Text = "Prompt For Perks";
		this.cb_promptForPerks.UseVisualStyleBackColor = true;
		this.cbPromptForEarlyId.AutoSize = true;
		this.cbPromptForEarlyId.Location = new System.Drawing.Point(248, 322);
		this.cbPromptForEarlyId.Name = "cbPromptForEarlyId";
		this.cbPromptForEarlyId.Size = new System.Drawing.Size(117, 17);
		this.cbPromptForEarlyId.TabIndex = 73;
		this.cbPromptForEarlyId.Text = "Prompt For Early ID";
		this.cbPromptForEarlyId.UseVisualStyleBackColor = true;
		this.cbTextClubEnabled.AutoSize = true;
		this.cbTextClubEnabled.Location = new System.Drawing.Point(385, 322);
		this.cbTextClubEnabled.Name = "cbTextClubEnabled";
		this.cbTextClubEnabled.Size = new System.Drawing.Size(113, 17);
		this.cbTextClubEnabled.TabIndex = 74;
		this.cbTextClubEnabled.Text = "Text Club Enabled";
		this.cbTextClubEnabled.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(15, 383);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(164, 13);
		this.label1.TabIndex = 76;
		this.label1.Text = "Recommended Titles Offer Code:";
		this.txtRecoTitlesOfferCode.Location = new System.Drawing.Point(188, 383);
		this.txtRecoTitlesOfferCode.Name = "txtRecoTitlesOfferCode";
		this.txtRecoTitlesOfferCode.Size = new System.Drawing.Size(236, 20);
		this.txtRecoTitlesOfferCode.TabIndex = 75;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(15, 409);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(156, 13);
		this.label4.TabIndex = 78;
		this.label4.Text = "Recommended Title/Group Ids:";
		this.txtRecoTitleIds.Location = new System.Drawing.Point(188, 409);
		this.txtRecoTitleIds.Name = "txtRecoTitleIds";
		this.txtRecoTitleIds.Size = new System.Drawing.Size(236, 20);
		this.txtRecoTitleIds.TabIndex = 77;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(429, 409);
		this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(107, 13);
		this.label12.TabIndex = 79;
		this.label12.Text = "(Comma delimited list)";
		this.rbGroupIds.AutoSize = true;
		this.rbGroupIds.Location = new System.Drawing.Point(188, 479);
		this.rbGroupIds.Margin = new System.Windows.Forms.Padding(2);
		this.rbGroupIds.Name = "rbGroupIds";
		this.rbGroupIds.Size = new System.Drawing.Size(111, 17);
		this.rbGroupIds.TabIndex = 82;
		this.rbGroupIds.TabStop = true;
		this.rbGroupIds.Text = "Product Group Ids";
		this.rbGroupIds.UseVisualStyleBackColor = true;
		this.rbTitleIds.AutoSize = true;
		this.rbTitleIds.Location = new System.Drawing.Point(303, 479);
		this.rbTitleIds.Margin = new System.Windows.Forms.Padding(2);
		this.rbTitleIds.Name = "rbTitleIds";
		this.rbTitleIds.Size = new System.Drawing.Size(62, 17);
		this.rbTitleIds.TabIndex = 83;
		this.rbTitleIds.TabStop = true;
		this.rbTitleIds.Text = "Title Ids";
		this.rbTitleIds.UseVisualStyleBackColor = true;
		this.txtTagsForEachTitle.Location = new System.Drawing.Point(188, 435);
		this.txtTagsForEachTitle.Name = "txtTagsForEachTitle";
		this.txtTagsForEachTitle.Size = new System.Drawing.Size(236, 20);
		this.txtTagsForEachTitle.TabIndex = 84;
		this.txtTagsForEachTitle.Text = "[wishlist] [recommended,wishlist]";
		this.lblTagsForEachTitle.AutoSize = true;
		this.lblTagsForEachTitle.Location = new System.Drawing.Point(15, 435);
		this.lblTagsForEachTitle.Name = "lblTagsForEachTitle";
		this.lblTagsForEachTitle.Size = new System.Drawing.Size(146, 13);
		this.lblTagsForEachTitle.TabIndex = 85;
		this.lblTagsForEachTitle.Text = "Tags For EachTitle (Example:";
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(429, 435);
		this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(121, 13);
		this.label13.TabIndex = 86;
		this.label13.Text = "(Space splitted array list)";
		this.lblTivoQueryId.AutoSize = true;
		this.lblTivoQueryId.Location = new System.Drawing.Point(15, 357);
		this.lblTivoQueryId.Name = "lblTivoQueryId";
		this.lblTivoQueryId.Size = new System.Drawing.Size(71, 13);
		this.lblTivoQueryId.TabIndex = 87;
		this.lblTivoQueryId.Text = "Tivo QueryId:";
		this.txtTivoQueryId.Location = new System.Drawing.Point(188, 357);
		this.txtTivoQueryId.Name = "txtTivoQueryId";
		this.txtTivoQueryId.Size = new System.Drawing.Size(236, 20);
		this.txtTivoQueryId.TabIndex = 88;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.txtTivoQueryId);
		base.Controls.Add(this.lblTivoQueryId);
		base.Controls.Add(this.label13);
		base.Controls.Add(this.lblTagsForEachTitle);
		base.Controls.Add(this.txtTagsForEachTitle);
		base.Controls.Add(this.rbTitleIds);
		base.Controls.Add(this.rbGroupIds);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.txtRecoTitleIds);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtRecoTitlesOfferCode);
		base.Controls.Add(this.cbTextClubEnabled);
		base.Controls.Add(this.cbPromptForEarlyId);
		base.Controls.Add(this.cb_promptForPerks);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.dtp_PointsExpirationDate);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.tb_pointsExpiring);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.tb_loyaltyTierCounter);
		base.Controls.Add(this.dtp_birthDate);
		base.Controls.Add(this.tb_loyaltyTier);
		base.Controls.Add(this.tb_loyaltyPointBal);
		base.Controls.Add(this.tb_postalCode);
		base.Controls.Add(this.tb_phoneNum);
		base.Controls.Add(this.tb_firstName);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.lbl_KioskLoginResponseErrors);
		base.Controls.Add(this.cb_KioskLoginResponseErrors);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "PersonalizationPreferencePage";
		base.Size = new System.Drawing.Size(542, 533);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
