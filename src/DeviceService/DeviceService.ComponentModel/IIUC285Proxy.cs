using System;
using System.Threading;
using System.Threading.Tasks;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface IIUC285Proxy
{
	bool IsConnected { get; }

	event OnConnected OnConnectedHandler;

	event OnDisconnected OnDisconnectedHandler;

	bool UpdateDeviceConfiguration();

	UnitHealthModel GetUnitHealth();

	UnitDataModel GetUnitData();

	Task<bool> ReadCard(ICardReadRequest request, CancellationToken token, Action<Base87CardReadModel> jobCompleted);

	bool ReadConfig(string group, string index, out string Data);

	bool WriteConfig(string group, string index, string Data);

	Task<bool> WriteFile(string filePath, bool rebootAfterWrite = false);

	bool Reboot();

	void ReadCancel();

	string GetVariable_29(string varID);

	InsertedStatus CheckIfCardInserted();
}
