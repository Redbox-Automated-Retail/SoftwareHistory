using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace ShockwaveFlashObjects;

internal sealed class _IShockwaveFlashEvents_EventProvider : _IShockwaveFlashEvents_Event, IDisposable
{
	private UCOMIConnectionPointContainer m_ConnectionPointContainer;

	private ArrayList m_aEventSinkHelpers;

	private UCOMIConnectionPoint m_ConnectionPoint;

	private void Init()
	{
		UCOMIConnectionPoint ppCP = null;
		Guid riid = new Guid(new byte[16]
		{
			109, 219, 124, 210, 109, 174, 207, 17, 150, 184,
			68, 69, 83, 84, 0, 0
		});
		m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
		m_ConnectionPoint = ppCP;
		m_aEventSinkHelpers = new ArrayList();
	}

	public void add_FlashCall(_IShockwaveFlashEvents_FlashCallEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = new _IShockwaveFlashEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(iShockwaveFlashEvents_SinkHelper, out pdwCookie);
			iShockwaveFlashEvents_SinkHelper.m_dwCookie = pdwCookie;
			iShockwaveFlashEvents_SinkHelper.m_FlashCallDelegate = param0;
			m_aEventSinkHelpers.Add(iShockwaveFlashEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void remove_FlashCall(_IShockwaveFlashEvents_FlashCallEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = (_IShockwaveFlashEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (iShockwaveFlashEvents_SinkHelper.m_FlashCallDelegate != null && ((iShockwaveFlashEvents_SinkHelper.m_FlashCallDelegate.Equals(param0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(iShockwaveFlashEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void add_FSCommand(_IShockwaveFlashEvents_FSCommandEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = new _IShockwaveFlashEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(iShockwaveFlashEvents_SinkHelper, out pdwCookie);
			iShockwaveFlashEvents_SinkHelper.m_dwCookie = pdwCookie;
			iShockwaveFlashEvents_SinkHelper.m_FSCommandDelegate = param0;
			m_aEventSinkHelpers.Add(iShockwaveFlashEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void remove_FSCommand(_IShockwaveFlashEvents_FSCommandEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = (_IShockwaveFlashEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (iShockwaveFlashEvents_SinkHelper.m_FSCommandDelegate != null && ((iShockwaveFlashEvents_SinkHelper.m_FSCommandDelegate.Equals(param0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(iShockwaveFlashEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void add_OnProgress(_IShockwaveFlashEvents_OnProgressEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = new _IShockwaveFlashEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(iShockwaveFlashEvents_SinkHelper, out pdwCookie);
			iShockwaveFlashEvents_SinkHelper.m_dwCookie = pdwCookie;
			iShockwaveFlashEvents_SinkHelper.m_OnProgressDelegate = param0;
			m_aEventSinkHelpers.Add(iShockwaveFlashEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void remove_OnProgress(_IShockwaveFlashEvents_OnProgressEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = (_IShockwaveFlashEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (iShockwaveFlashEvents_SinkHelper.m_OnProgressDelegate != null && ((iShockwaveFlashEvents_SinkHelper.m_OnProgressDelegate.Equals(param0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(iShockwaveFlashEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void add_OnReadyStateChange(_IShockwaveFlashEvents_OnReadyStateChangeEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = new _IShockwaveFlashEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(iShockwaveFlashEvents_SinkHelper, out pdwCookie);
			iShockwaveFlashEvents_SinkHelper.m_dwCookie = pdwCookie;
			iShockwaveFlashEvents_SinkHelper.m_OnReadyStateChangeDelegate = param0;
			m_aEventSinkHelpers.Add(iShockwaveFlashEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void remove_OnReadyStateChange(_IShockwaveFlashEvents_OnReadyStateChangeEventHandler param0)
	{
		Monitor.Enter(this);
		try
		{
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 >= count)
			{
				return;
			}
			do
			{
				_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = (_IShockwaveFlashEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (iShockwaveFlashEvents_SinkHelper.m_OnReadyStateChangeDelegate != null && ((iShockwaveFlashEvents_SinkHelper.m_OnReadyStateChangeDelegate.Equals(param0) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(iShockwaveFlashEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
						Marshal.ReleaseComObject(m_ConnectionPoint);
						m_ConnectionPoint = null;
						m_aEventSinkHelpers = null;
					}
					break;
				}
				num++;
			}
			while (num < count);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public _IShockwaveFlashEvents_EventProvider(object param0)
	{
		m_ConnectionPointContainer = (UCOMIConnectionPointContainer)param0;
	}

	public void Finalize()
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				return;
			}
			int count = m_aEventSinkHelpers.Count;
			int num = 0;
			if (0 < count)
			{
				do
				{
					_IShockwaveFlashEvents_SinkHelper iShockwaveFlashEvents_SinkHelper = (_IShockwaveFlashEvents_SinkHelper)m_aEventSinkHelpers[num];
					m_ConnectionPoint.Unadvise(iShockwaveFlashEvents_SinkHelper.m_dwCookie);
					num++;
				}
				while (num < count);
			}
			Marshal.ReleaseComObject(m_ConnectionPoint);
		}
		catch (Exception)
		{
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void Dispose()
	{
		Finalize();
		GC.SuppressFinalize(this);
	}
}
