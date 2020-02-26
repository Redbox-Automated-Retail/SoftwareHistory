using System.Windows.Input;

namespace Redbox.Rental.UI.Models;

public class PerksSignUpInfoPopupViewModel : BaseModel<PerksSignUpInfoPopupViewModel>
{
	public string TitleText { get; set; }

	public string EarnHeading { get; set; }

	public string UseHeading { get; set; }

	public string OKButtonText { get; set; }

	public ICommand OKButtonCommand { get; set; }

	public string EarnOption1Desc { get; set; }

	public string EarnOption1Value { get; set; }

	public string EarnOption2Desc { get; set; }

	public string EarnOption2Value { get; set; }

	public string EarnOption3Desc { get; set; }

	public string EarnOption3Value { get; set; }

	public string EarnOption4Desc { get; set; }

	public string EarnOption4Value { get; set; }

	public string UseOption1Desc { get; set; }

	public string UseOption1Value { get; set; }

	public string UseOption2Desc { get; set; }

	public string UseOption2Value { get; set; }

	public string UseOption3Desc { get; set; }

	public string UseOption3Value { get; set; }
}
