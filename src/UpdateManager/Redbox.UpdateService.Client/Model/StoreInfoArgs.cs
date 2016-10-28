namespace Redbox.UpdateService.Model;

internal class StoreInfoArgs
{
	public string StoreNumber { get; set; }

	public ClientStoreInfo ClientStoreInfo { get; set; }

	public int ClientStoreInfoSyncID { get; set; }
}
