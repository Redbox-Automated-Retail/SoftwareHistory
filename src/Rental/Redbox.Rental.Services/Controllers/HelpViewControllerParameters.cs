using System;

namespace Redbox.Rental.Services.Controllers;

public class HelpViewControllerParameters
{
	public Action GoBackAction { get; set; }

	public Action ClickRedboxLogoAction { get; set; }

	public bool ShowFaq { get; set; }

	public bool ShowTerms { get; set; }

	public bool ShowRedboxCard { get; set; }

	public bool ShowPrivacy { get; set; }

	public bool ShowPerks { get; set; }

	public bool ShowMobileFaq { get; set; }

	public string AnalysisContext { get; set; }
}
