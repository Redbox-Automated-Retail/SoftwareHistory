using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Fraud;
using Redbox.KioskEngine.PreferenceManager.Preferences;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.KioskEngine.PreferenceManager.ProxyService;

[PreferencePage(Name = "Fraud Service", Path = "Kiosk Client Service/Proxy Service/Fraud Service", TreeOrder = 390)]
public partial class FraudServiceUserControl : UserControl, IPreferencePageHost, IComponentConnector
{
	private class FraudServiceModel : DependencyObject
	{
		public static readonly DependencyProperty IsHardwareIrEnabledProperty = DependencyProperty.Register("IsHardwareIrEnabled", typeof(bool), typeof(FraudServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsPaperEnabledProperty = DependencyProperty.Register("IsPaperEnabled", typeof(bool), typeof(FraudServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty IsPeeledEnabledProperty = DependencyProperty.Register("IsPeeledEnabled", typeof(bool), typeof(FraudServiceModel), new FrameworkPropertyMetadata(false));

		public static readonly DependencyProperty PaperExclusionsProperty = DependencyProperty.Register("PaperExclusions", typeof(string), typeof(FraudServiceModel), new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty PeeledInclusionsProperty = DependencyProperty.Register("PeeledInclusions", typeof(string), typeof(FraudServiceModel), new FrameworkPropertyMetadata(null));

		public bool IsHardwareIrEnabled
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IsHardwareIrEnabledProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IsHardwareIrEnabledProperty, value);
				});
			}
		}

		public bool IsPaperEnabled
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IsPaperEnabledProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IsPaperEnabledProperty, value);
				});
			}
		}

		public bool IsPeeledEnabled
		{
			get
			{
				return base.Dispatcher.Invoke(() => (bool)GetValue(IsPeeledEnabledProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(IsPeeledEnabledProperty, value);
				});
			}
		}

		public string PaperExclusions
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(PaperExclusionsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PaperExclusionsProperty, value);
				});
			}
		}

		public string PeeledInclusions
		{
			get
			{
				return base.Dispatcher.Invoke(() => (string)GetValue(PeeledInclusionsProperty));
			}
			set
			{
				base.Dispatcher.Invoke(delegate
				{
					SetValue(PeeledInclusionsProperty, value);
				});
			}
		}
	}

	private FraudServiceModel _model = new FraudServiceModel();

	private SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

	public FraudServiceUserControl()
	{
		InitializeComponent();
	}

	public async void LoadValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Loading...");
			base.DataContext = _model;
			FraudServiceInfo fraudServiceInfo = await KioskEngineApiFraudServiceClient.GetFraudServiceInfoResponse();
			_model.IsHardwareIrEnabled = fraudServiceInfo?.HardwareSupportsIRScan ?? false;
			_model.IsPaperEnabled = fraudServiceInfo?.IsIRSecurityEnabled ?? false;
			_model.IsPeeledEnabled = fraudServiceInfo?.IsFraudImageEnabled ?? false;
			IEnumerable<TitleType> items = fraudServiceInfo?.FraudPaperExcludedFormats ?? new List<TitleType>();
			StringBuilder stringBuilder = new StringBuilder();
			items.ForEach(delegate(TitleType x)
			{
				stringBuilder.AppendLine(x.ToString());
			});
			_model.PaperExclusions = stringBuilder.ToString();
			stringBuilder.Clear();
			(fraudServiceInfo?.FraudPeelIncludeFormats ?? new List<TitleType>()).ForEach(delegate(TitleType x)
			{
				stringBuilder.AppendLine(x.ToString());
			});
			_model.PeeledInclusions = stringBuilder.ToString();
			PreferencePageHeaderUserControl.SetStatusText(null);
		}
		finally
		{
			semaphoreSlim.Release();
		}
	}

	public async Task<bool> SaveValues(IPreferencePage preferencePage)
	{
		await semaphoreSlim.WaitAsync();
		try
		{
			PreferencePageHeaderUserControl.SetStatusText("Saving...");
			return true;
		}
		finally
		{
			PreferencePageHeaderUserControl.SetStatusText(null);
			semaphoreSlim.Release();
		}
	}
}
