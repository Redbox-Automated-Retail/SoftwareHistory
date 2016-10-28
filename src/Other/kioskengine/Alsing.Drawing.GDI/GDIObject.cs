using System;
using System.Runtime.InteropServices;

namespace Alsing.Drawing.GDI;

[ComVisible(true)]
public abstract class GDIObject : IDisposable
{
	protected bool IsCreated;

	protected virtual void Destroy()
	{
		IsCreated = false;
	}

	protected void Create()
	{
		IsCreated = true;
	}

	public void Dispose()
	{
		Destroy();
	}
}
