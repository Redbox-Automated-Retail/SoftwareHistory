using System;
using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IBitmapCacheService : IDisposable
{
	Image GetImage(string name);

	Image RegisterImage(string name, byte[] data);

	Image RegisterImage(string name, string path);

	Image RegisterThumbnailImage(string name, byte[] data, int width, int height);

	Image RegisterThumbnailImage(string name, string newName, int width, int height);

	Image RegisterImageWithText(string name, string textToDisplay, Font textFont, Image baseImage, Rectangle textLocation);

	Image RegisterImageWithImage(string name, byte[] imageToDraw, Image baseImage, Rectangle imageLocation);

	void DropCache();

	int TrimCache(int maximumItems);
}
