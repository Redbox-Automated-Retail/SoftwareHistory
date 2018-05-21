using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class ScannerDeviceFactory : IScannerDeviceService, IConfigurationObserver
{
	private IScannerDevice m_configuredDevice;

	private ScannerServices ConfiguredService = ScannerServices.Emulated;

	private readonly List<IConfigurationObserver> ConfigurableScanners = new List<IConfigurationObserver>();

	private readonly CortexService CortexDevice;

	private readonly LegacyScanner LegacyDevice;

	private readonly NilScanner m_nilInstance = new NilScanner();

	public DateTime? IRHardwareInstall
	{
		get
		{
			string iRHardwareInstallDate = BarcodeConfiguration.Instance.IRHardwareInstallDate;
			if ("NONE" == iRHardwareInstallDate)
			{
				return null;
			}
			DateTime? dateTime = null;
			try
			{
				return DateTime.Parse(iRHardwareInstallDate);
			}
			catch
			{
				dateTime = null;
			}
			return dateTime;
		}
	}

	public IScannerDevice GetConfiguredDevice()
	{
		return m_configuredDevice;
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[ScannerFactory] Configuration load.");
		ConfiguredService = BarcodeConfiguration.Instance.ScannerService;
		SetConfiguredDevice();
		ConfigurableScanners.ForEach(delegate(IConfigurationObserver each)
		{
			each.NotifyConfigurationLoaded();
		});
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[ScannerFactory] Configuration change start.");
		m_configuredDevice.Stop();
		m_configuredDevice = m_nilInstance;
		ConfigurableScanners.ForEach(delegate(IConfigurationObserver each)
		{
			each.NotifyConfigurationChangeStart();
		});
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[ScannerFactory] Configuration change end.");
		ConfiguredService = BarcodeConfiguration.Instance.ScannerService;
		ConfigurableScanners.ForEach(delegate(IConfigurationObserver each)
		{
			each.NotifyConfigurationChangeEnd();
		});
		SetConfiguredDevice();
		m_configuredDevice.Start();
	}

	internal ScannerDeviceFactory()
	{
		m_configuredDevice = m_nilInstance;
		CortexDevice = new CortexService();
		LegacyDevice = new LegacyScanner();
		ConfigurableScanners.Add(CortexDevice);
		ConfigurableScanners.Add(LegacyDevice);
	}

	private void SetConfiguredDevice()
	{
		switch (ConfiguredService)
		{
		case ScannerServices.Legacy:
			m_configuredDevice = LegacyDevice;
			break;
		case ScannerServices.Cortex:
			m_configuredDevice = CortexDevice;
			break;
		default:
			m_configuredDevice = m_nilInstance;
			break;
		}
	}
}
