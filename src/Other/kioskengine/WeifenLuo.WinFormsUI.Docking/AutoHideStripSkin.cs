using System.ComponentModel;
using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

[TypeConverter(typeof(AutoHideStripConverter))]
public class AutoHideStripSkin
{
	private DockPanelGradient m_dockStripGradient;

	private TabGradient m_TabGradient;

	private Font m_textFont;

	public DockPanelGradient DockStripGradient
	{
		get
		{
			return m_dockStripGradient;
		}
		set
		{
			m_dockStripGradient = value;
		}
	}

	public TabGradient TabGradient
	{
		get
		{
			return m_TabGradient;
		}
		set
		{
			m_TabGradient = value;
		}
	}

	public Font TextFont
	{
		get
		{
			return m_textFont;
		}
		set
		{
			m_textFont = value;
		}
	}

	public AutoHideStripSkin()
	{
		m_dockStripGradient = new DockPanelGradient();
		m_dockStripGradient.StartColor = SystemColors.ControlLight;
		m_dockStripGradient.EndColor = SystemColors.ControlLight;
		m_TabGradient = new TabGradient();
		m_TabGradient.TextColor = SystemColors.ControlDarkDark;
		m_textFont = SystemFonts.MenuFont;
	}
}
