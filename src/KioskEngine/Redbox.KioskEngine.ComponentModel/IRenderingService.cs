using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IRenderingService
{
	IScene ActiveScene { get; set; }

	Color BackgroundColor { get; set; }

	IScene CreateScene(string name, int width, int height, Color backgroundColor);

	IScene GetScene(string name);

	void RemoveScene(string name);

	bool IsBitmapReferenced(Image image);
}
