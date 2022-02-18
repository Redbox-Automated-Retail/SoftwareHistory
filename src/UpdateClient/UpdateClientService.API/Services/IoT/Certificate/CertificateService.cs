using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UpdateClientService.API.Services.IoT.Security.Certificate;

namespace UpdateClientService.API.Services.IoT.Certificate;

internal class CertificateService : ICertificateService
{
	private readonly ILogger<CertificateService> _logger;

	private readonly IIoTCertificateServiceApiClient _iotCertApiClient;

	private readonly IStoreService _store;

	private readonly ISecurityService _securityService;

	private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private readonly int _lockWait = 2000;

	private string _encryptedThingType;

	private string _encryptedKioskId;

	private long _kioskId;

	private string _ioTCertificateServicePassword;

	private const string IoTCertificateDataFileName = "iotcertificatedata.json";

	private const string ThingType = "kiosks";

	private string _ioTCertificateDataFilePath;

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

	public CertificateService(ILogger<CertificateService> logger, IIoTCertificateServiceApiClient iotCertApiClient, IStoreService store, ISecurityService securityService)
	{
		_logger = logger;
		_iotCertApiClient = iotCertApiClient;
		_store = store;
		_securityService = securityService;
	}

	public async Task<IotCert> GetCertificateAsync(bool forceNew = false)
	{
		_logger.LogInfoWithSource($"force = {forceNew}", "GetCertificateAsync");
		try
		{
			long kioskId = _store.KioskId;
			if (kioskId <= 0)
			{
				_logger.LogErrorWithSource($"KioskId: {_store.KioskId} is not valid, no cert to return", "GetCertificateAsync");
				return null;
			}
			if (!(await UpdateIotData()))
			{
				_logger.LogErrorWithSource("IotData failed so we couldn't get certificate", "GetCertificateAsync");
				return null;
			}
			bool flag = IsExecutingAssemblyNewerThanCertificate();
			if (flag)
			{
				_logger.LogInfoWithSource("Executing Assembly is newer than IoT Certificate. Forcing creation of new certificate.", "GetCertificateAsync");
			}
			if (forceNew | flag)
			{
				if (!(await WriteIoTCertificateDataFileAsync(await _iotCertApiClient.GenerateNewCertificates(_encryptedKioskId, _encryptedThingType, _ioTCertificateServicePassword))))
				{
					return null;
				}
				return await LoadCertificate();
			}
			IotCert iotCert = await LoadCertificate();
			if (iotCert != null)
			{
				return iotCert;
			}
			if (!(await WriteIoTCertificateDataFileAsync(await _iotCertApiClient.GetCertificates(_encryptedKioskId, _encryptedThingType, _ioTCertificateServicePassword))))
			{
				return null;
			}
			return await LoadCertificate();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception running GetCertificateAsync");
			return null;
		}
	}

