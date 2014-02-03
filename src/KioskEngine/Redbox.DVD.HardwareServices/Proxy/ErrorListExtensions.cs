using Redbox.HAL.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

internal static class ErrorListExtensions
{
	public static void AddToLocalErrorList(this Redbox.HAL.Core.ErrorList foreginErrorList, Redbox.KioskEngine.ComponentModel.ErrorList errors)
	{
		foreach (Redbox.HAL.Core.Error foreginError in foreginErrorList)
		{
			errors.Add(foreginError.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(foreginError.Code, foreginError.Description, foreginError.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(foreginError.Code, foreginError.Description, foreginError.Details));
		}
	}
}
