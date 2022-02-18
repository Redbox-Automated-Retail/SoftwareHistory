using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.UI.Controls.PreferencePages;

public partial class TransactionServiceAuthorizePreferencePage : UserControl, IPreferencePageHost, IComponentConnector
{
	private class AuthorizeResponseItem
	{
		public string Code { get; set; }

		public string ResponseName { get; set; }

		public override string ToString()
		{
			return $"{ResponseName} {Code}";
		}
	}

	private class TransactionServiceAuthorizePreferencePageModel : DependencyObject
	{
		public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register("ErrorMessage", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty AccountNumberProperty = DependencyProperty.Register("AccountNumber", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileNumberProperty = DependencyProperty.Register("CustomerProfileNumber", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileFirstNameProperty = DependencyProperty.Register("CustomerProfileFirstName", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty CustomerProfileLastNameProperty = DependencyProperty.Register("CustomerProfileLastName", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty EmailProperty = DependencyProperty.Register("Email", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ReferenceNumberProperty = DependencyProperty.Register("ReferenceNumber", typeof(string), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty ConfirmationStatusProperty = DependencyProperty.Register("ConfirmationStatus", typeof(ConfirmationStatus), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(ConfirmationStatus.DontShow));

		public static readonly DependencyProperty AuthorizeResponseProperty = DependencyProperty.Register("AuthorizeResponse", typeof(AuthorizeResponseItem), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LiveCallProperty = DependencyProperty.Register("LiveCall", typeof(bool), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty SimulateCommunicationsFailureProperty = DependencyProperty.Register("SimulateCommunicationsFailure", typeof(bool), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty SimulateServerBusyProperty = DependencyProperty.Register("SimulateServerBusy", typeof(bool), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty HasCustomerProfileProperty = DependencyProperty.Register("HasCustomerProfile", typeof(bool), typeof(TransactionServiceAuthorizePreferencePageModel), new FrameworkPropertyMetadata(false));

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

		public AuthorizeResponseItem AuthorizeResponse
		{
			get
			{
				return base.Dispatcher.Invoke(() => (AuthorizeResponseItem)GetValue(AuthorizeResponseProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AuthorizeResponseProperty, value);
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

		public bool SimulateCommunicationsFailure
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SimulateCommunicationsFailureProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SimulateCommunicationsFailureProperty, value);
				});
			}
		}

		public bool SimulateServerBusy
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(SimulateServerBusyProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(SimulateServerBusyProperty, value);
				});
			}
		}

		public bool HasCustomerProfile
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(HasCustomerProfileProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(HasCustomerProfileProperty, value);
				});
			}
		}

		public string AccountNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(AccountNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(AccountNumberProperty, value);
				});
			}
		}

