namespace Redbox.KioskEngine.ComponentModel;

public interface IServiceProxy
{
	T GetProperty<T>(string name);

	void SetProperty<T>(string name, T value);

	object ExecuteMethod(string name, params object[] parms);
}
