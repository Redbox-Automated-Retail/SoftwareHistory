using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WUApiLib;

[ComImport]
[Guid("ED8BFE40-A60B-42EA-9652-817DFCFA23EC")]
[TypeLibType(4288)]
internal interface IWindowsDriverUpdateEntry
{
	[DispId(1610809345)]
	string DriverClass
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809345)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610809346)]
	string DriverHardwareID
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809346)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610809347)]
	string DriverManufacturer
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809347)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610809348)]
	string DriverModel
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809348)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610809349)]
	string DriverProvider
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809349)]
		[return: MarshalAs(UnmanagedType.BStr)]
		get;
	}

	[DispId(1610809350)]
	DateTime DriverVerDate
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809350)]
		get;
	}

	[DispId(1610809351)]
	int DeviceProblemNumber
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809351)]
		get;
	}

	[DispId(1610809352)]
	int DeviceStatus
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1610809352)]
		get;
	}
}
