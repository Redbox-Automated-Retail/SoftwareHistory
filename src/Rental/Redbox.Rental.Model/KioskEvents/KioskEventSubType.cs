namespace Redbox.Rental.Model.KioskEvents;

public static class KioskEventSubType
{
	public const int KioskHealth_TouchScreenInactivity = 100;

	public const int KioskHealth_TouchScreenReset = 101;

	public const int KioskHealth_TouchScreenActivity = 102;

	public const int KioskHealth_ArcusReset = 200;

	public const int KisokHealth_ArcusResetVendStatus = 201;

	public const int KioskHealth_RouterIsOffline = 300;

	public const int KioskHealth_RouterReset = 301;

	public const int KioskHealth_RouterIsOnline = 302;

	public const int KioskHealth_HardwareStatNone = 400;

	public const int KioskHealth_HardwareStatArcus = 401;

	public const int KioskHealth_HardwareStatCamera = 402;

	public const int KioskHealth_HardwareStatTouchscreen = 403;

	public const int KioskHealth_HardwareStatControllerRestart = 404;

	public const int KioskHealth_HardwareStatRouterRecycle = 405;

	public const int KioskHealth_HardwareStatUnexpectedPowerLoss = 406;

	public const int KioskHealth_ViewTimeout = 500;

	public const int KioskHealth_ViewReset = 501;

	public const int KioskHealth_ViewActivity = 502;

	public const int KioskHealth_CCReaderInactivity = 600;

	public const int KioskHealth_CCReaderReadError = 601;

	public const int KioskHealth_CCReaderActivity = 602;

	public const int KioskHealth_EMVReaderTechnicalFallbackSession = 700;

	public const int KioskHealth_EMVReaderTechnicalFallbackSessionReset = 701;
}
