using System;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.ComponentModel;

public interface IDeviceServiceClient
{
	bool IsConnectedToDeviceService { get; }

	int DefaultCommandTimeout { get; set; }

	bool AutoReconnect { get; set; }

	Version AssemblyVersion { get; }

	event LogHandler OnLog;

	event OnConnected OnConnectedHandler;

	event OnDisconnected OnDisconnectedHandler;

	event OnConnected OnCardReaderConnectedHandler;

	event OnDisconnected OnCardReaderDisconnectedHandler;

	void StartConnectionToDeviceService(string url, int? connectionTimeout);

	bool ConnectToDeviceService(string url);

	bool ConnectToDeviceService(string url, int connectionTimeout);

	bool DisconnectFromDeviceService();

	bool IsCardReaderConnected();

	UnitHealthModel GetUnitHealth();

	void ReadCard(Guid requestId, CardReadRequest request, Action<BaseResponseEvent> onComplete, int? timeout);

	IReadCardJob CreateReadCardJob(CardReadRequest request, Action<BaseResponseEvent> readCardJobCompleteCallback, int? timeout);

	bool CancelCommand(Guid guid, Action<BaseResponseEvent> cancelCompleteCallback);

	bool RebootCardReader();

	void ReportAuthorizeResult(bool success, Action<BaseResponseEvent> completeCallback);

	ValidateVersionModel ValidateVersion();
}
