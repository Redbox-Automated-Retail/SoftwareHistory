using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Belikov.GenuineChannels.Security;

[ComVisible(true)]
public class RSAUtility
{
	internal static FieldInfo _hKeyField;

	internal static object _hKeyFieldLock = new object();

	private static FieldInfo HKeyField
	{
		get
		{
			lock (_hKeyFieldLock)
			{
				if ((object)_hKeyField == null)
				{
					FieldInfo[] fields = typeof(RSACryptoServiceProvider).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					for (int i = 0; i < fields.Length; i++)
					{
						if (fields[i].Name == "_hKey")
						{
							_hKeyField = fields[i];
							break;
						}
					}
				}
				return _hKeyField;
			}
		}
	}

	[DllImport("Advapi32", SetLastError = true)]
	private static extern int CryptEncrypt(IntPtr hKey, IntPtr hHash, bool final, int dwFlags, byte[] pbData, ref int pdwDataLen, int dwBufLen);

	[DllImport("Advapi32", SetLastError = true)]
	private static extern int CryptDecrypt(IntPtr hKey, IntPtr hHash, bool final, int dwFlags, byte[] pbData, ref int pdwDataLen);

	public static byte[] Encrypt(RSACryptoServiceProvider rsa, byte[] source)
	{
		int num = rsa.KeySize / 8;
		int num2 = (source.Length / num + 1) * num;
		byte[] array = new byte[num2];
		Buffer.BlockCopy(source, 0, array, 0, source.Length);
		int pdwDataLen = source.Length;
		if (CryptEncrypt((IntPtr)HKeyField.GetValue(rsa), IntPtr.Zero, final: true, 0, array, ref pdwDataLen, array.Length) == 0)
		{
			throw new ApplicationException("RSA encryption failed with error code = " + Marshal.GetLastWin32Error());
		}
		if (pdwDataLen != array.Length)
		{
			byte[] array2 = new byte[pdwDataLen];
			Buffer.BlockCopy(array, 0, array2, 0, pdwDataLen);
			return array2;
		}
		return array;
	}

	public static byte[] Decrypt(RSACryptoServiceProvider rsa, byte[] source)
	{
		byte[] array = new byte[source.Length];
		Buffer.BlockCopy(source, 0, array, 0, source.Length);
		int pdwDataLen = source.Length;
		if (CryptDecrypt((IntPtr)HKeyField.GetValue(rsa), IntPtr.Zero, final: true, 0, array, ref pdwDataLen) == 0)
		{
			throw new ApplicationException("RSA encryption failed with error code = " + Marshal.GetLastWin32Error());
		}
		if (pdwDataLen != array.Length)
		{
			byte[] array2 = new byte[pdwDataLen];
			Buffer.BlockCopy(array, 0, array2, 0, pdwDataLen);
			return array2;
		}
		return array;
	}
}
