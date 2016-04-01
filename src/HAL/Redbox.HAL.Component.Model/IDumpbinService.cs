using System;

namespace Redbox.HAL.Component.Model;

public interface IDumpbinService : IDisposable
{
	bool Active { get; }

	ILocation PutLocation { get; }

	int Capacity { get; }

	ILocation RotationLocation { get; }

	bool IsFull();

	bool IsBin(ILocation loc);

	ILocation MoveableLocation();

	int CurrentCount();

	int RemainingSpace();

	bool ResetCount();

	void DumpContents(IFormattedLog log);

	bool Initialize();
}
