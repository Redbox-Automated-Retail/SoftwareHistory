using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.HAL.Core;

internal static class ByteArrayExtensions
{
	private static readonly byte[] m_keyValue = new Guid("{776DA6AF-3033-43ee-B379-2D4F28B5F1FC}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{F375D7E0-4572-4518-9C2F-E8F022F42AA7}").ToByteArray();

	public static void WriteToFile(this byte[] buffer, string fileName)
	{
		if (buffer == null || fileName == null)
		{
			return;
		}
		using FileStream fileStream = StreamBase.CreateFile(fileName);
		fileStream.Write(buffer, 0, buffer.Length);
		fileStream.Flush();
		fileStream.Close();
	}

	public static byte[] Compress(this byte[] buffer, CompressionType type)
	{
		return CompressionAlgorithm.GetAlgorithm(type).Compress(buffer);
	}

	public static byte[] Decompress(this byte[] buffer, CompressionType type)
	{
		return CompressionAlgorithm.GetAlgorithm(type).Decompress(buffer);
	}

	public static byte[] Encrypt(this byte[] inputArray)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = StreamBase.New();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	public static byte[] Decrypt(this byte[] inputArray)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateDecryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = StreamBase.New();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	public static string ToBase64(this byte[] inputArray)
	{
		return Convert.ToBase64String(inputArray);
	}

	public static int IndexOf(this byte[] array, byte b)
	{
		if (array == null || array.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == b)
			{
				return i;
			}
		}
		return -1;
	}

	public static string AsString(this byte[] array)
	{
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		foreach (byte b in array)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}
}
