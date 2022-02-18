using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.Rental.UI.Controls.PreferencePages;

public partial class TransactionServiceReturnPreferencePage : UserControl, IPreferencePageHost, IComponentConnector
{
	public TransactionServiceReturnPreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		UseTransactionServiceCheckBox.IsChecked = ServiceLocator.Instance.GetService<IConfiguration>().SendReturnToKioskClientService;
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		ServiceLocator.Instance.GetService<IConfiguration>().SendReturnToKioskClientService = UseTransactionServiceCheckBox.IsChecked ?? false;
		return true;
	}

	private void UseTransactionServiceClicked(object sender, RoutedEventArgs args)
	{
		ServiceLocator.Instance.GetService<IConfiguration>().SendReturnToKioskClientService = UseTransactionServiceCheckBox.IsChecked ?? false;
	}

	private void SendReturnClicked(object sender, RoutedEventArgs args)
	{
		ServiceLocator.Instance.GetService<IInventoryServices>().Return(ServiceLocator.Instance.GetService<IStoreServices>().StoreNumberString, Enum<ReturnType>.ParseIgnoringCase(ReturnTypeComboBox.SelectedValue.ToString(), ReturnType.KioskReturn), BarcodeTextbox.Text, int.Parse(DeckTextbox.Text), int.Parse(SlotTextbox.Text), DateTime.Now.ToString(), FailedSecurityReadCheckBox.IsChecked ?? false, FraudFilenameTextbox.Text, null);
	}
}
