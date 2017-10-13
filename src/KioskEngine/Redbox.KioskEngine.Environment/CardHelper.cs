using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Redbox.Core;

namespace Redbox.KioskEngine.Environment;

public static class CardHelper
{
	public static string GenerateCardId(SecureString cardNumber)
	{
		SHA256Managed sHA256Managed = new SHA256Managed();
		string result = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(ExtractFromSecure(cardNumber)))));
		sHA256Managed.Clear();
		return result;
	}

	public static SecureString CopyToSecure(string value)
	{
		SecureString secureString = new SecureString();
		char[] array = value.ToCharArray();
		foreach (char c in array)
		{
			secureString.AppendChar(c);
		}
		return secureString;
	}

	public static string ExtractFromSecure(SecureString value)
	{
		IntPtr intPtr = default(IntPtr);
		try
		{
			intPtr = Marshal.SecureStringToBSTR(value);
			return Marshal.PtrToStringBSTR(intPtr);
		}
		catch
		{
			return string.Empty;
		}
	}

	public static string EncryptCreditCardFromXmlKey(SecureString cardNumber, string keyXml)
	{
		if (cardNumber.Length == 0)
		{
			return null;
		}
		RSACryptoServiceProvider rSACryptoServiceProvider = null;
		try
		{
			rSACryptoServiceProvider = new RSACryptoServiceProvider();
			rSACryptoServiceProvider.FromXmlString(keyXml);
			return Convert.ToBase64String(rSACryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes(ExtractFromSecure(cardNumber)), fOAEP: false));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardHHelper.EncryptCreditCardFromXmlKey.", e);
		}
		finally
		{
			rSACryptoServiceProvider?.Clear();
		}
		return null;
	}

	public static string EncryptCreditCardFromCertificate(SecureString cardNumber, X509Certificate2 certificate)
	{
		if (cardNumber.Length == 0)
		{
			return null;
		}
		RSACryptoServiceProvider rSACryptoServiceProvider = null;
		try
		{
			rSACryptoServiceProvider = (RSACryptoServiceProvider)certificate.PublicKey.Key;
			return Convert.ToBase64String(rSACryptoServiceProvider.Encrypt(Encoding.ASCII.GetBytes(ExtractFromSecure(cardNumber)), fOAEP: false));
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in CardHHelper.EncryptCreditCardFromCertificate.", e);
		}
		finally
		{
			rSACryptoServiceProvider?.Clear();
		}
		return null;
	}
}
