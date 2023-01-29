namespace KioskClientService.API.Services.Campaigns;

internal class StartAssetControl : ViewControl, IStartAssetControl, IViewControl
{
	public Asset Asset { get; internal set; }

	public AssetTarget? Target { get; internal set; }

	public string TargetValue { get; internal set; }

	public bool? ShowPressToStart { get; internal set; }

	public bool? IncludeIfNoInventory { get; internal set; }

	public StartAssetControl()
	{
		base.ControlType = ViewControlType.StartAsset;
	}
}
