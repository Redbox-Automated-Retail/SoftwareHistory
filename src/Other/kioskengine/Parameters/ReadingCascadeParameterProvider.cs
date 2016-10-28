using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Parameters;

[ComVisible(true)]
public class ReadingCascadeParameterProvider : MarshalByRefObject, IParameterProvider
{
	private IParameterProvider _underlyingProvider;

	private object _underlyingProviderLock = new object();

	private object[] _readParameters;

	public IParameterProvider UnderlyingProvider
	{
		get
		{
			lock (_underlyingProviderLock)
			{
				return _underlyingProvider;
			}
		}
		set
		{
			lock (_underlyingProviderLock)
			{
				_underlyingProvider = value;
			}
		}
	}

	public object this[GenuineParameter genuineParameter]
	{
		get
		{
			if (_readParameters[(int)genuineParameter] != null)
			{
				return _readParameters[(int)genuineParameter];
			}
			return _underlyingProvider[genuineParameter];
		}
		set
		{
			_readParameters[(int)genuineParameter] = value;
		}
	}

	public ReadingCascadeParameterProvider(IDictionary parameters, IParameterProvider underlyingProvider)
	{
		_readParameters = new object[61];
		UnderlyingProvider = underlyingProvider;
		foreach (object parameter in parameters)
		{
			DictionaryEntry dictionaryEntry = (DictionaryEntry)parameter;
			switch (dictionaryEntry.Key.ToString().ToUpper())
			{
			case "MAXCONTENTSIZE":
				_readParameters[0] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "MAXQUEUEDITEMS":
				_readParameters[1] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "MAXTOTALSIZE":
				_readParameters[2] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "NOSIZECHECKING":
				_readParameters[3] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "COMPRESSION":
			case "COMPRESS":
				_readParameters[4] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "INVOCATIONTIMEOUT":
				_readParameters[5] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "SYNCRESPONSES":
				_readParameters[6] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "CONNECTTIMEOUT":
				_readParameters[7] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "SECURITYSESSIONFORPERSISTENTCONNECTIONS":
				_readParameters[8] = dictionaryEntry.Value.ToString();
				break;
			case "SECURITYSESSIONFORNAMEDCONNECTIONS":
				_readParameters[9] = dictionaryEntry.Value.ToString();
				break;
			case "SECURITYSESSIONFORINVOCATIONCONNECTIONS":
				_readParameters[10] = dictionaryEntry.Value.ToString();
				break;
			case "CLOSEPERSISTENTCONNECTIONAFTERINACTIVITY":
				_readParameters[11] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "CLOSENAMEDCONNECTIONAFTERINACTIVITY":
				_readParameters[12] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "CLOSEINVOCATIONCONNECTIONAFTERINACTIVITY":
				_readParameters[13] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "CLOSEONEWAYCONNECTIONAFTERINACTIVITY":
				_readParameters[14] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "PERSISTENTCONNECTIONSENDPINGAFTERINACTIVITY":
				_readParameters[15] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "MAXTIMESPANTORECONNECT":
				_readParameters[16] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "RECONNECTIONTRIES":
				_readParameters[17] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "SLEEPBETWEENRECONNECTIONS":
				_readParameters[18] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "TCPMAXSENDSIZE":
				_readParameters[19] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "TCPREADREQUESTBEFOREPROCESSING":
				_readParameters[20] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "TCPDONOTRESENDMESSAGES":
				_readParameters[21] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "TCPDISABLENAGGLING":
				_readParameters[22] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "TCPPREVENTDELAYEDACK":
				_readParameters[23] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "TCPRECEIVEBUFFERSIZE":
				_readParameters[24] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "TCPSENDBUFFERSIZE":
				_readParameters[25] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "SMSHARESIZE":
				_readParameters[26] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "SMSENDTIMEOUT":
				_readParameters[27] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "SMSESSIONLOCAL":
				_readParameters[28] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "UDPJOINTO":
				_readParameters[29] = dictionaryEntry.Value.ToString();
				break;
			case "UDPTTL":
				_readParameters[30] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "UDPRECEIVEBUFFER":
				_readParameters[31] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "UDPPACKETSIZE":
				_readParameters[32] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "UDPMTU":
				_readParameters[33] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "UDPMULTICASTTO":
				_readParameters[34] = dictionaryEntry.Value.ToString();
				break;
			case "UDPASSEMBLETIMESPAN":
				_readParameters[35] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "HTTPPROXYURI":
				_readParameters[36] = dictionaryEntry.Value.ToString();
				break;
			case "HTTPWEBUSERAGENT":
				_readParameters[37] = dictionaryEntry.Value.ToString();
				break;
			case "HTTPAUTHUSERNAME":
				_readParameters[38] = dictionaryEntry.Value.ToString();
				break;
			case "HTTPAUTHPASSWORD":
				_readParameters[39] = dictionaryEntry.Value.ToString();
				break;
			case "HTTPAUTHDOMAIN":
				_readParameters[40] = dictionaryEntry.Value.ToString();
				break;
			case "HTTPAUTHCREDENTIAL":
				_readParameters[41] = dictionaryEntry.Value as NetworkCredential;
				break;
			case "HTTPUSEGLOBALPROXY":
				_readParameters[42] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPBYPASSONLOCAL":
				_readParameters[43] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPUSEDEFAULTCREDENTIALS":
				_readParameters[44] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPALLOWWRITESTREAMBUFFERING":
				_readParameters[45] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPUNSAFECONNECTIONSHARING":
				_readParameters[46] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPRECOMMENDEDPACKETSIZE":
				_readParameters[47] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "HTTPKEEPALIVE":
				_readParameters[48] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPAUTHENTICATION":
				_readParameters[49] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPSTOREANDPROVIDEHTTPCONTEXT":
				_readParameters[50] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "HTTPWEBREQUESTINITIATIONTIMEOUT":
				_readParameters[51] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "HTTPASYNCHRONOUSREQUESTTIMEOUT":
				_readParameters[52] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "HTTPMIMEMEDIATYPE":
				_readParameters[53] = dictionaryEntry.Value.ToString();
				break;
			case "XHTTPREADHEADERTIMEOUT":
				_readParameters[54] = GenuineUtility.SafeConvertToTimeSpan(dictionaryEntry.Value);
				break;
			case "HOLDTHREADDURINGSECURITYSESSIONESTABLISHING":
				_readParameters[55] = GenuineUtility.SafeConvertToBool(dictionaryEntry.Value);
				break;
			case "ENABLEGLOBALLOGGINGTOMEMORY":
				_readParameters[56] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			case "ENABLEGLOBALLOGGINGTOFILE":
				_readParameters[57] = dictionaryEntry.Value.ToString();
				break;
			case "LOGGINGPARAMETERS":
				_readParameters[58] = dictionaryEntry.Value.ToString();
				break;
			case "COMPATIBILITYLEVEL":
				_readParameters[59] = GenuineUtility.SafeConvertToInt32(dictionaryEntry.Value);
				break;
			}
		}
	}
}
