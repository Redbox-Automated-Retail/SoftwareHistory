using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public class SspiApi
{
	[StructLayout(LayoutKind.Sequential)]
	public class SecHandle
	{
		public int dwLower;

		public int dwUpper;
	}

	private struct SEC_WINNT_AUTH_IDENTITY
	{
		public IntPtr User;

		public int UserLength;

		public IntPtr Domain;

		public int DomainLength;

		public IntPtr Password;

		public int PasswordLength;

		public int Flags;
	}

	[StructLayout(LayoutKind.Sequential)]
	private class SecBuffer
	{
		public int cbBuffer;

		public int BufferType;

		public IntPtr pvBuffer;
	}

	private struct SecBufferNative
	{
		public int cbBuffer;

		public int BufferType;

		public IntPtr pvBuffer;
	}

	private struct SecBufferDescNative
	{
		public int ulVersion;

		public int cBuffers;

		public IntPtr pBuffers;
	}

	public struct SecPkgContext_Sizes
	{
		public int cbMaxToken;

		public int cbMaxSignature;

		public int cbBlockSize;

		public int cbSecurityTrailer;
	}

	private enum EXTENDED_NAME_FORMAT
	{
		NameUnknown = 0,
		NameFullyQualifiedDN = 1,
		NameSamCompatible = 2,
		NameDisplay = 3,
		NameUniqueId = 6,
		NameCanonical = 7,
		NameUserPrincipal = 8,
		NameCanonicalEx = 9,
		NameServicePrincipal = 10,
		NameDnsDomain = 12
	}

	private const int SEC_WINNT_AUTH_IDENTITY_ANSI = 1;

	private const int SEC_WINNT_AUTH_IDENTITY_UNICODE = 2;

	public const int SECPKG_CRED_INBOUND = 1;

	public const int SECPKG_CRED_OUTBOUND = 2;

	public const int SECPKG_CRED_BOTH = 3;

	private const int SEC_E_OK = 0;

	private const int SEC_I_CONTINUE_NEEDED = 590610;

	private const int SEC_I_COMPLETE_NEEDED = 590611;

	private const int SEC_I_COMPLETE_AND_CONTINUE = 590612;

	private const int SEC_I_LOCAL_LOGON = 590613;

	private const int ISC_REQ_REPLAY_DETECT = 4;

	private const int ISC_REQ_SEQUENCE_DETECT = 8;

	private const int ISC_REQ_CONNECTION = 2048;

	private const int ISC_REQ_ALLOCATE_MEMORY = 256;

	private const int ISC_REQ_CONFIDENTIALITY = 16;

	private const int ISC_REQ_INTEGRITY = 65536;

	private const int ISC_REQ_MUTUAL_AUTH = 2;

	private const int ISC_REQ_DELEGATE = 1;

	private const int ASC_REQ_REPLAY_DETECT = 4;

	private const int ASC_REQ_SEQUENCE_DETECT = 8;

	private const int ASC_REQ_CONNECTION = 2048;

	private const int ASC_REQ_ALLOCATE_MEMORY = 256;

	private const int ASC_REQ_CONFIDENTIALITY = 16;

	private const int ASC_REQ_INTEGRITY = 131072;

	private const int ASC_REQ_MUTUAL_AUTH = 2;

	private const int ASC_REQ_DELEGATE = 1;

	private const int SECURITY_NATIVE_DREP = 16;

	private const int SECBUFFER_EMPTY = 0;

	private const int SECBUFFER_DATA = 1;

	private const int SECBUFFER_TOKEN = 2;

	private const int SECBUFFER_PADDING = 9;

	private const int SECBUFFER_VERSION = 0;

	private const int SECPKG_ATTR_SIZES = 0;

	private static readonly bool _isNT;

	private static uint DUPLICATE_SAME_ACCESS;

	private static uint PROCESS_DUP_HANDLE;

	private static IntPtr sspiAuthIdentityPointer;

	private SspiApi()
	{
	}

	static SspiApi()
	{
		DUPLICATE_SAME_ACCESS = 2u;
		PROCESS_DUP_HANDLE = 64u;
		sspiAuthIdentityPointer = IntPtr.Zero;
		_isNT = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major == 4;
	}

	[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll")]
	private static extern bool DuplicateHandle(IntPtr sourceProcessHandle, IntPtr sourceHandle, IntPtr targetProcessHandle, out IntPtr targetHandle, uint desiredAccess, bool inheritHandle, uint dwOptions);

	[DllImport("kernel32.dll")]
	private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

	[DllImport("secur32", CharSet = CharSet.Auto, EntryPoint = "GetUserNameEx", SetLastError = true)]
	private static extern int GetUserNameEx__(int NameFormat, StringBuilder lpNameBuffer, ref int nSize);

	[DllImport("secur32", CharSet = CharSet.Auto, EntryPoint = "AcquireCredentialsHandle")]
	private static extern int AcquireCredentialsHandleIdentity__(IntPtr pszPrincipal, string pszPackage, int fCredentialUse, IntPtr pvLogonId, ref SEC_WINNT_AUTH_IDENTITY pAuthData, IntPtr pGetKeyFn, IntPtr pvGetKeyArgument, SecHandle phCredential, ref long ptsExpiry);

	[DllImport("secur32", CharSet = CharSet.Auto, EntryPoint = "AcquireCredentialsHandle")]
	private static extern int AcquireCredentialsHandleIdentityNull__(IntPtr pszPrincipal, string pszPackage, int fCredentialUse, IntPtr pvLogonId, IntPtr pAuthData, IntPtr pGetKeyFn, IntPtr pvGetKeyArgument, SecHandle phCredential, ref long ptsExpiry);

	[DllImport("secur32", EntryPoint = "FreeCredentialsHandle")]
	private static extern int FreeCredentialsHandle__(SecHandle phContext);

	[DllImport("secur32", CharSet = CharSet.Auto, EntryPoint = "InitializeSecurityContext")]
	private static extern int InitializeSecurityContext__(SecHandle phCredential, SecHandle phContext, string pszTargetName, int fContextReq, int Reserved1, int TargetDataRep, ref SecBufferDescNative pInput, int Reserved2, SecHandle phNewContext, ref SecBufferDescNative pOutput, ref int pfContextAttr, ref long ptsExpiry);

	[DllImport("secur32", EntryPoint = "DeleteSecurityContext")]
	private static extern int DeleteSecurityContext__(SecHandle phContext);

	[DllImport("secur32", EntryPoint = "FreeContextBuffer")]
	private static extern int FreeContextBuffer__(IntPtr pvContextBuffer);

	[DllImport("secur32", EntryPoint = "AcceptSecurityContext")]
	private static extern int AcceptSecurityContext__(SecHandle phCredential, SecHandle phContext, ref SecBufferDescNative pInput, int fContextReq, int TargetDataRep, SecHandle phNewContext, ref SecBufferDescNative pOutput, ref int pfContextAttr, ref long ptsExpiry);

	[DllImport("secur32", EntryPoint = "QueryContextAttributes")]
	private static extern int QueryContextAttributes__(SecHandle phContext, int ulAttribute, ref SecPkgContext_Sizes pBuffer);

	[DllImport("secur32", EntryPoint = "QuerySecurityContextToken")]
	private static extern int QuerySecurityContextToken__(SecHandle phContext, out IntPtr phToken);

	[DllImport("secur32", EntryPoint = "ImpersonateSecurityContext")]
	private static extern int ImpersonateSecurityContext__(SecHandle phContext);

	[DllImport("secur32", EntryPoint = "RevertSecurityContext")]
	private static extern int RevertSecurityContext__(SecHandle phContext);

	[DllImport("secur32", EntryPoint = "MakeSignature")]
	private static extern int MakeSignature__(SecHandle phContext, int fQOP, ref SecBufferDescNative pMessage, int MessageSeqNo);

	[DllImport("secur32", EntryPoint = "VerifySignature")]
	private static extern int VerifySignature__(SecHandle phContext, ref SecBufferDescNative pMessage, int MessageSeqNo, ref int pfQOP);

	[DllImport("secur32", EntryPoint = "EncryptMessage")]
	private static extern int EncryptMessage__(SecHandle phContext, int pfQOP, ref SecBufferDescNative pMessage, int MessageSeqNo);

	[DllImport("secur32", EntryPoint = "DecryptMessage")]
	private static extern int DecryptMessage__(SecHandle phContext, ref SecBufferDescNative pMessage, int MessageSeqNo, ref int pfQOP);

	[DllImport("security", CharSet = CharSet.Unicode, EntryPoint = "GetUserNameEx", SetLastError = true)]
	private static extern int GetUserNameEx_NT(int NameFormat, StringBuilder lpNameBuffer, ref int nSize);

	[DllImport("security", CharSet = CharSet.Auto, EntryPoint = "AcquireCredentialsHandle")]
	private static extern int AcquireCredentialsHandleIdentity_NT(IntPtr pszPrincipal, string pszPackage, int fCredentialUse, IntPtr pvLogonId, ref SEC_WINNT_AUTH_IDENTITY pAuthData, IntPtr pGetKeyFn, IntPtr pvGetKeyArgument, SecHandle phCredential, ref long ptsExpiry);

	[DllImport("security", CharSet = CharSet.Auto, EntryPoint = "AcquireCredentialsHandle")]
	private static extern int AcquireCredentialsHandleIdentityNull_NT(IntPtr pszPrincipal, string pszPackage, int fCredentialUse, IntPtr pvLogonId, IntPtr pAuthData, IntPtr pGetKeyFn, IntPtr pvGetKeyArgument, SecHandle phCredential, ref long ptsExpiry);

	[DllImport("security", EntryPoint = "FreeCredentialsHandle")]
	private static extern int FreeCredentialsHandle_NT(SecHandle phContext);

	[DllImport("security", CharSet = CharSet.Auto, EntryPoint = "InitializeSecurityContext")]
	private static extern int InitializeSecurityContext_NT(SecHandle phCredential, SecHandle phContext, string pszTargetName, int fContextReq, int Reserved1, int TargetDataRep, ref SecBufferDescNative pInput, int Reserved2, SecHandle phNewContext, ref SecBufferDescNative pOutput, ref int pfContextAttr, ref long ptsExpiry);

	[DllImport("security", EntryPoint = "DeleteSecurityContext")]
	private static extern int DeleteSecurityContext_NT(SecHandle phContext);

	[DllImport("security", EntryPoint = "FreeContextBuffer")]
	private static extern int FreeContextBuffer_NT(IntPtr pvContextBuffer);

	[DllImport("security", CharSet = CharSet.Auto, EntryPoint = "AcceptSecurityContext")]
	private static extern int AcceptSecurityContext_NT(SecHandle phCredential, SecHandle phContext, ref SecBufferDescNative pInput, int fContextReq, int TargetDataRep, SecHandle phNewContext, ref SecBufferDescNative pOutput, ref int pfContextAttr, ref long ptsExpiry);

	[DllImport("security", EntryPoint = "QueryContextAttributes")]
	private static extern int QueryContextAttributes_NT(SecHandle phContext, int ulAttribute, ref SecPkgContext_Sizes pBuffer);

	[DllImport("security", EntryPoint = "QuerySecurityContextToken")]
	private static extern int QuerySecurityContextToken_NT(SecHandle phContext, out IntPtr phToken);

	[DllImport("security", EntryPoint = "ImpersonateSecurityContext")]
	private static extern int ImpersonateSecurityContext_NT(SecHandle phContext);

	[DllImport("security", EntryPoint = "RevertSecurityContext")]
	private static extern int RevertSecurityContext_NT(SecHandle phContext);

	[DllImport("security", EntryPoint = "MakeSignature")]
	private static extern int MakeSignature_NT(SecHandle phContext, int fQOP, ref SecBufferDescNative pMessage, int MessageSeqNo);

	[DllImport("security", EntryPoint = "VerifySignature")]
	private static extern int VerifySignature_NT(SecHandle phContext, ref SecBufferDescNative pMessage, int MessageSeqNo, ref int pfQOP);

	[DllImport("security", EntryPoint = "EncryptMessage")]
	private static extern int EncryptMessage_NT(SecHandle phContext, int pfQOP, ref SecBufferDescNative pMessage, int MessageSeqNo);

	[DllImport("security", EntryPoint = "DecryptMessage")]
	private static extern int DecryptMessage_NT(SecHandle phContext, ref SecBufferDescNative pMessage, int MessageSeqNo, ref int pfQOP);

	public static string GetUserNameEx()
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		int nSize = stringBuilder.Capacity;
		if (((!_isNT) ? GetUserNameEx__(2, stringBuilder, ref nSize) : GetUserNameEx_NT(2, stringBuilder, ref nSize)) == 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(Marshal.GetLastWin32Error());
		}
		return stringBuilder.ToString();
	}

	public static void AcquireCredentialsHandle(NetworkCredential authIdentity, string packageName, int fCredentialUse, SecHandle credHandle, ref long ptsExpiry)
	{
		SEC_WINNT_AUTH_IDENTITY pAuthData = default(SEC_WINNT_AUTH_IDENTITY);
		try
		{
			if (authIdentity != null)
			{
				pAuthData.Domain = Marshal.StringToCoTaskMemAuto(authIdentity.Domain);
				pAuthData.User = Marshal.StringToCoTaskMemAuto(authIdentity.UserName);
				pAuthData.Password = Marshal.StringToCoTaskMemAuto(authIdentity.Password);
				pAuthData.DomainLength = authIdentity.Domain.Length;
				pAuthData.UserLength = authIdentity.UserName.Length;
				pAuthData.PasswordLength = authIdentity.Password.Length;
				pAuthData.Flags = ((Marshal.SystemDefaultCharSize != 2) ? 1 : 2);
			}
			int num = 0;
			num = ((authIdentity != null) ? ((!_isNT) ? AcquireCredentialsHandleIdentity__(IntPtr.Zero, packageName, fCredentialUse, IntPtr.Zero, ref pAuthData, IntPtr.Zero, IntPtr.Zero, credHandle, ref ptsExpiry) : AcquireCredentialsHandleIdentity_NT(IntPtr.Zero, packageName, fCredentialUse, IntPtr.Zero, ref pAuthData, IntPtr.Zero, IntPtr.Zero, credHandle, ref ptsExpiry)) : ((!_isNT) ? AcquireCredentialsHandleIdentityNull__(IntPtr.Zero, packageName, fCredentialUse, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, credHandle, ref ptsExpiry) : AcquireCredentialsHandleIdentityNull_NT(IntPtr.Zero, packageName, fCredentialUse, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, credHandle, ref ptsExpiry)));
			if (num != 0)
			{
				throw GenuineExceptions.Get_Windows_SspiError(num);
			}
		}
		finally
		{
			if (pAuthData.Domain != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(pAuthData.Domain);
			}
			if (pAuthData.User != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(pAuthData.User);
			}
			if (pAuthData.Password != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(pAuthData.Password);
			}
		}
	}

	public static bool InitializeSecurityContext(SecHandle credHandle, SecHandle inputPhContext, SecHandle outputPhContext, string targetName, SspiFeatureFlags requiredFeatures, ref long ptsExpiry, Stream receivedData, Stream outputStream)
	{
		bool flag = false;
		int num = 0;
		if (receivedData != null)
		{
			BinaryReader binaryReader = new BinaryReader(receivedData);
			num = binaryReader.ReadInt32();
		}
		SecBufferDescNative pInput = default(SecBufferDescNative);
		SecBuffer secBuffer = new SecBuffer();
		SecBufferDescNative pOutput = default(SecBufferDescNative);
		SecBuffer secBuffer2 = new SecBuffer();
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		try
		{
			intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SecBufferDescNative)));
			intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SecBufferDescNative)));
			pInput.ulVersion = 0;
			pInput.cBuffers = 1;
			pInput.pBuffers = intPtr;
			secBuffer.BufferType = 2;
			secBuffer.cbBuffer = num;
			if (num > 0)
			{
				secBuffer.pvBuffer = Marshal.AllocHGlobal(num);
				byte[] array = new byte[num];
				GenuineUtility.ReadDataFromStream(receivedData, array, 0, num);
				Marshal.Copy(array, 0, secBuffer.pvBuffer, num);
			}
			else
			{
				secBuffer.pvBuffer = IntPtr.Zero;
			}
			Marshal.StructureToPtr((object)secBuffer, intPtr, false);
			pOutput.ulVersion = 0;
			pOutput.cBuffers = 1;
			pOutput.pBuffers = intPtr2;
			secBuffer2.BufferType = 2;
			secBuffer2.cbBuffer = 0;
			secBuffer2.pvBuffer = IntPtr.Zero;
			Marshal.StructureToPtr((object)secBuffer2, intPtr2, false);
			int num2 = 2308;
			if ((requiredFeatures & SspiFeatureFlags.Encryption) != SspiFeatureFlags.None)
			{
				num2 |= 0x10;
			}
			if ((requiredFeatures & SspiFeatureFlags.Signing) != SspiFeatureFlags.None)
			{
				num2 |= 0x10000;
			}
			if ((requiredFeatures & SspiFeatureFlags.Delegation) != SspiFeatureFlags.None)
			{
				num2 |= 3;
			}
			int pfContextAttr = 0;
			int num3 = 0;
			num3 = ((!_isNT) ? InitializeSecurityContext__(credHandle, inputPhContext, targetName, num2, 0, 16, ref pInput, 0, outputPhContext, ref pOutput, ref pfContextAttr, ref ptsExpiry) : InitializeSecurityContext_NT(credHandle, inputPhContext, targetName, num2, 0, 16, ref pInput, 0, outputPhContext, ref pOutput, ref pfContextAttr, ref ptsExpiry));
			flag = num3 switch
			{
				0 => true, 
				590610 => false, 
				_ => throw GenuineExceptions.Get_Windows_SspiError(num3), 
			};
			if (flag)
			{
				if ((requiredFeatures & SspiFeatureFlags.Encryption) != SspiFeatureFlags.None && (pfContextAttr & 0x10) == 0)
				{
					throw GenuineExceptions.Get_Windows_SspiDidNotProvideRequestedFeature("SspiFeatureFlags.Encryption");
				}
				if ((requiredFeatures & SspiFeatureFlags.Signing) != SspiFeatureFlags.None && (pfContextAttr & 0x10000) == 0)
				{
					throw GenuineExceptions.Get_Windows_SspiDidNotProvideRequestedFeature("SspiFeatureFlags.Signing");
				}
				if ((requiredFeatures & SspiFeatureFlags.Delegation) != SspiFeatureFlags.None && (pfContextAttr & 1) == 0)
				{
					throw GenuineExceptions.Get_Windows_SspiDidNotProvideRequestedFeature("SspiFeatureFlags.Delegation");
				}
			}
			Marshal.PtrToStructure(intPtr2, (object)secBuffer2);
			if (secBuffer2.cbBuffer > 0)
			{
				byte[] array2 = new byte[secBuffer2.cbBuffer];
				Marshal.Copy(secBuffer2.pvBuffer, array2, 0, secBuffer2.cbBuffer);
				BinaryWriter binaryWriter = new BinaryWriter(outputStream);
				binaryWriter.Write(secBuffer2.cbBuffer);
				binaryWriter.Write(array2);
			}
		}
		finally
		{
			if (secBuffer.pvBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(secBuffer.pvBuffer);
			}
			if (secBuffer2.pvBuffer != IntPtr.Zero)
			{
				FreeContextBuffer(secBuffer2.pvBuffer);
			}
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (intPtr2 != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr2);
			}
		}
		return flag;
	}

	public static bool AcceptSecurityContext(SecHandle credHandle, SecHandle phContext, SspiFeatureFlags requiredFeatures, ref long ptsExpiry, Stream receivedData, Stream outputStream, bool firstCall)
	{
		bool flag = false;
		int num = 0;
		if (receivedData != null)
		{
			BinaryReader binaryReader = new BinaryReader(receivedData);
			num = binaryReader.ReadInt32();
		}
		SecBufferDescNative pInput = default(SecBufferDescNative);
		SecBuffer secBuffer = new SecBuffer();
		SecBufferDescNative pOutput = default(SecBufferDescNative);
		SecBuffer secBuffer2 = new SecBuffer();
		IntPtr intPtr = IntPtr.Zero;
		IntPtr intPtr2 = IntPtr.Zero;
		try
		{
			intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SecBufferDescNative)));
			intPtr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SecBufferDescNative)));
			pInput.ulVersion = 0;
			pInput.cBuffers = 1;
			pInput.pBuffers = intPtr;
			secBuffer.BufferType = 2;
			secBuffer.cbBuffer = num;
			if (num > 0)
			{
				secBuffer.pvBuffer = Marshal.AllocHGlobal(num);
				byte[] array = new byte[num];
				receivedData.Read(array, 0, num);
				Marshal.Copy(array, 0, secBuffer.pvBuffer, num);
			}
			else
			{
				secBuffer.pvBuffer = IntPtr.Zero;
			}
			Marshal.StructureToPtr((object)secBuffer, intPtr, false);
			pOutput.ulVersion = 0;
			pOutput.cBuffers = 1;
			pOutput.pBuffers = intPtr2;
			secBuffer2.BufferType = 2;
			secBuffer2.cbBuffer = 0;
			secBuffer2.pvBuffer = IntPtr.Zero;
			Marshal.StructureToPtr((object)secBuffer2, intPtr2, false);
			int num2 = 2308;
			if ((requiredFeatures & SspiFeatureFlags.Encryption) != SspiFeatureFlags.None)
			{
				num2 |= 0x10;
			}
			if ((requiredFeatures & SspiFeatureFlags.Signing) != SspiFeatureFlags.None)
			{
				num2 |= 0x20000;
			}
			if ((requiredFeatures & SspiFeatureFlags.Delegation) != SspiFeatureFlags.None)
			{
				num2 |= 3;
			}
			int pfContextAttr = 0;
			int num3 = 0;
			num3 = ((!_isNT) ? AcceptSecurityContext__(credHandle, firstCall ? null : phContext, ref pInput, num2, 16, phContext, ref pOutput, ref pfContextAttr, ref ptsExpiry) : AcceptSecurityContext_NT(credHandle, firstCall ? null : phContext, ref pInput, num2, 16, phContext, ref pOutput, ref pfContextAttr, ref ptsExpiry));
			flag = num3 switch
			{
				0 => true, 
				590610 => false, 
				_ => throw GenuineExceptions.Get_Windows_SspiError(num3), 
			};
			if (flag)
			{
				if ((requiredFeatures & SspiFeatureFlags.Encryption) != SspiFeatureFlags.None && (pfContextAttr & 0x10) == 0)
				{
					throw GenuineExceptions.Get_Windows_SspiDidNotProvideRequestedFeature("SspiFeatureFlags.Encryption");
				}
				if ((requiredFeatures & SspiFeatureFlags.Signing) != SspiFeatureFlags.None && (pfContextAttr & 0x20000) == 0)
				{
					throw GenuineExceptions.Get_Windows_SspiDidNotProvideRequestedFeature("SspiFeatureFlags.Signing");
				}
				if ((requiredFeatures & SspiFeatureFlags.Delegation) != SspiFeatureFlags.None && (pfContextAttr & 1) == 0)
				{
					throw GenuineExceptions.Get_Windows_SspiDidNotProvideRequestedFeature("SspiFeatureFlags.Delegation");
				}
			}
			Marshal.PtrToStructure(pOutput.pBuffers, (object)secBuffer2);
			if (secBuffer2.cbBuffer > 0)
			{
				byte[] array2 = new byte[secBuffer2.cbBuffer];
				Marshal.Copy(secBuffer2.pvBuffer, array2, 0, secBuffer2.cbBuffer);
				BinaryWriter binaryWriter = new BinaryWriter(outputStream);
				binaryWriter.Write(secBuffer2.cbBuffer);
				binaryWriter.Write(array2);
			}
		}
		finally
		{
			if (secBuffer.pvBuffer != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(secBuffer.pvBuffer);
			}
			if (secBuffer2.pvBuffer != IntPtr.Zero)
			{
				FreeContextBuffer(secBuffer2.pvBuffer);
			}
			if (intPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr);
			}
			if (intPtr2 != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(intPtr2);
			}
		}
		return flag;
	}

	public static void DeleteSecurityContext(SecHandle phContext)
	{
		if (_isNT)
		{
			DeleteSecurityContext_NT(phContext);
		}
		else
		{
			DeleteSecurityContext__(phContext);
		}
		phContext.dwLower = 0;
		phContext.dwUpper = 0;
	}

	public static void FreeCredentialsHandle(SecHandle credHandle)
	{
		if (_isNT)
		{
			FreeCredentialsHandle_NT(credHandle);
		}
		else
		{
			FreeCredentialsHandle__(credHandle);
		}
		credHandle.dwLower = 0;
		credHandle.dwUpper = 0;
	}

	public static void FreeContextBuffer(IntPtr pvContextBuffer)
	{
		if (_isNT)
		{
			FreeContextBuffer_NT(pvContextBuffer);
		}
		else
		{
			FreeContextBuffer__(pvContextBuffer);
		}
	}

	public static void QueryContextSizes(SecHandle phContext, ref SecPkgContext_Sizes secPkgContext_Sizes)
	{
		int num = 0;
		num = ((!_isNT) ? QueryContextAttributes__(phContext, 0, ref secPkgContext_Sizes) : QueryContextAttributes_NT(phContext, 0, ref secPkgContext_Sizes));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
	}

	public static WindowsIdentity QuerySecurityContextToken(SecHandle phContext)
	{
		int num = 0;
		IntPtr phToken = IntPtr.Zero;
		num = ((!_isNT) ? QuerySecurityContextToken__(phContext, out phToken) : QuerySecurityContextToken_NT(phContext, out phToken));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
		try
		{
			return new WindowsIdentity(phToken);
		}
		finally
		{
			CloseHandle(phToken);
		}
	}

	public static IntPtr CloneSecurityToken(SecHandle phContext, int remoteProcessId)
	{
		int num = 0;
		IntPtr phToken = IntPtr.Zero;
		num = ((!_isNT) ? QuerySecurityContextToken__(phContext, out phToken) : QuerySecurityContextToken_NT(phContext, out phToken));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
		try
		{
			return CloneSecurityToken(phToken, remoteProcessId);
		}
		finally
		{
			CloseHandle(phToken);
		}
	}

	public static IntPtr CloneSecurityToken(IntPtr phToken, int remoteProcessId)
	{
		IntPtr zero = IntPtr.Zero;
		IntPtr zero2 = IntPtr.Zero;
		IntPtr intPtr = OpenProcess(PROCESS_DUP_HANDLE, bInheritHandle: true, remoteProcessId);
		if (intPtr == IntPtr.Zero)
		{
			int hRForLastWin32Error = Marshal.GetHRForLastWin32Error();
			if (zero2 != IntPtr.Zero)
			{
				WindowsIdentity.Impersonate(zero2);
			}
			Marshal.ThrowExceptionForHR(hRForLastWin32Error);
		}
		try
		{
			if (zero2 != IntPtr.Zero)
			{
				WindowsIdentity.Impersonate(zero2);
			}
			IntPtr targetHandle = IntPtr.Zero;
			if (!DuplicateHandle((IntPtr)(-1), phToken, intPtr, out targetHandle, 0u, inheritHandle: true, DUPLICATE_SAME_ACCESS))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			return targetHandle;
		}
		finally
		{
			CloseHandle(intPtr);
		}
	}

	public static void CloseClonedSecurityToken(IntPtr phToken)
	{
		if (!CloseHandle(phToken))
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}
	}

	public static void ImpersonateSecurityContext(SecHandle phContext)
	{
		int num = 0;
		num = ((!_isNT) ? ImpersonateSecurityContext__(phContext) : ImpersonateSecurityContext_NT(phContext));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
	}

	public static void RevertSecurityContext(SecHandle phContext)
	{
		int num = 0;
		num = ((!_isNT) ? RevertSecurityContext__(phContext) : RevertSecurityContext_NT(phContext));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
	}

	public static void MakeSignature(SecHandle phContext, byte[] contentBuffer, BinaryWriter outputSignature, ref SecPkgContext_Sizes secPkgContext_Sizes)
	{
		SecBufferDescNative pMessage = default(SecBufferDescNative);
		SecBufferNative[] array = new SecBufferNative[2];
		byte[] array2 = new byte[secPkgContext_Sizes.cbMaxSignature];
		using GCHandleKeeper gCHandleKeeper = new GCHandleKeeper(array2, GCHandleType.Pinned);
		array[0].BufferType = 2;
		array[0].cbBuffer = secPkgContext_Sizes.cbMaxSignature;
		array[0].pvBuffer = gCHandleKeeper.GCHandle.AddrOfPinnedObject();
		array[1].BufferType = 1;
		array[1].cbBuffer = contentBuffer.Length;
		using GCHandleKeeper gCHandleKeeper2 = new GCHandleKeeper(contentBuffer, GCHandleType.Pinned);
		array[1].pvBuffer = gCHandleKeeper2.GCHandle.AddrOfPinnedObject();
		pMessage.ulVersion = 0;
		pMessage.cBuffers = 2;
		using GCHandleKeeper gCHandleKeeper3 = new GCHandleKeeper(array, GCHandleType.Pinned);
		pMessage.pBuffers = gCHandleKeeper3.GCHandle.AddrOfPinnedObject();
		int num = 0;
		num = ((!_isNT) ? MakeSignature__(phContext, 0, ref pMessage, 0) : MakeSignature_NT(phContext, 0, ref pMessage, 0));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
		outputSignature.Write(array2, 0, array[0].cbBuffer);
	}

	public static void VerifySignature(SecHandle phContext, byte[] contentBuffer, byte[] signature)
	{
		SecBufferDescNative pMessage = default(SecBufferDescNative);
		SecBufferNative[] array = new SecBufferNative[2];
		using GCHandleKeeper gCHandleKeeper = new GCHandleKeeper(signature, GCHandleType.Pinned);
		array[0].BufferType = 2;
		array[0].cbBuffer = signature.Length;
		array[0].pvBuffer = gCHandleKeeper.GCHandle.AddrOfPinnedObject();
		using GCHandleKeeper gCHandleKeeper2 = new GCHandleKeeper(contentBuffer, GCHandleType.Pinned);
		array[1].BufferType = 1;
		array[1].cbBuffer = contentBuffer.Length;
		array[1].pvBuffer = gCHandleKeeper2.GCHandle.AddrOfPinnedObject();
		using GCHandleKeeper gCHandleKeeper3 = new GCHandleKeeper(array, GCHandleType.Pinned);
		pMessage.ulVersion = 0;
		pMessage.cBuffers = 2;
		pMessage.pBuffers = gCHandleKeeper3.GCHandle.AddrOfPinnedObject();
		int num = 0;
		int pfQOP = 0;
		num = ((!_isNT) ? VerifySignature__(phContext, ref pMessage, 0, ref pfQOP) : VerifySignature_NT(phContext, ref pMessage, 0, ref pfQOP));
		if (num != 0)
		{
			throw GenuineExceptions.Get_Windows_SspiError(num);
		}
	}

	public static void EncryptMessage(SecHandle phContext, Stream sourceContent, BinaryWriter outputContent, ref SecPkgContext_Sizes secPkgContext_Sizes)
	{
		SecBufferDescNative pMessage = default(SecBufferDescNative);
		SecBufferNative[] array = new SecBufferNative[3];
		byte[] array2 = new byte[secPkgContext_Sizes.cbSecurityTrailer];
		byte[] array3 = new byte[(int)sourceContent.Length];
		byte[] array4 = new byte[secPkgContext_Sizes.cbBlockSize];
		GenuineUtility.ReadDataFromStream(sourceContent, array3, 0, array3.Length);
		using (GCHandleKeeper gCHandleKeeper = new GCHandleKeeper(array2, GCHandleType.Pinned))
		{
			array[0].BufferType = 2;
			array[0].cbBuffer = array2.Length;
			array[0].pvBuffer = gCHandleKeeper.GCHandle.AddrOfPinnedObject();
			using GCHandleKeeper gCHandleKeeper2 = new GCHandleKeeper(array3, GCHandleType.Pinned);
			array[1].BufferType = 1;
			array[1].cbBuffer = array3.Length;
			array[1].pvBuffer = gCHandleKeeper2.GCHandle.AddrOfPinnedObject();
			using GCHandleKeeper gCHandleKeeper3 = new GCHandleKeeper(array4, GCHandleType.Pinned);
			array[2].BufferType = 9;
			array[2].cbBuffer = array4.Length;
			array[2].pvBuffer = gCHandleKeeper3.GCHandle.AddrOfPinnedObject();
			pMessage.ulVersion = 0;
			pMessage.cBuffers = 3;
			using GCHandleKeeper gCHandleKeeper4 = new GCHandleKeeper(array, GCHandleType.Pinned);
			pMessage.pBuffers = gCHandleKeeper4.GCHandle.AddrOfPinnedObject();
			int num = 0;
			num = ((!_isNT) ? EncryptMessage__(phContext, 0, ref pMessage, 0) : EncryptMessage_NT(phContext, 0, ref pMessage, 0));
			if (num != 0)
			{
				throw GenuineExceptions.Get_Windows_SspiError(num);
			}
		}
		outputContent.Write(array[0].cbBuffer);
		outputContent.Write(array[1].cbBuffer);
		outputContent.Write(array[2].cbBuffer);
		outputContent.Write(array2, 0, array[0].cbBuffer);
		outputContent.Write(array3, 0, array[1].cbBuffer);
		outputContent.Write(array4, 0, array[2].cbBuffer);
	}

	public static Stream DecryptMessage(SecHandle phContext, BinaryReader sourceContent)
	{
		SecBufferDescNative pMessage = default(SecBufferDescNative);
		SecBufferNative[] array = new SecBufferNative[3];
		int num = sourceContent.ReadInt32();
		int num2 = sourceContent.ReadInt32();
		int num3 = sourceContent.ReadInt32();
		byte[] array2 = new byte[num];
		byte[] array3 = new byte[num2];
		byte[] array4 = new byte[num3];
		GenuineUtility.ReadDataFromStream(sourceContent.BaseStream, array2, 0, num);
		GenuineUtility.ReadDataFromStream(sourceContent.BaseStream, array3, 0, num2);
		GenuineUtility.ReadDataFromStream(sourceContent.BaseStream, array4, 0, num3);
		using (GCHandleKeeper gCHandleKeeper = new GCHandleKeeper(array2, GCHandleType.Pinned))
		{
			array[0].BufferType = 2;
			array[0].cbBuffer = num;
			array[0].pvBuffer = gCHandleKeeper.GCHandle.AddrOfPinnedObject();
			using GCHandleKeeper gCHandleKeeper2 = new GCHandleKeeper(array3, GCHandleType.Pinned);
			array[1].BufferType = 1;
			array[1].cbBuffer = num2;
			array[1].pvBuffer = gCHandleKeeper2.GCHandle.AddrOfPinnedObject();
			using GCHandleKeeper gCHandleKeeper3 = new GCHandleKeeper(array4, GCHandleType.Pinned);
			array[2].BufferType = 9;
			array[2].cbBuffer = num3;
			array[2].pvBuffer = gCHandleKeeper3.GCHandle.AddrOfPinnedObject();
			pMessage.ulVersion = 0;
			pMessage.cBuffers = 3;
			using GCHandleKeeper gCHandleKeeper4 = new GCHandleKeeper(array, GCHandleType.Pinned);
			pMessage.pBuffers = gCHandleKeeper4.GCHandle.AddrOfPinnedObject();
			int num4 = 0;
			int pfQOP = 0;
			num4 = ((!_isNT) ? DecryptMessage__(phContext, ref pMessage, 0, ref pfQOP) : DecryptMessage_NT(phContext, ref pMessage, 0, ref pfQOP));
			if (num4 != 0)
			{
				throw GenuineExceptions.Get_Windows_SspiError(num4);
			}
		}
		return new MemoryStream(array3, 0, array[1].cbBuffer, writable: false, publiclyVisible: true);
	}
}
