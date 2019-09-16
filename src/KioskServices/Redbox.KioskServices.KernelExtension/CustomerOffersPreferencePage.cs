using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;

namespace Redbox.KioskServices.KernelExtension;

public class CustomerOffersPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_simulateSuccessCheckBox;

	private CheckBox m_includeFreeGameInCartOfferCheckbox;

	private CheckBox m_includeMovieInCart;

	private RadioButton rbInCart;

	private RadioButton rbFracFreeMovieStorable;

	private RadioButton rbFracFreeMovieRedeemable;

	private RadioButton rbFracDiscountStorable;

	private RadioButton rbFracDiscountRedeemable;

	private RadioButton rbNoOffer;

	public CustomerOffersPreferencePage()
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
		m_liveCallCheckBox.Checked = CustomerOfferProxy.Instance.LiveCall;
		m_simulateSuccessCheckBox.Checked = CustomerOfferProxy.Instance.SimulateSuccess;
		rbInCart.Checked = CustomerOfferProxy.Instance.IncludeFreeGameInCartOffer || CustomerOfferProxy.Instance.IncludeMovieDiscountInCartOffer;
		rbFracDiscountRedeemable.Checked = CustomerOfferProxy.Instance.IncludeFracRedeemableDiscountedDiscOffer;
		rbFracDiscountStorable.Checked = CustomerOfferProxy.Instance.IncludeFracStorableDiscountedDiscOffer;
		rbFracFreeMovieRedeemable.Checked = CustomerOfferProxy.Instance.IncludeFracRedeemableFreeMovieOffer;
		rbFracFreeMovieStorable.Checked = CustomerOfferProxy.Instance.IncludeFracStorableFreeMovieOffer;
		m_includeFreeGameInCartOfferCheckbox.Checked = CustomerOfferProxy.Instance.IncludeFreeGameInCartOffer;
		m_includeMovieInCart.Checked = CustomerOfferProxy.Instance.IncludeMovieDiscountInCartOffer;
		rbNoOffer.Checked = !rbFracDiscountRedeemable.Checked && !rbFracDiscountStorable.Checked && !rbFracFreeMovieRedeemable.Checked && !rbFracFreeMovieStorable.Checked && !rbInCart.Checked;
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSimulateSuccessCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.SimulateSuccess = m_simulateSuccessCheckBox.Checked;
		}
	}

	private void movieInCartOffer_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeMovieDiscountInCartOffer = m_includeMovieInCart.Checked;
		}
	}

	private void m_includeFreeGameInCartOfferCheckbox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFreeGameInCartOffer = m_includeFreeGameInCartOfferCheckbox.Checked;
		}
	}

	private void rbPresentInCart_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			if (rbInCart.Checked)
			{
				m_includeMovieInCart.Enabled = true;
				m_includeFreeGameInCartOfferCheckbox.Enabled = true;
				CustomerOfferProxy.Instance.IncludeFracRedeemableDiscountedDiscOffer = false;
				CustomerOfferProxy.Instance.IncludeFracStorableDiscountedDiscOffer = false;
				CustomerOfferProxy.Instance.IncludeFracRedeemableFreeMovieOffer = false;
				CustomerOfferProxy.Instance.IncludeFracStorableFreeMovieOffer = false;
			}
			else
			{
				m_includeMovieInCart.Enabled = false;
				m_includeMovieInCart.Checked = false;
				m_includeFreeGameInCartOfferCheckbox.Enabled = false;
				m_includeFreeGameInCartOfferCheckbox.Checked = false;
			}
		}
	}

	private void rbFracFreeMovieStorable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracStorableFreeMovieOffer = rbFracFreeMovieStorable.Checked;
		}
	}

	private void rbFracFreeMovieRedeemable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracRedeemableFreeMovieOffer = rbFracFreeMovieRedeemable.Checked;
		}
	}

	private void rbFracDiscountStorable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracStorableDiscountedDiscOffer = rbFracDiscountStorable.Checked;
		}
	}

	private void rbFracDiscountRedeemable_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			CustomerOfferProxy.Instance.IncludeFracRedeemableDiscountedDiscOffer = rbFracDiscountRedeemable.Checked;
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
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateSuccessCheckBox = new System.Windows.Forms.CheckBox();
		this.m_includeFreeGameInCartOfferCheckbox = new System.Windows.Forms.CheckBox();
		this.m_includeMovieInCart = new System.Windows.Forms.CheckBox();
		this.rbInCart = new System.Windows.Forms.RadioButton();
		this.rbFracFreeMovieStorable = new System.Windows.Forms.RadioButton();
		this.rbFracFreeMovieRedeemable = new System.Windows.Forms.RadioButton();
		this.rbFracDiscountStorable = new System.Windows.Forms.RadioButton();
		this.rbFracDiscountRedeemable = new System.Windows.Forms.RadioButton();
		this.rbNoOffer = new System.Windows.Forms.RadioButton();
		base.SuspendLayout();
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(32, 20);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 0;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_simulateSuccessCheckBox.AutoSize = true;
		this.m_simulateSuccessCheckBox.Location = new System.Drawing.Point(32, 53);
		this.m_simulateSuccessCheckBox.Name = "m_simulateSuccessCheckBox";
		this.m_simulateSuccessCheckBox.Size = new System.Drawing.Size(110, 17);
		this.m_simulateSuccessCheckBox.TabIndex = 1;
		this.m_simulateSuccessCheckBox.Text = "Simulate Success";
		this.m_simulateSuccessCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateSuccessCheckBox.CheckedChanged += new System.EventHandler(OnSimulateSuccessCheckChanged);
		this.m_includeFreeGameInCartOfferCheckbox.AutoSize = true;
		this.m_includeFreeGameInCartOfferCheckbox.Location = new System.Drawing.Point(73, 154);
		this.m_includeFreeGameInCartOfferCheckbox.Name = "m_includeFreeGameInCartOfferCheckbox";
		this.m_includeFreeGameInCartOfferCheckbox.Size = new System.Drawing.Size(176, 17);
		this.m_includeFreeGameInCartOfferCheckbox.TabIndex = 2;
		this.m_includeFreeGameInCartOfferCheckbox.Text = "Include Free Game In Cart Offer";
		this.m_includeFreeGameInCartOfferCheckbox.UseVisualStyleBackColor = true;
		this.m_includeFreeGameInCartOfferCheckbox.CheckedChanged += new System.EventHandler(m_includeFreeGameInCartOfferCheckbox_CheckedChanged);
		this.m_includeMovieInCart.AutoSize = true;
		this.m_includeMovieInCart.Location = new System.Drawing.Point(73, 131);
		this.m_includeMovieInCart.Name = "m_includeMovieInCart";
		this.m_includeMovieInCart.Size = new System.Drawing.Size(153, 17);
		this.m_includeMovieInCart.TabIndex = 3;
		this.m_includeMovieInCart.Text = "Include Movie In Cart Offer";
		this.m_includeMovieInCart.UseVisualStyleBackColor = true;
		this.m_includeMovieInCart.CheckedChanged += new System.EventHandler(movieInCartOffer_CheckedChanged);
		this.rbInCart.AutoSize = true;
		this.rbInCart.Location = new System.Drawing.Point(32, 112);
		this.rbInCart.Name = "rbInCart";
		this.rbInCart.Size = new System.Drawing.Size(126, 17);
		this.rbInCart.TabIndex = 4;
		this.rbInCart.TabStop = true;
		this.rbInCart.Text = "Present In Cart Offers";
		this.rbInCart.UseVisualStyleBackColor = true;
		this.rbInCart.CheckedChanged += new System.EventHandler(rbPresentInCart_CheckedChanged);
		this.rbFracFreeMovieStorable.AutoSize = true;
		this.rbFracFreeMovieStorable.Location = new System.Drawing.Point(32, 188);
		this.rbFracFreeMovieStorable.Name = "rbFracFreeMovieStorable";
		this.rbFracFreeMovieStorable.Size = new System.Drawing.Size(187, 17);
		this.rbFracFreeMovieStorable.TabIndex = 5;
		this.rbFracFreeMovieStorable.TabStop = true;
		this.rbFracFreeMovieStorable.Text = "Present FRAC Free Movie 1st Visit";
		this.rbFracFreeMovieStorable.UseVisualStyleBackColor = true;
		this.rbFracFreeMovieStorable.CheckedChanged += new System.EventHandler(rbFracFreeMovieStorable_CheckedChanged);
		this.rbFracFreeMovieRedeemable.AutoSize = true;
		this.rbFracFreeMovieRedeemable.Location = new System.Drawing.Point(32, 211);
		this.rbFracFreeMovieRedeemable.Name = "rbFracFreeMovieRedeemable";
		this.rbFracFreeMovieRedeemable.Size = new System.Drawing.Size(191, 17);
		this.rbFracFreeMovieRedeemable.TabIndex = 6;
		this.rbFracFreeMovieRedeemable.TabStop = true;
		this.rbFracFreeMovieRedeemable.Text = "Present FRAC Free Movie 2nd Visit";
		this.rbFracFreeMovieRedeemable.UseVisualStyleBackColor = true;
		this.rbFracFreeMovieRedeemable.CheckedChanged += new System.EventHandler(rbFracFreeMovieRedeemable_CheckedChanged);
		this.rbFracDiscountStorable.AutoSize = true;
		this.rbFracDiscountStorable.Location = new System.Drawing.Point(32, 234);
		this.rbFracDiscountStorable.Name = "rbFracDiscountStorable";
		this.rbFracDiscountStorable.Size = new System.Drawing.Size(212, 17);
		this.rbFracDiscountStorable.TabIndex = 7;
		this.rbFracDiscountStorable.TabStop = true;
		this.rbFracDiscountStorable.Text = "Present FRAC Discounted Disc 1st Visit";
		this.rbFracDiscountStorable.UseVisualStyleBackColor = true;
		this.rbFracDiscountStorable.CheckedChanged += new System.EventHandler(rbFracDiscountStorable_CheckedChanged);
		this.rbFracDiscountRedeemable.AutoSize = true;
		this.rbFracDiscountRedeemable.Location = new System.Drawing.Point(32, 257);
		this.rbFracDiscountRedeemable.Name = "rbFracDiscountRedeemable";
		this.rbFracDiscountRedeemable.Size = new System.Drawing.Size(216, 17);
		this.rbFracDiscountRedeemable.TabIndex = 8;
		this.rbFracDiscountRedeemable.TabStop = true;
		this.rbFracDiscountRedeemable.Text = "Present FRAC Discounted Disc 2nd Visit";
		this.rbFracDiscountRedeemable.UseVisualStyleBackColor = true;
		this.rbFracDiscountRedeemable.CheckedChanged += new System.EventHandler(rbFracDiscountRedeemable_CheckedChanged);
		this.rbNoOffer.AutoSize = true;
		this.rbNoOffer.Location = new System.Drawing.Point(32, 89);
		this.rbNoOffer.Name = "rbNoOffer";
		this.rbNoOffer.Size = new System.Drawing.Size(65, 17);
		this.rbNoOffer.TabIndex = 9;
		this.rbNoOffer.TabStop = true;
		this.rbNoOffer.Text = "No Offer";
		this.rbNoOffer.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.rbNoOffer);
		base.Controls.Add(this.rbFracDiscountRedeemable);
		base.Controls.Add(this.rbFracDiscountStorable);
		base.Controls.Add(this.rbFracFreeMovieRedeemable);
		base.Controls.Add(this.rbFracFreeMovieStorable);
		base.Controls.Add(this.rbInCart);
		base.Controls.Add(this.m_includeMovieInCart);
		base.Controls.Add(this.m_includeFreeGameInCartOfferCheckbox);
		base.Controls.Add(this.m_simulateSuccessCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "CustomerOffersPreferencePage";
		base.Size = new System.Drawing.Size(378, 329);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
