using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace stdole;

internal sealed class FontEvents_EventProvider : FontEvents_Event, IDisposable
{
	private UCOMIConnectionPointContainer m_ConnectionPointContainer;

	private ArrayList m_aEventSinkHelpers;

	private UCOMIConnectionPoint m_ConnectionPoint;

	private void Init()
	{
		UCOMIConnectionPoint ppCP = null;
		Guid riid = new Guid(new byte[16]
		{
			10, 16, 246, 78, 136, 175, 208, 17, 152, 70,
			0, 192, 79, 194, 153, 147
		});
		m_ConnectionPointContainer.FindConnectionPoint(ref riid, out ppCP);
		m_ConnectionPoint = ppCP;
		m_aEventSinkHelpers = new ArrayList();
	}

	public void add_FontChanged(FontEvents_FontChangedEventHandler A_1)
	{
		Monitor.Enter(this);
		try
		{
			if (m_ConnectionPoint == null)
			{
				Init();
			}
			FontEvents_SinkHelper fontEvents_SinkHelper = new FontEvents_SinkHelper();
			int pdwCookie = 0;
			m_ConnectionPoint.Advise(fontEvents_SinkHelper, out pdwCookie);
			fontEvents_SinkHelper.m_dwCookie = pdwCookie;
			fontEvents_SinkHelper.m_FontChangedDelegate = A_1;
			m_aEventSinkHelpers.Add(fontEvents_SinkHelper);
		}
		finally
		{
			Monitor.Exit(this);
		}
	}

	public void remove_FontChanged(FontEvents_FontChangedEventHandler A_1)
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
				FontEvents_SinkHelper fontEvents_SinkHelper = (FontEvents_SinkHelper)m_aEventSinkHelpers[num];
				if (fontEvents_SinkHelper.m_FontChangedDelegate != null && ((fontEvents_SinkHelper.m_FontChangedDelegate.Equals(A_1) ? 1u : 0u) & 0xFFu) != 0)
				{
					m_aEventSinkHelpers.RemoveAt(num);
					m_ConnectionPoint.Unadvise(fontEvents_SinkHelper.m_dwCookie);
					if (count <= 1)
					{
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

	public FontEvents_EventProvider(object A_1)
	{
		m_ConnectionPointContainer = (UCOMIConnectionPointContainer)A_1;
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
					FontEvents_SinkHelper fontEvents_SinkHelper = (FontEvents_SinkHelper)m_aEventSinkHelpers[num];
					m_ConnectionPoint.Unadvise(fontEvents_SinkHelper.m_dwCookie);
					num++;
				}
				while (num < count);
			}
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
