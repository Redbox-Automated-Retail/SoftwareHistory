namespace KioskClientService.API.Services.Campaigns;

public interface IStartAssetControl : IViewControl
{
	Asset Asset { get; }

	AssetTarget? Target { get; }

	string TargetValue { get; }

	bool? ShowPressToStart { get; }

	bool? IncludeIfNoInventory { get; }
}
