using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Parameters;

[ComVisible(true)]
public class DefaultParameterProvider : MarshalByRefObject, IParameterProvider
{
	private object[] _readParameters;

	public object this[GenuineParameter genuineParameter]
	{
		get
		{
			return _readParameters[(int)genuineParameter];
		}
		set
		{
			_readParameters[(int)genuineParameter] = value;
		}
	}

	public DefaultParameterProvider()
	{
		_readParameters = new object[61];
		_readParameters[0] = 20000000;
		_readParameters[1] = 100;
		_readParameters[2] = 20000000;
		_readParameters[3] = false;
		_readParameters[4] = false;
		_readParameters[5] = TimeSpan.FromSeconds(120.0);
		_readParameters[6] = true;
		_readParameters[7] = TimeSpan.FromSeconds(120.0);
		_readParameters[8] = null;
		_readParameters[9] = null;
		_readParameters[10] = null;
		_readParameters[11] = TimeSpan.FromSeconds(100.0);
		_readParameters[12] = TimeSpan.FromSeconds(120.0);
		_readParameters[13] = TimeSpan.FromSeconds(15.0);
		_readParameters[14] = TimeSpan.FromSeconds(15.0);
		_readParameters[15] = TimeSpan.FromSeconds(40.0);
		_readParameters[16] = TimeSpan.FromSeconds(180.0);
		_readParameters[17] = 180;
		_readParameters[18] = TimeSpan.FromMilliseconds(500.0);
		_readParameters[19] = 64000;
		_readParameters[20] = true;
		_readParameters[21] = false;
		_readParameters[22] = false;
		_readParameters[23] = false;
		_readParameters[24] = -1;
		_readParameters[25] = -1;
		_readParameters[26] = 300000;
		_readParameters[27] = TimeSpan.FromMilliseconds(4000.0);
		_readParameters[28] = false;
		_readParameters[31] = 64000;
		_readParameters[32] = 450;
		_readParameters[33] = 4500;
		_readParameters[35] = TimeSpan.FromMinutes(3.0);
		_readParameters[42] = false;
		_readParameters[43] = false;
		_readParameters[44] = false;
		_readParameters[45] = false;
		_readParameters[46] = false;
		_readParameters[47] = 100000;
		_readParameters[48] = true;
		_readParameters[49] = false;
		_readParameters[50] = false;
		_readParameters[51] = TimeSpan.FromSeconds(15.0);
		_readParameters[52] = TimeSpan.FromSeconds(180.0);
		_readParameters[54] = TimeSpan.FromSeconds(60.0);
		_readParameters[55] = true;
		_readParameters[56] = 0;
		_readParameters[57] = string.Empty;
		_readParameters[58] = "C1E1M2I3S1B2T1X1H1A1D2V1L0N1";
		_readParameters[59] = 1;
	}
}
