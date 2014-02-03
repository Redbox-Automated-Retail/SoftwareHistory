using System;
using System.Runtime.InteropServices;

namespace Redbox.Lua;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate int LuaCFunctionDelegate(IntPtr luaState);
