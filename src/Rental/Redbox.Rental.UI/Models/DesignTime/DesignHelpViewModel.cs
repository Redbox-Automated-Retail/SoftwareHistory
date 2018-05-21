using System.Collections.ObjectModel;

namespace Redbox.Rental.UI.Models.DesignTime;

public class DesignHelpViewModel : HelpViewModel
{
	public DesignHelpViewModel()
	{
		base.HelpButtonList = new ObservableCollection<HelpButtonData>
		{
			new HelpButtonData
			{
				ButtonText = "Rental FAQs",
				IsEnabled = false,
				UpdatedText = "UPDATED",
				ShowUpdatedFlag = true
			},
			new HelpButtonData
			{
				ButtonText = "Transaction Terms",
				IsEnabled = true,
				UpdatedText = "UPDATED",
				ShowUpdatedFlag = true
			},
			new HelpButtonData
			{
				ButtonText = "Redbox Card Terms",
				IsEnabled = true,
				UpdatedText = "UPDATED",
				ShowUpdatedFlag = true
			},
			new HelpButtonData
			{
				ButtonText = "Privacy",
				IsEnabled = true,
				UpdatedText = "UPDATED",
				ShowUpdatedFlag = true
			},
			new HelpButtonData
			{
				ButtonText = "Perks",
				IsEnabled = true,
				UpdatedText = "UPDATED",
				ShowUpdatedFlag = true
			},
			new HelpButtonData
			{
				ButtonText = "Mobile FAQs",
				IsEnabled = true,
				UpdatedText = "UPDATED",
				ShowUpdatedFlag = true
			}
		};
	}
}
