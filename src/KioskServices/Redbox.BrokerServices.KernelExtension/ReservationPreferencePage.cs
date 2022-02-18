using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Redbox.BrokerServices.Proxy;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment.CardDataReader;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Profile;
using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.BrokerServices.KernelExtension;

public class ReservationPreferencePage : UserControl, IPreferencePageHost
{
	private class ReservationTitle : ReservationTitle3
	{
		public long LongTitleId
		{
			get
			{
				return base.TitleID;
			}
			set
			{
				base.TitleID = (int)value;
			}
		}

		public ReservationTitle()
		{
			base.ReservationType = ReservationType._Unspecified;
		}
	}

	private class DropdownTitle
	{
		private ITitleProduct _titleProduct;

		public ITitleProduct TitleProduct => _titleProduct;

		public string DropdownText => $"{_titleProduct.LongName} [{_titleProduct.BarCodeProducts.Count} Barcodes] ";

		public long ProductId => _titleProduct.ProductId;

		public DropdownTitle(ITitleProduct newProduct)
		{
			_titleProduct = newProduct;
		}
	}

	private IDataService _dataService;

	private List<DropdownTitle> _allProducts;

	private Store _store;

	private BindingList<ReservationTitle> _reservationTitles;

	private BindingSource _titleBinding;

	private DropdownTitle _notSelectedTitle;

	private decimal _totalPrice;

	private decimal _discSubtotal;

	private decimal _onlinePromoValue;

	private decimal _taxRate;

	private decimal _taxAmt;

	private decimal _svcFeeTaxAmt;

	private decimal _grandTotal;

	private long _transactionId = 123456789L;

	private IContainer components;

	private ErrorProvider m_errorProvider;

	private Label m_headerLabel;

	private Label m_reservationCount;

	private Label label1;

	private DataGridView dgvTitles;

	private Button btnRemove;

	private Button btnAdd;

	private Panel panel1;

	private NumericUpDown nudSvcFeeActual;

	private Label label14;

	private NumericUpDown nudSvcFeeDefault;

	private Label label12;

	private Label lblServiceFee;

	private Label label13;

	private NumericUpDown nudRemainingCredits;

	private NumericUpDown nudOnlineDiscount;

	private NumericUpDown nudTaxRate;

	private Label label10;

	private Label lblGrandTotal;

	private Label label11;

	private Label lblTax;

	private Label lblDiscountedSubtotal;

	private Button button1;

	private Label label9;

	private Label label8;

	private TextBox txtPromoCode;

	private Label label7;

	private Label label6;

	private CheckBox cbMultiNight;

	private Label label5;

	private Label label4;

	private Label lblSubTotal;

	private Label label3;

	private TextBox txtCustNum;

	private Label label2;

	private CheckBox cbLoyaltyRedemption;

	private CheckBox cbAppliedMarketing;

	private CheckBox cbAppliedPromo;

	private CheckBox cbAppliedCredit;

	private Label lblSvcFeeTax;

	private Label label16;

	private CheckBox cbAuthorizeAtPickup;

	private DataGridViewTextBoxColumn TitleID;

	private DataGridViewComboBoxColumn Title;

	private DataGridViewComboBoxColumn Type;

	private DataGridViewTextBoxColumn Price;

	private DataGridViewTextBoxColumn Discount;

	private DataGridViewTextBoxColumn DiscountedPrice;

	private DataGridViewComboBoxColumn DiscountType;

	private DataGridViewTextBoxColumn LoyaltyPoints;

	private DataGridViewTextBoxColumn Credits;

	private CheckBox cbNewHash;

	private TextBox tb_last4;

	private TextBox tb_first6;

	private Label label17;

	private Label label15;

	private IDataService DataService
	{
		get
		{
			if (_dataService == null)
			{
				_dataService = ServiceLocator.Instance.GetService<IDataService>();
			}
			return _dataService;
		}
	}

	private BindingList<ReservationTitle> ReservationTitles
	{
		get
		{
			if (_reservationTitles == null)
			{
				_reservationTitles = new BindingList<ReservationTitle>();
			}
			return _reservationTitles;
		}
	}

	private Store Store
	{
		get
		{
			if (_store == null)
			{
				_store = DataService.GetStore();
			}
			return _store;
		}
	}

