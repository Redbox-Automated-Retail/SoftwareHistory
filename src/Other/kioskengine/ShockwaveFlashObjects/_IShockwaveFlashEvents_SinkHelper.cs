using System.Runtime.InteropServices;

namespace ShockwaveFlashObjects;

[ClassInterface(ClassInterfaceType.None)]
public sealed class _IShockwaveFlashEvents_SinkHelper : _IShockwaveFlashEvents
{
	public _IShockwaveFlashEvents_FlashCallEventHandler m_FlashCallDelegate;

	public _IShockwaveFlashEvents_FSCommandEventHandler m_FSCommandDelegate;

	public _IShockwaveFlashEvents_OnProgressEventHandler m_OnProgressDelegate;

	public _IShockwaveFlashEvents_OnReadyStateChangeEventHandler m_OnReadyStateChangeDelegate;

	public int m_dwCookie;

	public void FlashCall(string param0)
	{
		if (m_FlashCallDelegate != null)
		{
			m_FlashCallDelegate(param0);
		}
	}

	public void FSCommand(string param0, string param1)
	{
		if (m_FSCommandDelegate != null)
		{
			m_FSCommandDelegate(param0, param1);
		}
	}

	public void OnProgress(int param0)
	{
		if (m_OnProgressDelegate != null)
		{
			m_OnProgressDelegate(param0);
		}
	}

	public void OnReadyStateChange(int param0)
	{
		if (m_OnReadyStateChangeDelegate != null)
		{
			m_OnReadyStateChangeDelegate(param0);
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
