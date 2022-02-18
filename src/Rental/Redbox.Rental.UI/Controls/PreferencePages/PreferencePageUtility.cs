using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.TestData;

namespace Redbox.Rental.UI.Controls.PreferencePages;

public class PreferencePageUtility
{
	public bool ReadDataSucceeded { get; set; }

	public string CategoryName { get; set; }

	public Action<string> UpdateUIErrorMessage { get; set; }

	public void ReadKCSTestDataProperties(List<PreferencePageReadPropertyItem> readPropertyItems)
	{
		if (readPropertyItems == null)
		{
			return;
		}
		IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
		bool readDataSucceeded = true;
		foreach (PreferencePageReadPropertyItem readPropertyItem in readPropertyItems)
		{
			readPropertyItem.ReadResult = service?.TestData?.ReadPropertyValue(CategoryName, readPropertyItem.PropertyName);
			if (readPropertyItem.ReadResult == null || !readPropertyItem.ReadResult.Success)
			{
				readDataSucceeded = false;
			}
		}
		ReadDataSucceeded = readDataSucceeded;
	}

	public void ProcessReadActions(List<PreferencePageReadPropertyItem> readPropertyItems)
	{
		if (readPropertyItems == null)
		{
			return;
		}
		foreach (PreferencePageReadPropertyItem readPropertyItem in readPropertyItems)
		{
			if (readPropertyItem.ReadResult != null && readPropertyItem.ReadResult.Success)
			{
				readPropertyItem.ReadSuccessAction(readPropertyItem.ReadResult);
				continue;
			}
			readPropertyItem.ReadFailAction();
			SetErrorMessage($"Error in KCS reading property {readPropertyItem.PropertyName}: ", readPropertyItem.ReadResult?.Errors, $"Unable to read property {readPropertyItem.PropertyName}");
		}
	}

	public void WriteKCSTestDataProperty(string propertyName, string propertyValue)
	{
		if (ReadDataSucceeded)
		{
			IWriteTestDataValueResult writeTestDataValueResult = ServiceLocator.Instance.GetService<IKioskClientService>()?.TestData?.WritePropertyValue(CategoryName, propertyName, propertyValue);
			if (writeTestDataValueResult != null && writeTestDataValueResult.Success)
			{
				UpdateUIErrorMessage?.Invoke(null);
			}
			else
			{
				SetErrorMessage($"Error in KCS saving property {propertyName} with value '{propertyValue}': ", writeTestDataValueResult?.Errors, $"Unable to save property {propertyName}");
			}
		}
		else
		{
			UpdateUIErrorMessage?.Invoke($"Save of property {propertyName} aborted because prior read failed.  Exit F4 screen and re-enter to attempt data reload.");
		}
	}

	public void SetErrorMessage(string messageHeader, List<IError> errors, string genericErrorMessage)
	{
		if (errors != null)
		{
			StringBuilder stringBuilder = new StringBuilder(messageHeader);
			string arg = null;
			foreach (IError error in errors)
			{
				stringBuilder.Append($"({error.Code}) {error.Message}{arg}");
				arg = ", ";
			}
			UpdateUIErrorMessage?.Invoke(stringBuilder.ToString());
		}
		else
		{
			UpdateUIErrorMessage?.Invoke(genericErrorMessage);
		}
	}
}
