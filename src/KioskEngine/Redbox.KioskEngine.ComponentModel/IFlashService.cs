using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IFlashService
{
	void Reset();

	void ClearCache();

	void SwitchTopMovie(string fileName);

	void PopFlashMovie();

	string PeekFlashMovie();

	void PushFlashMovie(string fileName, Rectangle zone);
}