	public ReservationPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		_dataService = null;
		_reservationTitles = null;
		_titleBinding = null;
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		UpdateReservationCount();
		_allProducts = new List<DropdownTitle>();
		List<IKioskProduct> products = new List<IKioskProduct>();
		DataService.GetProducts(BrowseViewFilter.All, TitleFamily.All, TitleType.All, BrowseSort.Alpha, null, null, null, isADAView: true, null, null, out products);
		products.ForEach(delegate(IKioskProduct p)
		{
			_allProducts.Add(new DropdownTitle((ITitleProduct)p));
		});
		_notSelectedTitle = new DropdownTitle(new TitleProduct(new Title
		{
			ProductId = 0L,
			LongName = "Not Selected"
		}, null, TitleType.DVD, null, null, null));
		_titleBinding = new BindingSource();
		_titleBinding.DataSource = ReservationTitles;
		_titleBinding.AllowNew = true;
		dgvTitles.AutoGenerateColumns = false;
		dgvTitles.DataSource = _titleBinding;
		DataGridViewComboBoxColumn obj = (DataGridViewComboBoxColumn)dgvTitles.Columns["Title"];
		obj.DataPropertyName = "LongTitleId";
		obj.DataSource = _allProducts;
		obj.ValueMember = "ProductId";
		obj.DisplayMember = "DropdownText";
		obj.DefaultCellStyle.NullValue = "Not Selected";
		obj.DropDownWidth = 400;
		DataGridViewComboBoxColumn obj2 = (DataGridViewComboBoxColumn)dgvTitles.Columns["Type"];
		obj2.DataPropertyName = "ReservationType";
		List<ReservationType> dataSource = new List<ReservationType>
		{
			ReservationType.Rental,
			ReservationType.Purchase
		};
		obj2.DataSource = dataSource;
		DataGridViewComboBoxColumn obj3 = (DataGridViewComboBoxColumn)dgvTitles.Columns["DiscountType"];
		obj3.DataPropertyName = "DiscountType";
		List<DiscountType> dataSource2 = new List<DiscountType>
		{
			(DiscountType)0,
			(DiscountType)1,
			(DiscountType)2,
			(DiscountType)3
		};
		obj3.DataSource = dataSource2;
		AddTitle();
	}

	private void UpdateReservationCount()
	{
		List<Redbox.Rental.Model.Reservation.IReservation> list = DataService?.GetAllReservations();
		new Dictionary<string, int>();
		List<string> list2 = new List<string>();
		if (list != null)
		{
			foreach (Redbox.Rental.Model.Reservation.IReservation item in list)
			{
				list2.Add(item.ReferenceNumber);
			}
		}
		m_reservationCount.Text = list2.Join(", ");
	}

	private void OnCreateReservationClick(object sender, EventArgs e)
	{
		_ = ReservationServicesProxy.Instance.LiveCall;
	}

	private void dgvTitles_DataError(object sender, DataGridViewDataErrorEventArgs e)
	{
		try
		{
			if (e.Exception.Message == "DataGridViewComboBoxCell value is not valid.")
			{
				_ = dgvTitles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
				MessageBox.Show($"Invalid Combo box Value in row {e.RowIndex} column {e.ColumnIndex} value {dgvTitles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value}");
			}
			throw e.Exception;
		}
		catch (Exception ex)
		{
			MessageBox.Show(string.Format("Failed to bind ComboBox. \n\n" + ex.Message));
		}
	}

	private void dgvTitles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.ColumnIndex < 0 || e.ColumnIndex >= dgvTitles.ColumnCount || e.RowIndex >= dgvTitles.RowCount)
		{
			return;
		}
		string name = dgvTitles.Columns[e.ColumnIndex].Name;
		if (dgvTitles.Rows[e.RowIndex].DataBoundItem is ReservationTitle reservationTitle)
		{
			if (name == "Title" || name == "Type")
			{
				FillTitlePricing(reservationTitle);
			}
			reservationTitle.DiscountedPrice = reservationTitle.Price + reservationTitle.Discount;
			dgvTitles.RefreshEdit();
			UpdateTotals();
		}
	}

	private void FillTitlePricing(ReservationTitle title)
	{
		DropdownTitle dropdownTitle = _allProducts.FirstOrDefault((DropdownTitle p) => p.ProductId == title.LongTitleId);
		if (title.LongTitleId == 0L || title.ReservationType == ReservationType._Unspecified)
		{
			return;
		}
		IPricingRecord pricingRecord = ServiceLocator.Instance.GetService<IPricingService>()?.GetOneDayProductPricingRecord(dropdownTitle.ProductId);
		if (pricingRecord != null)
		{
			if (title.ReservationType == ReservationType.Purchase)
			{
				title.Price = pricingRecord.Purchase;
			}
			else
			{
				title.Price = pricingRecord.InitialNight;
			}
		}
	}

	private void UpdateTotals()
	{
		decimal num = 0m;
		decimal num2 = 0m;
		foreach (DataGridViewRow item in (IEnumerable)dgvTitles.Rows)
		{
			if (item.DataBoundItem != null)
			{
				ReservationTitle reservationTitle = item.DataBoundItem as ReservationTitle;
				if (reservationTitle.LongTitleId != 0L && reservationTitle.ReservationType != ReservationType._Unspecified)
				{
					num += reservationTitle.Price;
					num2 += reservationTitle.Discount;
				}
			}
		}
		nudOnlineDiscount.Value = num2;
		_onlinePromoValue = num2;
		lblSubTotal.Text = num.ToString("###,###.00");
		_discSubtotal = num + num2;
		lblDiscountedSubtotal.Text = _discSubtotal.ToString("###,###.00");
		_taxRate = nudTaxRate.Value;
		_taxAmt = _taxRate / 100m * _discSubtotal;
		lblTax.Text = _taxAmt.ToString("###,###.00");
		lblServiceFee.Text = nudSvcFeeActual.Value.ToString("###,###.00");
		_svcFeeTaxAmt = nudSvcFeeActual.Value * Store.ServiceFeeTaxRate / 100m;
		lblSvcFeeTax.Text = _svcFeeTaxAmt.ToString("###,###.00");
		_grandTotal = _discSubtotal + _taxAmt + nudSvcFeeActual.Value + _svcFeeTaxAmt;
		lblGrandTotal.Text = _grandTotal.ToString("###,###.00");
	}

	private void nudTaxRate_ValueChanged(object sender, EventArgs e)
	{
		UpdateTotals();
	}

	private void nudSvcFeeActual_ValueChanged(object sender, EventArgs e)
	{
		UpdateTotals();
	}

	private void nudSvcFeeTaxRate_ValueChanged(object sender, EventArgs e)
	{
		UpdateTotals();
	}

	private void nudOnlineDiscount_ValueChanged(object sender, EventArgs e)
	{
		UpdateTotals();
	}

	public void SimulateCreateReservation()
	{
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Expected O, but got Unknown
		Dictionary<string, List<int>> dictionary = new Dictionary<string, List<int>>();
		List<ReservationTitle3> list = new List<ReservationTitle3>();
		bool flag = false;
		if (ReservationTitles.Count == 0)
		{
			MessageBox.Show("At least one title must be selected to be reserved.");
			return;
		}
		foreach (ReservationTitle title in ReservationTitles)
		{
			list.Add(title);
			string key = title.ReservationType.ToString().ToLower();
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, new List<int>());
			}
			dictionary[key].Add(title.TitleID);
			flag = flag || _allProducts.FirstOrDefault((DropdownTitle p) => p.ProductId == title.TitleID)?.TitleProduct is MDVProduct;
			DataService.GetProduct(title.LongTitleId, excludeTitleRollup: true, out var kioskProduct);
			if (!(kioskProduct is MDVProduct mDVProduct))
			{
				continue;
			}
			foreach (ITitleProduct eachSecondardProduct in mDVProduct.TitleProducts)
			{
				if (!list.Any((ReservationTitle3 x) => x.TitleID == eachSecondardProduct.ProductId))
				{
					ReservationTitle3 reservationTitle = new ReservationTitle3
					{
						TitleID = (int)eachSecondardProduct.ProductId,
						ReservationType = title.ReservationType
					};
					list.Add(reservationTitle);
					dictionary[key].Add(reservationTitle.TitleID);
				}
			}
		}
		string sourcString = ((!string.IsNullOrWhiteSpace(tb_first6.Text)) ? tb_first6.Text : "1111");
		string last = ((!string.IsNullOrWhiteSpace(tb_last4.Text)) ? tb_last4.Text : "411111");
		ReservationCardHash val = new ReservationCardHash
		{
			Last4 = last,
			CardBinHash = CardHelper.GenerateHash(sourcString)
		};
		string text = "tE+xZHoxAjrOrueI34+SXg4Q+xwchyIQu8vdRMSTz3k=";
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<Redbox.Rental.Model.Reservation.IReservation> list2 = service?.GetReservations(text);
		if (list2 == null)
		{
			list2 = new List<Redbox.Rental.Model.Reservation.IReservation>();
		}
		list2.AddRange(service?.GetReservations(val.CardBinHash + CardHelper.GenerateHash(val.Last4)));
		long transactionId = _transactionId;
		if (list2.Count > 0)
		{
			transactionId = list2.Max((Redbox.Rental.Model.Reservation.IReservation r) => long.Parse(r.ReferenceNumber));
		}
		_transactionId = transactionId;
		IReservationResult reservationResult = ReservationServicesProxy.Instance.SimulateCreateReservation(text, "joecool@redbox.com", ++_transactionId, DateTime.Now, dictionary, cbAppliedCredit.Checked, cbAppliedPromo.Checked, cbAppliedMarketing.Checked, txtCustNum.Text, _discSubtotal, _grandTotal, new List<int>(), _onlinePromoValue, cbMultiNight.Checked, (int)nudRemainingCredits.Value, txtPromoCode.Text, _totalPrice, _taxAmt, list, isGc: false, new List<string>(), new List<string>(), _taxRate, "99999", flag, cbLoyaltyRedemption.Checked, nudSvcFeeDefault.Value, nudSvcFeeActual.Value, cbAuthorizeAtPickup.Checked, cbNewHash.Checked ? val : null, null);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IReservedItem item in reservationResult.Items)
		{
			stringBuilder.AppendLine($"Product ID: {item.ProductId} Barcode: {item.Barcode}");
		}
		if (!reservationResult.Success)
		{
			if (!string.IsNullOrEmpty(reservationResult.ErrorMessage))
			{
				MessageBox.Show(reservationResult.ErrorMessage + "\n" + stringBuilder.ToString());
			}
			else
			{
				MessageBox.Show("Reservation creation not successful.\n" + stringBuilder.ToString());
			}
		}
		else
		{
			MessageBox.Show($"Reservation created for {reservationResult.Items.Count} items.\n" + stringBuilder.ToString());
		}
		UpdateReservationCount();
	}

	private void dgvTitles_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
	{
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		AddTitle();
	}

	private void AddTitle()
	{
		ReservationTitle reservationTitle = new ReservationTitle();
		_titleBinding.Add(reservationTitle);
		reservationTitle.TitleID = (int)_allProducts.First((DropdownTitle t) => !ReservationTitles.Any((ReservationTitle r) => r.TitleID == t.ProductId)).ProductId;
		((ReservationTitle3)reservationTitle).ReservationType = ReservationType.Rental;
		FillTitlePricing(reservationTitle);
		dgvTitles.RefreshEdit();
		UpdateTotals();
	}

	private void btnRemove_Click(object sender, EventArgs e)
	{
		foreach (DataGridViewRow selectedRow in dgvTitles.SelectedRows)
		{
			_reservationTitles.Remove((ReservationTitle)selectedRow.DataBoundItem);
		}
		dgvTitles.RefreshEdit();
		UpdateTotals();
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
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.m_reservationCount = new System.Windows.Forms.Label();
		this.dgvTitles = new System.Windows.Forms.DataGridView();
		this.TitleID = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Title = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.Type = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.Price = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Discount = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.DiscountedPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.DiscountType = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.LoyaltyPoints = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Credits = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.btnAdd = new System.Windows.Forms.Button();
		this.btnRemove = new System.Windows.Forms.Button();
		this.cbAppliedCredit = new System.Windows.Forms.CheckBox();
		this.cbAppliedPromo = new System.Windows.Forms.CheckBox();
		this.cbAppliedMarketing = new System.Windows.Forms.CheckBox();
		this.cbLoyaltyRedemption = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.txtCustNum = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.lblSubTotal = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.cbMultiNight = new System.Windows.Forms.CheckBox();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.txtPromoCode = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.lblDiscountedSubtotal = new System.Windows.Forms.Label();
		this.lblTax = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.lblGrandTotal = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.nudTaxRate = new System.Windows.Forms.NumericUpDown();
		this.nudOnlineDiscount = new System.Windows.Forms.NumericUpDown();
		this.nudRemainingCredits = new System.Windows.Forms.NumericUpDown();
		this.label12 = new System.Windows.Forms.Label();
		this.nudSvcFeeDefault = new System.Windows.Forms.NumericUpDown();
		this.label14 = new System.Windows.Forms.Label();
		this.nudSvcFeeActual = new System.Windows.Forms.NumericUpDown();
		this.panel1 = new System.Windows.Forms.Panel();
		this.tb_last4 = new System.Windows.Forms.TextBox();
		this.tb_first6 = new System.Windows.Forms.TextBox();
		this.label17 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.cbNewHash = new System.Windows.Forms.CheckBox();
		this.cbAuthorizeAtPickup = new System.Windows.Forms.CheckBox();
		this.lblSvcFeeTax = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.lblServiceFee = new System.Windows.Forms.Label();
		this.label13 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.dgvTitles).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudTaxRate).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudOnlineDiscount).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudRemainingCredits).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudSvcFeeDefault).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.nudSvcFeeActual).BeginInit();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_errorProvider.ContainerControl = this;
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkMagenta;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(723, 47);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Broker Service: Simulate Reservations (Deprecated)";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(4, 64);
		this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(129, 17);
		this.label1.TabIndex = 9;
		this.label1.Text = "Reservation Count:";
		this.m_reservationCount.AutoSize = true;
		this.m_reservationCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_reservationCount.Location = new System.Drawing.Point(143, 62);
		this.m_reservationCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.m_reservationCount.Name = "m_reservationCount";
		this.m_reservationCount.Size = new System.Drawing.Size(18, 20);
		this.m_reservationCount.TabIndex = 10;
		this.m_reservationCount.Text = "0";
		this.dgvTitles.AllowUserToAddRows = false;
		this.dgvTitles.AllowUserToDeleteRows = false;
		this.dgvTitles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvTitles.Columns.AddRange(this.TitleID, this.Title, this.Type, this.Price, this.Discount, this.DiscountedPrice, this.DiscountType, this.LoyaltyPoints, this.Credits);
		this.dgvTitles.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
		this.dgvTitles.Location = new System.Drawing.Point(11, 80);
		this.dgvTitles.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.dgvTitles.Name = "dgvTitles";
		this.dgvTitles.RowHeadersWidth = 51;
		this.dgvTitles.Size = new System.Drawing.Size(719, 228);
		this.dgvTitles.TabIndex = 11;
		this.dgvTitles.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(dgvTitles_CellValueChanged);
		this.dgvTitles.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(dgvTitles_DataError);
		this.dgvTitles.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(dgvTitles_DefaultValuesNeeded);
		this.TitleID.DataPropertyName = "TitleID";
		this.TitleID.HeaderText = "TitleID";
		this.TitleID.MinimumWidth = 6;
		this.TitleID.Name = "TitleID";
		this.TitleID.ReadOnly = true;
		this.TitleID.Width = 50;
		this.Title.HeaderText = "Title";
		this.Title.MinimumWidth = 6;
		this.Title.Name = "Title";
		this.Title.Width = 150;
		this.Type.HeaderText = "Type";
		this.Type.MinimumWidth = 6;
		this.Type.Name = "Type";
		this.Type.Width = 75;
		this.Price.DataPropertyName = "Price";
		this.Price.HeaderText = "Price";
		this.Price.MinimumWidth = 6;
		this.Price.Name = "Price";
		this.Price.Width = 50;
		this.Discount.DataPropertyName = "Discount";
		this.Discount.HeaderText = "Discount";
		this.Discount.MinimumWidth = 6;
		this.Discount.Name = "Discount";
		this.Discount.Width = 55;
		this.DiscountedPrice.DataPropertyName = "DiscountedPrice";
		this.DiscountedPrice.HeaderText = "Discounted Price";
		this.DiscountedPrice.MinimumWidth = 6;
		this.DiscountedPrice.Name = "DiscountedPrice";
		this.DiscountedPrice.ReadOnly = true;
		this.DiscountedPrice.Width = 75;
		this.DiscountType.HeaderText = "Discount Type";
		this.DiscountType.MinimumWidth = 6;
		this.DiscountType.Name = "DiscountType";
		this.DiscountType.Width = 125;
		this.LoyaltyPoints.DataPropertyName = "LoyaltyPoints";
		this.LoyaltyPoints.HeaderText = "Loyalty Points";
		this.LoyaltyPoints.MinimumWidth = 6;
		this.LoyaltyPoints.Name = "LoyaltyPoints";
		this.LoyaltyPoints.Width = 125;
		this.Credits.DataPropertyName = "NumberOfCredits";
		this.Credits.HeaderText = "Credits";
		this.Credits.MinimumWidth = 6;
		this.Credits.Name = "Credits";
		this.Credits.Width = 50;
		this.btnAdd.Location = new System.Drawing.Point(5, 315);
		this.btnAdd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(100, 28);
		this.btnAdd.TabIndex = 13;
		this.btnAdd.Text = "Add Title";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.btnAdd.Click += new System.EventHandler(btnAdd_Click);
		this.btnRemove.Location = new System.Drawing.Point(132, 315);
		this.btnRemove.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.btnRemove.Name = "btnRemove";
		this.btnRemove.Size = new System.Drawing.Size(123, 28);
		this.btnRemove.TabIndex = 14;
		this.btnRemove.Text = "Remove Title";
		this.btnRemove.UseVisualStyleBackColor = true;
		this.btnRemove.Click += new System.EventHandler(btnRemove_Click);
		this.cbAppliedCredit.AutoSize = true;
		this.cbAppliedCredit.Location = new System.Drawing.Point(392, 4);
		this.cbAppliedCredit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbAppliedCredit.Name = "cbAppliedCredit";
		this.cbAppliedCredit.Size = new System.Drawing.Size(118, 21);
		this.cbAppliedCredit.TabIndex = 0;
		this.cbAppliedCredit.Text = "Applied Credit";
		this.cbAppliedCredit.UseVisualStyleBackColor = true;
		this.cbAppliedPromo.AutoSize = true;
		this.cbAppliedPromo.Location = new System.Drawing.Point(392, 32);
		this.cbAppliedPromo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbAppliedPromo.Name = "cbAppliedPromo";
		this.cbAppliedPromo.Size = new System.Drawing.Size(190, 21);
		this.cbAppliedPromo.TabIndex = 1;
		this.cbAppliedPromo.Text = "Applied Online Promotion";
		this.cbAppliedPromo.UseVisualStyleBackColor = true;
		this.cbAppliedMarketing.AutoSize = true;
		this.cbAppliedMarketing.Location = new System.Drawing.Point(392, 60);
		this.cbAppliedMarketing.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbAppliedMarketing.Name = "cbAppliedMarketing";
		this.cbAppliedMarketing.Size = new System.Drawing.Size(188, 21);
		this.cbAppliedMarketing.TabIndex = 2;
		this.cbAppliedMarketing.Text = "Applied Online Marketing";
		this.cbAppliedMarketing.UseVisualStyleBackColor = true;
		this.cbLoyaltyRedemption.AutoSize = true;
		this.cbLoyaltyRedemption.Location = new System.Drawing.Point(392, 86);
		this.cbLoyaltyRedemption.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbLoyaltyRedemption.Name = "cbLoyaltyRedemption";
		this.cbLoyaltyRedemption.Size = new System.Drawing.Size(155, 21);
		this.cbLoyaltyRedemption.TabIndex = 3;
		this.cbLoyaltyRedemption.Text = "Loyalty Redemption";
		this.cbLoyaltyRedemption.UseVisualStyleBackColor = true;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(4, 5);
		this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(126, 17);
		this.label2.TabIndex = 4;
		this.label2.Text = "Customer Number:";
		this.txtCustNum.Location = new System.Drawing.Point(193, 1);
		this.txtCustNum.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.txtCustNum.Name = "txtCustNum";
		this.txtCustNum.Size = new System.Drawing.Size(132, 22);
		this.txtCustNum.TabIndex = 5;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(388, 139);
		this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(64, 17);
		this.label3.TabIndex = 6;
		this.label3.Text = "Subtotal:";
		this.lblSubTotal.Location = new System.Drawing.Point(541, 139);
		this.lblSubTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lblSubTotal.Name = "lblSubTotal";
		this.lblSubTotal.Size = new System.Drawing.Size(137, 16);
		this.lblSubTotal.TabIndex = 7;
		this.lblSubTotal.Text = "0.00";
		this.lblSubTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(388, 156);
		this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(139, 17);
		this.label4.TabIndex = 8;
		this.label4.Text = "Discounted Subtotal:";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(5, 127);
		this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(180, 17);
		this.label5.TabIndex = 9;
		this.label5.Text = "Online Promotion Discount:";
		this.cbMultiNight.AutoSize = true;
		this.cbMultiNight.Location = new System.Drawing.Point(392, 114);
		this.cbMultiNight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbMultiNight.Name = "cbMultiNight";
		this.cbMultiNight.Size = new System.Drawing.Size(144, 21);
		this.cbMultiNight.TabIndex = 11;
		this.cbMultiNight.Text = "Multi-Night Pricing";
		this.cbMultiNight.UseVisualStyleBackColor = true;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(4, 34);
		this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(127, 17);
		this.label6.TabIndex = 12;
		this.label6.Text = "Remaining Credits:";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(7, 66);
		this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(90, 17);
		this.label7.TabIndex = 14;
		this.label7.Text = "Promo Code:";
		this.txtPromoCode.Location = new System.Drawing.Point(193, 63);
		this.txtPromoCode.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.txtPromoCode.Name = "txtPromoCode";
		this.txtPromoCode.Size = new System.Drawing.Size(132, 22);
		this.txtPromoCode.TabIndex = 15;
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(388, 174);
		this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(35, 17);
		this.label8.TabIndex = 16;
		this.label8.Text = "Tax:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(7, 97);
		this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(69, 17);
		this.label9.TabIndex = 17;
		this.label9.Text = "Tax Rate:";
		this.button1.Location = new System.Drawing.Point(547, 245);
		this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(148, 28);
		this.button1.TabIndex = 8;
		this.button1.Text = "Create Reservation";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(OnCreateReservationClick);
		this.lblDiscountedSubtotal.Location = new System.Drawing.Point(541, 158);
		this.lblDiscountedSubtotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lblDiscountedSubtotal.Name = "lblDiscountedSubtotal";
		this.lblDiscountedSubtotal.Size = new System.Drawing.Size(137, 16);
		this.lblDiscountedSubtotal.TabIndex = 19;
		this.lblDiscountedSubtotal.Text = "0.00";
		this.lblDiscountedSubtotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.lblTax.Location = new System.Drawing.Point(541, 174);
		this.lblTax.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lblTax.Name = "lblTax";
		this.lblTax.Size = new System.Drawing.Size(137, 16);
		this.lblTax.TabIndex = 20;
		this.lblTax.Text = "0.00";
		this.lblTax.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(388, 225);
		this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(88, 17);
		this.label11.TabIndex = 21;
		this.label11.Text = "Grand Total:";
		this.lblGrandTotal.Location = new System.Drawing.Point(541, 225);
		this.lblGrandTotal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lblGrandTotal.Name = "lblGrandTotal";
		this.lblGrandTotal.Size = new System.Drawing.Size(137, 16);
		this.lblGrandTotal.TabIndex = 22;
		this.lblGrandTotal.Text = "0.00";
		this.lblGrandTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(329, 97);
		this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(20, 17);
		this.label10.TabIndex = 25;
		this.label10.Text = "%";
		this.nudTaxRate.DecimalPlaces = 2;
		this.nudTaxRate.Location = new System.Drawing.Point(193, 94);
		this.nudTaxRate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.nudTaxRate.Name = "nudTaxRate";
		this.nudTaxRate.Size = new System.Drawing.Size(133, 22);
		this.nudTaxRate.TabIndex = 26;
		this.nudTaxRate.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.nudTaxRate.ValueChanged += new System.EventHandler(nudTaxRate_ValueChanged);
		this.nudOnlineDiscount.DecimalPlaces = 2;
		this.nudOnlineDiscount.Location = new System.Drawing.Point(193, 124);
		this.nudOnlineDiscount.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.nudOnlineDiscount.Minimum = new decimal(new int[4] { 100, 0, 0, -2147483648 });
		this.nudOnlineDiscount.Name = "nudOnlineDiscount";
		this.nudOnlineDiscount.Size = new System.Drawing.Size(133, 22);
		this.nudOnlineDiscount.TabIndex = 27;
		this.nudOnlineDiscount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.nudOnlineDiscount.ValueChanged += new System.EventHandler(nudOnlineDiscount_ValueChanged);
		this.nudRemainingCredits.Location = new System.Drawing.Point(193, 32);
		this.nudRemainingCredits.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.nudRemainingCredits.Name = "nudRemainingCredits";
		this.nudRemainingCredits.Size = new System.Drawing.Size(133, 22);
		this.nudRemainingCredits.TabIndex = 28;
		this.nudRemainingCredits.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(4, 158);
		this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(136, 17);
		this.label12.TabIndex = 31;
		this.label12.Text = "Default Service Fee:";
		this.nudSvcFeeDefault.DecimalPlaces = 2;
		this.nudSvcFeeDefault.Location = new System.Drawing.Point(193, 155);
		this.nudSvcFeeDefault.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.nudSvcFeeDefault.Name = "nudSvcFeeDefault";
		this.nudSvcFeeDefault.Size = new System.Drawing.Size(133, 22);
		this.nudSvcFeeDefault.TabIndex = 32;
		this.nudSvcFeeDefault.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(4, 188);
		this.label14.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(130, 17);
		this.label14.TabIndex = 33;
		this.label14.Text = "Actual Service Fee:";
		this.nudSvcFeeActual.DecimalPlaces = 2;
		this.nudSvcFeeActual.Location = new System.Drawing.Point(193, 186);
		this.nudSvcFeeActual.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.nudSvcFeeActual.Name = "nudSvcFeeActual";
		this.nudSvcFeeActual.Size = new System.Drawing.Size(133, 22);
		this.nudSvcFeeActual.TabIndex = 34;
		this.nudSvcFeeActual.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.nudSvcFeeActual.ValueChanged += new System.EventHandler(nudSvcFeeActual_ValueChanged);
		this.panel1.Controls.Add(this.tb_last4);
		this.panel1.Controls.Add(this.tb_first6);
		this.panel1.Controls.Add(this.label17);
		this.panel1.Controls.Add(this.label15);
		this.panel1.Controls.Add(this.cbNewHash);
		this.panel1.Controls.Add(this.cbAuthorizeAtPickup);
		this.panel1.Controls.Add(this.lblSvcFeeTax);
		this.panel1.Controls.Add(this.label16);
		this.panel1.Controls.Add(this.nudSvcFeeActual);
		this.panel1.Controls.Add(this.label14);
		this.panel1.Controls.Add(this.nudSvcFeeDefault);
		this.panel1.Controls.Add(this.label12);
		this.panel1.Controls.Add(this.lblServiceFee);
		this.panel1.Controls.Add(this.label13);
		this.panel1.Controls.Add(this.nudRemainingCredits);
		this.panel1.Controls.Add(this.nudOnlineDiscount);
		this.panel1.Controls.Add(this.nudTaxRate);
		this.panel1.Controls.Add(this.label10);
		this.panel1.Controls.Add(this.lblGrandTotal);
		this.panel1.Controls.Add(this.label11);
		this.panel1.Controls.Add(this.lblTax);
		this.panel1.Controls.Add(this.lblDiscountedSubtotal);
		this.panel1.Controls.Add(this.button1);
		this.panel1.Controls.Add(this.label9);
		this.panel1.Controls.Add(this.label8);
		this.panel1.Controls.Add(this.txtPromoCode);
		this.panel1.Controls.Add(this.label7);
		this.panel1.Controls.Add(this.label6);
		this.panel1.Controls.Add(this.cbMultiNight);
		this.panel1.Controls.Add(this.label5);
		this.panel1.Controls.Add(this.label4);
		this.panel1.Controls.Add(this.lblSubTotal);
		this.panel1.Controls.Add(this.label3);
		this.panel1.Controls.Add(this.txtCustNum);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.cbLoyaltyRedemption);
		this.panel1.Controls.Add(this.cbAppliedMarketing);
		this.panel1.Controls.Add(this.cbAppliedPromo);
		this.panel1.Controls.Add(this.cbAppliedCredit);
		this.panel1.Location = new System.Drawing.Point(0, 350);
		this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(719, 287);
		this.panel1.TabIndex = 12;
		this.tb_last4.Location = new System.Drawing.Point(592, 80);
		this.tb_last4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.tb_last4.Name = "tb_last4";
		this.tb_last4.Size = new System.Drawing.Size(121, 22);
		this.tb_last4.TabIndex = 46;
		this.tb_first6.Location = new System.Drawing.Point(592, 26);
		this.tb_first6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.tb_first6.Name = "tb_first6";
		this.tb_first6.Size = new System.Drawing.Size(121, 22);
		this.tb_first6.TabIndex = 45;
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(592, 60);
		this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(51, 17);
		this.label17.TabIndex = 44;
		this.label17.Text = "Last 4:";
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(593, 6);
		this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(51, 17);
		this.label15.TabIndex = 43;
		this.label15.Text = "First 6:";
		this.cbNewHash.AutoSize = true;
		this.cbNewHash.Location = new System.Drawing.Point(5, 245);
		this.cbNewHash.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbNewHash.Name = "cbNewHash";
		this.cbNewHash.Size = new System.Drawing.Size(157, 21);
		this.cbNewHash.TabIndex = 42;
		this.cbNewHash.Text = "Use New Card Hash";
		this.cbNewHash.UseVisualStyleBackColor = true;
		this.cbAuthorizeAtPickup.AutoSize = true;
		this.cbAuthorizeAtPickup.Location = new System.Drawing.Point(5, 220);
		this.cbAuthorizeAtPickup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		this.cbAuthorizeAtPickup.Name = "cbAuthorizeAtPickup";
		this.cbAuthorizeAtPickup.Size = new System.Drawing.Size(153, 21);
		this.cbAuthorizeAtPickup.TabIndex = 41;
		this.cbAuthorizeAtPickup.Text = "Authorize At Pickup";
		this.cbAuthorizeAtPickup.UseVisualStyleBackColor = true;
		this.lblSvcFeeTax.Location = new System.Drawing.Point(541, 208);
		this.lblSvcFeeTax.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lblSvcFeeTax.Name = "lblSvcFeeTax";
		this.lblSvcFeeTax.Size = new System.Drawing.Size(137, 16);
		this.lblSvcFeeTax.TabIndex = 40;
		this.lblSvcFeeTax.Text = "0.00";
		this.lblSvcFeeTax.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(388, 208);
		this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(114, 17);
		this.label16.TabIndex = 37;
		this.label16.Text = "Service Fee Tax:";
		this.lblServiceFee.Location = new System.Drawing.Point(541, 191);
		this.lblServiceFee.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lblServiceFee.Name = "lblServiceFee";
		this.lblServiceFee.Size = new System.Drawing.Size(137, 16);
		this.lblServiceFee.TabIndex = 30;
		this.lblServiceFee.Text = "0.00";
		this.lblServiceFee.TextAlign = System.Drawing.ContentAlignment.TopRight;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(388, 191);
		this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(83, 17);
		this.label13.TabIndex = 29;
		this.label13.Text = "Service Fee";
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.btnRemove);
		base.Controls.Add(this.btnAdd);
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.dgvTitles);
		base.Controls.Add(this.m_reservationCount);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_headerLabel);
		base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
		base.Name = "ReservationPreferencePage";
		base.Size = new System.Drawing.Size(723, 623);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		((System.ComponentModel.ISupportInitialize)this.dgvTitles).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudTaxRate).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudOnlineDiscount).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudRemainingCredits).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudSvcFeeDefault).EndInit();
		((System.ComponentModel.ISupportInitialize)this.nudSvcFeeActual).EndInit();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
