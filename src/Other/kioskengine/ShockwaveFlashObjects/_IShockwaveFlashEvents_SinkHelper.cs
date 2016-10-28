using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class _IShockwaveFlashEvents_SinkHelper : _IShockwaveFlashEvents
{
	public _IShockwaveFlashEvents_FlashCallEventHandler m_FlashCallDelegate;

	public _IShockwaveFlashEvents_FSCommandEventHandler m_FSCommandDelegate;

	public _IShockwaveFlashEvents_OnProgressEventHandler m_OnProgressDelegate;

	public _IShockwaveFlashEvents_OnReadyStateChangeEventHandler m_OnReadyStateChangeDelegate;

	public int m_dwCookie;

	public void FlashCall(string P_0)
	{
		if (m_FlashCallDelegate != null)
		{
			m_FlashCallDelegate(P_0);
		}
	}

	public void FSCommand(string P_0, string P_1)
	{
		if (m_FSCommandDelegate != null)
		{
			m_FSCommandDelegate(P_0, P_1);
		}
	}

	public void OnProgress(int P_0)
	{
		if (m_OnProgressDelegate != null)
		{
			m_OnProgressDelegate(P_0);
		}
	}

	public void OnReadyStateChange(int P_0)
	{
		if (m_OnReadyStateChangeDelegate != null)
		{
			m_OnReadyStateChangeDelegate(P_0);
		}
	}

	internal _IShockwaveFlashEvents_SinkHelper()
	{
		m_dwCookie = 0;
		m_FlashCallDelegate = null;
		m_FSCommandDelegate = null;
		m_OnProgressDelegate = null;
		m_OnReadyStateChangeDelegate = null;
	}
}
