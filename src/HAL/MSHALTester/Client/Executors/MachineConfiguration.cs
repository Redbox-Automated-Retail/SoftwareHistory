using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Client.Executors;

public sealed class MachineConfiguration : JobExecutor
{
	public bool IsDenseMachine { get; private set; }

	public bool LegacyCamera { get; private set; }

	public bool VMZConfigured { get; private set; }

	public bool DoorSensorsConfigured { get; private set; }

	public bool HasQuickReturn
	{
		get
		{
			throw new NotImplementedException("HasQuickReturn property deprecated.");
		}
	}

	public AirExchangerStatus AirExchangerStatus { get; private set; }

	public ExchangerFanStatus AirExchangerFanStatus { get; private set; }

	public bool AirExchangerConfigured { get; private set; }

	public bool HasFraudDevice { get; private set; }

	public string DoorSensorStatus { get; private set; }

	public bool HasAuxRelayBoard { get; private set; }

	public bool HasABEDevice { get; private set; }

	public DeviceStatus QuickReturnStatus { get; private set; }

	public bool HasRouterPowerRelay { get; private set; }

	public bool ArcusResetConfigured { get; private set; }

	public bool HasIRHardware => IRHardwareInstall.HasValue;

	public bool SupportsFraudScan { get; private set; }

	public DateTime? IRHardwareInstall { get; private set; }

	protected override string JobName => "kiosk-configuration-job";

	public MachineConfiguration(HardwareService service)
		: base(service)
	{
		IRHardwareInstall = null;
	}

	protected override void OnJobCompleted()
	{
		base.Results.ForEach(delegate(ProgramResult result)
		{
			switch (result.Code)
			{
			case "AuxRelayBoard":
				HasAuxRelayBoard = result.Message == "Configured";
				break;
			case "MachineConfig":
				IsDenseMachine = result.Message == "Dense";
				break;
			case "CameraConfig":
				LegacyCamera = result.Message == "Legacy";
				break;
			case "MerchMode":
				VMZConfigured = result.Message == "VMZ";
				break;
			case "DoorSensors":
				DoorSensorsConfigured = result.Message == "On";
				break;
			case "DoorSensorStatus":
				DoorSensorStatus = result.Message;
				break;
			case "AirExchangerConfigured":
				AirExchangerConfigured = result.Message == "Configured";
				break;
			case "FraudSensorEnabled":
				HasFraudDevice = result.Message == "Enabled";
				break;
			case "AbeDeviceStatus":
				HasABEDevice = result.Message.Equals("ATTACHED", StringComparison.CurrentCultureIgnoreCase);
				break;
			case "QuickReturnStatus":
				QuickReturnStatus = Enum<DeviceStatus>.ParseIgnoringCase(result.Message, DeviceStatus.Unknown);
				break;
			case "AirExchangerStatus":
				AirExchangerStatus = Enum<AirExchangerStatus>.ParseIgnoringCase(result.Message, AirExchangerStatus.NotConfigured);
				break;
			case "AirExchangerFanStatus":
				AirExchangerFanStatus = Enum<ExchangerFanStatus>.ParseIgnoringCase(result.Message, ExchangerFanStatus.On);
				break;
			case "RouterPowerRelay":
				HasRouterPowerRelay = result.Message == "Configured";
				break;
			case "SupportsArcusReset":
				ArcusResetConfigured = result.Message == "Configured";
				break;
			case "SupportsFraudScan":
				SupportsFraudScan = result.Message == "Configured";
				break;
			case "IRCameraHardwareInstall":
				if (result.Message != "NONE")
				{
					try
					{
						IRHardwareInstall = DateTime.Parse(result.Message);
						break;
					}
					catch
					{
						IRHardwareInstall = null;
						break;
					}
				}
				break;
			}
		});
	}
}
