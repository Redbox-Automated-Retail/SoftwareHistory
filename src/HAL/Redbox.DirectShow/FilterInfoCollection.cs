using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;
using Redbox.DirectShow.Interop;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.DirectShow;

public class FilterInfoCollection : CollectionBase
{
	private static bool? m_useOldOption = null;

	public FilterInfo this[int index] => (FilterInfo)base.InnerList[index];

	private static bool UseOld
	{
		get
		{
			if (m_useOldOption.HasValue)
			{
				return m_useOldOption.Value;
			}
			RegistryKey registryKey = null;
			try
			{
				registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\HAL");
				if (registryKey == null)
				{
					m_useOldOption = false;
					return m_useOldOption.Value;
				}
				object value = registryKey.GetValue("UseOldEnumerator");
				if (value == null)
				{
					m_useOldOption = false;
					return m_useOldOption.Value;
				}
				bool value2 = ConversionHelper.ChangeType<bool>(value);
				m_useOldOption = value2;
				return m_useOldOption.Value;
			}
			catch (Exception)
			{
				m_useOldOption = false;
				return m_useOldOption.Value;
			}
			finally
			{
				registryKey?.Close();
			}
		}
	}

	public FilterInfoCollection(Guid category)
	{
		CollectFilters(category);
	}

	private void CollectFilters(Guid category)
	{
		object obj = null;
		ICreateDevEnum createDevEnum = null;
		IEnumMoniker enumMoniker = null;
		IMoniker[] array = new IMoniker[1];
		try
		{
			Type typeFromCLSID = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum);
			if (typeFromCLSID == null)
			{
				throw new ApplicationException("Failed creating device enumerator");
			}
			obj = Activator.CreateInstance(typeFromCLSID);
			createDevEnum = (ICreateDevEnum)obj;
			if (createDevEnum.CreateClassEnumerator(ref category, out enumMoniker, 0) != 0)
			{
				throw new ApplicationException("No devices of the category");
			}
			IntPtr zero = IntPtr.Zero;
			while (enumMoniker.Next(1, array, zero) == 0)
			{
				if (array[0] == null)
				{
					if (UseOld)
					{
						break;
					}
				}
				else
				{
					FilterInfo value = new FilterInfo(array[0]);
					base.InnerList.Add(value);
					Marshal.ReleaseComObject(array[0]);
					array[0] = null;
				}
			}
			base.InnerList.Sort();
		}
		catch
		{
		}
		finally
		{
			createDevEnum = null;
			if (obj != null)
			{
				Marshal.ReleaseComObject(obj);
				obj = null;
			}
			if (enumMoniker != null)
			{
				Marshal.ReleaseComObject(enumMoniker);
				enumMoniker = null;
			}
			if (array[0] != null)
			{
				Marshal.ReleaseComObject(array[0]);
				array[0] = null;
			}
		}
	}
}
