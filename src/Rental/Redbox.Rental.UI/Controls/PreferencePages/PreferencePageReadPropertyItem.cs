using System;
using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.UI.Controls.PreferencePages;

public class PreferencePageReadPropertyItem
{
	public string PropertyName { get; set; }

	public Action<IReadTestDataValueResult> ReadSuccessAction { get; set; }

	public Action ReadFailAction { get; set; }

	public IReadTestDataValueResult ReadResult { get; set; }

	public PreferencePageReadPropertyItem(string propertyName, Action<IReadTestDataValueResult> readSuccessAction, Action readFailureAction)
	{
		PropertyName = propertyName;
		ReadSuccessAction = readSuccessAction;
		ReadFailAction = readFailureAction;
	}
}
