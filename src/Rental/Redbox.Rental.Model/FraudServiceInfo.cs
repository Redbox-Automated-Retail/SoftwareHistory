using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public class FraudServiceInfo
{
	public bool HardwareSupportsIRScan { get; set; }

	public bool IsIRSecurityEnabled { get; set; }

	public bool IsFraudImageEnabled { get; set; }

	public IEnumerable<TitleType> FraudPaperExcludedFormats { get; set; }

	public IEnumerable<TitleType> FraudPeelIncludeFormats { get; set; }
}
