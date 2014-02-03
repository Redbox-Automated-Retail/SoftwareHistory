using System;
using System.ComponentModel.Design;

namespace Redbox.Core;

internal class Redbox_002ECommand_002ETokenizer2091_002EServiceLocator : IServiceProvider
{
	private ServiceContainer m_services;

	public IServiceContainer Services
	{
		get
		{
			if (m_services == null)
			{
				m_services = new ServiceContainer();
			}
			return m_services;
		}
	}

	public static Redbox_002ECommand_002ETokenizer2091_002EServiceLocator Instance => Redbox_002ECommand_002ETokenizer2091_002ESingleton<Redbox_002ECommand_002ETokenizer2091_002EServiceLocator>.Instance;

	public T GetService<T>()
	{
		return (T)Services.GetService(typeof(T));
	}

	public object GetService(Type serviceType)
	{
		return Services.GetService(serviceType);
	}

	private Redbox_002ECommand_002ETokenizer2091_002EServiceLocator()
	{
	}
}
