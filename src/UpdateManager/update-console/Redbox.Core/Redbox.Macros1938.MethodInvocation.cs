using System.Reflection;

namespace Redbox.Core;

internal class Redbox_002EMacros1938_002EMethodInvocation
{
	private readonly object m_target;

	private readonly MethodInfo m_method;

	private readonly object[] m_parameters;

	public Redbox_002EMacros1938_002EMethodInvocation(object target, MethodInfo method, params object[] parms)
	{
		m_target = target;
		m_method = method;
		m_parameters = parms;
	}

	public object Invoke()
	{
		return (m_method == null) ? null : m_method.Invoke(m_target, m_parameters);
	}
}
