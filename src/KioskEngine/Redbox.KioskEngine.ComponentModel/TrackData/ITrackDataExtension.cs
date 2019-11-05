using System;
using System.Linq;
using DeviceService.ComponentModel;

namespace Redbox.KioskEngine.ComponentModel.TrackData;

public static class ITrackDataExtension
{
	private static class ErrorCodes
	{
		public const string SwipedCardIsChipEnabled = "T020";
	}

	public static bool CardSourceTypeIsEMVContactOrQuickChip(this ITrackData td)
	{
		if (td != null)
		{
			if (td.CardSourceType != CardSourceType.EMVContact)
			{
				return td.CardSourceType == CardSourceType.QuickChip;
			}
			return true;
		}
		return false;
	}

	public static bool CardSourceTypeIsEMVContactlessOrMobileOrMSDContactless(this ITrackData td)
	{
		if (td != null)
		{
			if (td.CardSourceType != CardSourceType.EMVContactless && td.CardSourceType != CardSourceType.Mobile)
			{
				return td.CardSourceType == CardSourceType.MSDContactless;
			}
			return true;
		}
		return false;
	}

	public static bool HasErrors(this ITrackData td)
	{
		if (td != null && td.Errors != null)
		{
			return td.Errors.Count > 0;
		}
		return false;
	}

	public static bool HasSwipedCardIsChipEnabledError(this ITrackData td)
	{
		if (td.HasErrors())
		{
			return td.Errors.ContainsCode("T020");
		}
		return false;
	}

	public static bool HasTamperedError(this ITrackData td)
	{
		if (td.HasErrors())
		{
			return td.Errors.Any((Error e) => string.Equals(e.Code, "TAMPERED", StringComparison.CurrentCultureIgnoreCase));
		}
		return false;
	}
}
