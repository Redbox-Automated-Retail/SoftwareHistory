using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.UI.Models;

public class FormatSelectionPopupViewModel : BaseModel<FormatSelectionPopupViewModel>
{
	public Action OnCancelButtonClicked;

	public Action<TitleType> OnFormatButtonClicked;

	public Action OnDigitalCodeInfoButtonClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public string HeaderText { get; set; }

	public string DescriptionText { get; set; }

	public string DigitalCodeInfoText { get; set; }

	public ObservableCollection<FormatButtonData> ButtonData { get; set; }

	public string CancelButtonText { get; set; }

	public int DialogHeight { get; set; }

	public int DialogWidth { get; set; }

	public Visibility DigitalCodeInfoVisibility
	{
		get
		{
			if (!ButtonData.Any((FormatButtonData b) => b.Format == TitleType.DigitalCode && b.IsEnabled && !string.IsNullOrEmpty(DigitalCodeInfoText)))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public FormatSelectionPopupViewModel()
	{
		ButtonData = new ObservableCollection<FormatButtonData>();
	}

	public void ProcessFormatButtonClicked(TitleType tag)
	{
		OnFormatButtonClicked?.Invoke(tag);
	}

	public void ProcessCancelButtonClicked()
	{
		OnCancelButtonClicked?.Invoke();
	}

	public void ProcessDigitalCodeInfoButtonClicked()
	{
		OnDigitalCodeInfoButtonClicked?.Invoke();
	}
}
