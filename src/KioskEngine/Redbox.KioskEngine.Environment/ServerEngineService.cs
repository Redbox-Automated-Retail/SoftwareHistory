using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ServerEngineService : IServerEngineService
{
	internal const int DefaultServerEngineServiceTimeout = 30000;

	internal const string DefaultServerEngineServiceUrl = "rcp://localhost:7008";

	public static ServerEngineService Instance => Singleton<ServerEngineService>.Instance;

	public string Url
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Local Services\\Server Engine", "ServerEngineServiceURL", "rcp://localhost:7008");
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Local Services\\Server Engine", "ServerEngineServiceURL", value);
			Reset();
		}
	}

	public int Timeout
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Local Services\\Server Engine", "ServerEngineServiceTimeout", 30000);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Local Services\\Server Engine", "ServerEngineServiceTimeout", value);
			Reset();
		}
	}

	public ErrorList Initialize()
	{
		ErrorList result = new ErrorList();
		ServiceLocator.Instance.AddService(typeof(IServerEngineService), Instance);
		return result;
	}

	public void Reset()
	{
	}

	private ServerEngineService()
	{
	}
}
