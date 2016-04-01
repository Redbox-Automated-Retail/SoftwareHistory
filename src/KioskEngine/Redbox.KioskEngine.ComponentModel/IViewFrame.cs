using System;
using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IViewFrame : IDisposable
{
	int Width { get; }

	int Height { get; }

	bool Clear { get; }

	bool Cached { get; }

	IScene Scene { get; }

	string ViewName { get; }

	string ViewVersion { get; }

	RenderType ViewRenderType { get; }

	string ActiveFlag { get; }

	string SceneName { get; }

	ErrorList Errors { get; }

	Rectangle? ViewWindow { get; }

	Color BackgroundColor { get; }

	Image BackgroundImage { get; }

	string OnEnterResourceName { get; }

	string OnLeaveResourceName { get; }

	void UpdateScene();

	void RaiseOnEnter();

	void RaiseOnLeave();
}
