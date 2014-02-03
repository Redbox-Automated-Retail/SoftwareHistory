using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.WUA;

namespace Redbox.UpdateManager.DownloadFile;

internal static class Extensions
{
	public static void ToLogHelper(this Redbox.WUA.ErrorList errors)
	{
		if (errors.Count > 0)
		{
			LogHelper.Instance.Log(errors.ToSingleString());
		}
	}

	public static string ToSingleString(this Redbox.WUA.ErrorList errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Redbox.WUA.Error error in errors)
		{
			stringBuilder.AppendLine(error.ToString());
			stringBuilder.AppendLine(error.Details);
		}
		return stringBuilder.ToString();
	}

	public static Redbox.UpdateManager.ComponentModel.ErrorList ToErrors(this Redbox.WUA.ErrorList errors)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		foreach (Redbox.WUA.Error error in errors)
		{
			errorList.Add(error.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(error.Code, error.Description, error.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(error.Code, error.Description, error.Details));
		}
		return errorList;
	}
}
