using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class ServiceProxy : IServiceProxy
{
	public T GetProperty<T>(string name)
	{
		return name switch
		{
			"IsOnline" => (T)(object)ServiceLocator.Instance.GetService<IHealthServices>().IsOnline, 
			"IsStandAlone" => (T)(object)ServiceLocator.Instance.GetService<IStoreServices>().IsStandAlone, 
			"Url" => (T)(object)KioskServicesProxy.Instance.Url, 
			_ => default(T), 
		};
	}

	public void SetProperty<T>(string name, T value)
	{
	}

	public object ExecuteMethod(string name, params object[] parms)
	{
		if (!(name == "ExecuteMessage"))
		{
			return null;
		}
		return KioskServicesProxy.Instance.ExecuteMessage(parms[0]);
	}
}
