using System.Collections.Generic;

namespace Redbox.HAL.DataStorage;

public static class TableNames
{
	public const string DumpBinInventory = "BarcodesInBin";

	public const string CameraMetrics = "CameraData";

	public const string SecretOptions = "SecretOptions_v2";

	public const string Counters = "Counters";

	public const string Inventory = "Inventory_v1";

	public const string HardwareCorrection = "HardwareCorrection";

	internal static readonly Dictionary<string, string> CreateMap;

	static TableNames()
	{
		CreateMap = new Dictionary<string, string>();
		CreateMap["CameraData"] = string.Format("CREATE TABLE [{0}] ( [SecureCount] INT NOT NULL, [ClearImageScan] bit NOT NULL, [CameraType] INT NOT NULL, [ReadOk] BIT NOT NULL, [ScanDate] DateTime NOT NULL, [ScanTime] Decimal NOT NULL )", "CameraData");
		CreateMap["BarcodesInBin"] = string.Format("CREATE TABLE [{0}] ( [Barcode] VarChar(100) NOT NULL, [PutTime] DateTime NOT NULL )", "BarcodesInBin");
		CreateMap["SecretOptions_v2"] = string.Format("CREATE TABLE [{0}] ( [OptionName] VarChar(32) NOT NULL PRIMARY KEY, [OptionValue] VarChar(64) NOT NULL, [OptionEncrypted] bit NOT NULL )", "SecretOptions_v2");
		CreateMap["Inventory_v1"] = string.Format("CREATE TABLE [{0}] ( [Deck] INT NOT NULL, [Slot] INT NOT NULL, [ID] VarChar(100) NOT NULL, [LastReturnTime] DateTime, [Excluded] bit NOT NULL, [EmptyStuckCount] INT NOT NULL, [MerchFlags] INT NOT NULL)", "Inventory_v1");
		CreateMap["HardwareCorrection"] = string.Format("CREATE TABLE [{0}] ( [Type] INT NOT NULL, [Job] VarChar(12) NOT NULL, [CorrectionTime] DateTime NOT NULL, [Success] bit NOT NULL)", "HardwareCorrection");
	}
}
