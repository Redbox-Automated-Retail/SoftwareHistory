namespace Redbox.Lua;

internal interface ILuaDebugFileFactory
{
	LuaDebugFile CreateFile(LuaDebugger debugger, string fileName);
}
