using System.Collections;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.BrokerServices.Kernel;

public static class IRemoteServiceResultExtensions
{
	public static LuaTable AsTable(this IRemoteServiceResult result)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service.NewTable();
		luaTable["success"] = result.Success;
		LuaTable luaTable2 = (LuaTable)service.NewTable();
		luaTable2["hours"] = result.ExecutionTime.Hours;
		luaTable2["minutes"] = result.ExecutionTime.Minutes;
		luaTable2["seconds"] = result.ExecutionTime.Seconds;
		luaTable2["milliseconds"] = result.ExecutionTime.Milliseconds;
		luaTable["execution_time"] = luaTable2;
		LuaTable luaTable3 = (LuaTable)service.NewTable();
		foreach (string key in result.Properties.Keys)
		{
			object obj = result.Properties[key];
			if (!(obj is IEnumerable) || obj is string)
			{
				luaTable3[key] = obj;
			}
		}
		luaTable["properties"] = luaTable3;
		int num = 1;
		LuaTable luaTable4 = (LuaTable)service.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in result.Errors)
		{
			LuaTable luaTable5 = (LuaTable)service.NewTable();
			luaTable5["code"] = error.Code;
			luaTable5["is_warning"] = error.IsWarning;
			luaTable5["description"] = error.Description;
			luaTable5["details"] = error.Details;
			luaTable4[num++] = luaTable5;
		}
		luaTable["errors"] = luaTable4;
		return luaTable;
	}
}
