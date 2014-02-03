using System;

namespace Alsing.Drawing.GDI;

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
