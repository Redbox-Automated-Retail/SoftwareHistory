using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace UpdateClientService.API.Services.IoT.Certificate;

internal class CertificateService : ICertificateService
{
	private readonly ILogger<CertificateService> _logger;

	private readonly IIoTCertificateServiceApiClient _iotCertApiClient;

	private object _lockObj = new object();

	private const string IoTCertificateDataFileName = "iotcertificatedata.json";

	private string _ioTCertificateDataFilePath;

	private X509Certificate RootCa { get; set; }

	private X509Certificate2 DeviceCertPfx { get; set; }

	private string CertificateId { get; set; }

	private string IoTCertificateDataFilePath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_ioTCertificateDataFilePath))
			{
				return _ioTCertificateDataFilePath;
			}
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData, Environment.SpecialFolderOption.Create);
			string text = Path.Combine(folderPath, "Redbox\\UpdateClient\\IoT");
			Directory.CreateDirectory(text);
			string path = Path.Combine(text, "iotcertificatedata.json");
			_ioTCertificateDataFilePath = Path.GetFullPath(path);
			return _ioTCertificateDataFilePath;
		}
	}

	public CertificateService(ILogger<CertificateService> logger, IIoTCertificateServiceApiClient iotCertApiClient)
	{
		_logger = logger;
		_iotCertApiClient = iotCertApiClient;
		LoadCertificates();
	}

	public async Task<(X509Certificate rootCa, X509Certificate2 deviceCertPfx)> GetCertificatesAsync()
	{
		if (DeviceCertPfx == null || RootCa == null || CertificateId == null)
		{
			await WriteIoTCertificateDataFileAsync(await _iotCertApiClient.GetCertificates());
			LoadCertificates();
		}
		return (rootCa: RootCa, deviceCertPfx: DeviceCertPfx);
	}

	public async Task<(X509Certificate rootCa, X509Certificate2 deviceCertPfx)> GenerateNewCertificatesAsync()
	{
		await WriteIoTCertificateDataFileAsync(await _iotCertApiClient.GenerateNewCertificates());
		LoadCertificates();
		return (rootCa: RootCa, deviceCertPfx: DeviceCertPfx);
	}

	public async Task DeleteLocalCertsIfNotValidAsync()
	{
		Monitor.Enter(_lockObj);
		try
		{
			if (string.IsNullOrWhiteSpace(CertificateId))
			{
				_logger.LogInfoWithSource($"CertificateId was null not trying to delete local certs, DeviceCertPfx: {DeviceCertPfx}, RootCa: {RootCa}", "DeleteLocalCertsIfNotValidAsync");
			}
			else if (!_iotCertApiClient.IsCertificateValid(CertificateId).GetAwaiter().GetResult())
			{
				_logger.LogInfoWithSource("Deleting the certificateData file", "DeleteLocalCertsIfNotValidAsync");
				File.Delete(IoTCertificateDataFilePath);
				DeviceCertPfx = null;
				RootCa = null;
				CertificateId = null;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured, deleting the local certs");
			File.Delete(IoTCertificateDataFilePath);
		}
		finally
		{
			Monitor.Exit(_lockObj);
		}
	}

	private void LoadCertificates()
	{
		Monitor.Enter(_lockObj);
		try
		{
			string value = File.ReadAllText(IoTCertificateDataFilePath);
			IoTCertificateServiceResponse ioTCertificateServiceResponse = JsonConvert.DeserializeObject<IoTCertificateServiceResponse>(value);
			RootCa = new X509Certificate(Encoding.ASCII.GetBytes(ioTCertificateServiceResponse.RootCa));
			DeviceCertPfx = new X509Certificate2(Convert.FromBase64String(ioTCertificateServiceResponse.DeviceCertPfxBase64), "p");
			CertificateId = ioTCertificateServiceResponse.CertificateId;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "LoadCertificates");
		}
		finally
		{
			Monitor.Exit(_lockObj);
		}
	}

	private async Task WriteIoTCertificateDataFileAsync(IoTCertificateServiceResponse response)
	{
		Monitor.Enter(_lockObj);
		try
		{
			if (response == null)
			{
				_logger.LogInfoWithSource("Response was null, skipping writing certificates", "WriteIoTCertificateDataFileAsync");
			}
			else
			{
				File.WriteAllText(IoTCertificateDataFilePath, response.ToJson());
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Response: " + response.ToJson());
		}
		finally
		{
			Monitor.Exit(_lockObj);
		}
	}
}
