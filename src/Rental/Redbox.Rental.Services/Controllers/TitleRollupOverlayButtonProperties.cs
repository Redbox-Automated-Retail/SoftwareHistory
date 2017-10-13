using System.Windows;

namespace Redbox.Rental.Services.Controllers;

public class TitleRollupOverlayButtonProperties
{
	public string ButtonText { get; set; }

	public Visibility Visibility { get; set; }

	public bool Enabled { get; set; }

	public GridLength MarginGridRowHeight { get; set; }

	public GridLength TextGridRowHeight { get; set; }
}
