namespace Redbox.Lua;

internal interface ILuaDebugBreakpointFactory
{
	LuaDebugBreakpoint CreateBreakpoint(LuaDebugFile file, int line);
}
