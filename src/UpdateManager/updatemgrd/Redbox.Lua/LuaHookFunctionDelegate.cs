using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void LuaHookFunctionDelegate(IntPtr luaState, ref LuaDebug luaDebug);
