using System;
using System.Collections.Generic;
using System.Linq;
using WUApiLib;

namespace Redbox.WUA;

internal static class Extensions
{
	public static void Add(this ErrorList errorList, string code, string description, string details)
	{
		errorList.Add(Error.NewError(code, description, details));
	}

	public static void Add(this ErrorList errorList, string code, string description, Exception exception)
	{
		errorList.Add(Error.NewError(code, description, exception));
	}

	public static void Add(this ErrorList errorList, IUpdateException updateException)
	{
		errorList.Add(Error.NewError("WUA101", updateException.Message, "HResult: " + updateException.HResult));
	}

	public static Error ToError(this OperationResultCode operationResultCode)
	{
		return operationResultCode switch
		{
			OperationResultCode.orcAborted => Error.NewError("WUA102", "Aborted", Enum.GetName(typeof(OperationResultCode), operationResultCode)), 
			OperationResultCode.orcFailed => Error.NewError("WUA103", "Failed", Enum.GetName(typeof(OperationResultCode), operationResultCode)), 
			OperationResultCode.orcInProgress => Error.NewError("WUA104", "In Progress", Enum.GetName(typeof(OperationResultCode), operationResultCode)), 
			OperationResultCode.orcNotStarted => Error.NewError("WUA105", "Not Started", Enum.GetName(typeof(OperationResultCode), operationResultCode)), 
			OperationResultCode.orcSucceededWithErrors => Error.NewError("WUA106", "Succeeded with errors", Enum.GetName(typeof(OperationResultCode), operationResultCode)), 
			OperationResultCode.orcSucceeded => null, 
			_ => null, 
		};
	}

	public static List<Update> ToListOfUpdates(this UpdateCollection updateCollection)
	{
		return (from IUpdate update in updateCollection
			select new Update(update)).ToList();
	}

	public static string ExtractCodeFromBrackets(this string value, string prefix, string postfix)
	{
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		int num4 = 0;
		do
		{
			string text = string.Empty;
			if (num4 + prefix.Length < value.Length)
			{
				text = value.Substring(num4, prefix.Length);
			}
			string text2 = string.Empty;
			if (num4 + text2.Length < value.Length)
			{
				text2 = value.Substring(num4, postfix.Length);
			}
			if (text == prefix)
			{
				if (num2 == -1)
				{
					num2 = num4 + prefix.Length;
				}
				num++;
			}
			else if (text2 == postfix)
			{
				num--;
				if (num == 0)
				{
					num3 = num4 - postfix.Length;
					break;
				}
			}
			num4++;
		}
		while (num4 < value.Length);
		if (num2 == -1 || num3 == -1)
		{
			return null;
		}
		return value.Substring(num2, num3 - num2 + postfix.Length);
	}
}
