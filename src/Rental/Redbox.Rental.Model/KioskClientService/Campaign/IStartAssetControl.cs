namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IStartAssetControl : IViewControl
{
	Asset Asset { get; }

	AssetTarget? Target { get; }

	string TargetValue { get; }

	short? Order { get; }

	bool? ShowPressToStart { get; }

	bool? IncludeIfNoInventory { get; }
}
