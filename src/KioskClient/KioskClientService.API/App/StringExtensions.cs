using System;
using System.Security.Cryptography;
using System.Text;

namespace KioskClientService.API.App;

public static class StringExtensions
{
	public static string ToSHA512Hash(this string input)
	{
		using SHA512 sHA = SHA512.Create();
		byte[] array = sHA.ComputeHash(Encoding.UTF8.GetBytes(input));
		return BitConverter.ToString(array).Replace("-", "").ToLower();
	}

	public static string MaskLogValue(this string source, int visibleChars)
	{
		if (!string.IsNullOrEmpty(source))
		{
			return new string('X', source.Length - visibleChars) + source.Substring(source.Length - visibleChars);
		}
		return "null";
	}
}
