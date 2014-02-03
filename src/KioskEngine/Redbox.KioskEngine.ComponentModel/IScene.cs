using System;
using System.Collections.Generic;
using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IScene : IDisposable
{
	int ActiveHitHandlers { get; }

	int Width { get; }

	int Height { get; }

	string Name { get; }

	Color BackgroundColor { get; set; }

	Image BackgroundImage { get; set; }

	LinkedList<IActor> Actors { get; }

	IActor CreateActor();

	IActor HitTest(int x, int y);

	void Clear();

	void ResumeDrawing();

	void SuspendDrawing();

	void Remove(IActor actor);

	void AddLast(IActor actor);

	void AddFirst(IActor actor);

	IActor Remove(string name);

	IActor GetActor(string name);

	List<IActor> GetActorByTag(string tag);

	IActor GetActorByTabOrder(int tabOrder);

	void Render(Graphics context, out bool changed);

	void MakeDirty(Rectangle[] rectangles);
}
