using System.Security.Cryptography;
using System.Text;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

internal class ZeroProofAuthorizationUtility
{
	private ZeroProofAuthorizationUtility()
	{
	}

	public static byte[] GenerateArbitrarySequence(int size)
	{
		RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
		byte[] array = new byte[size];
		randomNumberGenerator.GetNonZeroBytes(array);
		return array;
	}

	public static byte[] GeneratePasswordBasedSequence(string password, byte[] salt, int size)
	{
		PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(password, salt);
		return passwordDeriveBytes.GetBytes(size);
	}

	public static byte[] CalculateDefaultKeyedHash(string password, byte[] salt)
	{
		HMACSHA1 hMACSHA = new HMACSHA1();
		hMACSHA.Key = salt;
		return hMACSHA.ComputeHash(Encoding.Unicode.GetBytes(password));
	}

	public static bool CompareBuffers(byte[] first, byte[] second, int size)
	{
		if (first.Length < size || second.Length < size)
		{
			return false;
		}
		for (int i = 0; i < first.Length; i++)
		{
			if (first[i] != second[i])
			{
				return false;
			}
		}
		return true;
	}
}
