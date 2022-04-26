using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.IoT.FileSets;

public class KioskFileSetVersionsService : IKioskFileSetVersionsService
{
	private readonly IIoTCommandClient _iotCommandClient;

	private readonly IStoreService _store;

	private readonly ILogger<KioskFileSetVersionsService> _logger;

	public KioskFileSetVersionsService(IIoTCommandClient iotCommandClient, IStoreService store, ILogger<KioskFileSetVersionsService> logger)
	{
		_iotCommandClient = iotCommandClient;
		_store = store;
		_logger = logger;
	}

	public async Task<bool> ReportFileVersion(FileSetPollRequest version)
	{
		bool flag = (await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			RequestId = Guid.NewGuid().ToString(),
			Version = 2,
			SourceId = _store.KioskId.ToString(),
			Payload = new FileSetPollRequestList
			{
				FileSetPollRequests = new List<FileSetPollRequest> { version }
			},
			Command = CommandEnum.ReportFileSetVersions
		}, new PerformIoTCommandParameters
		{
			RequestTimeout = TimeSpan.FromSeconds(15.0),
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		})).StatusCode == 200;
		if (!flag)
		{
			_logger.LogErrorWithSource("Something went wrong while reporting FileSet version: " + version.ToJson(), "ReportFileVersion", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
		}
		return flag;
	}

	public async Task ReportFileVersions()
	{
		var (payload, list) = GetPollRequest();
		if (list.Any())
		{
			_logger.LogErrorWithSource("Couldn't report file set versions! Errors: " + list.ToJson(), "ReportFileVersions", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
			return;
		}
		IoTCommandResponse<ObjectResult> ioTCommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			RequestId = Guid.NewGuid().ToString(),
			Version = 2,
			SourceId = _store.KioskId.ToString(),
			Payload = payload,
			Command = CommandEnum.ReportFileSetVersions
		}, new PerformIoTCommandParameters
		{
			RequestTimeout = TimeSpan.FromSeconds(15.0),
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		});
		if (ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200 && ioTCommandResponse != null)
		{
			_ = ioTCommandResponse.Payload?.StatusCode == 200;
		}
	}

	private (FileSetPollRequestList requestList, List<Error> errors) GetPollRequest()
	{
		List<Error> list = new List<Error>();
		FileSetPollRequestList fileSetPollRequestList = new FileSetPollRequestList();
		try
		{
			list = StateFile.GetCurrentState(out fileSetPollRequestList);
			if (list.Count() > 0)
			{
				return (requestList: null, errors: list);
			}
			_logger.LogInfoWithSource("Store file poll request: " + fileSetPollRequestList.ToJson(), "GetPollRequest", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
		}
		catch (Exception e)
		{
			list.Add(new Error
			{
				Message = "Unhandled exception occurred. Exception: " + e.GetFullMessage()
			});
		}
		return (requestList: fileSetPollRequestList, errors: list);
	}
}
