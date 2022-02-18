using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.Subscriptions;
using Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.TestData;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.App;
using Redbox.Rental.Services.KioskClientService.Subscriptions;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Subscription Plans", Path = "Kiosk Client Service/Proxy Service/Subscription Plans")]
public partial class GetSubscriptionPlansUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class ProxyServiceSubscriptionModel : DependencyObject
	{
		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(ProxyServiceSubscriptionModel), new FrameworkPropertyMetadata(true));

		public static readonly DependencyProperty BrowseStarzSubscriptionPlanProperty = DependencyProperty.Register("BrowseStarzSubscriptionPlan", typeof(string), typeof(ProxyServiceSubscriptionModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty MyBagStarzSubscriptionPlanProperty = DependencyProperty.Register("MyBagStarzSubscriptionPlan", typeof(string), typeof(ProxyServiceSubscriptionModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(ProxyServiceSubscriptionModel), new FrameworkPropertyMetadata(null));

		public string ErrorMessage
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(ErrorMessageProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ErrorMessageProperty, value);
				});
			}
		}

		public string BrowseStarzSubscriptionPlan
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(BrowseStarzSubscriptionPlanProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(BrowseStarzSubscriptionPlanProperty, value);
				});
			}
		}

		public string MyBagStarzSubscriptionPlan
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(MyBagStarzSubscriptionPlanProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(MyBagStarzSubscriptionPlanProperty, value);
				});
			}
		}

		public bool LiveCall
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(LiveCallProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(LiveCallProperty, value);
				});
			}
		}
	}

	private ProxyServiceSubscriptionModel _model = new ProxyServiceSubscriptionModel();

	private TestDataUtility _testDataUtility;

	private List<TestDataReadPropertyItem> _testDataReadPropertyItems = new List<TestDataReadPropertyItem>();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public GetSubscriptionPlansUserControl()
	{
		InitializeComponent();
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GetSubscriptionPlans.LiveCall", delegate(ReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GetSubscriptionPlans.BrowseStarzSubscriptionPlan", delegate(ReadTestDataValueResult result)
		{
			_model.BrowseStarzSubscriptionPlan = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.BrowseStarzSubscriptionPlan = null;
		}));
		_testDataReadPropertyItems.Add(new TestDataReadPropertyItem("GetSubscriptionPlans.MyBagStarzSubscriptionPlan", delegate(ReadTestDataValueResult result)
		{
			_model.MyBagStarzSubscriptionPlan = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.MyBagStarzSubscriptionPlan = null;
		}));
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Saving...");
			await _testDataUtility.WriteKCSTestDataProperty("GetSubscriptionPlans.LiveCall", _model.LiveCall.ToString());
			await _testDataUtility.WriteKCSTestDataProperty("GetSubscriptionPlans.BrowseStarzSubscriptionPlan", _model?.BrowseStarzSubscriptionPlan);
			await _testDataUtility.WriteKCSTestDataProperty("GetSubscriptionPlans.MyBagStarzSubscriptionPlan", _model?.MyBagStarzSubscriptionPlan);
			return true;
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	private void InitializeTestDataUtility()
	{
		_testDataUtility = new TestDataUtility
		{
			CategoryName = "ProxyServiceSubscriptions"
		};
		_testDataUtility.UpdateUIErrorMessage = delegate(string errorMessage)
		{
			PreferencePageHeaderUserControl.SetStatusText(errorMessage);
		};
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Loading...");
			if (_testDataUtility == null)
			{
				InitializeTestDataUtility();
			}
			if (await PopulateSubscriptionComboBoxes())
			{
				_testDataUtility.ReadDataSucceeded = false;
				base.DataContext = _model;
				await _testDataUtility.ReadKCSTestDataProperties(_testDataReadPropertyItems);
				_testDataUtility.ProcessReadActions(_testDataReadPropertyItems);
				if (_testDataUtility.ReadDataSucceeded)
				{
					PreferencePageHeaderUserControl.SetStatusText(null);
				}
			}
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	private async Task<bool> PopulateSubscriptionComboBoxes()
	{
		bool result = false;
		BrowseSubscriptionPlanComboBox.Items.Clear();
		BrowseSubscriptionPlanComboBox.Items.Add(string.Empty);
		MyBagSubscriptionPlanComboBox.Items.Clear();
		MyBagSubscriptionPlanComboBox.Items.Add(string.Empty);
		APIResponse<GetSubscriptionPlanResponse> aPIResponse = await Redbox.KioskEngine.PreferenceManager.KioskClientServiceClient.Subscriptions.KioskClientServiceSubscriptions.GetSubscriptionPlans();
		if (aPIResponse?.Response?.Success ?? false)
		{
			result = true;
			ISubscriptionPlans subscriptionPlans = new SubscriptionPlans(aPIResponse?.Response);
			if (subscriptionPlans?.SubscriptionProducts != null)
			{
				foreach (ISubscriptionProduct item in subscriptionPlans.SubscriptionProducts.Where((ISubscriptionProduct x) => x.SubscriptionType == SubscriptionType.Starz))
				{
					BrowseSubscriptionPlanComboBox.Items.Add(item.SubscriptionId);
					MyBagSubscriptionPlanComboBox.Items.Add(item.SubscriptionId);
				}
			}
		}
		else
		{
			PreferencePageHeaderUserControl.SetStatusText($"Error - unable to load Subscription data from KCS. StatusCode={aPIResponse?.StatusCode}");
		}
		return result;
	}
}