	public async Task Validate()
	{
		_logger.LogInfoWithSource("Attempting to run Validate", "Validate");
		try
		{
			if (!(await UpdateIotData()))
			{
				_logger.LogErrorWithSource("Validate failed because IotData was not valid", "Validate");
			}
			else if (!((await IsCertificateValid()) ?? true))
			{
				await DeleteLocalCertificate();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception running validate");
		}
	}

	private bool IsExecutingAssemblyNewerThanCertificate()
	{
		bool flag = false;
		_logger.LogInfoWithSource("Attempting to check if Executing Assembly is newer than cert", "IsExecutingAssemblyNewerThanCertificate");
		try
		{
			if (File.Exists(IoTCertificateDataFilePath))
			{
				DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(IoTCertificateDataFilePath);
				string location = Assembly.GetExecutingAssembly().Location;
				DateTime lastWriteTimeUtc2 = File.GetLastWriteTimeUtc(location);
				flag = lastWriteTimeUtc2 > lastWriteTimeUtc;
				if (flag)
				{
					_logger.LogInfoWithSource($"Assembly date {lastWriteTimeUtc2} is newer than certificate date {lastWriteTimeUtc}", "IsExecutingAssemblyNewerThanCertificate");
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while comparing certificate date with assembly date.");
		}
		return flag;
	}

	private async Task<bool> UpdateIotData()
	{
		_logger.LogInfoWithSource("Running UpdateIotData", "UpdateIotData");
		try
		{
			long kioskId = _store.KioskId;
			if (kioskId <= 0)
			{
				_logger.LogErrorWithSource($"KioskId: {_store.KioskId} is not valid", "UpdateIotData");
				_kioskId = 0L;
			}
			if (kioskId == _kioskId && !string.IsNullOrEmpty(_encryptedKioskId) && !string.IsNullOrEmpty(_ioTCertificateServicePassword) && !string.IsNullOrEmpty(_encryptedThingType))
			{
				_logger.LogInfoWithSource("IotData is already loaded", "UpdateIotData");
				return true;
			}
			_kioskId = kioskId;
			_encryptedKioskId = await _securityService.Encrypt(_store.KioskId.ToString());
			_encryptedThingType = await _securityService.Encrypt("kiosks");
			_ioTCertificateServicePassword = await _securityService.GetIoTCertServicePassword(_store.KioskId.ToString());
			_logger.LogInfoWithSource("Finished loading Iot data", "UpdateIotData");
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured loading IotData");
			return false;
		}
	}

	private async Task<bool?> IsCertificateValid()
	{
		_logger.LogInfoWithSource("Attempting to run IsCertificateValid", "IsCertificateValid");
		try
		{
			IotCert iotCert = await LoadCertificate();
			if (iotCert == null)
			{
				_logger.LogInfoWithSource("iIotCert is null, cert is not valid", "IsCertificateValid");
				return false;
			}
			string certificateId = await _securityService.Encrypt(iotCert.CertificateId);
			IsCertificateValidResponse isCertificateValidResponse = await _iotCertApiClient.IsCertificateValid(_encryptedKioskId, _encryptedThingType, _ioTCertificateServicePassword, certificateId);
			if (!isCertificateValidResponse.IsValid.HasValue)
			{
				_logger.LogInfoWithSource("iotCertApiClient.IsCertificateValid was null, call failed can't tell if valid or not", "IsCertificateValid");
				return null;
			}
			if (isCertificateValidResponse.IsValid.Value)
			{
				_logger.LogInfoWithSource("iotCertApiClient.IsCertificateValid was true, cert is valid", "IsCertificateValid");
				return true;
			}
			_logger.LogInfoWithSource("iotCertApiClient.IsCertificateValid was false, cert is not valid", "IsCertificateValid");
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured checking to see if the certificate is valid");
			return null;
		}
	}

	private async Task DeleteLocalCertificate()
	{
		_logger.LogInfoWithSource("Deleting the local certificate file: " + IoTCertificateDataFilePath, "DeleteLocalCertificate");
		if (!(await _lock.WaitAsync(_lockWait)))
		{
			_logger.LogErrorWithSource("Lock failed, certificate not deleted", "DeleteLocalCertificate");
			return;
		}
		try
		{
			File.Delete(IoTCertificateDataFilePath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception occured deleting certificate file");
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<IotCert> LoadCertificate()
	{
		_logger.LogInfoWithSource("Attempting to load Certificate", "LoadCertificate");
		if (!(await _lock.WaitAsync(_lockWait)))
		{
			_logger.LogErrorWithSource("Lock failed, certificate cannot be loaded", "LoadCertificate");
			return null;
		}
		try
		{
			if (!File.Exists(IoTCertificateDataFilePath))
			{
				_logger.LogErrorWithSource("Certificate file does not exist: " + IoTCertificateDataFilePath, "LoadCertificate");
				return null;
			}
			string value = File.ReadAllText(IoTCertificateDataFilePath);
			IoTCertificateServiceResponse localCerts = JsonConvert.DeserializeObject<IoTCertificateServiceResponse>(value);
			string pfxCertPassword = await _securityService.GetCertificatePassword(_store.KioskId.ToString());
			return new IotCert(localCerts.DeviceCertPfxBase64, localCerts.RootCa, localCerts.CertificateId, pfxCertPassword);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception loading certificate");
			return null;
		}
		finally
		{
			_lock.Release();
		}
	}

	private async Task<bool> WriteIoTCertificateDataFileAsync(IoTCertificateServiceResponse response)
	{
		if (response == null)
		{
			_logger.LogInfoWithSource("Response was null or not successful, skipping writing certificates", "WriteIoTCertificateDataFileAsync");
			await DeleteLocalCertificate();
			return false;
		}
		_logger.LogInfoWithSource("Attempting to write certificate to a file", "WriteIoTCertificateDataFileAsync");
		if (!(await _lock.WaitAsync(_lockWait)))
		{
			_logger.LogErrorWithSource("Lock failed, certificate cannot be saved", "WriteIoTCertificateDataFileAsync");
			return false;
		}
		try
		{
			await File.WriteAllTextAsync(IoTCertificateDataFilePath, response.ToJson());
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Response: " + response.ToJson());
			return false;
		}
		finally
		{
			_lock.Release();
		}
	}
}
