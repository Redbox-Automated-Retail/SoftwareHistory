using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.IoT.Commands.KioskFiles;

public class KioskFilesService : IKioskFilesService
{
	private readonly ILogger<KioskFilesService> _logger;

	public KioskFilesService(ILogger<KioskFilesService> logger)
	{
		_logger = logger;
	}

	public async Task<MqttResponse<Dictionary<string, string>>> PeekRequestedFilessAsync(KioskFilePeekRequest logRequestModel)
	{
		MqttResponse<Dictionary<string, string>> mqttResponse = new MqttResponse<Dictionary<string, string>>
		{
			Data = new Dictionary<string, string>()
		};
		DirectoryInfo directoryInfo = new DirectoryInfo(await GetRequestedLogPath(logRequestModel.FileType));
		new List<FileInfo>();
		List<FileInfo> list = ((!logRequestModel.Date.Equals(null)) ? (from x in directoryInfo.GetFiles()
			where x.LastWriteTime.ToShortDateString() == logRequestModel.Date?.ToShortDateString()
			select x).ToList() : directoryInfo.GetFiles().ToList());
		foreach (FileInfo item in list)
		{
			mqttResponse.Data.Add(item.Name, item.LastWriteTime.ToString());
		}
		if (!mqttResponse.Data.Any())
		{
			mqttResponse.Error = "There's no file found from given date. " + mqttResponse.Error;
		}
		_logger.LogInfoWithSource("Returning peeked file list: " + mqttResponse.ToJson(), "PeekRequestedFilessAsync");
		return mqttResponse;
	}

	public async Task<MqttResponse<string>> GetKioskFileAsync(KioskFileRequest kioskFileRequest)
	{
		MqttResponse<string> mqttResponse = new MqttResponse<string>
		{
			Data = kioskFileRequest.S3PreSignedUrl
		};
		string path = await GetRequestedLogPath(kioskFileRequest.FileType) + "\\" + kioskFileRequest.FileName;
		HttpWebResponse httpWebResponse = new HttpWebResponse();
		try
		{
			HttpWebRequest httpWebRequest = WebRequest.Create(kioskFileRequest.S3PreSignedUrl) as HttpWebRequest;
			httpWebRequest.Method = "PUT";
			using (Stream stream = httpWebRequest.GetRequestStream())
			{
				byte[] array = new byte[8000];
				using FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				int count;
				while ((count = fileStream.Read(array, 0, array.Length)) > 0)
				{
					stream.Write(array, 0, count);
				}
			}
			httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error occured while trying to upload requested file to S3. Request Model: " + kioskFileRequest.ToJson());
			mqttResponse.Error = $"Error occured while uploading requested file to S3. Error: {ex}";
		}
		finally
		{
			httpWebResponse.Dispose();
		}
		_logger.LogInfoWithSource("Returning mqttresponse: " + mqttResponse.ToJson(), "GetKioskFileAsync");
		return mqttResponse;
	}

	private async Task<string> GetRequestedLogPath(FileTypeEnum logType)
	{
		string result = string.Empty;
		switch (logType)
		{
		case FileTypeEnum.DeviceServiceLogs:
			result = "C:\\Program Files\\Redbox\\REDS\\DeviceService\\logs";
			break;
		case FileTypeEnum.HALLogs:
			result = "C:\\Program Files\\Redbox\\KioskLogs\\Service";
			break;
		case FileTypeEnum.KioskEngineLogs:
			result = "C:\\Program Files\\Redbox\\reds\\Kiosk Engine\\logs";
			break;
		case FileTypeEnum.KioskClientServiceLogs:
			result = "C:\\ProgramData\\Redbox\\KioskClient\\Logs";
			break;
		case FileTypeEnum.UpdateClientServiceLogs:
			result = "C:\\ProgramData\\Redbox\\UpdateClient\\Logs";
			break;
		case FileTypeEnum.KioskEngineAnalyticsSessions:
			result = "C:\\Program Files\\Redbox\\reds\\Kiosk Engine\\data\\AnalyticsSessions";
			break;
		case FileTypeEnum.KioskEngineDataDirectory:
			result = "C:\\Program Files\\Redbox\\reds\\Kiosk Engine\\data";
			break;
		}
		return result;
	}
}
