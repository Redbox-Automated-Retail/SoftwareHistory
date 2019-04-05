namespace Redbox.Rental.Model.KioskEvents;

public static class KioskEventSubType
{
	public static int KioskHealth_TouchScreenInactivity = 100;

	public static int KioskHealth_TouchScreenReset = 101;

	public static int KioskHealth_TouchScreenActivity = 102;

	public static int KioskHealth_ArcusReset = 200;

	public static int KisokHealth_ArcusResetVendStatus = 201;

	public static int KioskHealth_RouterIsOffline = 300;

	public static int KioskHealth_RouterReset = 301;

	public static int KioskHealth_RouterIsOnline = 302;

	public static int KioskHealth_HardwareStatNone = 400;

	public static int KioskHealth_HardwareStatArcus = 401;

	public static int KioskHealth_HardwareStatCamera = 402;

	public static int KioskHealth_HardwareStatTouchscreen = 403;

	public static int KioskHealth_HardwareStatControllerRestart = 404;

	public static int KioskHealth_HardwareStatRouterRecycle = 405;

	public static int KioskHealth_HardwareStatUnexpectedPowerLoss = 406;

	public static int KioskHealth_ViewTimeout = 500;

	public static int KioskHealth_ViewReset = 501;

	public static int KioskHealth_ViewActivity = 502;

	public static int KioskHealth_CCReaderInactivity = 600;

	public static int KioskHealth_CCReaderReadError = 601;

	public static int KioskHealth_CCReaderActivity = 602;
}