		public string Email
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(EmailProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(EmailProperty, value);
				});
			}
		}

		public string ReferenceNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(ReferenceNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ReferenceNumberProperty, value);
				});
			}
		}

		public ConfirmationStatus ConfirmationStatus
		{
			get
			{
				return base.Dispatcher.Invoke(() => (ConfirmationStatus)GetValue(ConfirmationStatusProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(ConfirmationStatusProperty, value);
				});
			}
		}

		public string CustomerProfileNumber
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(CustomerProfileNumberProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CustomerProfileNumberProperty, value);
				});
			}
		}

		public string CustomerProfileFirstName
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(CustomerProfileFirstNameProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CustomerProfileFirstNameProperty, value);
				});
			}
		}

		public string CustomerProfileLastName
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(CustomerProfileLastNameProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(CustomerProfileLastNameProperty, value);
				});
			}
		}
	}

	private TransactionServiceAuthorizePreferencePageModel _model = new TransactionServiceAuthorizePreferencePageModel();

	private PreferencePageUtility _preferencePageUtility;

	private List<AuthorizeResponseItem> _responseItems = new List<AuthorizeResponseItem>();

	private List<PreferencePageReadPropertyItem> _preferencePageReadPropertyItems = new List<PreferencePageReadPropertyItem>();

	public TransactionServiceAuthorizePreferencePage()
	{
		InitializeComponent();
		foreach (object value in Enum.GetValues(typeof(AuthorizeResponse)))
		{
			Type enumType = typeof(AuthorizeResponse);
			object[] customAttributes = enumType.GetMember(value.ToString()).FirstOrDefault((MemberInfo m) => m.DeclaringType == enumType).GetCustomAttributes(typeof(ResponseCodeAttribute), inherit: false);
			string responseName = value.ToString();
			string code = ((customAttributes.Count() > 0) ? (customAttributes[0] as ResponseCodeAttribute)?.Name : null);
			AuthorizeResponseItem authorizeResponseItem = new AuthorizeResponseItem
			{
				Code = code,
				ResponseName = responseName
			};
			_responseItems.Add(authorizeResponseItem);
			AuthorizeResponseComboBox.Items.Add(authorizeResponseItem);
		}
		foreach (object value2 in Enum.GetValues(typeof(ConfirmationStatus)))
		{
			ConfirmationStatusComboBox.Items.Add(value2);
		}
		AuthorizeResponseItem approvedResponseItem = _responseItems.FirstOrDefault((AuthorizeResponseItem x) => x.Code == null);
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.ErrorCode", delegate(IReadTestDataValueResult result)
		{
			_model.AuthorizeResponse = _responseItems.FirstOrDefault((AuthorizeResponseItem x) => x.Code == result.GetPropertyValue<string>()) ?? approvedResponseItem;
		}, delegate
		{
			_model.AuthorizeResponse = approvedResponseItem;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.LiveCall", delegate(IReadTestDataValueResult result)
		{
			_model.LiveCall = result.GetPropertyValue<bool?>() == true || result.PropertyValue == null;
		}, delegate
		{
			_model.LiveCall = true;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.SimulateCommunicationFailure", delegate(IReadTestDataValueResult result)
		{
			_model.SimulateCommunicationsFailure = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.SimulateCommunicationsFailure = false;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.HasCustomerProfile", delegate(IReadTestDataValueResult result)
		{
			_model.HasCustomerProfile = result.GetPropertyValue<bool?>() == true;
		}, delegate
		{
			_model.HasCustomerProfile = false;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.AccountNumber", delegate(IReadTestDataValueResult result)
		{
			_model.AccountNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.AccountNumber = null;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.CustomerProfileNumber", delegate(IReadTestDataValueResult result)
		{
			_model.CustomerProfileNumber = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileNumber = null;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.CustomerProfileFirstName", delegate(IReadTestDataValueResult result)
		{
			_model.CustomerProfileFirstName = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileFirstName = null;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.CustomerProfileLastName", delegate(IReadTestDataValueResult result)
		{
			_model.CustomerProfileLastName = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.CustomerProfileLastName = null;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.Email", delegate(IReadTestDataValueResult result)
		{
			_model.Email = result.GetPropertyValue<string>();
		}, delegate
		{
			_model.Email = null;
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.ReferenceNumber", delegate(IReadTestDataValueResult result)
		{
			_model.ReferenceNumber = result.GetPropertyValue<long?>()?.ToString();
		}, delegate
		{
			_model.ReferenceNumber = "0";
		}));
		_preferencePageReadPropertyItems.Add(new PreferencePageReadPropertyItem("Authorize.ConfirmationStatus", delegate(IReadTestDataValueResult result)
		{
			ConfirmationStatus confirmationStatus = ConfirmationStatus.DontShow;
			if (Enum.TryParse<ConfirmationStatus>(result.GetPropertyValue<string>(), out var result2))
			{
				confirmationStatus = result2;
			}
			_model.ConfirmationStatus = confirmationStatus;
		}, delegate
		{
			_model.ConfirmationStatus = ConfirmationStatus.DontShow;
		}));
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		if (_preferencePageUtility == null)
		{
			InitializePreferencePageUtility();
		}
		_preferencePageUtility.ReadDataSucceeded = false;
		_model.ErrorMessage = "Loading data...";
		base.DataContext = _model;
		Task.Run(delegate
		{
			_preferencePageUtility.ReadKCSTestDataProperties(_preferencePageReadPropertyItems);
		}).ContinueWith(delegate
		{
			_preferencePageUtility.ProcessReadActions(_preferencePageReadPropertyItems);
			if (_preferencePageUtility.ReadDataSucceeded)
			{
				_model.ErrorMessage = null;
			}
		}, CancellationToken.None, TaskContinuationOptions.None, ServiceLocator.Instance.GetService<IViewService>()?.UITaskScheduler);
	}

	private void InitializePreferencePageUtility()
	{
		_preferencePageUtility = new PreferencePageUtility
		{
			CategoryName = "TransactionService"
		};
		_preferencePageUtility.UpdateUIErrorMessage = delegate(string errorMessage)
		{
			_model.ErrorMessage = errorMessage;
		};
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.ErrorCode", _model.AuthorizeResponse?.Code);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.LiveCall", _model.LiveCall.ToString());
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.SimulateCommunicationFailure", _model.SimulateCommunicationsFailure.ToString());
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.HasCustomerProfile", _model.HasCustomerProfile.ToString());
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.AccountNumber", _model.AccountNumber);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.CustomerProfileNumber", _model.CustomerProfileNumber);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.CustomerProfileFirstName", _model.CustomerProfileFirstName);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.CustomerProfileLastName", _model.CustomerProfileLastName);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.Email", _model.Email);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.ReferenceNumber", _model.ReferenceNumber);
		_preferencePageUtility.WriteKCSTestDataProperty("Authorize.ConfirmationStatus", _model.ConfirmationStatus.ToString());
		return true;
	}
}
